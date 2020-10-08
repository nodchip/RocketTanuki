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
    static class MoveGenerator
    {
        class Direction
        {
            public int DeltaFile { get; set; }
            public int DeltaRank { get; set; }
        }

        private static Direction UpLeft = new Direction { DeltaFile = +1, DeltaRank = -1 };
        private static Direction Up = new Direction { DeltaFile = 0, DeltaRank = -1 };
        private static Direction UpRight = new Direction { DeltaFile = -1, DeltaRank = -1 };
        private static Direction Left = new Direction { DeltaFile = +1, DeltaRank = 0 };
        private static Direction Right = new Direction { DeltaFile = -1, DeltaRank = 0 };
        private static Direction DownLeft = new Direction { DeltaFile = +1, DeltaRank = +1 };
        private static Direction Down = new Direction { DeltaFile = 0, DeltaRank = +1 };
        private static Direction DownRight = new Direction { DeltaFile = -1, DeltaRank = +1 };

        class MoveDirection
        {
            public Direction Direction { get; set; }
            public bool Long { get; set; } = false;
        }

        private static List<MoveDirection>[] MoveDirections = {
            // NoPiece
            null,
            // BlackPawn
            new List<MoveDirection>{
                new MoveDirection{Direction=Up},
            },
            // BlackLance
            new List<MoveDirection>{
                new MoveDirection{Direction=Up,Long=true},
            },
            // BlackKnight
            new List<MoveDirection>{
                new MoveDirection{Direction=new Direction{DeltaRank=-2,DeltaFile=+1}},
                new MoveDirection{Direction=new Direction{DeltaRank=-2,DeltaFile=-1}},
            },
            // BlackSilver
            new List<MoveDirection>{
                new MoveDirection{Direction=UpLeft},
                new MoveDirection{Direction=Up},
                new MoveDirection{Direction=UpRight},
                new MoveDirection{Direction=DownLeft},
                new MoveDirection{Direction=DownRight},
            },
            // BlackGold
            new List<MoveDirection>{
                new MoveDirection{Direction=UpLeft},
                new MoveDirection{Direction=Up},
                new MoveDirection{Direction=UpRight},
                new MoveDirection{Direction=Left},
                new MoveDirection{Direction=Right},
                new MoveDirection{Direction=Down},
            },
            // BlackBishop
            new List<MoveDirection>{
                new MoveDirection{Direction=UpLeft,Long=true},
                new MoveDirection{Direction=UpRight,Long=true},
                new MoveDirection{Direction=DownLeft,Long=true},
                new MoveDirection{Direction=DownRight,Long=true},
            },
            // BlackRook
            new List<MoveDirection>{
                new MoveDirection{Direction=Up,Long=true},
                new MoveDirection{Direction=Left,Long=true},
                new MoveDirection{Direction=Right,Long=true},
                new MoveDirection{Direction=Down,Long=true},
            },
            // BlackKing
            new List<MoveDirection>{
                new MoveDirection{Direction=UpLeft},
                new MoveDirection{Direction=Up},
                new MoveDirection{Direction=UpRight},
                new MoveDirection{Direction=Left},
                new MoveDirection{Direction=Right},
                new MoveDirection{Direction=DownLeft},
                new MoveDirection{Direction=Down},
                new MoveDirection{Direction=DownRight},
            },
            // BlackPromotedPawn
            new List<MoveDirection>{
                new MoveDirection{Direction=UpLeft},
                new MoveDirection{Direction=Up},
                new MoveDirection{Direction=UpRight},
                new MoveDirection{Direction=Left},
                new MoveDirection{Direction=Right},
                new MoveDirection{Direction=Down},
            },
            // BlackPromotedLance
            new List<MoveDirection>{
                new MoveDirection{Direction=UpLeft},
                new MoveDirection{Direction=Up},
                new MoveDirection{Direction=UpRight},
                new MoveDirection{Direction=Left},
                new MoveDirection{Direction=Right},
                new MoveDirection{Direction=Down},
            },
            // BlackPromotedKnight
            new List<MoveDirection>{
                new MoveDirection{Direction=UpLeft},
                new MoveDirection{Direction=Up},
                new MoveDirection{Direction=UpRight},
                new MoveDirection{Direction=Left},
                new MoveDirection{Direction=Right},
                new MoveDirection{Direction=Down},
            },
            // BlackPromotedSilver
            new List<MoveDirection>{
                new MoveDirection{Direction=UpLeft},
                new MoveDirection{Direction=Up},
                new MoveDirection{Direction=UpRight},
                new MoveDirection{Direction=Left},
                new MoveDirection{Direction=Right},
                new MoveDirection{Direction=Down},
            },
            // BlackHorse
            new List<MoveDirection>{
                new MoveDirection{Direction=UpLeft,Long=true},
                new MoveDirection{Direction=Up},
                new MoveDirection{Direction=UpRight,Long=true},
                new MoveDirection{Direction=Left},
                new MoveDirection{Direction=Right},
                new MoveDirection{Direction=DownLeft,Long=true},
                new MoveDirection{Direction=Down},
                new MoveDirection{Direction=DownRight,Long=true},
            },
            // BlackDragon
            new List<MoveDirection>{
                new MoveDirection{Direction=UpLeft},
                new MoveDirection{Direction=Up,Long=true},
                new MoveDirection{Direction=UpRight},
                new MoveDirection{Direction=Left,Long=true},
                new MoveDirection{Direction=Right,Long=true},
                new MoveDirection{Direction=DownLeft},
                new MoveDirection{Direction=Down,Long=true},
                new MoveDirection{Direction=DownRight},
            },
            // WhitePawn
            new List<MoveDirection>{
                new MoveDirection{Direction=Down},
            },
            // WhiteLance
            new List<MoveDirection>{
                new MoveDirection{Direction=Down,Long=true},
            },
            // WhiteKnight
            new List<MoveDirection>{
                new MoveDirection{Direction=new Direction{DeltaRank=2,DeltaFile=+1}},
                new MoveDirection{Direction=new Direction{DeltaRank=2,DeltaFile=-1}},
            },
            // WhiteSilver
            new List<MoveDirection>{
                new MoveDirection{Direction=UpLeft},
                new MoveDirection{Direction=UpRight},
                new MoveDirection{Direction=DownLeft},
                new MoveDirection{Direction=Down},
                new MoveDirection{Direction=DownRight},
            },
            // WhiteGold
            new List<MoveDirection>{
                new MoveDirection{Direction=Up},
                new MoveDirection{Direction=Left},
                new MoveDirection{Direction=Right},
                new MoveDirection{Direction=DownLeft},
                new MoveDirection{Direction=Down},
                new MoveDirection{Direction=DownRight},
            },
            // WhiteBishop
            new List<MoveDirection>{
                new MoveDirection{Direction=UpLeft,Long=true},
                new MoveDirection{Direction=UpRight,Long=true},
                new MoveDirection{Direction=DownLeft,Long=true},
                new MoveDirection{Direction=DownRight,Long=true},
            },
            // WhiteRook
            new List<MoveDirection>{
                new MoveDirection{Direction=Up,Long=true},
                new MoveDirection{Direction=Left,Long=true},
                new MoveDirection{Direction=Right,Long=true},
                new MoveDirection{Direction=Down,Long=true},
            },
            // WhiteKing
            new List<MoveDirection>{
                new MoveDirection{Direction=UpLeft},
                new MoveDirection{Direction=Up},
                new MoveDirection{Direction=UpRight},
                new MoveDirection{Direction=Left},
                new MoveDirection{Direction=Right},
                new MoveDirection{Direction=DownLeft},
                new MoveDirection{Direction=Down},
                new MoveDirection{Direction=DownRight},
            },
            // WhitePromotedPawn
            new List<MoveDirection>{
                new MoveDirection{Direction=Up},
                new MoveDirection{Direction=Left},
                new MoveDirection{Direction=Right},
                new MoveDirection{Direction=DownLeft},
                new MoveDirection{Direction=Down},
                new MoveDirection{Direction=DownRight},
            },
            // WhitePromotedLance
            new List<MoveDirection>{
                new MoveDirection{Direction=Up},
                new MoveDirection{Direction=Left},
                new MoveDirection{Direction=Right},
                new MoveDirection{Direction=DownLeft},
                new MoveDirection{Direction=Down},
                new MoveDirection{Direction=DownRight},
            },
            // WhitePromotedKnight
            new List<MoveDirection>{
                new MoveDirection{Direction=Up},
                new MoveDirection{Direction=Left},
                new MoveDirection{Direction=Right},
                new MoveDirection{Direction=DownLeft},
                new MoveDirection{Direction=Down},
                new MoveDirection{Direction=DownRight},
            },
            // WhitePromotedSilver
            new List<MoveDirection>{
                new MoveDirection{Direction=Up},
                new MoveDirection{Direction=Left},
                new MoveDirection{Direction=Right},
                new MoveDirection{Direction=DownLeft},
                new MoveDirection{Direction=Down},
                new MoveDirection{Direction=DownRight},
            },
            // WhiteHorse
            new List<MoveDirection>{
                new MoveDirection{Direction=UpLeft,Long=true},
                new MoveDirection{Direction=Up},
                new MoveDirection{Direction=UpRight,Long=true},
                new MoveDirection{Direction=Left},
                new MoveDirection{Direction=Right},
                new MoveDirection{Direction=DownLeft,Long=true},
                new MoveDirection{Direction=Down},
                new MoveDirection{Direction=DownRight,Long=true},
            },
            // WhiteDragon
            new List<MoveDirection>{
                new MoveDirection{Direction=UpLeft},
                new MoveDirection{Direction=Up,Long=true},
                new MoveDirection{Direction=UpRight},
                new MoveDirection{Direction=Left,Long=true},
                new MoveDirection{Direction=Right,Long=true},
                new MoveDirection{Direction=DownLeft},
                new MoveDirection{Direction=Down,Long=true},
                new MoveDirection{Direction=DownRight},
            },
            // NumPieces
            null,
        };

        /// <summary>
        /// 指し手を生成する
        /// </summary>
        /// <param name="position"></param>
        /// <returns></returns>
        public static IEnumerable<Move> Generate(Position position)
        {
            var sideToMove = position.SideToMove;
            var board = position.Board;
            var handPieces = position.HandPieces;

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
                                };
                            }

                            if (pieceFrom.CanPromote() &&
                                ((sideToMove == Color.Black && rankTo <= 2) || (sideToMove == Color.White && rankTo >= 6)))
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
                            Drop = false,
                            Promotion = false,
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
