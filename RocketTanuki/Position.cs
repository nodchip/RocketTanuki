using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Configuration;
using System.Text;
using System.Threading.Tasks;

namespace RocketTanuki
{
    /// <summary>
    /// 局面を表すデータ構造
    /// </summary>
    class Position
    {
        public const int BoardSize = 9;
        public Color SideToMove { get; set; }
        public Piece[,] Board { get; } = new Piece[BoardSize, BoardSize];
        public int[] HandPieces { get; } = new int[(int)Piece.NumPieces];

        /// <summary>
        /// 与えられた指し手に従い、局面を更新する。
        /// </summary>
        /// <param name="move"></param>
        public void DoMove(Move move)
        {
            Debug.Assert(move.Drop || Board[move.FileFrom, move.RankFrom] == move.PieceFrom);
            Debug.Assert(move.Drop || Board[move.FileTo, move.RankTo] == move.PieceTo);

            // 相手の駒を取る
            if (move.PieceTo != Piece.NoPiece)
            {
                ++HandPieces[(int)move.PieceTo.ToOpponentsHandPiece()];
            }

            Board[move.FileTo, move.RankTo] = move.Promotion
                ? Types.ToPromoted(move.PieceFrom)
                : move.PieceFrom;

            if (move.Drop)
            {
                // 駒を打つ指し手
                Debug.Assert(HandPieces[(int)move.PieceFrom] > 0);
                --HandPieces[(int)move.PieceFrom];
            }
            else
            {
                // 駒を移動する指し手
                Board[move.FileFrom, move.RankFrom] = Piece.NoPiece;
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
                ++HandPieces[(int)move.PieceFrom];
            }
            else
            {
                // 駒を移動する指し手
                Board[move.FileFrom, move.RankFrom] = move.PieceFrom;
            }

            Board[move.FileTo, move.RankTo] = move.PieceTo;

            // 相手の駒を取る
            if (move.PieceTo != Piece.NoPiece)
            {
                Debug.Assert(HandPieces[(int)move.PieceTo] > 0);
                --HandPieces[(int)move.PieceTo.ToOpponentsHandPiece()];
            }
        }
    }
}
