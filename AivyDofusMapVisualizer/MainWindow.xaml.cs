using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace AivyDofusMapVisualizer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly MapRepresentation _map;

        public MainWindow()
        {
            InitializeComponent();

            _map = new MapRepresentation(560, 14, 20);

            InitMap();
        }

        private void Polygon_MouseUp(object sender, MouseButtonEventArgs e)
        {
            MessageBox.Show("Hello world");
        }

        public void InitMap()
        {
            CellRepresentation[] representations = _map.Cells;

            int width = 86;
            int height = 43;

            int start_canvas = 100;

            for(int i = 0; i< representations.Length;i++)
            {
                CellRepresentation cell = representations[i];
                Polygon poly = cell.AsPolygon(width, height);

                int current_width = i % _map.MapWidth;
                int current_height = i / _map.MapWidth;

                int c_top = start_canvas + (current_height * height) ;// + (current_height * height);
                int c_left = start_canvas + (current_width * width);// + (current_width * width);//start_canvas + (i * (width / 2) + (i % 2 * (width / 2)));

                Canvas.SetTop(poly, c_top);
                Canvas.SetLeft(poly, c_left);

                MapCanvas.Children.Add(poly);
            }
        }
    }
}
