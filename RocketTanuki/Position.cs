using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Configuration;
using System.Text;
using System.Threading.Tasks;
using static RocketTanuki.Types;

namespace RocketTanuki
{
    /// <summary>
    /// 局面を表すデータ構造
    /// </summary>
    public class Position
    {
        public const string StartposSfen = "lnsgkgsnl/1r5b1/ppppppppp/9/9/9/PPPPPPPPP/1B5R1/LNSGKGSNL b - 1";
        public const string MatsuriSfen = "l6nl/5+P1gk/2np1S3/p1p4Pp/3P2Sp1/1PPb2P1P/P5GS1/R8/LN4bKL w GR5pnsg 1";
        public const string MaxSfen = "8R/kSS1S1K2/4B4/9/9/9/9/9/3L1L1L1 b RBGSNLP3g3n17p 1";

        public const int BoardSize = 9;
        public Color SideToMove { get; set; }
        public Piece[,] Board { get; } = new Piece[BoardSize, BoardSize];
        public int[] HandPieces { get; } = new int[(int)Piece.NumPieces];
        public int Ply { get; set; }
        public long Hash { get; set; }
        public int BlackKingFile { get; set; }
        public int BlackKingRank { get; set; }
        public int WhiteKingFile { get; set; }
        public int WhiteKingRank { get; set; }

        public static void Initialize()
        {
        }

        /// <summary>
        /// 与えられた指し手に従い、局面を更新する。
        /// </summary>
        /// <param name="move"></param>
        public void DoMove(Move move)
        {
            Debug.Assert(SideToMove == move.SideToMove);
            Debug.Assert(move.Drop || Board[move.FileFrom, move.RankFrom] == move.PieceFrom);
            Debug.Assert(move.Drop || Board[move.FileTo, move.RankTo] == move.PieceTo);

            // 相手の駒を取る
            if (move.PieceTo != Piece.NoPiece)
            {
                Debug.Assert(move.PieceTo.ToColor() != SideToMove);
                Debug.Assert(move.PieceTo.ToOpponentsHandPiece().ToColor() == SideToMove);
                ++HandPieces[(int)move.PieceTo.ToOpponentsHandPiece()];
                Hash += Zobrist.Instance.HandPiece[(int)move.PieceTo.ToOpponentsHandPiece()];
            }

            Hash -= Zobrist.Instance.PieceSquare[(int)Board[move.FileTo, move.RankTo], move.FileTo, move.RankTo];
            Board[move.FileTo, move.RankTo] = move.Promotion
                ? Types.ToPromoted(move.PieceFrom)
                : move.PieceFrom;
            Hash += Zobrist.Instance.PieceSquare[(int)Board[move.FileTo, move.RankTo], move.FileTo, move.RankTo];
            Debug.Assert(Board[move.FileTo, move.RankTo].ToColor() == SideToMove);

            if (move.Drop)
            {
                // 駒を打つ指し手
                Debug.Assert(move.PieceFrom.ToColor() == SideToMove);
                Debug.Assert(HandPieces[(int)move.PieceFrom] > 0);
                --HandPieces[(int)move.PieceFrom];
                Hash -= Zobrist.Instance.HandPiece[(int)move.PieceFrom];
            }
            else
            {
                // 駒を移動する指し手
                Hash -= Zobrist.Instance.PieceSquare[(int)Board[move.FileFrom, move.RankFrom], move.FileFrom, move.RankFrom];
                Board[move.FileFrom, move.RankFrom] = Piece.NoPiece;
                Hash += Zobrist.Instance.PieceSquare[(int)Board[move.FileFrom, move.RankFrom], move.FileFrom, move.RankFrom];
            }

            SideToMove = SideToMove.ToOpponent();
            Hash ^= Zobrist.Instance.Side;

            if (move.PieceFrom == Piece.BlackKing)
            {
                BlackKingFile = move.FileTo;
                BlackKingRank = move.RankTo;
            }
            else if (move.PieceFrom == Piece.WhiteKing)
            {
                WhiteKingFile = move.FileTo;
                WhiteKingRank = move.RankTo;
            }
        }

        /// <summary>
        /// 与えられた指し手に従い、局面を1手戻す。
        /// </summary>
        /// <param name="move"></param>
        public void UndoMove(Move move)
        {
            Debug.Assert(SideToMove != move.SideToMove);

            Hash ^= Zobrist.Instance.Side;
            SideToMove = SideToMove.ToOpponent();

            if (move.PieceFrom == Piece.BlackKing)
            {
                BlackKingFile = move.FileFrom;
                BlackKingRank = move.RankFrom;
            }
            else if (move.PieceFrom == Piece.WhiteKing)
            {
                WhiteKingFile = move.FileFrom;
                WhiteKingRank = move.RankFrom;
            }

            if (move.Drop)
            {
                // 駒を打つ指し手
                Debug.Assert(move.PieceFrom.ToColor() == SideToMove);
                Hash += Zobrist.Instance.HandPiece[(int)move.PieceFrom];
                ++HandPieces[(int)move.PieceFrom];
            }
            else
            {
                // 駒を移動する指し手
                Debug.Assert(move.PieceFrom.ToColor() == SideToMove);
                Hash -= Zobrist.Instance.PieceSquare[(int)Board[move.FileFrom, move.RankFrom], move.FileFrom, move.RankFrom];
                Board[move.FileFrom, move.RankFrom] = move.PieceFrom;
                Hash += Zobrist.Instance.PieceSquare[(int)Board[move.FileFrom, move.RankFrom], move.FileFrom, move.RankFrom];
            }

            Hash -= Zobrist.Instance.PieceSquare[(int)Board[move.FileTo, move.RankTo], move.FileTo, move.RankTo];
            Board[move.FileTo, move.RankTo] = move.PieceTo;
            Hash += Zobrist.Instance.PieceSquare[(int)Board[move.FileTo, move.RankTo], move.FileTo, move.RankTo];

            // 相手の駒を取る
            if (move.PieceTo != Piece.NoPiece)
            {
                Debug.Assert(move.PieceTo.ToColor() != SideToMove);
                Debug.Assert(HandPieces[(int)move.PieceTo.ToOpponentsHandPiece()] > 0);
                Hash -= Zobrist.Instance.HandPiece[(int)move.PieceTo.ToOpponentsHandPiece()];
                --HandPieces[(int)move.PieceTo.ToOpponentsHandPiece()];
            }
        }

        /// <summary>
        /// sfen文字列をセットする
        /// </summary>
        /// <param name="sfen"></param>
        public void Set(string sfen)
        {
            // 盤面
            int file = BoardSize - 1;
            int rank = 0;
            int index = 0;
            bool promotion = false;
            while (true)
            {
                var ch = sfen[index++];
                if (ch == ' ')
                {
                    break;
                }
                else if (ch == '/')
                {
                    Debug.Assert(file == -1);
                    ++rank;
                    file = BoardSize - 1;
                }
                else if (ch == '+')
                {
                    promotion = true;
                }
                else if (Char.IsDigit(ch))
                {
                    int numNoPieces = ch - '0';
                    do
                    {
                        Board[file, rank] = Piece.NoPiece;
                        Hash += Zobrist.Instance.PieceSquare[(int)Piece.NoPiece, file, rank];
                        --file;
                    } while (--numNoPieces > 0);
                }
                else
                {
                    var piece = CharToPiece[ch];
                    Debug.Assert(piece != null);
                    if (promotion)
                    {
                        piece = piece.ToPromoted();
                        promotion = false;
                    }
                    Board[file, rank] = piece;
                    Hash += Zobrist.Instance.PieceSquare[(int)piece, file, rank];

                    if (piece == Piece.BlackKing)
                    {
                        BlackKingFile = file;
                        BlackKingRank = rank;
                    }
                    else if (piece == Piece.WhiteKing)
                    {
                        WhiteKingFile = file;
                        WhiteKingRank = rank;
                    }

                    --file;
                }
            }

            // 手番
            var sideToMove = sfen[index++];
            Debug.Assert(sideToMove == 'b' || sideToMove == 'w');
            if (sideToMove == 'b')
            {
                SideToMove = Color.Black;
            }
            else
            {
                SideToMove = Color.White;
                Hash ^= Zobrist.Instance.Side;
            }
            ++index;

            // 持ち駒
            for (int handPieceIndex = 0; handPieceIndex < (int)Piece.NumPieces; ++handPieceIndex)
            {
                HandPieces[handPieceIndex] = 0;
            }
            while (true)
            {
                var ch = sfen[index++];
                if (ch == ' ')
                {
                    break;
                }

                var piece = CharToPiece[ch];
                Debug.Assert(piece != null);
                ++HandPieces[(int)piece];
                Hash += Zobrist.Instance.HandPiece[(int)piece];
            }

            Ply = int.Parse(sfen.Substring(index));
        }

        public override String ToString()
        {
            var writer = new StringWriter();
            writer.WriteLine("+----+----+----+----+----+----+----+----+----+");
            for (int rank = 0; rank < BoardSize; ++rank)
            {
                writer.Write("|");
                for (int file = BoardSize - 1; file >= 0; --file)
                {
                    writer.Write(PieceToString[(int)Board[file, rank]]);
                    writer.Write("|");
                }
                writer.WriteLine();

                writer.WriteLine("+----+----+----+----+----+----+----+----+----+");
            }

            writer.Write("先手 手駒 : ");
            for (var piece = Piece.BlackPawn; piece < Piece.WhitePawn; ++piece)
            {
                for (int i = 0; i < HandPieces[(int)piece]; ++i)
                {
                    writer.Write(PieceToString[(int)piece][0]);
                }
            }

            writer.Write(" , 後手 手駒 : ");
            for (var piece = Piece.WhitePawn; piece < Piece.NumPieces; ++piece)
            {
                for (int i = 0; i < HandPieces[(int)piece]; ++i)
                {
                    writer.Write(PieceToString[(int)piece][0]);
                }
            }
            writer.WriteLine();

            writer.Write("手番 = ");
            writer.Write(SideToMove == Color.Black ? "先手" : "後手");
            writer.WriteLine();

            return writer.ToString();
        }

        public bool IsChecked(Color color)
        {
            var king = color == Color.Black ? Piece.BlackKing : Piece.WhiteKing;
            FindPiece(king, out int file, out int rank);

            // 駒を移動する指し手
            for (int fileFrom = 0; fileFrom < Position.BoardSize; ++fileFrom)
            {
                for (int rankFrom = 0; rankFrom < Position.BoardSize; ++rankFrom)
                {
                    var pieceFrom = Board[fileFrom, rankFrom];
                    if (pieceFrom == Piece.NoPiece)
                    {
                        // 駒が置かれていないので何もしない
                        continue;
                    }

                    if (pieceFrom.ToColor() == color)
                    {
                        // 味方の駒なので何もしない
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

                            var pieceTo = Board[fileTo, rankTo];
                            if (pieceTo == king)
                            {
                                // 味方の玉に利きを持っている
                                return true;
                            }

                            if (pieceTo != Piece.NoPiece)
                            {
                                // 味方の玉以外の駒がある
                                break;
                            }
                        }
                    }
                }
            }

            return false;
        }

        private void FindPiece(Piece piece, out int file, out int rank)
        {
            for (int f = 0; f < BoardSize; ++f)
            {
                for (int r = 0; r < BoardSize; ++r)
                {
                    if (Board[f, r] == piece)
                    {
                        file = f;
                        rank = r;
                        return;
                    }
                }
            }

            throw new Exception($"Piece not found. piece={piece}");
        }

        /// <summary>
        /// 与えられた指し手を、現局面で指すことができるかどうか返す。
        /// 置換表の指し手を確認することが主目的の為、細かいチェックはしていない。
        /// </summary>
        /// <param name="move"></param>
        /// <returns></returns>
        public bool IsValid(Move move)
        {
            if (move.Drop)
            {
                if (HandPieces[(int)move.PieceFrom] == 0)
                {
                    return false;
                }
            }
            else
            {
                if (Board[move.FileFrom, move.RankFrom] != move.PieceFrom)
                {
                    return false;
                }
            }

            if (Board[move.FileTo, move.RankTo] != move.PieceTo)
            {
                return false;
            }

            if (move.SideToMove != SideToMove)
            {
                return false;
            }

            return true;
        }
    }
}
