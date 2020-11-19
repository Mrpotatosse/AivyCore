using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AivyDofus.Pathfinding.Positions
{
    public class MovementPath
    {
        // Fields
        public MapPoint CellEnd;

        public List<PathElement> Cells = new List<PathElement>();

        public MapPoint CellStart;

        // Methods
        internal void Compress()
        {
            if (Cells.Count > 0)
            {
                var i = Cells.Count - 1;
                while (i > 0)
                {
                    if (Cells[i].Orientation == Cells[i - 1].Orientation)
                    {
                        Cells.RemoveAt(i);
                        i -= 1;
                    }
                    i -= 1;
                }
            }
        }
    }
}
