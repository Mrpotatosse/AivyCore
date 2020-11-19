using AivyData.API.Proxy;
using AivyData.Entities;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace AivyDomain.Mappers.Proxy
{
    public class ProxyEntityMapper : IMapper<Func<ProxyEntity, bool>, ProxyEntity>
    {
        private readonly ObservableCollection<ProxyEntity> _proxys;

        public ProxyEntityMapper()
        {
            _proxys = new ObservableCollection<ProxyEntity>();
        }

        public ProxyEntity MapFrom(Func<ProxyEntity, bool> input)
        {
            if (input is null) throw new ArgumentNullException(nameof(input));
            if(input(new ProxyEntity()))
            {
                ProxyEntity proxy = new ProxyEntity()
                {
                    Socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp),
                    IsRunning = false,
                    HookInterface = new HookInterfaceEntity(),
                    IpRedirectedStack = new Queue<IPEndPoint>(),
                    AccountData = new ProxyAccountMinimumInformationData()
                };
                _proxys.Add(proxy);
                return proxy;
            }

            return _proxys.FirstOrDefault(input);
        }

        public bool Remove(Func<ProxyEntity, bool> predicat)
        {
            return _proxys.Remove(_proxys.FirstOrDefault(predicat));
        }
    }
}
