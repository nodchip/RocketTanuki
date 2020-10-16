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
    /// 指し手生成ルーチン
    /// </summary>
    public static class MoveGenerator
    {
        /// <summary>
        /// 指し手を生成する
        /// </summary>
        /// <param name="position"></param>
        /// <param name="position">置換表に登録されている指し手</param>
        /// <returns></returns>
        public static IEnumerable<Move> Generate(Position position, Move transpositionTableMove)
        {
            if (transpositionTableMove != null
                && transpositionTableMove != Move.Resign
                && transpositionTableMove != Move.Win
                && position.IsValid(transpositionTableMove))
            {
                yield return transpositionTableMove;
            }

            var sideToMove = position.SideToMove;
            var board = position.Board;
            var handPieces = position.HandPieces;

            // 駒を移動する指し手
            for (int squareIndex = 0; squareIndex < position.NumSquaresUnderPiece[(int)position.SideToMove]; ++squareIndex)
            {
                int square = position.SquaresUnderPiece[(int)position.SideToMove, squareIndex];
                int fileFrom = square / 9;
                int rankFrom = square % 9;
                var pieceFrom = board[fileFrom, rankFrom];
                if (pieceFrom == Piece.NoPiece)
                {
                    // 駒が置かれていないので何もしない
                    continue;
                }

                if (pieceFrom.ToColor() != sideToMove)
                {
                    // 相手の駒なので何もしない
                    continue;
                }

                foreach (var moveDirection in MoveDirections[(int)pieceFrom])
                {
                    int maxDistance = moveDirection.Long ? 8 : 1;
                    int fileTo = fileFrom;
                    int rankTo = rankFrom;
                    for (int distance = 0; distance < maxDistance; ++distance)
                    {
                        fileTo += moveDirection.Direction.DeltaFile;
                        rankTo += moveDirection.Direction.DeltaRank;

                        if (fileTo < 0 || Position.BoardSize <= fileTo || rankTo < 0 || Position.BoardSize <= rankTo)
                        {
                            // 盤外
                            continue;
                        }

                        var pieceTo = board[fileTo, rankTo];
                        if (pieceTo != Piece.NoPiece && pieceTo.ToColor() == sideToMove)
                        {
                            // 味方の駒がいる
                            break;
                        }

                        if (CanPutWithoutPromotion(pieceFrom, rankTo))
                        {
                            // 成らずに移動する
                            yield return new Move
                            {
                                FileFrom = fileFrom,
                                RankFrom = rankFrom,
                                PieceFrom = pieceFrom,
                                FileTo = fileTo,
                                RankTo = rankTo,
                                PieceTo = pieceTo,
                                Drop = false,
                                Promotion = false,
                                SideToMove = sideToMove,
                            };
                        }

                        if (pieceFrom.CanPromote() &&
                            ((sideToMove == Color.Black && rankTo <= 2)
                            || (sideToMove == Color.White && rankTo >= 6)
                            || (sideToMove == Color.Black && rankFrom <= 2)
                            || (sideToMove == Color.White && rankFrom >= 6)))
                        {
                            // 成って移動する
                            yield return new Move
                            {
                                FileFrom = fileFrom,
                                RankFrom = rankFrom,
                                PieceFrom = pieceFrom,
                                FileTo = fileTo,
                                RankTo = rankTo,
                                PieceTo = pieceTo,
                                Drop = false,
                                Promotion = true,
                                SideToMove = sideToMove,
                            };
                        }

                        if (pieceTo != Piece.NoPiece)
                        {
                            // 相手の駒なので、ここで利きが止まる
                            break;
                        }
                    }
                }
            }

            // 駒を打つ指し手
            var minPiece = sideToMove == Color.Black ? Piece.BlackPawn : Piece.WhitePawn;
            var maxPiece = sideToMove == Color.Black ? Piece.BlackRook : Piece.WhiteRook;
            for (Piece pieceFrom = minPiece; pieceFrom <= maxPiece; ++pieceFrom)
            {
                if (handPieces[(int)pieceFrom] == 0)
                {
                    // 対象の持ち駒を持っていない
                    continue;
                }

                for (int fileTo = 0; fileTo < Position.BoardSize; ++fileTo)
                {
                    for (int rankTo = 0; rankTo < Position.BoardSize; ++rankTo)
                    {
                        if (board[fileTo, rankTo] != Piece.NoPiece)
                        {
                            continue;
                        }

                        if (!CanPutWithoutPromotion(pieceFrom, rankTo))
                        {
                            continue;
                        }

                        if ((pieceFrom == Piece.BlackPawn || pieceFrom == Piece.WhitePawn)
                            && IsPawnExist(board, fileTo, pieceFrom))
                        {
                            // 2歩
                            continue;
                        }

                        // 駒打ち
                        yield return new Move
                        {
                            FileFrom = -1,
                            RankFrom = -1,
                            PieceFrom = pieceFrom,
                            FileTo = fileTo,
                            RankTo = rankTo,
                            PieceTo = Piece.NoPiece,
                            Drop = true,
                            Promotion = false,
                            SideToMove = sideToMove,
                        };
                    }
                }
            }
        }

        /// <summary>
        /// 歩と香が敵陣1段目、桂が敵陣1段目と2段目に成らずに移動できないことを判定する
        /// </summary>
        /// <param name="pieceFrom"></param>
        /// <param name="rankTo"></param>
        /// <returns></returns>
        private static bool CanPutWithoutPromotion(Piece pieceFrom, int rankTo)
        {
            return (pieceFrom == Piece.BlackPawn && rankTo >= 1)
                || (pieceFrom == Piece.BlackLance && rankTo >= 1)
                || (pieceFrom == Piece.BlackKnight && rankTo >= 2)
                || (pieceFrom == Piece.WhitePawn && rankTo <= 7)
                || (pieceFrom == Piece.WhiteLance && rankTo <= 7)
                || (pieceFrom == Piece.WhiteKnight && rankTo <= 6)
                || (pieceFrom != Piece.BlackPawn && pieceFrom != Piece.BlackLance && pieceFrom != Piece.BlackKnight && pieceFrom != Piece.WhitePawn && pieceFrom != Piece.WhiteLance && pieceFrom != Piece.WhiteKnight);
        }

        private static bool IsPawnExist(Piece[,] board, int file, Piece pawn)
        {
            for (int rank = 0; rank < Position.BoardSize; ++rank)
            {
                if (board[file, rank] == pawn)
                {
                    return true;
                }
            }
            return false;
        }
    }
}
