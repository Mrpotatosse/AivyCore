using AivyData.API;
using AivyData.Entities;
using AivyDomain.Callback.Client;
using AivyDomain.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;

namespace AivyDomain.UseCases.Client
{
    public class ClientSenderRequest : IRequestHandler<ClientEntity, byte[], ClientEntity>
    {
        private readonly IRepository<ClientEntity, ClientData> _repository;

        public ClientSenderRequest(IRepository<ClientEntity, ClientData> repository)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        }

        public ClientEntity Handle(ClientEntity client, string data)
        {
            return Handle(client, Encoding.ASCII.GetBytes(data));
        }

        public ClientEntity Handle(ClientEntity client, byte[] data)
        {   
            return _repository.ActionResult(x => x.IsRunning ? x.Socket.RemoteEndPoint == client.Socket.RemoteEndPoint
                                               : x.RemoteIp == client.RemoteIp, x =>
            {
                if (data is null) throw new ArgumentNullException(nameof(data));
                if (!x.IsRunning) return x;

                x.Socket.BeginSend(data, 0, data.Length, SocketFlags.None, new ClientSendCallback(x).Callback, x.Socket);

                return x;
            });
        }

        // for lua
        public ClientEntity Handle(params object[] args)
        {
            if (args[0] is ClientEntity client && args[1] is byte[] data)
                return Handle(client, data);
            throw new ArgumentException();
        }
    }
}
