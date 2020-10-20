using AivyData.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace AivyData.API.Nodes
{
    public class Node
    {
        private readonly Dictionary<OrientationEnum, Node> _adjacents;

        public Node()
        {
            _adjacents = new Dictionary<OrientationEnum, Node>();
        }

        public Node this[OrientationEnum orientation]
        {
            get
            {
                if (_adjacents.ContainsKey(orientation)) return _adjacents[orientation];
                return null;                
            }
            set
            {
                if (_adjacents.ContainsKey(orientation))
                    _adjacents[orientation] = value;
                else
                    _adjacents.Add(orientation, value);
            }
        }
    }
}
