using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RocketTanuki
{
    public class Evaluator
    {
        public static int Evaluate(Position position)
        {
            int value = 0;
            for (int file = 0; file < Position.BoardSize; ++file)
            {
                for (int rank = 0; rank < Position.BoardSize; ++rank)
                {
                    value += MaterialValues[(int)position.Board[file, rank]];
                }
            }

            return position.SideToMove == Color.Black ? value : -value;
        }

        public static int MateIn(int play)
        {
            return MateValue - play;
        }

        public static int MatedIn(int play)
        {
            return -MateValue + play;
        }

        public const int ZeroValue = 0;
        public const int MateValue = 32000;
        public const int InfiniteValue = 32001;
        public const int InvalidValue = 32002;
        public const int MaxPlay = 128;
        public const int MateInMaxPlayValue = MateValue - MaxPlay;
        public const int MatedInMaxPlayValue = -MateValue + MaxPlay;
        public const int DrawValue = -1;

        public const int PawnValue = 90;
        public const int LanceValue = 315;
        public const int KnightValue = 405;
        public const int SilverValue = 495;
        public const int GoldValue = 540;
        public const int BishopValue = 855;
        public const int RookValue = 990;
        public const int ProPawnValue = 540;
        public const int ProLanceValue = 540;
        public const int ProKnightValue = 540;
        public const int ProSilverValue = 540;
        public const int HorseValue = 945;
        public const int DragonValue = 1395;
        public const int KingValue = 15000;

        private static int[] MaterialValues = {
            ZeroValue,
            PawnValue,
            LanceValue,
            KnightValue,
            SilverValue,
            GoldValue,
            BishopValue,
            RookValue,
            KingValue,
            ProPawnValue,
            ProLanceValue,
            ProKnightValue,
            ProSilverValue,
            HorseValue,
            DragonValue,
            -PawnValue,
            -LanceValue,
            -KnightValue,
            -SilverValue,
            -GoldValue,
            -BishopValue,
            -RookValue,
            -KingValue,
            -ProPawnValue,
            -ProLanceValue,
            -ProKnightValue,
            -ProSilverValue,
            -HorseValue,
            -DragonValue,
            InvalidValue,
        };
    }
}
