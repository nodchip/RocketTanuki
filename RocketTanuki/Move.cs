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

        public string ToUsiString()
        {
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
            return move;
        }
    }
}
