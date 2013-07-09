﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using SimsLib;
using SimsLib.ThreeD;

namespace TSOClient.Code.Data.Model
{
    public class Collection : List<CollectionItem>
    {
        public Collection(byte[] data)
        {
            using (var stream = new MemoryStream(data))
            {
                Parse(new BinaryReader(stream));
            }
        }

        public Collection(BinaryReader reader)
        {
            Parse(reader);
        }

        private void Parse(BinaryReader reader)
        {
            var count = Endian.SwapInt32(reader.ReadInt32());

            for (int i = 0; i < count; i++)
            {
                var item = new CollectionItem
                {
                    Index = reader.ReadInt32(),
                    FileID = Endian.SwapUInt64(reader.ReadUInt64())
                };

                this.Add(item);
            }
        }
    }

    public class CollectionItem
    {
        public int Index;
        public ulong FileID;

        private PurchasableOutfit m_PurchasableOutfit;
        public PurchasableOutfit PurchasableOutfit
        {
            get
            {
                if (m_PurchasableOutfit == null)
                {
                    m_PurchasableOutfit = new PurchasableOutfit(ContentManager.GetResourceFromLongID(FileID));
                }
                return m_PurchasableOutfit;
            }
        }
    }
}