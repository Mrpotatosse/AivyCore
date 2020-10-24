using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AivyDofus.DofusMap.Visualizer
{
    [Serializable]
    [Flags]
    public enum DrawMode
    {
        None = 0,
        Movements = 1,
        Fights = 2,
        Triggers = 4,
        Others = 8,
        All = 0xF,
    }
}
