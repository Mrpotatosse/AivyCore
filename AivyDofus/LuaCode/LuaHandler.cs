using AivyDofus.Extension;
using AivyDofus.Protocol.Elements;
using AivyDomain.Callback.Client;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AivyDofus.LuaCode
{
    public class LuaHandler
    {
        static readonly Logger logger = LogManager.GetCurrentClassLogger();

        private readonly Dictionary<string, IList<Action<AbstractClientReceiveCallback, NetworkElement, NetworkContentElement>>> _handlers = 
            new Dictionary<string, IList<Action<AbstractClientReceiveCallback, NetworkElement, NetworkContentElement>>>();

        public async Task Execute(string protocol_name, AbstractClientReceiveCallback callback, NetworkElement element, NetworkContentElement content)
        {
            if (_handlers.ContainsKey(protocol_name)) 
            {
                foreach (Action<AbstractClientReceiveCallback, NetworkElement, NetworkContentElement> action in _handlers[protocol_name])
                    await AsyncExtension.ExecuteAsync(() => { action(callback, element, content); }, null, e => { logger.Error(e); });
            }
        }

        public void Set(string protocol_name, IList<Action<AbstractClientReceiveCallback, NetworkElement, NetworkContentElement>> action)
        {
            Clear(protocol_name);
            _handlers.Add(protocol_name, action);
        }

        public void Add(LuaAbstractMessageHandler message)
        {
            Add(message.protocol_name, message.handle);
        }

        public void Add(string protocol_name, Action<AbstractClientReceiveCallback, NetworkElement, NetworkContentElement> action)
        {
            if (_handlers.ContainsKey(protocol_name))
                _handlers[protocol_name].Add(action);
            else
                _handlers.Add(protocol_name, new Action<AbstractClientReceiveCallback, NetworkElement, NetworkContentElement>[] { action });
        }

        public void Clear(string protocol_name)
        {
            if (_handlers.ContainsKey(protocol_name))
                _handlers.Remove(protocol_name);
        }
    }
}
