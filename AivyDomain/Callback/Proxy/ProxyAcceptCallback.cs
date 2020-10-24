using AivyData.Entities;
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
using System.Threading;

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

        protected bool _is_string_request(ClientEntity client)
        {
            if (client.Socket.Available is int available && available > 0) // ip redirect request
            {
                byte[] buffer = new byte[available];
                int code = client.Socket.Receive(buffer);

                string start_request_ip = "AIVY_REQUEST->request_ip=";
                if (Encoding.ASCII.GetString(buffer) is string _request && _request.StartsWith(start_request_ip))
                {
                    string full_ip = _request.Replace(start_request_ip, "");
                    string[] ip_port = full_ip.Split(':');

                    logger.Info($"{client} request redirect to : '{ip_port[0]}:{ip_port[1]}'");

                    try
                    {
                        _proxy.IpRedirectedStack.Enqueue(new IPEndPoint(IPAddress.Parse(ip_port[0]), int.Parse(ip_port[1])));
                        _client_disconnector.Handle(client);
                        return true;
                    }
                    catch (Exception e)
                    {
                        logger.Error(e);
                    }
                }
            }
            return false;
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

                logger.Info($"client available data : {client.Socket.Available}");
                if (!_is_string_request(client))
                {
                    while (_proxy.IpRedirectedStack.Count == 0) ;
                    ClientEntity remote = _client_creator.Handle(_proxy.IpRedirectedStack.Dequeue());
                    remote = _client_linker.Handle(remote, new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp));
                    ProxyClientReceiveCallback remote_rcv_callback = new ProxyClientReceiveCallback(remote, client, _client_repository, _client_creator, _client_linker, _client_connector, _client_disconnector, _client_sender, _proxy, ProxyTagEnum.Server);
                    remote = _client_connector.Handle(remote, new ClientConnectCallback(remote, remote_rcv_callback));

                    if (client.IsRunning && remote.IsRunning)
                    {
                        client = _client_receiver.Handle(client, new ProxyClientReceiveCallback(client, remote, _client_repository, _client_creator, _client_linker, _client_connector, _client_disconnector, _client_sender, _proxy, ProxyTagEnum.Client));

                        logger.Info("client connected");
                    }
                }
                /*if (client.Socket.Available is int available && available > 0) // ip redirect request
                {
                    byte[] buffer = new byte[available];
                    int code = client.Socket.Receive(buffer);

                    string start_request_ip = "AIVY_REQUEST->request_ip=";
                    if (Encoding.ASCII.GetString(buffer) is string _request && _request.StartsWith(start_request_ip))
                    {
                        string full_ip = _request.Replace(start_request_ip, "");
                        string[] ip_port = full_ip.Split(':');

                        logger.Info($"{client} request redirect to : '{ip_port[0]}:{ip_port[1]}'");

                        try
                        {
                            _proxy.IpRedirectedStack.Enqueue(new IPEndPoint(IPAddress.Parse(ip_port[0]), int.Parse(ip_port[1])));
                            _client_disconnector.Handle(client);
                        }
                        catch (Exception e)
                        {
                            logger.Error(e);
                        }
                    }
                }
                else
                {
                }*/

                _proxy.Socket.BeginAccept(Callback, _proxy.Socket);
            }
        }
    }
}
