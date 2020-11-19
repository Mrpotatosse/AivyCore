using AivyDofus.Protocol.Elements;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AivyDofus.Protocol.Parser
{
    public class BotofuParser
    {
        readonly string DofusInvokerPath;

        public static readonly string _this_executable_name = AppDomain.CurrentDomain.BaseDirectory;
        public static readonly string _parser_executable_name = Path.Combine(_this_executable_name, "botofu_protocol_parser.exe");

        public string _output_path => Path.Combine(_this_executable_name, $"{_output_name}.json");
        public readonly string _output_name;

        public BotofuParser(string dofusInvoker, string outputName)
        {
            _output_name = outputName ?? throw new ArgumentNullException(nameof(outputName));
            DofusInvokerPath = dofusInvoker ?? throw new ArgumentNullException(nameof(dofusInvoker));
        }

        public void Parse(Action end_parse = null)
        {
            byte[] _byte_exe = Properties.Resources.botofu_protocol_parser;
            File.WriteAllBytes(_parser_executable_name, _byte_exe);

            Process _parser_process = Process.Start(_parser_executable_name, $"--indent 1 {DofusInvokerPath} ./{_output_name}.json");

            _parser_process.EnableRaisingEvents = true;
            _parser_process.Exited += (sender, e) =>
            {
                end_parse?.Invoke();
            };
        }
    }
}
