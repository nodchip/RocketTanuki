using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
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
            var bookFilePath = options[Program.BookFile];
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

        private bool ignoreBookPlay;
        private Dictionary<string, List<BookMove>> sfenToBookMoves;
    }
}
