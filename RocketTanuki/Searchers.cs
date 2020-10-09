﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
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

            searchers.Clear();
            searchTasks.Clear();

            {
                var searcher = new Searcher(0);
                searchers.Add(searcher);
                searchTasks.Add(Task.Run(() => { return searcher.Search(position); }));
            }

            // 全ての探索タスクが終了するまで待つ
            WaitAllSearchTasks();

            // 指し手を選択する
            BestMove bestMove = new BestMove
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

            // info pvを出力する
            Usi.OutputPv(bestMove);

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
            if (bestMove.Next != null)
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
                numSearchedNodes += searcher.NumSearchedNodes;
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