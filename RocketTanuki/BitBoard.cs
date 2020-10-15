using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using System.Runtime.Intrinsics.X86;
using System.Runtime.Intrinsics;
using System.Numerics;

namespace RocketTanuki
{
    public class BitBoard
    {
        public BitBoard()
        {
            this.value = Vector128.Create(0UL);
        }

        public BitBoard(Vector128<ulong> value)
        {
            this.value = value;
        }

        public static void Initialize()
        {
            for (int i = 0; i < 64; ++i)
            {
                OneBits[i] = new BitBoard(Vector128.Create(0UL, 1UL << i));
            }
            for (int i = 64; i < Position.BoardSize * Position.BoardSize; ++i)
            {
                OneBits[i] = new BitBoard(Vector128.Create(1UL << (i - 64), 0));
            }
        }

        public static BitBoard operator &(BitBoard lh, BitBoard rh)
        {
            return new BitBoard(Sse41.And(lh.value, rh.value));
        }

        public static BitBoard operator |(BitBoard lh, BitBoard rh)
        {
            return new BitBoard(Sse41.Or(lh.value, rh.value));
        }

        public static BitBoard operator ~(BitBoard lh)
        {
            return new BitBoard(Sse41.Xor(lh.value, Ones));
        }

        public int LowestBitPosition()
        {
            ulong lower = Sse41.X64.Extract(value, 1);
            if (lower != 0)
            {
                return (int)Bmi1.X64.TrailingZeroCount(lower);
            }
            else
            {
                ulong higher = Sse41.X64.Extract(value, 0);
                return (int)Bmi1.X64.TrailingZeroCount(higher) + 64;
            }
        }

        public bool IsZero { get { return Sse41.TestZ(value, value); } }

        private Vector128<ulong> value;

        private static Vector128<ulong> Zero { get; } = Vector128.Create(0UL);
        private static Vector128<ulong> Ones = Vector128.Create(0xffffffffffffffffUL);
        public static BitBoard[] OneBits { get; } = new BitBoard[Position.BoardSize * Position.BoardSize];
    }
}
