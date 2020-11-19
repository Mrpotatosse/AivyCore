using AivyDofus.DofusMap.Map;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AivyDofus.DofusMap.Visualizer
{
    public partial class DofusMapVisualizer : Form
    {
        public MapControl Control => mapControl1;

        public DofusMapVisualizer()
        {
            InitializeComponent();
        }

        public void Clear()
        {
            int[] cells = new int[560];
            for (int i = 0; i < cells.Length; i++)
                cells[i] = i;

            Draw(cells.ToDictionary(k => k, v => CellState.None));
        }

        public void FromMap(Map.Map map)
        {
            Dictionary<int, CellState> dictionary = new Dictionary<int, CellState>();
            for (int i = 0; i < map.CellsCount; i++)// CellData cell in map.Cells)
            {
                CellData cell = map.Cells[i];

                CellState current_state = CellState.NonWalkable;

                if (cell.mov) current_state = CellState.Walkable;

                dictionary.Add(i, current_state);
            }

            Draw(dictionary);
            map.Clear();
        }

        public void FromPath(params int[] path)
        {
            List<MapCell> draw = new List<MapCell>();
            for (int i = 0; i < 560; i++)
            {
                if (Control.GetCell(i) is MapCell cell)
                {
                    if (cell.State == CellState.Road && !path.Contains(cell.Id))
                    {
                        cell.State = CellState.Walkable;
                        draw.Add(cell);
                    }
                    else if (cell.State == CellState.Walkable && path.Contains(cell.Id))
                    {
                        cell.State = CellState.Road;
                        draw.Add(cell);
                    }
                }
            }

            Control.Invalidate(draw);
        }

        public void FromPath(params long[] path)
        {
            List<MapCell> draw = new List<MapCell>();
            for(int i = 0; i < 560; i++)
            {
                if(Control.GetCell(i) is MapCell cell)
                {
                    if (cell.State == CellState.Road && !path.Contains(cell.Id))
                    {
                        cell.State = CellState.Walkable;
                        draw.Add(cell);
                    }
                    else if(cell.State == CellState.Walkable && path.Contains(cell.Id))
                    {
                        cell.State = CellState.Road;
                        draw.Add(cell);
                    }
                }
            }

            Control.Invalidate(draw);
        }

        public void Draw(Dictionary<int, CellState> dictionary)
        {
            List<MapCell> draw = new List<MapCell>();

            foreach (var element in dictionary)
            {
                MapCell cell = Control.GetCell(element.Key);
                cell.State = element.Value;
                draw.Add(cell);
            }

            Control.Invalidate(draw);
        }
    }
}
