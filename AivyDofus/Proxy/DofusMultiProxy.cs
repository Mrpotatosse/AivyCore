using AivyData.API;
using AivyData.Entities;
using AivyData.Enums;
using AivyDofus.DofusMap;
using AivyDofus.Protocol.Elements;
using AivyDofus.Protocol.Parser;
using AivyDofus.Proxy.API;
using AivyDofus.Proxy.Callbacks;
using AivyDomain.API;
using AivyDomain.Callback.Client;
using AivyDomain.Callback.Proxy;
using AivyDomain.Mappers.Proxy;
using AivyDomain.Repository.Proxy;
using AivyDomain.UseCases.Proxy;
using NLog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace AivyDofus.Proxy
{
    // for lua
    public class DofusMultiProxy : DofusMultiProxy<OpenProxyConfigurationApi>
    {
        public DofusMultiProxy() : base() { }
    }

    public class DofusMultiProxy<ProxyApi> where ProxyApi : class, IApi<ProxyData>
    {
        static readonly Logger logger = LogManager.GetCurrentClassLogger();

        public readonly ProxyApi _proxy_api = Activator.CreateInstance(typeof(ProxyApi), new object[] { "./proxy_api_information.json" }) as ProxyApi;

        public readonly ProxyRepository _proxy_repository;

        protected readonly ProxyEntityMapper _proxy_mapper = new ProxyEntityMapper();

        protected readonly ProxyCreatorRequest _proxy_creator;
        protected readonly RemoteProxyCreatorRequest _remote_proxy_creator;
        protected readonly ProxyActivatorRequest _proxy_activator;

        private readonly Dictionary<int, ProxyAcceptCallback> _proxy_callbacks;

        public ProxyAcceptCallback this[int port]
        {
            get
            {
                try
                {
                    return _proxy_callbacks[port];
                }
                catch { return null; }
            }
        }

        public DofusMultiProxy()
        {
            if(_proxy_repository is null)
            {
                _proxy_repository = new ProxyRepository(_proxy_api, _proxy_mapper);
                _proxy_creator = new ProxyCreatorRequest(_proxy_repository);
                _remote_proxy_creator = new RemoteProxyCreatorRequest(_proxy_repository);
                _proxy_activator = new ProxyActivatorRequest(_proxy_repository);

                _proxy_callbacks = new Dictionary<int, ProxyAcceptCallback>();
            }
        }

        public ProxyEntity RemoteActive(bool active, int port, string redirection_ip, string folder_path, string exe_name)
        {
            string exe_path = Path.Combine(folder_path, $"{exe_name}.exe");
            if (active)
            {
                ProxyEntity result = _remote_proxy_creator.Handle(exe_path, port, redirection_ip);
                ProxyAcceptCallback callback = new ProxyAcceptCallback(result);
                result = _proxy_activator.Handle(result, active, callback);
                return result;
            }
            else
            {
                if (_proxy_repository.Remove(x => x.Port == port))
                {
                    _proxy_callbacks.Remove(port);
                    return null;
                }
                throw new ArgumentNullException($"cannot disable proxy with port : {port}");
            }
        }

        public ProxyEntity RemoteActive(ProxyCallbackTypeEnum type, bool active, int port, string redirection_ip, string folder_path, string exe_name)
        {
            switch (type)
            {
                case ProxyCallbackTypeEnum.Dofus2: return RemoteActive<DofusProxyAcceptCallback>(active, port, redirection_ip, folder_path, exe_name);
                case ProxyCallbackTypeEnum.DofusRetro: return RemoteActive<DofusRetroProxyAcceptCallback>(active, port, redirection_ip, folder_path, exe_name);
                default: return RemoteActive(active, port, redirection_ip, folder_path, exe_name);
            }
        }

        public ProxyEntity RemoteActive<Callback>(bool active, int port, string redirection_ip, string folder_path, string exe_name) where Callback : ProxyAcceptCallback
        {
            string exe_path = Path.Combine(folder_path, $"{exe_name}.exe");
            if (active)
            {
                ProxyEntity result = _remote_proxy_creator.Handle(exe_path, port, redirection_ip);
                Callback callback = Activator.CreateInstance(typeof(Callback), new object[] { result }) as Callback;
                result = _proxy_activator.Handle(result, active, callback);
                return result;
            }
            else
            {
                if (_proxy_repository.Remove(x => x.Port == port))
                {
                    _proxy_callbacks.Remove(port);
                    return null;
                }
                throw new ArgumentNullException($"cannot disable proxy with port : {port}");
            }
        }

        public ProxyEntity Active<Callback>(bool active, int port, string exe_path, out Callback callback) where Callback : ProxyAcceptCallback
        {
            if (active)
            {
                ProxyEntity result = _proxy_creator.Handle(exe_path, port);
                callback = Activator.CreateInstance(typeof(Callback), new object[] { result }) as Callback;
                result = _proxy_activator.Handle(result, active, callback);
                return result;
            }
            else
            {
                callback = null;
                if (_proxy_repository.Remove(x => x.Port == port))
                {
                    _proxy_callbacks.Remove(port);
                    return null;
                }
                throw new ArgumentNullException($"cannot disable proxy with port : {port}");
            }
        }

        public ProxyEntity Active(ProxyCallbackTypeEnum type, bool active, int port, string folder_path, string exe_name)
        {
            string exe_path = Path.Combine(folder_path, $"{exe_name}.exe");
            if (!File.Exists(exe_path))
            {
                Console.WriteLine($"NOT FOUND : {exe_path}");
                return null;
            }
            ProxyEntity result;
            switch (type)
            {
                case ProxyCallbackTypeEnum.Dofus2:
                    string invoker_path = Path.Combine(folder_path, "DofusInvoker.swf");
                    string map_path = Path.Combine(folder_path, @"content\maps");
                    if (BotofuProtocolManager.Instance[ProxyCallbackTypeEnum.Dofus2] is null)
                    {
                        BotofuProtocolManager.Instance.AddParser(type, new BotofuParser(invoker_path, "MultiProxyProtocol"));
                        MapManager.InitManager(map_path);
                    }
                    result = Active(active, port, exe_path, out DofusProxyAcceptCallback dofus2_callback);
                    if(dofus2_callback != null)
                        _proxy_callbacks.Add(port, dofus2_callback);
                    return result;
                case ProxyCallbackTypeEnum.DofusRetro:
                    result = Active(active, port, exe_path, out DofusRetroProxyAcceptCallback dofusretro_callback);
                    if (dofusretro_callback != null)
                        _proxy_callbacks.Add(port, dofusretro_callback);
                    return result;
                default: 
                    logger.Info("default proxy callback was called");
                    result = Active(active, port, exe_path, out ProxyAcceptCallback callback); 
                    if (callback != null)
                        _proxy_callbacks.Add(port, callback);
                    return result;
            }
        }
    }
}
