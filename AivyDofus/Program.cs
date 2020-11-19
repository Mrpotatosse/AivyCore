using AivyDofus.DofusMap;
using AivyDofus.DofusMap.Visualizer;
using AivyDofus.Handler;
using AivyDofus.LuaCode;
using AivyDofus.Pathfinding;
using AivyDofus.Protocol.Elements;
using AivyDofus.Proxy;
using AivyDofus.Proxy.Handlers;
using AivyDofus.Server;
using AivyDofus.Server.Handlers;
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

        public static readonly DofusMultiProxy Multy_Proxy = new DofusMultiProxy();
        public static readonly DofusServer Dofus_Server = new DofusServer();

        public static void Main(string[] args)
        {
            int reader = 0;
            Console.Title = "AivyCore - 1.0.0";

            configuration.AddRule(LogLevel.Error, LogLevel.Error, log_console);
            if(args.Length > reader && args[reader] == "-l")
            {
                reader++;
                configuration.AddRule(LogLevel.Debug, LogLevel.Fatal, log_console);
            }

            LogManager.Configuration = configuration;

            using (CodeSession session = new CodeSession("System",
                                                         "System.Link",
                                                         "AivyData",
                                                         "AivyData.API",
                                                         "AivyData.API.Server",// for server
                                                         "AivyData.API.Server.Actor",// for server
                                                         "AivyData.API.Server.Look",// for server
                                                         "AivyData.API.Shared",
                                                         "AivyData.API.Shared.Actors",
                                                         "AivyData.Enums",
                                                         "AivyDofus",
                                                         "AivyDofus.DofusMap",
                                                         "AivyDofus.DofusMap.Map",
                                                         "AivyDofus.DofusMap.Visualizer",
                                                         "AivyDofus.Extension.Server.Data",// for server
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
                session["multi_proxy"] = Multy_Proxy;
                session["dofus_server"] = Dofus_Server;
                session["protocol_manager"] = BotofuProtocolManager.Instance;
                session["proxy_handlers"] = MessageHandler<ProxyHandlerAttribute>.LuaHandler;
                session["server_handlers"] = MessageHandler<ServerHandlerAttribute>.LuaHandler;
                session["sleeper"] = new CodeSleep();
                session["maps"] = new LuaMap();
                //session["visualizer"] = VisualizerManager.LuaInstance;
                // game
                session["breeds"] = StaticValues.BREEDS;
                session["heads"] = StaticValues.HEADS;

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

                Console.WriteLine("waiting code ...");
                while (Console.ReadLine() is string _code)
                {
                    switch (_code)
                    {
                        case "test":
                            for (int i = 0; i < 100; i++) Console.WriteLine($"test {i}");
                            break;
                        case "log":
                            configuration.AddRule(LogLevel.Debug, LogLevel.Fatal, log_console);
                            LogManager.Configuration = configuration; break;
                        case "clear": Console.Clear(); break;
                        //case "map": Console.WriteLine("PORT ?"); 
                            //if(int.TryParse(Console.ReadLine(), out int port)) VisualizerManager.OpenUI(port);                             
                            //break;
                        case "end": Environment.Exit(0); break;
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
