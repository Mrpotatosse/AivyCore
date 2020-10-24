using AivyData.API.Shared.Actors;
using AivyDofus.DofusMap;
using AivyDofus.DofusMap.Map;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AivyDofus.Pathfinding
{
    public class GameMapInformations
    {
        public Dictionary<int, List<ActorData>> Actors { get; set; } = new Dictionary<int, List<ActorData>>();
        public uint MapId { get; set; }

        public Map CurrentMap
        {
            get
            {
                return MapManager.SafeGetMap(MapId, Map.DefaultEncryptionKeyString);
            }
        }
        
        public short[] GetCompressedPath(short start, short end, bool diagonal = true)
        {
            Pathfinder pathfinder = new Pathfinder();
            pathfinder.SetMap(this, diagonal);
            return pathfinder.GetCompressedPath(start, end);
        }

        public void Clear()
        {
            Actors.Clear();
            MapId = 0;
        }

        public void Add(int cellId, ActorData actor)
        {
            if (Actors.ContainsKey(cellId))
                Actors[cellId].Add(actor);
            else
                Actors.Add(cellId, new List<ActorData>() { actor });
        }

        public bool Remove(int cellId, ActorData actor)
        {
            if (Actors.ContainsKey(cellId))
                return Actors[cellId].Remove(actor);
            return false;
        }
    }
}
