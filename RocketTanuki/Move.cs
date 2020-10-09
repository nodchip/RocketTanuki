using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static RocketTanuki.Types;

namespace RocketTanuki
{
    /// <summary>
    /// 指し手を表すデータ構造
    /// </summary>
    public class Move
    {
        public int FileFrom { get; set; }
        public int RankFrom { get; set; }
        public Piece PieceFrom { get; set; }
        public int FileTo { get; set; }
        public int RankTo { get; set; }
        public Piece PieceTo { get; set; }
        public bool Drop { get; set; }
        public bool Promotion { get; set; }
        public Color SideToMove { get; set; }

        public override string ToString()
        {
            return $"{(char)(SideToMove == Color.Black ? '☗' : '☖')}{(char)('１' + FileTo)}{RankToKanjiLetters[RankTo]}{PieceToString[(int)PieceFrom].Trim()[0]}{(Promotion ? "成" : "")}";
        }

        // override object.Equals
        public override bool Equals(object obj)
        {
            //       
            // See the full list of guidelines at
            //   http://go.microsoft.com/fwlink/?LinkID=85237  
            // and also the guidance for operator== at
            //   http://go.microsoft.com/fwlink/?LinkId=85238
            //

            if (obj == null || GetType() != obj.GetType())
            {
                return false;
            }

            var rh = (Move)obj;
            return FileFrom == rh.FileFrom
                && RankFrom == rh.RankFrom
                && PieceFrom == rh.PieceFrom
                && FileTo == rh.FileTo
                && RankTo == rh.RankTo
                && PieceTo == rh.PieceTo
                && Drop == rh.Drop
                && Promotion == rh.Promotion
                && SideToMove == rh.SideToMove;
        }

        // override object.GetHashCode
        public override int GetHashCode()
        {
            // boost/container_hash/hash.hpp - 1.74.0 https://www.boost.org/doc/libs/1_74_0/boost/container_hash/hash.hpp
            int seed = 0;
            seed ^= FileFrom + (seed << 6) + (seed >> 2);
            seed ^= RankFrom + (seed << 6) + (seed >> 2);
            seed ^= (int)PieceFrom + (seed << 6) + (seed >> 2);
            seed ^= FileTo + (seed << 6) + (seed >> 2);
            seed ^= RankTo + (seed << 6) + (seed >> 2);
            seed ^= (int)PieceTo + (seed << 6) + (seed >> 2);
            seed ^= Convert.ToInt32(Drop) + (seed << 6) + (seed >> 2);
            seed ^= Convert.ToInt32(Promotion) + (seed << 6) + (seed >> 2);
            seed ^= (int)SideToMove + (seed << 6) + (seed >> 2);
            return seed;
        }

        public string ToUsiString()
        {
            if (this == Resign)
            {
                return "resign";
            }
            else if (this == Win)
            {
                return "win";
            }

            string usiString = "";
            if (Drop)
            {
                usiString += char.ToUpper(PieceToChar[(int)PieceFrom]);
                usiString += "*";
            }
            else
            {
                usiString += (char)(FileFrom + '1');
                usiString += (char)(RankFrom + 'a');
            }

            usiString += (char)(FileTo + '1');
            usiString += (char)(RankTo + 'a');

            if (Promotion)
            {
                usiString += "+";
            }

            return usiString;
        }

        public static Move FromUsiString(Position position, string moveString)
        {
            var move = new Move();
            if (moveString[1] == '*')
            {
                // 駒打ちの指し手
                move.FileFrom = -1;
                move.RankFrom = -1;
                move.PieceFrom = CharToPiece[moveString[0]];
                if (position.SideToMove == Color.White)
                {
                    move.PieceFrom = move.PieceFrom.ToOpponentsHandPiece();
                }
                move.Drop = true;
            }
            else
            {
                // 駒を移動する指し手
                move.FileFrom = moveString[0] - '1';
                move.RankFrom = moveString[1] - 'a';
                move.PieceFrom = position.Board[move.FileFrom, move.RankFrom];
                move.Drop = false;
            }

            move.FileTo = moveString[2] - '1';
            move.RankTo = moveString[3] - 'a';
            move.PieceTo = position.Board[move.FileTo, move.RankTo];

            move.Promotion = moveString.Length == 5;
            move.SideToMove = position.SideToMove;
            return move;
        }

        public static Move Resign = new Move();
        public static Move Win = new Move();
        private static string[] RankToKanjiLetters = { "一", "二", "三", "四", "五", "六", "七", "八", "九" };
    }
}
