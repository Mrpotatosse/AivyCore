﻿using AivyData.Entities;
using AivyDomain.Repository;
using System;
using System.Collections.Generic;
using System.Text;

namespace AivyDomain.UseCases.Server
{
    public class ServerCreatorRequest : IRequestHandler<int, ServerEntity>
    {
        private readonly IRepository<ServerEntity> _repository;

        public ServerCreatorRequest(IRepository<ServerEntity> repository)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        }

        public ServerEntity Handle(int port)
        {
            return _repository.ActionResult(x => x.Port == port, x => 
            { 
                x.Port = port;
                return x;
            });
        }
    }
}
