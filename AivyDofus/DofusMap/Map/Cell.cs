using AivyDofus.DofusMap.Map.Elements;
using AivyDofus.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AivyDofus.DofusMap.Map
{
    [Serializable()]
    public class Cell
    {
        #region Declarations
        public int cellId { get; set; }
        public int elementsCount { get; set; }
        public List<BasicElement> elements { get; set; }

        #endregion

        #region Constructeur

        public Cell(BigEndianReader raw, sbyte mapVersion)
        {
            cellId = raw.ReadShort();
            elementsCount = raw.ReadShort();
            elements = new List<BasicElement>();

            for (int i = 0; i < elementsCount; i++)
            {
                byte type = raw.ReadByte();
                elements.Add(BasicElement.GetElementFromType(type, raw, mapVersion));
            }
        }

        #endregion
    }
}
