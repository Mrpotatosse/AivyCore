using AivyDofus.DofusMap.File;
using AivyDofus.Extension;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AivyDofus.DofusMap
{
    public class MapManager : SafeSingelton
    {
        static MapManager manager;// = new MapManager();

        public static Map.Map SafeGetMap(uint mapId, string key)
        {
            return SafeGetMap(mapId, Encoding.UTF8.GetBytes(key));
        }

        public static Map.Map SafeGetMap(uint mapId, byte[] key)
        {
            if (manager is null) manager = new MapManager();

            Map.Map ret = null;
            manager.SafeRun(() =>
            {
                ret = manager.GetMap(mapId, key);
            });
            return ret;
        }

        public static void InitManager(string folder_path)
        {
            manager = new MapManager(folder_path);
        }

        public Folder Folder { get; private set; }

        private MapManager(string path = @"D:\AppDofus\content\maps")
        {
            Folder = new Folder(path);
        }

        Mutex mutex = new Mutex();

        public Map.Map GetMap(uint mapId, byte[] key)
        {
            mutex.WaitOne();
            foreach (File.File f in Folder.Files)
            {
                if (f.LoadedMaps.ContainsKey(mapId))
                {
                    Map.Map m = new Map.Map(f.LoadedMaps[mapId], key);
                    mutex.ReleaseMutex();
                    return m;
                }
            }
            mutex.ReleaseMutex();
            throw new NotImplementedException();
        }
    }
}
