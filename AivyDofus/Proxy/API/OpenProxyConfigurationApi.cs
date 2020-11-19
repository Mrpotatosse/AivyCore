using AivyData.API;
using AivyData.API.Proxy;
using AivyDomain.API;
using Newtonsoft.Json;
using NLog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace AivyDofus.Proxy.API
{
    public class OpenProxyConfigurationApi : IApi<ProxyData>
    {
        static readonly Logger logger = LogManager.GetCurrentClassLogger();

        readonly string _location;

        private List<ProxyData> _data { get; set; }

        public ProxyData[] Data
        {
            get
            {
                return _data.ToArray();
            }
        }

        public OpenProxyConfigurationApi(string location)
        {
            _location = location ?? throw new ArgumentNullException(nameof(location));
            _save();
        }

        private bool _save()
        {
            try
            {
                _data = _fromFile();
                _toFile();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public string IndentedJsonData
        {
            get
            {
                return JsonConvert.SerializeObject(_data, Formatting.Indented);
            }
        }

        private List<ProxyData> _fromFile()
        {
            try
            {
                return JsonConvert.DeserializeObject<List<ProxyData>>(File.ReadAllText(_location), new JsonSerializerSettings() { Formatting = Formatting.Indented });
            }
            catch(FileNotFoundException)
            {
                logger.Info($"file : '{_location}' not found -> file creation");
                return new List<ProxyData>();
            }
            catch(Exception e)
            {
                logger.Error(e);
                return new List<ProxyData>();
            }
        }

        private bool _toFile()
        {
            try
            {
                File.WriteAllText(_location, IndentedJsonData);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public ProxyData GetData(Func<ProxyData, bool> predicat)
        {
            _save();
            return _data.FirstOrDefault(predicat);
        }

        public ProxyData UpdateData(ProxyData data)
        {
            if (_data.FirstOrDefault(x => x.Name == data.Name) is ProxyData _found)
            {
                if (data.FolderPath is null || data.ExeName is null)
                {
                    if (_data.Remove(_found))
                    {
                        if (_toFile())
                            return _found;
                        return null;
                    }
                    return null;
                }

                _found.ExeName = data.ExeName;
                _found.FolderPath = data.FolderPath;
                _found.Type = data.Type;

                if (_toFile())
                    return _found;
                return null;
            }

            _data.Add(data);
            if (_toFile()) return data;
            return null;
        }
    }
}
