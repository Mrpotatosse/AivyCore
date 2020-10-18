using AivyData.API.Proxy;
using AivyData.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace AivyData.API
{
    public class ProxyData
    {
        public string Name { get; set; }
        public string FolderPath { get; set; }
        public string ExeName { get; set; }
        public ProxyCallbackTypeEnum Type { get; set; }
        //public ProxyCustomServerData[] custom_servers { get; set; }
    }
}
