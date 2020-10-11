﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace RocketTanuki
{
    public struct TranspositionTableEntry
    {
        public long Hash { get; set; }
        public ushort Move { get; set; }
        public ushort Depth { get; set; }
        public ushort Generation { get; set; }
        public short Value { get; set; }
    }

    public class TranspositionTable
    {
        public static TranspositionTable Instance { get; } = new TranspositionTable();
        private const int EntrySize = 16;

        public void Resize(int hashSizeMb)
        {
            long numEntries = hashSizeMb / EntrySize * 1024 * 1024;
            numEntries = Math.Min(numEntries, 0x40000000);
            Entries = new TranspositionTableEntry[numEntries];
        }

        public void NewSearch()
        {
            generation = (generation + 1) & 0xffff;
        }

        public void Save(long hash, int value, int depth, Move move)
        {
            long numEntries = Entries.LongLength;
            long mask = numEntries - 1;
            long index = hash & mask;

            if (Entries[index].Generation == generation && Entries[index].Depth >= depth)
            {
                // 世代が同じで、深さが同じか低かった場合、
                // 価値が低いので記録しない。
                return;
            }

            Entries[index].Hash = hash;
            Entries[index].Move = move.ToUshort();
            Entries[index].Depth = (ushort)depth;
            Entries[index].Generation = (ushort)generation;
            Entries[index].Value = (short)value;
        }

        public TranspositionTableEntry Probe(long hash, out bool found)
        {
            long numEntries = Entries.LongLength;
            long mask = numEntries - 1;
            long index = hash & mask;
            found = Entries[index].Hash == hash;
            return Entries[index];
        }

        private TranspositionTableEntry[] Entries;
        private int generation;
    }
}