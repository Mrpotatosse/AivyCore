using AivyDofus.DofusMap;
using AivyDofus.DofusMap.Map;
using AivyDofus.Pathfinding;
using AivyDofus.Pathfinding.Positions;
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

        public int[] CompressedPath(GameMapData map, int start, int end, bool inFight = false, bool troughEntity = true, bool diagonal = true, bool obstacle = true)
        {
            using (DofusPathfinder pathfinder = new DofusPathfinder(map, inFight, troughEntity, diagonal, obstacle))
            {
                MovementPath path = pathfinder.FindPath(start, end);
                int[] result = new int[2 + path.Cells.Count];
                result[0] = path.CellStart.CellId;
                result[result.Length - 1] = path.CellEnd.CellId;
                for (int i = 0; i < path.Cells.Count; i++)
                {
                    result[i + 1] = path.Cells[i].Cell.CellId;
                }
                return result;
            }
        }
    }
}
