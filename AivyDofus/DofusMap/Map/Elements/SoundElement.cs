using AivyDofus.IO;
using System;

namespace AivyDofus.DofusMap.Map.Elements
{
    [Serializable()]
    public class SoundElement : BasicElement
    {

        #region Declarations

        public int soundId { get; set; }
        public int baseVolume { get; set; }
        public int fullVolumeDistance { get; set; }
        public int nullVolumeDistance { get; set; }
        public int minDelayBetweenLoops { get; set; }
        public int maxDelayBetweenLoops { get; set; }

        #endregion

        #region Constructeur

        public SoundElement(BigEndianReader raw)
        {
            soundId = raw.ReadInt();
            baseVolume = raw.ReadShort();
            fullVolumeDistance = raw.ReadInt();
            nullVolumeDistance = raw.ReadInt();
            minDelayBetweenLoops = raw.ReadShort();
            maxDelayBetweenLoops = raw.ReadShort();
        }

        #endregion

    }
}
