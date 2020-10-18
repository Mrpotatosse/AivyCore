using NLog;
using NLua;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AivyDofus.LuaCode
{
    public class CodeSession : IDisposable
    {
        public static readonly Logger logger = LogManager.GetCurrentClassLogger();

        private readonly Lua _lua = new Lua();

        public CodeSession(params string[] importing)
        {
            _lua.LoadCLRPackage();

            foreach (string import in importing)
                Execute($"import '{import}'");
        }

        public Exception ExecuteFile(string filename)
        {
            try
            {
                _lua.DoFile(filename);
                return null;
            }
            catch (Exception e)
            {
                return e;
            }
        }

        public Exception Execute(string code)
        {
            try
            {
                _lua.DoString(code);
                return null;
            }
            catch (Exception e)
            {
                return e;
            }
        }

        public object this[string name]
        {
            get
            {
                return _lua[name];
            }
            set
            {
                _lua[name] = value;
            }
        }

        public T Element<T>(string name) where T : class
        {
            return this[name] as T;
        }

        public void Dispose()
        {
            _lua.Dispose();
        }
    }
}
