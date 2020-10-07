using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Configuration;
using System.Text;
using System.Threading.Tasks;

namespace RocketTanuki
{
    class Position
    {
        const int BoardSize = 9;
        public Color SideToMove { get; set; }
        public Piece[,] Board { get; } = new Piece[BoardSize, BoardSize];
        public int[] HandPieces { get; } = new int[(int)Piece.NumPieces];

        /// <summary>
        /// 与えられた指し手に従い、局面を更新する。
        /// </summary>
        /// <param name="move"></param>
        public void DoMove(Move move)
        {
            Debug.Assert(move.Drop || Board[move.FromFile, move.FromRank] == move.FromPiece);
            Debug.Assert(move.Drop || Board[move.ToFile, move.ToRank] == move.ToPiece);

            // 相手の駒を取る
            if (move.ToPiece != Piece.NoPiece)
            {
                ++HandPieces[(int)move.ToPiece];
            }

            Board[move.ToFile, move.ToRank] = move.Promotion
                ? Types.ToPromoted(move.FromPiece)
                : move.FromPiece;

            if (move.Drop)
            {
                // 駒を打つ指し手
                Debug.Assert(HandPieces[(int)move.FromPiece] > 0);
                --HandPieces[(int)move.FromPiece];
            }
            else
            {
                // 駒を移動する指し手
                Board[move.FromFile, move.FromRank] = Piece.NoPiece;
            }
        }

        /// <summary>
        /// 与えられた指し手に従い、局面を1手戻す。
        /// </summary>
        /// <param name="move"></param>
        public void UndoMove(Move move)
        {
            if (move.Drop)
            {
                // 駒を打つ指し手
                ++HandPieces[(int)move.FromPiece];
            }
            else
            {
                // 駒を移動する指し手
                Board[move.FromFile, move.FromRank] = move.FromPiece;
            }

            Board[move.ToFile, move.ToRank] = move.ToPiece;

            // 相手の駒を取る
            if (move.ToPiece != Piece.NoPiece)
            {
                Debug.Assert(HandPieces[(int)move.ToPiece] > 0);
                --HandPieces[(int)move.ToPiece];
            }
        }
    }
}
