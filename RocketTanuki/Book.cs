using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace RocketTanuki
{
    public class Book
    {
        public class BookMove
        {
            // 6g7g 4h5i+ -599 0 1
            public string BestMove { get; set; }
            public string NextMove { get; set; }
            public int Value { get; set; }
            public int Depth { get; set; }
            public int Num { get; set; }
        }

        private Book() { }

        public static Book Instance { get; } = new Book();

        public void Load(Dictionary<string, string> options)
        {
            if (options[Program.BookFile] == "no_book")
            {
                sfenToBookMoves = null;
                return;
            }

            var bookFilePath = Path.Combine("book", options[Program.BookFile]);
            ignoreBookPlay = options[Program.IgnoreBookPlay] == "true";

            Console.WriteLine("info string Loading a book...");
            Console.WriteLine($"info string BookFile={bookFilePath}");
            Console.WriteLine($"info string IgnoreBookPlay={ignoreBookPlay}");
            Console.Out.Flush();

            sfenToBookMoves = new Dictionary<string, List<BookMove>>();

            var reader = new StreamReader(bookFilePath);
            string line;
            string sfen = null;
            while ((line = reader.ReadLine()) != null)
            {
                if (line.StartsWith("#"))
                {
                    // バージョン識別文字列
                    // 読み飛ばす
                }
                else if (line.StartsWith("//"))
                {
                    // コメント行
                    // 読み飛ばす
                }
                else if (line.StartsWith("sfen"))
                {
                    // "sfen "を削除して格納する
                    sfen = line.Substring(5);

                    if (ignoreBookPlay)
                    {
                        // 末尾の数字と空白を削除する
                        // 手数を無視してマッチングするため
                        sfen = sfen.TrimEnd('0', '1', '2', '3', '4', '5', '6', '7', '8', '9', ' ');
                    }
                }
                else
                {
                    var split = line.Split();
                    Debug.Assert(split.Length == 5);
                    var bookMove = new BookMove
                    {
                        BestMove = split[0],
                        NextMove = split[1],
                        Value = int.Parse(split[2]),
                        Depth = int.Parse(split[3]),
                        Num = int.Parse(split[4]),
                    };

                    Debug.Assert(sfen != null);
                    if (!sfenToBookMoves.ContainsKey(sfen))
                    {
                        sfenToBookMoves[sfen] = new List<BookMove>();
                    }
                    sfenToBookMoves[sfen].Add(bookMove);
                }
            }

            Console.WriteLine("info string Loaded a book.");
            Console.Out.Flush();
        }

        /// <summary>
        /// 現在の局面に登録されている指し手を返す。
        /// </summary>
        /// <param name="position"></param>
        /// <returns></returns>
        public BestMove Select(Position position)
        {
            if (sfenToBookMoves == null)
            {
                return null;
            }

            var sfen = position.ToSfenString();
            if (ignoreBookPlay)
            {
                // 末尾の数字と空白を削除する
                // 手数を無視してマッチングするため
                sfen = sfen.TrimEnd('0', '1', '2', '3', '4', '5', '6', '7', '8', '9', ' ');
            }

            if (!sfenToBookMoves.ContainsKey(sfen))
            {
                return null;
            }

            var bookMoves = sfenToBookMoves[sfen].ToArray();
            int numBookMoves = bookMoves.Length;

            int maxValue = bookMoves.Max(x => x.Value);
            bookMoves = bookMoves.Where(x => x.Value >= maxValue - 30).ToArray();
            Console.WriteLine($"info string BookEvalDiff {numBookMoves} -> {bookMoves.Length}");
            numBookMoves = bookMoves.Length;

            bookMoves = bookMoves
                .Where(x => x.Value > (position.SideToMove == Color.Black ? BookEvalBlackLimit : BookEvalWhiteLimit))
                .ToArray();
            Console.WriteLine($"info string BookEvalXXXXXLimit {numBookMoves} -> {bookMoves.Length}");
            numBookMoves = bookMoves.Length;

            foreach (var move in bookMoves)
            {
                Console.WriteLine($"info string {move.BestMove} {move.NextMove}");
            }

            if (bookMoves.Length == 0)
            {
                return null;
            }

            var bookMove = bookMoves[random.Next(bookMoves.Length)];

            return new BestMove
            {
                Value = bookMove.Value,
                Move = Move.FromUsiString(position, bookMove.BestMove),
                Next = new BestMove
                {
                    Move = Move.FromUsiString(position, bookMove.NextMove),
                },
                Depth = bookMove.Depth,
            };
        }

        private bool ignoreBookPlay;
        private Dictionary<string, List<BookMove>> sfenToBookMoves;
        private Random random = new Random();
        private const int BookEvalDiff = 30;
        private const int BookEvalBlackLimit = 0;
        private const int BookEvalWhiteLimit = -140;
    }
}
