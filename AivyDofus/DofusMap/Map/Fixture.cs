using AivyDofus.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace AivyDofus.DofusMap.Map
{
    [Serializable()]
    public class Fixture
    {

        #region Declarations
        public int fixtureId { get; set; }
        public Point offset { get; set; }
        public int hue { get; set; }
        public int redMultiplier { get; set; }
        public int greenMultiplier { get; set; }
        public int blueMultiplier { get; set; }
        public sbyte alpha { get; set; }
        public int xScale { get; set; }
        public int yScale { get; set; }
        public int rotation { get; set; }

        #endregion

        #region Constructeur

        public Fixture(BigEndianReader raw)
        {
            fixtureId = raw.ReadInt();

            short offset_x = raw.ReadShort();
            short offset_y = raw.ReadShort();
            offset = new Point(offset_x, offset_y);

            rotation = raw.ReadShort();
            xScale = raw.ReadShort();
            yScale = raw.ReadShort();
            redMultiplier = raw.ReadByte();
            greenMultiplier = raw.ReadByte();
            blueMultiplier = raw.ReadByte();
            hue = redMultiplier | greenMultiplier | blueMultiplier;
            alpha = raw.ReadUnsignedByte();
        }

        #endregion

    }
}
