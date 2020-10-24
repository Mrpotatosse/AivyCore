using AivyDofus.DofusMap.Enum;
using AivyDofus.IO;
using System;

namespace AivyDofus.DofusMap.Map.Elements
{
    [Serializable()]
    public abstract class BasicElement
    {
        #region Methode Publique
        public static BasicElement GetElementFromType(int type, BigEndianReader _reader, sbyte mapVersion)
        {
            switch ((ElementTypesEnum)type)
            {
                case ElementTypesEnum.GRAPHICAL:
                    return new GraphicalElement(_reader, mapVersion);
                case ElementTypesEnum.SOUND:
                    return new SoundElement(_reader);
                default:
                    throw new ArgumentException("[Common.Data.Maps.Map.Elements] Invalid Type in BasicElement.cs (" + type.ToString() + ")");
            }
        }
        #endregion
    }
}
