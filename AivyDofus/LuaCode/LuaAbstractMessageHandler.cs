﻿using AivyDofus.Protocol.Elements;
using AivyDomain.Callback.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AivyDofus.LuaCode
{
    public class LuaAbstractMessageHandler
    {
        public string protocol_name { get; set; }
        public Action<AbstractClientReceiveCallback, NetworkElement, NetworkContentElement> handle { get; set; }

        public LuaAbstractMessageHandler(string protocol_name, Action<AbstractClientReceiveCallback, NetworkElement, NetworkContentElement> handle)
        {
            this.protocol_name = protocol_name ?? throw new ArgumentException(nameof(protocol_name));
            this.handle = handle ?? throw new ArgumentException(nameof(handle));
        }
    }
}
