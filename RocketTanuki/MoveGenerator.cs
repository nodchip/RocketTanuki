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
        /// 指し手を生成する。通常の探索から使用することを想定している。
        /// </summary>
        /// <param name="position"></param>
        /// <param name="position">置換表に登録されている指し手</param>
        /// <returns></returns>
        public static IEnumerable<Move> Generate(Position position, Move transpositionTableMove)
        {
            if (transpositionTableMove != null
                && transpositionTableMove != Move.Resign
                && transpositionTableMove != Move.Win
                && transpositionTableMove != Move.None
                && position.IsValid(transpositionTableMove))
            {
                // 置換表に登録されている指し手を優先的に返す

                // onlyCaptureが指定されている場合は、駒を取る指し手のみ返す。
                yield return transpositionTableMove;
            }

            var sideToMove = position.SideToMove;
            var board = position.Board;
            var handPieces = position.HandPieces;
            var nonCapturePromotionMoves = new List<Move>();
            var nonCaptureNonPromotionMoves = new List<Move>();

            // 駒を移動する指し手
            for (int fileFrom = 0; fileFrom < Position.BoardSize; ++fileFrom)
            {
                for (int rankFrom = 0; rankFrom < Position.BoardSize; ++rankFrom)
                {
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

                            if (pieceFrom.CanPromote() &&
                                ((sideToMove == Color.Black && rankTo <= 2)
                                || (sideToMove == Color.White && rankTo >= 6)
                                || (sideToMove == Color.Black && rankFrom <= 2)
                                || (sideToMove == Color.White && rankFrom >= 6)))
                            {
                                // 成って移動する

                                var move = new Move
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

                                if (move.PieceTo != Piece.NoPiece)
                                {
                                    // 駒を取る指し手の場合はすぐに返す
                                    yield return move;
                                }
                                else
                                {
                                    // 駒を取らない指し手の場合は後で返す
                                    nonCapturePromotionMoves.Add(move);
                                }
                            }

                            if (CanPutWithoutPromotion(pieceFrom, rankTo))
                            {
                                // 成らずに移動する

                                var move = new Move
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

                                if (move.PieceTo != Piece.NoPiece)
                                {
                                    // 駒を取る指し手の場合はすぐに返す
                                    yield return move;
                                }
                                else
                                {
                                    // 駒を取らない指し手の場合は後で返す
                                    nonCaptureNonPromotionMoves.Add(move);
                                }
                            }

                            if (pieceTo != Piece.NoPiece)
                            {
                                // 相手の駒なので、ここで利きが止まる
                                break;
                            }
                        }
                    }
                }
            }

            // 駒を取らない、成る指し手
            foreach (var move in nonCapturePromotionMoves)
            {
                yield return move;
            }

            // 駒を取らない、成らない指し手
            foreach (var move in nonCaptureNonPromotionMoves)
            {
                yield return move;
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
        /// 指し手を生成する。特定のマスでの駒の取り合いの指し手のみ生成する。
        /// </summary>
        /// <param name="position"></param>
        /// <param name="transpositionTableMove"></param>
        /// <param name="onlyCapture"></param>
        /// <param name="fileCapture"></param>
        /// <param name="rankCapture"></param>
        /// <returns></returns>
        public static IEnumerable<Move> Generate(Position position, Move transpositionTableMove, int fileCapture, int rankCapture)
        {
            if (transpositionTableMove != null
                && transpositionTableMove != Move.Resign
                && transpositionTableMove != Move.Win
                && transpositionTableMove != Move.None
                && position.IsValid(transpositionTableMove))
            {
                // 置換表に登録されている指し手を優先的に返す

                if (transpositionTableMove.PieceTo != Piece.NoPiece
                    && transpositionTableMove.FileTo == fileCapture
                    && transpositionTableMove.RankTo == rankCapture)
                {
                    // onlyCaptureが指定されている場合は、駒を取る指し手のみ返す。
                    yield return transpositionTableMove;
                }
            }

            var sideToMove = position.SideToMove;
            var board = position.Board;
            var handPieces = position.HandPieces;
            var nonCaptureMoves = new List<Move>();

            // 駒を移動する指し手
            for (int fileFrom = 0; fileFrom < Position.BoardSize; ++fileFrom)
            {
                for (int rankFrom = 0; rankFrom < Position.BoardSize; ++rankFrom)
                {
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

                            if (pieceFrom.CanPromote() &&
                                ((sideToMove == Color.Black && rankTo <= 2)
                                || (sideToMove == Color.White && rankTo >= 6)
                                || (sideToMove == Color.Black && rankFrom <= 2)
                                || (sideToMove == Color.White && rankFrom >= 6)))
                            {
                                // 成って移動する
                                if (pieceTo != Piece.NoPiece && fileTo == fileCapture && rankTo == rankCapture)
                                {
                                    // 駒を取る指し手かつ指定されたマスに移動する指し手のみ返す
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
                            }

                            if (CanPutWithoutPromotion(pieceFrom, rankTo))
                            {
                                // 成らずに移動する
                                if (pieceTo != Piece.NoPiece && fileTo == fileCapture && rankTo == rankCapture)
                                {
                                    // 駒を取る指し手かつ指定されたマスに移動する指し手のみ返す
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
                            }

                            if (pieceTo != Piece.NoPiece)
                            {
                                // 相手の駒なので、ここで利きが止まる
                                break;
                            }
                        }
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
