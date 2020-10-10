using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace RocketTanuki
{
    public class Evaluator
    {
        public static Evaluator Instance { get; } = new Evaluator();

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
        private short[] featureTransformerBiases = new short[256];
        private short[] featureTransformerWeights = new short[256 * 125388];
        private int[] firstBiases = new int[32];
        private sbyte[] firstWeights = new sbyte[32 * 512];
        private int[] secondBiases = new int[32];
        private sbyte[] secondWeights = new sbyte[32 * 32];
        private int[] thirdBiases = new int[1];
        private sbyte[] thirdWeights = new sbyte[1 * 32];
    }
}
