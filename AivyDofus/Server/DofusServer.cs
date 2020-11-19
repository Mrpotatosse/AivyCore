using AivyData.API;
using AivyData.Entities;
using AivyData.Enums;
using AivyDofus.DofusMap;
using AivyDofus.Protocol.Elements;
using AivyDofus.Protocol.Parser;
using AivyDofus.Server.API;
using AivyDofus.Server.Callbacks;
using AivyDomain.API;
using AivyDomain.API.Server;
using AivyDomain.Mappers.Server;
using AivyDomain.Repository.Server;
using AivyDomain.UseCases.Server;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace AivyDofus.Server
{
    public class DofusServer
    {
        public readonly OpenServerDatabaseApi _server_api = new OpenServerDatabaseApi("./server_database.db");

        readonly ServerEntityMapper _server_mapper = new ServerEntityMapper();

        public readonly ServerRepository _server_repository;
        readonly ServerCreatorRequest _server_creator;
        readonly ServerActivatorRequest _server_activator;

        public string _app_emplacement { get; set; }

        public DofusServer()
        {
            if (_server_repository is null)
            {
                _server_repository = new ServerRepository(_server_api, _server_mapper);
                _server_creator = new ServerCreatorRequest(_server_repository);
                _server_activator = new ServerActivatorRequest(_server_repository);
            }
        }

        public ServerEntity Active(bool active, int port)
        {
            if (active)
            {
                string invoker_path = Path.Combine(_app_emplacement ?? "./", "DofusInvoker.swf");
                string map_path = Path.Combine(_app_emplacement ?? "./", @"content\maps");
                if (BotofuProtocolManager.Instance[ProxyCallbackTypeEnum.Dofus2] is null)
                {
                    if (File.Exists(invoker_path) && Directory.Exists(map_path))
                    {
                        BotofuProtocolManager.Instance.AddParser(ProxyCallbackTypeEnum.Dofus2, new BotofuParser(invoker_path, "MultiProxyProtocol"));
                        MapManager.InitManager(map_path);
                    }
                    else
                    {
                        throw new Exception(nameof(_app_emplacement));
                    }
                }

                ServerEntity result = _server_creator.Handle(port);
                result = _server_activator.Handle(result, active, new DofusServerAcceptCallback(result));
                return result;
            }
            else
            {
                if (_server_repository.Remove(x => x.Port == port))
                {
                    return null;
                }
                throw new ArgumentNullException($"cannot disable proxy with port : {port}");
            }
        }
    }
}
