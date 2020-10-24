using AivyDofus.DofusMap;
using AivyDofus.DofusMap.Visualizer;
using AivyDofus.Handler;
using AivyDofus.LuaCode;
using AivyDofus.Pathfinding;
using AivyDofus.Protocol.Elements;
using AivyDofus.Proxy;
using AivyDofus.Proxy.Handlers;
using KeraLua;
using NLog;
using NLog.Config;
using NLog.Targets;
using NLua;
using System;
using System.Runtime.Remoting.Channels;
using System.Text;

namespace AivyDofus
{
    public class Program
    {
        static readonly ConsoleTarget log_console = new ConsoleTarget("log_console");
        static readonly FileTarget log_file = new FileTarget("log_file") { FileName = "./log.txt" };
        static readonly LoggingConfiguration configuration = new LoggingConfiguration();

        public static void Main(string[] args)
        {
            int reader = 0;
            Console.Title = "AivyCore - 1.0.0";

            configuration.AddRule(LogLevel.Error, LogLevel.Error, log_console);
            if(args.Length > 0 && args[reader] == "-l")
            {
                reader++;
                configuration.AddRule(LogLevel.Debug, LogLevel.Fatal, log_console);
            }

            LogManager.Configuration = configuration;

            using (CodeSession session = new CodeSession("System",
                                                         "System.Link",
                                                         "AivyData",
                                                         "AivyData.API",
                                                         "AivyData.API.Shared",
                                                         "AivyData.API.Shared.Actors",
                                                         "AivyData.Enums",
                                                         "AivyDofus",
                                                         "AivyDofus.DofusMap",
                                                         "AivyDofus.DofusMap.Map",
                                                         "AivyDofus.DofusMap.Visualizer",
                                                         "AivyDofus.Handler",
                                                         "AivyDofus.IO",
                                                         "AivyDofus.LuaCode",
                                                         "AivyDofus.Pathfinding",
                                                         "AivyDofus.Protocol",
                                                         "AivyDofus.Protocol.Buffer",
                                                         "AivyDofus.Protocol.Elements",
                                                         "AivyDofus.Proxy",
                                                         "AivyDomain"))
            {
                // global variables
                session["multi_proxy"] = new DofusMultiProxy();
                session["protocol_manager"] = BotofuProtocolManager.Instance;
                session["proxy_handlers"] = MessageHandler<ProxyHandlerAttribute>.LuaHandler;
                session["sleeper"] = new CodeSleep();
                session["maps"] = new LuaMap();
                session["visualizer"] = VisualizerManager.Visualizer;

                // lua code init
                try
                {
                    session.Execute(Encoding.UTF8.GetString(Properties.Resources.AivyDofusLua));
                    session.Execute(Encoding.UTF8.GetString(Properties.Resources.AivyDofusLuaHandler));
                }
                catch(Exception error)
                {
                    CodeSession.logger.Error(error);
                }

                while (Console.ReadLine() is string _code)
                {
                    switch (_code)
                    {
                        case "log":
                            configuration.AddRule(LogLevel.Debug, LogLevel.Fatal, log_console);
                            LogManager.Configuration = configuration; break;
                        case "clear": Console.Clear(); break;
                        case "map": VisualizerManager.OpenUI(); break;
                        case "end": Environment.Exit(0); break;
                        case "reload_handler": session.Execute(Encoding.UTF8.GetString(Properties.Resources.AivyDofusLuaHandler)); break;
                        default:
                            if (session.Execute(_code) is Exception error)
                            {
                                CodeSession.logger.Error(error);
                            }
                            break;
                    }
                }
            }
        }

    }
}
