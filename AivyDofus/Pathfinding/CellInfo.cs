﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AivyDofus.Pathfinding
{
    public class CellInfo
    {
        // Methods
        internal CellInfo(double heuristic, int[] parent, bool opened, bool closed, int x, int y)
        {
            Heuristic = heuristic;
            Parent = parent;
            Opened = opened;
            Closed = closed;
            X = x;
            Y = y;
        }

        public double Heuristic { get; set; }
        public int[] Parent { get; set; }
        public bool Opened { get; set; }
        public bool Closed { get; set; }
        public int MovementCost { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
    }
}
