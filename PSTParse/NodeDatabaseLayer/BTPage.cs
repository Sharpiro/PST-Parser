using PSTParse.Utilities;
using System;
using System.Collections.Generic;

namespace PSTParse.NodeDatabaseLayer
{
    /// <summary>
    /// B-Tree Page (BTPage)<br/>
    /// Implements a generic BTree using 512-byte pages.
    /// </summary>
    public class BTPage
    {
        /// <summary>
        /// cEnt<br/>
        /// The number of BTree entries stored in the page data.
        /// </summary>
        private readonly int _numEntries;
        /// <summary>
        /// cEntMax<br/>
        /// The maximum number of entries that can fit inside the page data.
        /// </summary>
        private readonly int _maxEntries;
        private readonly int _cbEnt;
        private readonly int _cLevel;
        //privatreadonly e bool _isNBT;
        private readonly PageTrailer _trailer;
        private readonly BREF _bRef;

        public List<BTPAGEENTRY> Entries { get; }
        public List<BTPage> InternalChildren { get; }
        public bool IsNode => _trailer.PageType == PageType.NBT;
        public bool IsBlock => _trailer.PageType == PageType.BBT;
        public ulong BID => _trailer.BID;

        public BTPage(byte[] pageData, BREF bRef, PSTFile pst)
        {
            _bRef = bRef;
            InternalChildren = new List<BTPage>();
            _numEntries = pageData[488];
            _maxEntries = pageData[489];
            _cbEnt = pageData[490];
            _cLevel = pageData[491];
            _trailer = new PageTrailer(pageData.RangeSubset(496, 16));

            Entries = new List<BTPAGEENTRY>();
            for (var i = 0; i < _numEntries; i++)
            {
                var curEntryBytes = pageData.RangeSubset(i * _cbEnt, _cbEnt);
                if (_cLevel == 0)
                {
                    if (_trailer.PageType == PageType.NBT)
                        Entries.Add(new NBTENTRY(curEntryBytes));
                    else
                    {
                        var curEntry = new BBTENTRY(curEntryBytes);
                        Entries.Add(curEntry);
                    }
                }
                else
                {
                    //btentries
                    var entry = new BTENTRY(curEntryBytes);
                    Entries.Add(entry);
                    using (var view = pst.PSTMMF.CreateViewAccessor((long)entry.BREF.IB, 512))
                    {
                        var bytes = new byte[512];
                        view.ReadArray(0, bytes, 0, 512);
                        InternalChildren.Add(new BTPage(bytes, entry.BREF, pst));
                    }
                }
            }
        }

        public BBTENTRY GetBIDBBTEntry(ulong BID)
        {
            int ii = 0;
            if (BID % 2 == 1)
                ii++;
            BID = BID & 0xfffffffffffffffe;
            for (int i = 0; i < Entries.Count; i++)
            {
                var entry = Entries[i];
                if (i == Entries.Count - 1)
                {

                    if (entry is BTENTRY)
                        return InternalChildren[i].GetBIDBBTEntry(BID);
                    else
                    {
                        var temp = entry as BBTENTRY;
                        if (BID == temp.Key)
                            return temp;
                    }

                }
                else
                {
                    var entry2 = Entries[i + 1];
                    if (entry is BTENTRY)
                    {
                        var cur = entry as BTENTRY;
                        var next = entry2 as BTENTRY;
                        if (BID >= cur.BtKey && BID < next.BtKey)
                            return InternalChildren[i].GetBIDBBTEntry(BID);
                    }
                    else if (entry is BBTENTRY)
                    {
                        var cur = entry as BBTENTRY;
                        if (BID == cur.Key)
                            return cur;
                    }
                }
            }
            return null;
        }

        public Tuple<ulong, ulong> GetNIDBID(ulong NID)
        {
            if (Entries.Count == 0)
            {
                throw new Exception($"Empty entries while getting NID 0x{NID:x2}");
            }

            var isBTEntry = Entries[0] is BTENTRY;
            for (int i = 0; i < Entries.Count; i++)
            {
                if (i == Entries.Count - 1)
                {
                    if (isBTEntry)
                        return InternalChildren[i].GetNIDBID(NID);
                    var cur = Entries[i] as NBTENTRY;
                    return new Tuple<ulong, ulong>(cur.BID_Data, cur.BID_SUB);
                }

                var curEntry = Entries[i];
                var nextEntry = Entries[i + 1];
                if (isBTEntry)
                {
                    var cur = curEntry as BTENTRY;
                    var next = nextEntry as BTENTRY;
                    if (NID >= cur.BtKey && NID < next.BtKey)
                        return InternalChildren[i].GetNIDBID(NID);
                }
                else
                {
                    var cur = curEntry as NBTENTRY;
                    if (NID == cur.NID)
                        return new Tuple<ulong, ulong>(cur.BID_Data, cur.BID_SUB);
                }
            }
            return new Tuple<ulong, ulong>(0, 0);
        }
    }
}
