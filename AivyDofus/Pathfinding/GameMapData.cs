using AivyData.API.Shared.Actors;
using AivyDofus.DofusMap;
using AivyDofus.DofusMap.Map;
using AivyDofus.Protocol.Elements;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace AivyDofus.Pathfinding
{
    public class GameMapData
    {
        public readonly Dictionary<int, List<ActorData>> Actors; 
        public GameMapData()
        {
            Actors = new Dictionary<int, List<ActorData>>();
        }

        public uint MapId { get; set; }

        public Map CurrentMap
        {
            get
            {
                return MapManager.SafeGetMap(MapId, Map.DefaultEncryptionKeyString);
            }
        }

        public void AddActor(int cellId, ActorData data)
        {
            Remove(data.Id);
            if (Actors.ContainsKey(cellId))
            {
                Actors[cellId].Add(data);
            }
            else
            {
                Actors.Add(cellId, new List<ActorData>() { data });
            }
        }

        public void Remove(int id)
        {
            int[] cells = Actors.Select(x => x.Key).ToArray();
            foreach (int cell in cells)
            {
                if(Actors[cell].FirstOrDefault(x => x.Id == id) is ActorData found)
                {
                    Actors[cell].Remove(found);
                    return;
                }
            }
        }

        public void FromMap(NetworkContentElement map_comp_message_content)
        {
            /*foreach (dynamic actor in map_comp_message_content["actors"])
            {
                int cellId = (int)actor["disposition"]["cellId"];
                int id = (int)actor["contextualId"];

                if (Actors.ContainsKey(cellId))
                {
                    if (!(Actors[cellId].FirstOrDefault(x => x.Id == id) is ActorData))
                        Actors[cellId].Add(new ActorData() { Id = id });
                }
                else
                    Actors.Add(cellId, new List<ActorData>() { new ActorData() { Id = id } });
            }*/

            /*MapId = (uint)map_comp_message_content["mapId"];
            IEnumerable<NetworkContentElement> actors = (map_comp_message_content["actors"] as dynamic[]).Select(x => x is NetworkContentElement c ? c : throw new ArgumentException(nameof(actors)));

            foreach(NetworkContentElement actor in actors)
            {
                int cellId = actor["disposition"]["cellId"];
                int id = (int)actor["contextualId"];

                AddId(cellId, id);
            }*/
        }

        public void FromShowActor(NetworkContentElement show_actor_message_content)
        {
            /*int cellId = show_actor_message_content["informations"]["disposition"]["cellId"];
            int id = (int)show_actor_message_content["informations"]["contextualId"];

            AddId(cellId, id);*/
        }

        public void FromRemoveId(NetworkContentElement remove_id_message_content)
        {
            /*int id = (int)remove_id_message_content["id"];

            RemoveId(id);*/
        }

        public void FromMovement(NetworkContentElement map_movement_message)
        {
            /*IEnumerable<short> keys = (map_movement_message["keyMovements"] as dynamic[]).Select(x => x is short v ? v : throw new ArgumentException(nameof(keys)));
            int actorId = (int)map_movement_message["actorId"];

            int last_cell = keys.LastOrDefault() & 4095;

            RemoveId(actorId);
            AddId(last_cell, actorId);*/
        }
    }
}
