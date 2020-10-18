﻿using AivyData.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace AivyDomain.Mappers.Client
{
    public class ClientEntityMapper : IMapper<Func<ClientEntity, bool>, ClientEntity>
    {
        private readonly object _locker = new object();
        private readonly List<ClientEntity> _clients;

        public ClientEntityMapper()
        {
            _clients = new List<ClientEntity>();
        }

        public ClientEntity MapFrom(Func<ClientEntity, bool> input)
        {
            lock (_locker)
            {
                if (input is null) throw new ArgumentNullException(nameof(input));
                if (input(new ClientEntity()))
                {
                    ClientEntity client = new ClientEntity()
                    {
                        ReceiveBufferLength = 4096 // to do -> get from file ? 
                    };
                    _clients.Add(client);
                    return client;
                }
                return _clients.FirstOrDefault(input);
            }
        }

        public bool Remove(Func<ClientEntity, bool> predicat)
        {
            return _clients.Remove(_clients.FirstOrDefault(predicat));
        }
    }
}
