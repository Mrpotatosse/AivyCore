using AivyDofus.DofusMap.Map;
using AivyDofus.Pathfinding;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
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

        public void FromDofusMap(GameMapInformations informations)
        {
            Map.Map map = informations.CurrentMap;

            Dictionary<int, CellState> dictionary = new Dictionary<int, CellState>();
            for(int i = 0; i< map.CellsCount;i++)// CellData cell in map.Cells)
            {
                CellData cell = map.Cells[i];

                CellState current_state = CellState.NonWalkable;

                if (cell.mov) current_state = CellState.Walkable;

                dictionary.Add(i, current_state);
            }

            Draw(dictionary);
        }

        public void ClearState()
        {
            List<MapCell> redraw = new List<MapCell>();
            for(int i = 0;i<560;i++)
            {
                MapCell cell = Control.GetCell(i);
                if(cell.State != CellState.None)
                {
                    cell.State = CellState.None;
                    redraw.Add(cell);
                }
            }

            Control.Invalidate(redraw);
        }

        public void Draw(int cellId, CellState state)
        {
            Draw(Control.GetCell(cellId), state);
        }

        public void Draw(MapCell cell, CellState state)
        {
            using(Graphics graphics = CreateGraphics())
            {
                cell.State = state;
                cell.DrawBackground(Control, graphics, DrawMode.All);
                Control.Invalidate(cell);
            }
        }        

        public void Draw(Dictionary<int, CellState> dictionary)
        {
            List<MapCell> draw = new List<MapCell>();

            foreach(var element in dictionary)
            {
                MapCell cell = Control.GetCell(element.Key);
                cell.State = element.Value;
                draw.Add(cell);
            }

            Control.Invalidate(draw);
        }

        public void DrawPath(params long[] path)
        {
            DrawTest(CellState.Road, CellState.Walkable, path);
            /*List<MapCell> draw = new List<MapCell>();

            using(Graphics graphics = CreateGraphics())
            {
                for(long cellId = 0;cellId < 560; cellId++)
                {
                    MapCell cell = Control.GetCell((int)cellId);

                    if (path.Contains(cellId))
                    {
                        if (cell.State == CellState.Walkable)
                            cell.State = CellState.Road;
                    }
                    else
                    {
                        if(cell.State == CellState.Road)
                            cell.State = CellState.Walkable;
                    }
                    cell.DrawBackground(Control, graphics, DrawMode.All);
                    draw.Add(cell);
                }
            }

            Control.Invalidate(draw);*/
        }

        public void DrawTest(CellState drawState, CellState onState, params long[] path)
        {
            List<MapCell> draw = new List<MapCell>();

            using (Graphics graphics = CreateGraphics())
            {
                for (long cellId = 0; cellId < 560; cellId++)
                {
                    MapCell cell = Control.GetCell((int)cellId);

                    if (path.Contains(cellId))
                    {
                        if (cell.State == onState)
                            cell.State = drawState;
                    }
                    else
                    {
                        if (cell.State == drawState)
                            cell.State = onState;
                    }

                    cell.DrawBackground(Control, graphics, DrawMode.All);
                    draw.Add(cell);
                }
            }

            Control.Invalidate(draw);
        }
    }
}
