using AivyDofus.DofusMap.Atouin;
using AivyDofus.DofusMap.Map.Elements.Color;
using AivyDofus.IO;
using System;
using System.Windows;

namespace AivyDofus.DofusMap.Map.Elements
{
    [Serializable()]
    public class GraphicalElement : BasicElement
    {

        #region Declarations

        public uint elementId { get; set; }
        public ColorMultiplicator hue { get; set; }
        public ColorMultiplicator shadow { get; set; }
        public ColorMultiplicator finalTeint { get; set; }
        public Point offset { get; set; }
        public Point pixelOffset { get; set; }
        public int altitude { get; set; }
        public uint identifier { get; set; }

        #endregion

        #region Constructeur
        public GraphicalElement(BigEndianReader raw, sbyte mapVersion)
        {
            elementId = raw.ReadUnsignedInt();
            hue = new ColorMultiplicator(raw.ReadByte(), raw.ReadByte(), raw.ReadByte());
            shadow = new ColorMultiplicator(raw.ReadByte(), raw.ReadByte(), raw.ReadByte());

            if (mapVersion <= 4)
            {
                byte offset_x = raw.ReadByte();
                byte offset_y = raw.ReadByte();
                offset = new Point(offset_x, offset_y);
                pixelOffset = new Point((int) (offset.X * AtouinConstants.CELL_HALF_WIDTH), (int) (offset.Y * AtouinConstants.CELL_HALF_HEIGHT));
            }

            else
            {
                short offset_x = raw.ReadShort();
                short offset_y = raw.ReadShort();
                pixelOffset = new Point(offset_x, offset_y);
                offset = new Point((int)(pixelOffset.X / AtouinConstants.CELL_HALF_WIDTH), (int)(pixelOffset.Y / AtouinConstants.CELL_HALF_HEIGHT));
            }

            altitude = raw.ReadByte();
            identifier = raw.ReadUnsignedInt();
        }

        #endregion

    }
}
