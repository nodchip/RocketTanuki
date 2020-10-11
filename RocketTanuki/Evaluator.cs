using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using static System.Math;

namespace RocketTanuki
{
    public class Evaluator
    {
        /// <summary>
        /// P特徴量のインデックス
        /// </summary>
        private enum PieceId
        {
            PieceIdZero = 0,
            FriendHandPawn = PieceIdZero + 1,
            EnemyHandPawn = 20,

            FriendHandLance = 39,
            EnemyHandLance = 44,
            FriendHandKnight = 49,
            EnemyHandKnight = 54,
            FriendHandSilver = 59,
            EnemyHandSilver = 64,
            FriendHandGold = 69,
            EnemyHandGold = 74,
            FriendHandBishop = 79,
            EnemyHandBishop = 82,
            FriendHandRook = 85,
            EnemyHandRook = 88,
            FriendEnemyHandEnd = 90,

            FriendPawn = FriendEnemyHandEnd,
            EnemyPawn = FriendPawn + 81,
            FriendLance = EnemyPawn + 81,
            EnemyLance = FriendLance + 81,
            FriendKnight = EnemyLance + 81,
            EnemyKnight = FriendKnight + 81,
            FriendSilver = EnemyKnight + 81,
            EnemySilver = FriendSilver + 81,
            FriendGold = EnemySilver + 81,
            EnemyGold = FriendGold + 81,
            FriendBishop = EnemyGold + 81,
            EnemyBishop = FriendBishop + 81,
            FriendHorse = EnemyBishop + 81,
            EnemyHorse = FriendHorse + 81,
            FriendRook = EnemyHorse + 81,
            EnemyRook = FriendRook + 81,
            FriendDragon = EnemyRook + 81,
            EnemyDragon = FriendDragon + 81,
            FriendEnemyEnd = EnemyDragon + 81,

            FriendKing = FriendEnemyEnd,
            EnemyKing = FriendKing + Position.BoardSize,
            FriendeEnemyEnd2 = EnemyKing + Position.BoardSize,
        };

        /// <summary>
        /// 盤上の駒のPieceIdのオフセット。
        /// [Piece][先手視点・後手視点]でアクセスする。
        /// </summary>
        private static PieceId[] BoardPieceIds = new PieceId[]{
            PieceId.PieceIdZero,
            PieceId.FriendPawn,
            PieceId.FriendLance,
            PieceId.FriendKnight,
            PieceId.FriendSilver,
            PieceId.FriendGold,
            PieceId.FriendBishop,
            PieceId.FriendRook,
            PieceId.PieceIdZero,
            PieceId.FriendGold,
            PieceId.FriendGold,
            PieceId.FriendGold,
            PieceId.FriendGold,
            PieceId.FriendHorse,
            PieceId.FriendDragon,
            PieceId.EnemyPawn,
            PieceId.EnemyLance,
            PieceId.EnemyKnight,
            PieceId.EnemySilver,
            PieceId.EnemyGold,
            PieceId.EnemyBishop,
            PieceId.EnemyRook,
            PieceId.PieceIdZero,
            PieceId.EnemyGold,
            PieceId.EnemyGold,
            PieceId.EnemyGold,
            PieceId.EnemyGold,
            PieceId.EnemyHorse,
            PieceId.EnemyDragon,
        };

        /// <summary>
        /// 持ち駒のPieceIdのオフセット。
        /// [Piece][先手視点・後手視点]でアクセスする。
        /// </summary>
        private static PieceId[] HandPieceIds = new PieceId[] {
            PieceId.PieceIdZero,
            PieceId.FriendHandPawn,
            PieceId.FriendHandLance,
            PieceId.FriendHandKnight,
            PieceId.FriendHandSilver,
            PieceId.FriendHandGold,
            PieceId.FriendHandBishop,
            PieceId.FriendHandRook,
            PieceId.PieceIdZero,
            PieceId.PieceIdZero,
            PieceId.PieceIdZero,
            PieceId.PieceIdZero,
            PieceId.PieceIdZero,
            PieceId.PieceIdZero,
            PieceId.PieceIdZero,
            PieceId.EnemyHandPawn,
            PieceId.EnemyHandLance,
            PieceId.EnemyHandKnight,
            PieceId.EnemyHandSilver,
            PieceId.EnemyHandGold,
            PieceId.EnemyHandBishop,
            PieceId.EnemyHandRook,
            PieceId.PieceIdZero,
            PieceId.PieceIdZero,
            PieceId.PieceIdZero,
            PieceId.PieceIdZero,
            PieceId.PieceIdZero,
            PieceId.PieceIdZero,
            PieceId.PieceIdZero,
        };

        public static Evaluator Instance { get; } = new Evaluator();

        private Evaluator()
        {
            Debug.Assert((int)PieceId.FriendEnemyEnd == 1548);
            Debug.Assert(BoardPieceIds.Length == (int)Piece.NumPieces);
            Debug.Assert(HandPieceIds.Length == (int)Piece.NumPieces);
        }

        public void Load(Dictionary<string, string> options)
        {
            Debug.Assert(options.ContainsKey(Program.EvalFile));
            var evalFilePath = options[Program.EvalFile];

            using (var reader = new BinaryReader(File.Open(evalFilePath, FileMode.Open)))
            {
                var version = reader.ReadUInt32();
                Debug.Assert(version == 2062757654, $"Unsupported version: version={version}");

                var hashValue = reader.ReadUInt32();
                Debug.Assert(hashValue == 1046128366, $"Unsupported hash value: hashValue={hashValue}");

                var size = reader.ReadUInt32();
                var architecture = reader.ReadBytes((int)size);

                // 入力層と隠れ層第1層の間のネットワークパラメーター
                // feature_transformer
                var featureTransformerHeader = reader.ReadUInt32();
                Debug.Assert(featureTransformerHeader == 1567217592, $"Unsupported feature transformer header: featureTransformerHeader={featureTransformerHeader}");
                for (int i = 0; i < featureTransformerBiases.Length; ++i)
                {
                    featureTransformerBiases[i] = reader.ReadInt16();
                }
                for (int i = 0; i < featureTransformerWeights.Length; ++i)
                {
                    featureTransformerWeights[i] = reader.ReadInt16();
                }

                // 隠れ層第1層と隠れ層第2層の間のネットワークパラメーター
                var networkHeader = reader.ReadUInt32();
                Debug.Assert(networkHeader == 1664315734, $"Unsupported network header: networkHeader={networkHeader}");
                for (int i = 0; i < firstBiases.Length; ++i)
                {
                    firstBiases[i] = reader.ReadInt32();
                }
                for (int i = 0; i < firstWeights.Length; ++i)
                {
                    firstWeights[i] = reader.ReadSByte();
                }

                // 隠れ層第2層と隠れ層第3層の間のネットワークパラメーター
                for (int i = 0; i < secondBiases.Length; ++i)
                {
                    secondBiases[i] = reader.ReadInt32();
                }
                for (int i = 0; i < secondWeights.Length; ++i)
                {
                    secondWeights[i] = reader.ReadSByte();
                }
                // 隠れ層第3層と出力層の間のネットワークパラメーター
                for (int i = 0; i < thirdBiases.Length; ++i)
                {
                    thirdBiases[i] = reader.ReadInt32();
                }
                for (int i = 0; i < thirdWeights.Length; ++i)
                {
                    thirdWeights[i] = reader.ReadSByte();
                }

                Debug.Assert(reader.BaseStream.Position == reader.BaseStream.Length);
            }
        }

        public int Evaluate(Position position)
        {
            // 入力層と隠れ層第1層の間のネットワークパラメーター
            var pieceIdsFromBlack = new int[38];
            var pieceIdsFromWhite = new int[38];
            int pieceIdsIndex = 0;

            // 盤上の駒
            for (int file = 0; file < Position.BoardSize; ++file)
            {
                for (int rank = 0; rank < Position.BoardSize; ++rank)
                {
                    if (position.Board[file, rank] == Piece.NoPiece
                        || position.Board[file, rank] == Piece.BlackKing
                        || position.Board[file, rank] == Piece.WhiteKing)
                    {
                        continue;
                    }

                    pieceIdsFromBlack[pieceIdsIndex] = MakeBoardPieceId(position.Board[file, rank], file, rank);
                    pieceIdsFromWhite[pieceIdsIndex] = MakeBoardPieceId(position.Board[file, rank].ToOpponentPiece(), 8 - file, 8 - rank);
                    ++pieceIdsIndex;
                }
            }

            // 持ち駒
            for (var handPiece = Piece.NoPiece; handPiece < Piece.NumPieces; ++handPiece)
            {
                for (int numHandPieces = 1; numHandPieces <= position.HandPieces[(int)handPiece]; ++numHandPieces)
                {
                    pieceIdsFromBlack[pieceIdsIndex] = MakeHandPieceId(handPiece, numHandPieces);
                    pieceIdsFromWhite[pieceIdsIndex] = MakeHandPieceId(handPiece.ToOpponentPiece(), numHandPieces);
                    ++pieceIdsIndex;
                }
            }
            Debug.Assert(pieceIdsIndex == pieceIdsFromBlack.Length);

            var z1Black = new int[HalfDimentions];
            var z1White = new int[HalfDimentions];
            Array.Copy(featureTransformerBiases, z1Black, HalfDimentions);
            Array.Copy(featureTransformerBiases, z1White, HalfDimentions);
            for (int i = 0; i < pieceIdsFromBlack.Length; ++i)
            {
                int kpIndexBlack = MakeKPIndex(position.BlackKingFile, position.BlackKingRank, pieceIdsFromBlack[i]);
                int kpIndexWhite = MakeKPIndex(8 - position.WhiteKingFile, 8 - position.WhiteKingRank, pieceIdsFromWhite[i]);

                for (int j = 0; j < HalfDimentions; ++j)
                {
                    z1Black[j] += featureTransformerWeights[HalfDimentions * kpIndexBlack + j];
                    z1White[j] += featureTransformerWeights[HalfDimentions * kpIndexWhite + j];
                }
            }

            // ClippedReLU
            var a1 = new int[HalfDimentions * 2];
            int[] first;
            int[] second;

            if (position.SideToMove == Color.Black)
            {
                first = z1Black;
                second = z1White;
            }
            else
            {
                first = z1White;
                second = z1Black;
            }
            for (int i = 0; i < HalfDimentions; ++i)
            {
                a1[i] = Max(0, Min(127, first[i]));
                a1[i + HalfDimentions] = Max(0, Min(127, second[i]));
            }

            // 隠れ層第1層から隠れ層第2層の間のネットワークパラメーター
            var z2 = new int[32];
            for (int outputIndex = 0; outputIndex < z2.Length; ++outputIndex)
            {
                var sum = firstBiases[outputIndex];
                for (int inputIndex = 0; inputIndex < a1.Length; ++inputIndex)
                {
                    sum += firstWeights[outputIndex * z2.Length + inputIndex] * a1[inputIndex];
                }
            }

            var a2 = new int[32];
            for (int outputIndex = 0; outputIndex < z2.Length; ++outputIndex)
            {
                a2[outputIndex] = Max(0, Min(127, z2[outputIndex] >> WeightScaleBits));
            }

            // 隠れ層第2層から隠れ層第3層の間のネットワークパラメーター
            var z3 = new int[32];
            for (int outputIndex = 0; outputIndex < z3.Length; ++outputIndex)
            {
                var sum = secondBiases[outputIndex];
                for (int inputIndex = 0; inputIndex < a2.Length; ++inputIndex)
                {
                    sum += secondWeights[outputIndex * z3.Length + inputIndex] * a2[inputIndex];
                }
            }

            var a3 = new int[32];
            for (int outputIndex = 0; outputIndex < z3.Length; ++outputIndex)
            {
                a3[outputIndex] = Max(0, Min(127, z3[outputIndex] >> WeightScaleBits));
            }

            // 隠れ層第3層から出力層の間のネットワークパラメーター
            var z4 = thirdBiases[0];
            for (int inputIndex = 0; inputIndex < a3.Length; ++inputIndex)
            {
                z4 += secondWeights[inputIndex] * a3[inputIndex];
            }

            return z4 / FVScale;
        }

        /// <summary>
        /// 盤上の駒のPieceIdを計算する。
        /// </summary>
        /// <param name="piece"></param>
        /// <param name="file"></param>
        /// <param name="rank"></param>
        /// <returns></returns>
        private static int MakeBoardPieceId(Piece piece, int file, int rank)
        {
            int square = file * Position.BoardSize + rank;
            PieceId offset = BoardPieceIds[(int)piece];
            Debug.Assert(offset != PieceId.PieceIdZero);
            return (int)offset + square;
        }

        /// <summary>
        /// 持ち駒のPieceIdを計算する
        /// </summary>
        /// <param name="piece"></param>
        /// <param name="numHandPieces"></param>
        /// <returns></returns>
        private static int MakeHandPieceId(Piece piece, int numHandPieces)
        {
            PieceId offset = HandPieceIds[(int)piece];
            Debug.Assert(offset != PieceId.PieceIdZero);
            return (int)offset + numHandPieces;
        }

        /// <summary>
        /// KP特徴量インデックスを計算する。
        /// </summary>
        /// <param name="kingFile"></param>
        /// <param name="kingRank"></param>
        /// <param name="pieceId"></param>
        /// <returns></returns>
        private static int MakeKPIndex(int kingFile, int kingRank, int pieceId)
        {
            return (kingFile * Position.BoardSize + kingRank) * (int)PieceId.FriendEnemyEnd + pieceId;
        }

        /// <summary>
        /// 詰みのスコアを返す。
        /// </summary>
        /// <param name="play">Rootノードからの手数</param>
        /// <returns></returns>
        public static int MateIn(int play)
        {
            return MateValue - play;
        }

        /// <summary>
        /// 待たされたときのスコアを返す。
        /// </summary>
        /// <param name="play">Rootノードからの手数</param>
        /// <returns></returns>
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
        private const int HalfDimentions = 256;
        private const int WeightScaleBits = 6;
        private const int FVScale = 32;

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
        private short[] featureTransformerBiases = new short[HalfDimentions];
        private short[] featureTransformerWeights = new short[HalfDimentions * 125388];
        private int[] firstBiases = new int[32];
        private sbyte[] firstWeights = new sbyte[32 * HalfDimentions * 2];
        private int[] secondBiases = new int[32];
        private sbyte[] secondWeights = new sbyte[32 * 32];
        private int[] thirdBiases = new int[1];
        private sbyte[] thirdWeights = new sbyte[1 * 32];
    }
}
