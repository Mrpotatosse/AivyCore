using AivyDofus.DofusMap;
using AivyDofus.DofusMap.Map;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AivyDofus.LuaCode
{
    public class LuaMap
    {
        public Map GetMap(uint mapId)
        {
            return MapManager.SafeGetMap(mapId, Map.DefaultEncryptionKeyString);
        }
    }
}
