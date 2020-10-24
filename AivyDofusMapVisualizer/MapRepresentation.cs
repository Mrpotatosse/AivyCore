using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace AivyDofusMapVisualizer
{
    public class MapRepresentation
    {
        public readonly CellRepresentation[] Cells;
        public readonly int MapWidth;
        public readonly int MapHeight;

        public MapRepresentation(int cellscount, int width, int height)
        {
            MapWidth = width;
            MapHeight = height;

            Cells = new CellRepresentation[cellscount];

            for(short i = 0; i < cellscount; i++)
            {
                Cells[i] = new CellRepresentation()
                {
                    Id = i,
                    Color = Colors.Red
                };
            }
        }
    }
}
