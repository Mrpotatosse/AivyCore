using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AivyDofus.DofusMap.Visualizer
{
    [Serializable]
    [Flags]
    public enum CellState
    {
        None = 0,
        Walkable = 1,
        NonWalkable = 2,
        BluePlacement = 4,
        RedPlacement = 8,
        Trigger = 16,
        Road = 32,
        Testing = 64
    }
}
