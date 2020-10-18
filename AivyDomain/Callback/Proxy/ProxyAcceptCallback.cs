﻿using AivyData.Entities;
using AivyData.Enums;
using AivyDomain.API.Client;
using AivyDomain.Callback.Client;
using AivyDomain.Mappers.Client;
using AivyDomain.Repository;
using AivyDomain.Repository.Client;
using AivyDomain.UseCases.Client;
using NLog;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace AivyDomain.Callback.Proxy
{
    public class ProxyAcceptCallback : ProxyCallback
    {
        static readonly Logger logger = LogManager.GetCurrentClassLogger();

        protected readonly OpenClientApi _client_api;
        protected readonly ClientEntityMapper _client_mapper;

        public readonly ClientRepository _client_repository;

        protected readonly ClientCreatorRequest _client_creator;
        protected readonly ClientConnectorRequest _client_connector;

        protected readonly ClientLinkerRequest _client_linker;
        protected readonly ClientReceiverRequest _client_receiver;

        public readonly ClientDisconnectorRequest _client_disconnector;
        public readonly ClientSenderRequest _client_sender;

        public virtual ClientEntity _main_local_client()
        {
            return _client_repository.GetResult(x => x.IsGameClient && x.RemoteIp.Address.ToString() == "127.0.0.1" && x.IsRunning);
        }

        public virtual ClientEntity _main_remote_client()
        {
            return _client_repository.GetResult(x => x.IsGameClient && x.RemoteIp.Address.ToString() != "127.0.0.1" && x.IsRunning);
        }

        public ProxyAcceptCallback(ProxyEntity proxy) 
            : base(proxy)
        {
            _client_api = new OpenClientApi("./proxy_callback_api.json");
            _client_mapper = new ClientEntityMapper();
            _client_repository = new ClientRepository(_client_api, _client_mapper);

            _client_creator = new ClientCreatorRequest(_client_repository);
            _client_connector = new ClientConnectorRequest(_client_repository);
            _client_disconnector = new ClientDisconnectorRequest(_client_repository);
            _client_linker = new ClientLinkerRequest(_client_repository);
            _client_receiver = new ClientReceiverRequest(_client_repository);
            _client_sender = new ClientSenderRequest(_client_repository);
        }

        public override void Callback(IAsyncResult result)
        {
            if (!_proxy.IsRunning) return;

            _proxy.Socket = (Socket)result.AsyncState;

            if (_proxy.IsRunning)
            {
                Socket _client_socket = _proxy.Socket.EndAccept(result);

                ClientEntity client = _client_creator.Handle(_client_socket.RemoteEndPoint as IPEndPoint);
                client = _client_linker.Handle(client, _client_socket);

                ClientEntity remote = _client_creator.Handle(_proxy.IpRedirectedStack.Dequeue());
                remote = _client_linker.Handle(remote, new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp));
                ProxyClientReceiveCallback remote_rcv_callback = new ProxyClientReceiveCallback(remote, client, _client_repository, _client_creator, _client_linker, _client_connector, _client_disconnector, _client_sender, ProxyTagEnum.Server);
                remote = _client_connector.Handle(remote, new ClientConnectCallback(remote, remote_rcv_callback));


                if (client.IsRunning && remote.IsRunning)
                {
                    client = _client_receiver.Handle(client, new ProxyClientReceiveCallback(client, remote, _client_repository, _client_creator, _client_linker, _client_connector, _client_disconnector, _client_sender, ProxyTagEnum.Client));

                    logger.Info("client connected");
                }

                _proxy.Socket.BeginAccept(Callback, _proxy.Socket);
            }
        }
    }
}
