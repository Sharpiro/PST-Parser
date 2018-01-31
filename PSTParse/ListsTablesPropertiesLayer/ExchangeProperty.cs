﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PSTParse.MessageLayer;
using PSTParse.NodeDatabaseLayer;
using PSTParse.Utilities;

namespace PSTParse.ListsTablesPropertiesLayer
{
    public class ExchangeProperty
    {
        public static Dictionary<MessageProperty, ExchangeProperty>
            PropertyLookupByTypeID = new Dictionary
                <MessageProperty, ExchangeProperty>
                                         {
                                             {(MessageProperty)0x0002,new ExchangeProperty{ByteCount = 2,Type =  (MessageProperty)0x0002,MultiValue = false, Variable = false}},
                                             {(MessageProperty)0x0003,new ExchangeProperty{ByteCount = 4,Type =  (MessageProperty)0x0003,MultiValue = false, Variable = false}},
                                             {(MessageProperty)0x0004,new ExchangeProperty{ByteCount = 4,Type =  (MessageProperty)0x0004,MultiValue = false, Variable = false}},
                                             {(MessageProperty)0x0005,new ExchangeProperty{ByteCount = 8,Type =  (MessageProperty)0x0005,MultiValue = false, Variable = false}},
                                             {(MessageProperty)0x0006,new ExchangeProperty{ByteCount = 8,Type =  (MessageProperty)0x0006,MultiValue = false, Variable = false}},
                                             {(MessageProperty)0x0007,new ExchangeProperty{ByteCount = 8,Type =  (MessageProperty)0x0007,MultiValue = false, Variable = false}},
                                             {(MessageProperty)0x000A,new ExchangeProperty{ByteCount = 4,Type =  (MessageProperty)0x000A,MultiValue = false, Variable = false}},
                                             {(MessageProperty)0x000B,new ExchangeProperty{ByteCount = 1,Type =  (MessageProperty)0x000B,MultiValue = false, Variable = false}},
                                             {(MessageProperty)0x0014,new ExchangeProperty{ByteCount = 8,Type =  (MessageProperty)0x0014,MultiValue = false, Variable = false}},
                                             {(MessageProperty)0x001F,new ExchangeProperty{ByteCount = 0,Type =  (MessageProperty)0x001F,MultiValue = true, Variable = true}},
                                             {(MessageProperty)0x001E,new ExchangeProperty{ByteCount = 0,Type =  (MessageProperty)0x001E,MultiValue = true, Variable = true}},
                                             {(MessageProperty)0x0040,new ExchangeProperty{ByteCount = 8,Type =  (MessageProperty)0x0040,MultiValue = false, Variable = false}},
                                             {(MessageProperty)0x0048,new ExchangeProperty{ByteCount = 16,Type = (MessageProperty)0x0048,MultiValue = false, Variable = false}},
                                             {(MessageProperty)0x00FB,new ExchangeProperty{ByteCount = 0,Type =  (MessageProperty)0x00FB,MultiValue = false, Variable = true}},
                                             {(MessageProperty)0x00FD,new ExchangeProperty{ByteCount = 0,Type =  (MessageProperty)0x00FD,MultiValue = false, Variable = true}},
                                             {(MessageProperty)0x00FE,new ExchangeProperty{ByteCount = 0,Type =  (MessageProperty)0x00FE,MultiValue = true, Variable = true}},
                                             {(MessageProperty)0x0102,new ExchangeProperty{ByteCount = 1,Type =  (MessageProperty)0x0102,MultiValue = true, Variable = false}},
                                             {(MessageProperty)0x1002,new ExchangeProperty{ByteCount = 2,Type =  (MessageProperty)0x1002,MultiValue = true, Variable = false}},
                                             {(MessageProperty)0x1003,new ExchangeProperty{ByteCount = 4,Type =  (MessageProperty)0x1003,MultiValue = true, Variable = false}},
                                             {(MessageProperty)0x1004,new ExchangeProperty{ByteCount = 4,Type =  (MessageProperty)0x1004,MultiValue = true, Variable = false}},
                                             {(MessageProperty)0x1005,new ExchangeProperty{ByteCount = 8,Type =  (MessageProperty)0x1005,MultiValue = true, Variable = false}},
                                             {(MessageProperty)0x1006,new ExchangeProperty{ByteCount = 8,Type =  (MessageProperty)0x1006,MultiValue = true, Variable = false}},
                                             {(MessageProperty)0x1007,new ExchangeProperty{ByteCount = 8,Type =  (MessageProperty)0x1007,MultiValue = true, Variable = false}},
                                             {(MessageProperty)0x1014,new ExchangeProperty{ByteCount = 8,Type =  (MessageProperty)0x1014,MultiValue = true, Variable = false}},
                                             {(MessageProperty)0x101F,new ExchangeProperty{ByteCount = 0,Type =  (MessageProperty)0x101F,MultiValue = true, Variable = true}},
                                             {(MessageProperty)0x101E,new ExchangeProperty{ByteCount = 0,Type =  (MessageProperty)0x101E,MultiValue = true, Variable = true}},
                                             {(MessageProperty)0x1040,new ExchangeProperty{ByteCount = 8,Type =  (MessageProperty)0x1040,MultiValue = true, Variable = false}},
                                             {(MessageProperty)0x1048,new ExchangeProperty{ByteCount = 8,Type =  (MessageProperty)0x1048,MultiValue = true, Variable = false}},
                                             {(MessageProperty)0x1102,new ExchangeProperty{ByteCount = 0,Type =  (MessageProperty)0x1102,MultiValue = true, Variable = true}},
                                             //{0x1102,new ExchangeProperty{ByteCount = 0,Type = 0x1102,MultiValue = true, Variable = true}}
                                         };

        public MessageProperty ID { get; set; }
        public MessageProperty Type { get; set; }
        public bool MultiValue { get; set; }
        public bool Variable { get; set; }
        public uint ByteCount { get; set; }
        public byte[] Data { get; set; }
        //private BTHDataEntry entry;

        private readonly byte[] _key;

        public ExchangeProperty() { }

        public ExchangeProperty(UInt16 ID, UInt16 type, BTH heap, byte[] key)
        {
            this.ID = (MessageProperty)ID;
            this.Type = (MessageProperty)type;
            /*var tempKey = new byte[key.Length + 2];
            tempKey[0] = 0x00;
            tempKey[1] = 0x00;
            for (int i = 0; i < key.Length; i++)
                tempKey[i + 2] = key[i];*/
            this._key = key;

            GetData(heap, true);
        }

        public ExchangeProperty(BTHDataEntry entry, BTH heap)
        {
            //this.entry = entry;

            _key = entry.Data.RangeSubset(2, entry.Data.Length - 2);
            ID = (MessageProperty)BitConverter.ToUInt16(entry.Key, 0);
            Type = (MessageProperty)BitConverter.ToUInt16(entry.Data, 0);

            GetData(heap);
        }

        private void GetData(BTH heap, bool isTable = false)
        {
            if (PropertyLookupByTypeID.ContainsKey(Type))
            {
                var prop = PropertyLookupByTypeID[this.Type];
                MultiValue = prop.MultiValue;
                Variable = prop.Variable;
                ByteCount = prop.ByteCount;
            }
            //get data here

            if (!this.MultiValue && !this.Variable)
            {
                if (this.ByteCount <= 4 || (isTable && this.ByteCount <= 8))
                {
                    this.Data = this._key;
                }
                else
                {
                    this.Data = heap.GetHIDBytes(new HID(this._key)).Data;
                }
            }
            else
            {
                //oh no, it's an HNID
                var curID = BitConverter.ToUInt32(this._key, 0);

                if (curID == 0)
                {

                }
                else if ((curID & 0x1F) == 0) //must be HID
                {
                    this.Data = heap.GetHIDBytes(new HID(this._key)).Data;
                }
                else //let's assume NID
                {
                    var totalSize = 0;
                    var dataBlocks = new List<BlockDataDTO>();
                    if (heap.HeapNode.HeapSubNode.ContainsKey(curID))
                        dataBlocks = heap.HeapNode.HeapSubNode[curID].NodeData;
                    else
                    {
                        var tempSubNodeXREF = new Dictionary<ulong, NodeDataDTO>();
                        foreach (var heapSubNode in heap.HeapNode.HeapSubNode)
                            tempSubNodeXREF.Add(heapSubNode.Key & 0xFFFFFFFF, heapSubNode.Value);
                        dataBlocks = tempSubNodeXREF[curID].NodeData;
                        //dataBlocks = entry.ParentTree.HeapNode.HeapSubNode[curID].NodeData;
                    }
                    foreach (var dataBlock in dataBlocks)
                        totalSize += dataBlock.Data.Length;
                    var allData = new byte[totalSize];
                    var curPos = 0;
                    foreach (var datablock in dataBlocks)
                    {
                        for (int i = 0; i < datablock.Data.Length; i++)
                        {
                            allData[i + curPos] = datablock.Data[i];
                        }
                        curPos += datablock.Data.Length;
                    }
                    this.Data = allData;
                }
            }
        }
    }
}