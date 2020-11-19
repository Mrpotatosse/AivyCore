using NLua;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace AivyDofus.Protocol.Elements
{
    public class NetworkContentElement : IDisposable
    {
        public void Dispose()
        {
            if (fields != null)
            {
                string[] keys = fields.Select(x => x.Key).ToArray();
                foreach (string key in keys)
                {
                    if (fields[key] is NetworkContentElement _son)
                        _son.Dispose();
                }
                fields.Clear();
                fields = null;
            }
        }

        public Dictionary<string, dynamic> fields { get; set; } = new Dictionary<string, dynamic>();

        public dynamic this[string key]
        {
            get
            {
                if (fields.ContainsKey(key))
                    return fields[key];
                return null;
            }
            set
            {
                if (fields.ContainsKey(key))
                {
                    fields[key] = value;
                }
                else
                {
                    fields.Add(key, value);
                }
            }
        }

        public override string ToString()
        {
            return Newtonsoft.Json.JsonConvert.SerializeObject(this, Newtonsoft.Json.Formatting.Indented);
        }

        public NetworkContentElement ReplaceElement(NetworkContentElement content)
        {
            foreach (string key in content.fields.Keys)
            {
                this[key] = content[key];
            }

            return this;
        }

        public NetworkContentElement[] ConcatTo(object[] array)
        {
            return ConcatTo(array.Select(x => x is NetworkContentElement c ? c : throw new ArgumentException(nameof(array))));
        }

        public NetworkContentElement[] ConcatTo(IEnumerable<NetworkContentElement> array)
        {
            NetworkContentElement[] result = new NetworkContentElement[array.Count() + 1];

            for (int i = 0; i < result.Length - 1; i++)
            {
                result[i] = array.ElementAt(i);
            }
            result[result.Length - 1] = this;
            return result;
        }

        public static NetworkContentElement operator +(NetworkContentElement content1, NetworkContentElement content2)
        {
            return content1.ReplaceElement(content2);
            /*foreach(string key in content2.fields.Keys)
            {
                content1[key] = content2[key];
            }

            return content1;*/
        }

        public static NetworkContentElement[] operator +(NetworkContentElement[] content1, NetworkContentElement content2)
        {
            return content2.ConcatTo(content1.AsEnumerable());
            /*NetworkContentElement[] result = new NetworkContentElement[content1.Length + 1];

            int i;
            for(i = 0; i < content1.Length; i++)
            {
                result[i] = content1[i];
            }
            result[i] = content2;

            return result;*/
        }
    }
}
