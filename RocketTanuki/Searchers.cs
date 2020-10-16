using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using System.Runtime.Intrinsics.X86;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static RocketTanuki.Evaluator;

namespace RocketTanuki
{
    public class Searchers
    {
        private Searchers() { }

        public void Start(Position position)
        {
            searchTask = Task.Run(() => StartBody(position));
        }

        public void Stop()
        {
            thinking = false;
        }

        private void StartBody(Position position)
        {
            thinking = false;

            // 探索タスクが停止するのを待つ。
            WaitAllSearchTasks();

            thinking = true;

            BestMove bestMove = Book.Instance.Select(position);
            int selectiveDepth = 0;
            if (bestMove == null)
            {
                searchers.Clear();
                searchTasks.Clear();

                TranspositionTable.Instance.NewSearch();

                var searcher = new Searcher(0);
                searchers.Add(searcher);
                searchTasks.Add(Task.Run(() => { return searcher.Search(position); }));

                // 全ての探索タスクが終了するまで待つ
                WaitAllSearchTasks();
                selectiveDepth = searcher.SelectiveDepth;

                // 指し手を選択する
                bestMove = new BestMove
                {
                    Value = MatedIn(1),
                    Move = Move.Resign,
                };
                foreach (var task in searchTasks)
                {
                    var bestMoveCandidate = task.Result;
                    if (bestMove.Value < bestMoveCandidate.Value)
                    {
                        bestMove = bestMoveCandidate;
                    }
                }
            }

            while (thinking && (TimeManager.Instance.Ponder || TimeManager.Instance.Infinite))
            {
                Thread.Sleep(1);
            }

            // info pvを出力する
            Usi.OutputPv(bestMove, -InfiniteValue, InfiniteValue, selectiveDepth);

            // bestmoveを出力する
            var writer = new StringWriter();
            writer.Write("bestmove ");
            // stopコマンド受信時にbestMove.Moveがnullの場合があるため
            if (bestMove.Move != null)
            {
                writer.Write(bestMove.Move.ToUsiString());
            }
            else
            {
                writer.Write("resign");
            }
            if (bestMove.Next != null &&
                bestMove.Next.Move != null &&
                bestMove.Next.Move != Move.Resign &&
                bestMove.Next.Move != Move.Win &&
                bestMove.Next.Move != Move.None)
            {
                writer.Write(" ponder ");
                writer.Write(bestMove.Next.Move.ToUsiString());
            }
            Console.WriteLine(writer);
        }

        public long NumSearchedNodes()
        {
            long numSearchedNodes = 0;
            foreach (var searcher in searchers)
            {
                numSearchedNodes += Interlocked.Read(ref searcher.numSearchedNodes);
            }
            return numSearchedNodes;
        }

        public void WaitAllSearchTasks()
        {
            Task.WaitAll(searchTasks.ToArray());
        }
        public void WaitSearchTask()
        {
            if (searchTask != null)
            {
                searchTask.Wait();
            }
        }

        private Task searchTask;
        private List<Searcher> searchers = new List<Searcher>();
        private List<Task<BestMove>> searchTasks = new List<Task<BestMove>>();

        // [雑記] スレッド間の競合回避 - C# によるプログラミング入門 | ++C++; // 未確認飛行 C https://ufcpp.net/study/csharp/misc_synchronize.html
        public volatile bool thinking = false;

        public static Searchers Instance { get; } = new Searchers();
    }
}
