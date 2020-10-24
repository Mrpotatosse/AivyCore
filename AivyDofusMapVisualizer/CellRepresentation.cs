using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;

namespace AivyDofusMapVisualizer
{
    public class CellRepresentation
    {
        public short Id { get; set; }
        public Color Color { get; set; }

        public Polygon AsPolygon(int Width, int Height)
        {
            return new Polygon()
            {
                Points = new PointCollection(new Point[]
                {
                    new Point(0,0),
                    new Point(Width / 2, -Height / 2),
                    new Point(Width,0),
                    new Point(Width / 2, Height / 2),
                }),
                Fill = new SolidColorBrush(Color)
            };
        }
    }
}
