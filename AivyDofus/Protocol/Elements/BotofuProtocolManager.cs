using AivyData.Enums;
using AivyDofus.Protocol.Parser;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace AivyDofus.Protocol.Elements
{
    public sealed class BotofuProtocolManager
    {
        #region public properties
        private readonly Dictionary<ProxyCallbackTypeEnum, BotofuProtocol> _parser;

        private BotofuProtocol _read_from_parser(BotofuParser parser)
        {
            return JsonConvert.DeserializeObject<BotofuProtocol>(File.ReadAllText(parser._output_path), new JsonSerializerSettings() { Formatting = Formatting.Indented });
        }

        public BotofuProtocol this[ProxyCallbackTypeEnum callback]
        {
            get
            {
                if(_parser.ContainsKey(callback))
                    return _parser[callback];
                return null;
            }
        }

        private BotofuProtocolManager()
        {
            _parser = new Dictionary<ProxyCallbackTypeEnum, BotofuProtocol>();
        }

        public void AddParser(ProxyCallbackTypeEnum type, BotofuParser parser)
        {
            if (_parser.ContainsKey(type))
            {
                if (!RemoveParser(type))
                {
                    return;
                }
            }

            parser.Parse(() =>
            {
                _parser.Add(type, _read_from_parser(parser));
            });
        }

        public bool RemoveParser(ProxyCallbackTypeEnum type)
        {
            if(_parser.ContainsKey(type))
                return _parser.Remove(type);
            return false;
        }
        #endregion

        #region singleton
        private static readonly object syncroot = new object();

        public static BotofuProtocolManager Instance
        {
            get
            {
                lock (syncroot)
                {
                    return SingletonAllocator.instance;
                }
            }
            set
            {
                SingletonAllocator.instance = value;
            }
        }

        internal static class SingletonAllocator
        {
            internal static BotofuProtocolManager instance;

            static SingletonAllocator()
            {
                CreateInstance(typeof(BotofuProtocolManager));
            }

            public static BotofuProtocolManager CreateInstance(Type type)
            {
                var ctorsPublic = type.GetConstructors(
                    BindingFlags.Instance | BindingFlags.Public);

                if (ctorsPublic.Length > 0)
                    return instance = (BotofuProtocolManager)Activator.CreateInstance(type);

                var ctorNonPublic = type.GetConstructor(
                    BindingFlags.Instance | BindingFlags.NonPublic, null, Type.EmptyTypes, new ParameterModifier[0]);

                if (ctorNonPublic == null)
                    throw new Exception(
                        type.FullName +
                        " doesn't have a private/protected constructor so the property cannot be enforced.");

                try
                {
                    return instance = (BotofuProtocolManager)ctorNonPublic.Invoke(new object[0]);
                }
                catch (Exception e)
                {
                    throw new Exception(
                        "The Singleton couldnt be constructed, check if " + type.FullName +
                        " has a default constructor",
                        e);
                }
            }
        }
        #endregion
    }
}
