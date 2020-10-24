using NLog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AivyDofus.DofusMap.File
{
    public class Folder
    {
        static readonly Logger logger = LogManager.GetCurrentClassLogger();

        #region Declarations

        public string Path { get; set; }
        public List<File> Files { get; set; }

        #endregion

        #region Constructeur

        public Folder(string _folderPath)
        {
            Path = _folderPath;
            Files = new List<File>();

            Initialize();
        }

        #endregion

        #region Methode privee

        private void Initialize()
        {
            if (!Directory.Exists(Path))
            {
                logger.Error("[Common.Data.Maps.File] Invalid Folder Path");
            }

            foreach (string File in Directory.GetFiles(Path, "*.d2p", SearchOption.TopDirectoryOnly))
            {
                Files.Add(new File(File));
            }

            if (Files.Count == 0)
            {
                new ArgumentException("[Common.Data.Maps.File] No valid files found");
            }
        }

        #endregion

    }
}
