﻿using AivyDofus.Extension;
using AivyDofus.Protocol.Elements;
using AivyDomain.Callback.Client;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace AivyDofus.LuaCode
{
    public class LuaHandler
    {
        static readonly Logger logger = LogManager.GetCurrentClassLogger();

        private readonly Dictionary<string, IEnumerable<Func<AbstractClientReceiveCallback, NetworkElement, NetworkContentElement, bool>>> _handlers = 
            new Dictionary<string, IEnumerable<Func<AbstractClientReceiveCallback, NetworkElement, NetworkContentElement, bool>>>();

        public async Task<bool> Execute(string protocol_name, AbstractClientReceiveCallback callback, NetworkElement element, NetworkContentElement content)
        {
            bool result = true;

            if (_handlers.ContainsKey(protocol_name)) 
            {
                foreach (Func<AbstractClientReceiveCallback, NetworkElement, NetworkContentElement, bool> action in _handlers[protocol_name])
                    await AsyncExtension.ExecuteAsync(() => { result = result && action(callback, element, content); }, null, e => { logger.Error(e); });
            }

            return result;
        }

        public void Set(string protocol_name, IList<Func<AbstractClientReceiveCallback, NetworkElement, NetworkContentElement, bool>> action)
        {
            Clear(protocol_name);
            _handlers.Add(protocol_name, action);
        }

        public void Add(LuaAbstractMessageHandler message)
        {
            Add(message.protocol_name, message.handle);
        }

        public void Add(string protocol_name, Func<AbstractClientReceiveCallback, NetworkElement, NetworkContentElement, bool> action)
        {
            if (_handlers.ContainsKey(protocol_name))
            {
                _handlers[protocol_name] = _handlers[protocol_name].Append(action);
            }
            else
                _handlers.Add(protocol_name, new Func<AbstractClientReceiveCallback, NetworkElement, NetworkContentElement, bool>[] { action });
        }

        public void Clear(string protocol_name)
        {
            if (_handlers.ContainsKey(protocol_name))
                _handlers.Remove(protocol_name);
        }
    }
}