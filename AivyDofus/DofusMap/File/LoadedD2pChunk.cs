﻿using AivyDofus.IO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AivyDofus.DofusMap.File
{
    public class LoadedD2pChunk
    {
        #region Declarations
        public string Path { get; set; }
        public string IndexName { get; set; }
        public uint Offset { get; set; }
        public uint BytesCount { get; set; }
        public bool IsMap { get; set; }
        public bool IsImage { get; set; }
        public bool IsInvalid { get; set; }
        public BigEndianReader raw { get; set; }
        #endregion

        #region Constructeur

        public LoadedD2pChunk(BigEndianReader _raw, string _path)
        {
            raw = _raw;
            Path = _path;

            ReadMapInformation();
        }

        #endregion

        #region Methodes Publiques

        public uint GetId()
        {
            return uint.Parse(IndexName.Substring(IndexName.IndexOf('/') + 1).Replace(".dlm", ""));
        }

        public string GetStrId()
        {
            return IndexName.Substring(IndexName.IndexOf('/') + 1).Replace(".png", "");
        }
        byte[] data = null;
        public byte[] GetData()
        {
            return data;
        }

        private void ReadMapInformation()
        {
            IndexName = raw.ReadUTF();

            if (IndexName.Contains(".dlm"))
                IsMap = true;
            else if (IndexName.Contains("png"))
                IsImage = true;
            else
            {
                IsInvalid = true;
                return;
            }
            Offset = raw.ReadUnsignedInt() + 2;
            BytesCount = raw.ReadUnsignedInt();
            if (IsImage)
            {
                int pos = (int)raw.Position;
                raw.Seek((int)Offset, SeekOrigin.Begin);
                data = raw.ReadBytes((int)BytesCount);
                raw.Seek(pos, SeekOrigin.Begin);

            }
        }
        #endregion
    }
}