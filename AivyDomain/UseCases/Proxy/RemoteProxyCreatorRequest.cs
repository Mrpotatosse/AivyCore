using AivyData.API;
using AivyData.Entities;
using AivyDomain.Repository;
using NLog;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace AivyDomain.UseCases.Proxy
{
    public class RemoteProxyCreatorRequest : IRequestHandler<string, int, string, ProxyEntity>
    {
        static readonly Logger logger = LogManager.GetCurrentClassLogger();

        private readonly IRepository<ProxyEntity, ProxyData> _repository;
        private readonly HookCreatorRequest _hook_creator;

        public RemoteProxyCreatorRequest(IRepository<ProxyEntity, ProxyData> repository)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            _hook_creator = new HookCreatorRequest(_repository);
        }

        public ProxyEntity Handle(string exePath, int port, string redirectionIp)
        {
            return _repository.ActionResult(x => true, x =>
            {
                x.Port = port;
                x.HookInterface.OnIpRedirected += (ip, processId, portRed) =>
                {
                    IPEndPoint remoteIp = new IPEndPoint(IPAddress.Parse(redirectionIp), port);
                    string message = $"AIVY_REQUEST->request_ip={ip.Address}:{ip.Port}";

                    using(OneLifeClientEntity message_sender = new OneLifeClientEntity(remoteIp, message))
                    {
                        message_sender._connect_and_send();
                    }
                };
                x.HookInterface.IpGetterFunction += () =>
                {
                    return redirectionIp;
                };

                if(System.IO.File.Exists(exePath))
                    x.Hooker = _hook_creator.Handle(exePath, x);

                return x;
            });
        }
    }
}
