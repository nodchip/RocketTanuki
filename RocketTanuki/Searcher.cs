using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static RocketTanuki.Evaluator;
using static System.Math;

namespace RocketTanuki
{
    public class Searcher
    {
        public Searcher(int threadId)
        {
            this.threadId = threadId;
        }

        public BestMove Search(Position position)
        {
            BestMove bestMove = new BestMove
            {
                Move = Move.Resign,
                Depth = 1,
            };
            numSearchedNodes = 0;
            SelectiveDepth = 0;

            for (int depth = 1; depth < MaxPlay && Searchers.Instance.thinking; ++depth)
            {
                int alpha = -InfiniteValue;
                int beta = InfiniteValue;
                int delta = InfiniteValue;

                if (depth >= 4)
                {
                    delta = 17;
                    alpha = Math.Max(bestMove.Value - delta, -InfiniteValue);
                    beta = Math.Min(bestMove.Value + delta, InfiniteValue);
                }

                BestMove bestMoveCandidate;
                while (true)
                {
                    bestMoveCandidate = search(position, alpha, beta, depth, 0, -1, -1);
                    bestMoveCandidate.Depth = depth;
                    if ((bestMoveCandidate.Value <= alpha || beta <= bestMoveCandidate.Value)
                        && TimeManager.Instance.ElapsedMs() > 3000)
                    {
                        Usi.OutputPv(bestMoveCandidate, alpha, beta, SelectiveDepth);
                    }

                    if (!Searchers.Instance.thinking)
                    {
                        break;
                    }

                    if (bestMoveCandidate.Value <= alpha)
                    {
                        beta = (alpha + beta) / 2;
                        alpha = Math.Max(bestMoveCandidate.Value - delta, -InfiniteValue);
                    }
                    else if (beta <= bestMoveCandidate.Value)
                    {
                        beta = Math.Min(bestMoveCandidate.Value + delta, InfiniteValue);
                    }
                    else
                    {
                        break;
                    }

                    delta += delta / 4 + 5;

                    Debug.Assert(-InfiniteValue <= alpha);
                    Debug.Assert(beta <= InfiniteValue);
                }

                if (Searchers.Instance.thinking)
                {
                    bestMove = bestMoveCandidate;
                }

                Usi.OutputPv(bestMove, -InfiniteValue, InfiniteValue, SelectiveDepth);
            }
            return bestMove;
        }

        /// <summary>
        /// Principal Variation Search - Chessprogramming wiki https://www.chessprogramming.org/Principal_Variation_Search
        /// </summary>
        /// <param name="position"></param>
        /// <param name="alpha"></param>
        /// <param name="beta"></param>
        /// <param name="depth"></param>
        /// <param name="playFromRootNode">Root局面からの手数</param>
        /// <returns></returns>
        private BestMove search(Position position, int alpha, int beta, int depth, int playFromRootNode, int fileLastMove, int rankLastMove)
        {
            if (depth == 0)
            {
                return new BestMove
                {
                    Value = Evaluator.Instance.Evaluate(position),
                };
            }

            if (threadId == 0 && (callCount++) % 4096 == 0)
            {
                // TimeManager.IsThinking()は重いと思うので、
                // 定期的に結果を確認し、Searchers.thinkingに代入する。
                if (!TimeManager.Instance.IsThinking())
                {
                    Searchers.Instance.thinking = false;
                }
            }

            SelectiveDepth = Max(SelectiveDepth, playFromRootNode);

            var transpositionTableEntry = TranspositionTable.Instance.Probe(position.Hash, out bool found);
            if (found
                && depth <= transpositionTableEntry.Depth
                && (transpositionTableEntry.Value >= beta
                    ? (transpositionTableEntry.Bound & (int)Bound.Lower) != 0
                    : (transpositionTableEntry.Bound & (int)Bound.Upper) != 0))
            {
                return new BestMove
                {
                    Value = FromTranspositionTableValue(transpositionTableEntry.Value, playFromRootNode),
                    Move = Move.FromUshort(position, transpositionTableEntry.Move),
                };
            }

            BestMove bestChildBestMove = null;
            Move bestMove = Move.Resign;
            bool searchPv = true;
            Move transpositionTableMove = found ? Move.FromUshort(position, transpositionTableEntry.Move) : null;
            foreach (var move in MoveGenerator.Generate(position, transpositionTableMove))
            {
                if (!Searchers.Instance.thinking)
                {
                    break;
                }

                BestMove childBestMove;
                Interlocked.Increment(ref numSearchedNodes);
                using (var mover = new Mover(position, move))
                {
                    if (!mover.IsValid())
                    {
                        // 王手を放置しているので、処理しない。
                        continue;
                    }

                    if (searchPv)
                    {
                        childBestMove = search(position, -beta, -alpha, depth - 1, playFromRootNode + 1, move.FileTo, move.RankTo);
                    }
                    else
                    {
                        childBestMove = search(position, -alpha - 1, -alpha, depth - 1, playFromRootNode + 1, move.FileTo, move.RankTo);
                        if (-childBestMove.Value > alpha)
                        {
                            childBestMove = search(position, -beta, -alpha, depth - 1, playFromRootNode + 1, move.FileTo, move.RankTo);
                        }
                    }
                }

                if (!Searchers.Instance.thinking)
                {
                    return new BestMove
                    {
                        Value = DrawValue,
                        Move = bestMove,
                        Next = bestChildBestMove,
                    };
                }

                if (-childBestMove.Value >= beta)
                {
                    TranspositionTable.Instance.Save(position.Hash, ToTranspositionTableValue(beta, playFromRootNode), depth, bestMove, Bound.Lower);

                    return new BestMove
                    {
                        Value = beta, //  fail hard beta-cutoff
                        Move = move,
                        Next = childBestMove,
                    };
                }

                if (-childBestMove.Value > alpha)
                {
                    alpha = -childBestMove.Value; // alpha acts like max in MiniMax
                    bestMove = move;
                    bestChildBestMove = childBestMove;
                    searchPv = false;
                }
            }

            int value = bestMove == Move.Resign
                // 合法手が存在しなかった=負け
                ? MatedIn(playFromRootNode)
                : alpha;

            // Null Window Search等で指し手が見つからなかった場合に
            // 投了の指し手を置換表に登録していまうのを防ぐハック
            if (bestMove != Move.Resign)
            {
                TranspositionTable.Instance.Save(position.Hash, ToTranspositionTableValue(value, playFromRootNode), depth, bestMove, Bound.Exact);
            }

            return new BestMove
            {
                Move = bestMove,
                Next = bestChildBestMove,
                Value = value,
            };
        }

        /// <summary>
        /// Quiescence Search - Chessprogramming wiki https://www.chessprogramming.org/Quiescence_Search
        /// </summary>
        /// <param name="position"></param>
        /// <param name="alpha"></param>
        /// <param name="beta"></param>
        /// <param name="depth"></param>
        /// <param name="playFromRootNode"></param>
        /// <returns></returns>
        private BestMove QuiescenceSearch(Position position, int alpha, int beta, int depth, int playFromRootNode, int fileLastMove, int rankLastMove)
        {
            if (threadId == 0 && (callCount++) % 4096 == 0)
            {
                // TimeManager.IsThinking()は重いと思うので、
                // 定期的に結果を確認し、Searchers.thinkingに代入する。
                if (!TimeManager.Instance.IsThinking())
                {
                    Searchers.Instance.thinking = false;
                }
            }

            SelectiveDepth = Max(SelectiveDepth, playFromRootNode);

            var transpositionTableEntry = TranspositionTable.Instance.Probe(position.Hash, out bool found);
            if (found && 0 <= transpositionTableEntry.Depth)
            {
                return new BestMove
                {
                    Value = FromTranspositionTableValue(transpositionTableEntry.Value, playFromRootNode),
                    Move = Move.FromUshort(position, transpositionTableEntry.Move),
                };
            }

            int stand_pat = Evaluator.Instance.Evaluate(position);
            if (stand_pat >= beta)
            {
                return new BestMove
                {
                    Value = beta,
                };
            }
            if (alpha < stand_pat)
                alpha = stand_pat;

            BestMove bestChildBestMove = null;
            Move bestMove = Move.None;
            bool searchPv = true;
            foreach (var move in MoveGenerator.Generate(position, null, fileLastMove, rankLastMove))
            {
                BestMove childBestMove;
                Interlocked.Increment(ref numSearchedNodes);
                using (var mover = new Mover(position, move))
                {
                    if (!mover.IsValid())
                    {
                        // 王手を放置しているので、処理しない。
                        continue;
                    }

                    if (searchPv)
                    {
                        childBestMove = QuiescenceSearch(position, -beta, -alpha, depth - 1, playFromRootNode + 1, fileLastMove, rankLastMove);
                    }
                    else
                    {
                        childBestMove = QuiescenceSearch(position, -alpha - 1, -alpha, depth - 1, playFromRootNode + 1, fileLastMove, rankLastMove);
                        if (-childBestMove.Value > alpha)
                        {
                            childBestMove = QuiescenceSearch(position, -beta, -alpha, depth - 1, playFromRootNode + 1, fileLastMove, rankLastMove);
                        }
                    }
                }

                if (-childBestMove.Value >= beta)
                {
                    TranspositionTable.Instance.Save(position.Hash, ToTranspositionTableValue(beta, playFromRootNode), depth, bestMove, Bound.Lower);

                    return new BestMove
                    {
                        Value = beta, //  fail hard beta-cutoff
                        Move = move,
                        Next = childBestMove,
                    };
                }

                if (-childBestMove.Value > alpha)
                {
                    alpha = -childBestMove.Value; // alpha acts like max in MiniMax
                    bestMove = move;
                    bestChildBestMove = childBestMove;
                    searchPv = true;
                }
            }

            TranspositionTable.Instance.Save(position.Hash, ToTranspositionTableValue(alpha, playFromRootNode), depth, bestMove, Bound.Exact);

            return new BestMove
            {
                Move = bestMove,
                Next = bestChildBestMove,
                Value = alpha,
            };
        }

        /// <summary>
        /// 探索における評価値(詰みの局面はMax-Root局面からの手数)を
        /// 置換表における評価値(詰みの局面はMax-現局面からの手数)に変換する。
        /// </summary>
        /// <param name="searchValue">探索の評価値</param>
        /// <param name="playFromRootNode">Root局面からの手数</param>
        /// <returns></returns>
        private int ToTranspositionTableValue(int searchValue, int playFromRootNode)
        {
            if (searchValue < MatedInMaxPlayValue)
            {
                return searchValue + playFromRootNode;
            }
            else if (searchValue > MateInMaxPlayValue)
            {
                return searchValue - playFromRootNode;
            }
            else
            {
                return searchValue;
            }
        }

        /// <summary>
        /// 置換表における評価値(詰みの局面はMax-現局面からの手数)を
        /// 探索における評価値(詰みの局面はMax-Root局面からの手数)に変換する。
        /// </summary>
        /// <param name="searchValue">探索の評価値</param>
        /// <param name="playFromRootNode">Root局面からの手数</param>
        /// <returns></returns>
        private int FromTranspositionTableValue(int searchValue, int playFromRootNode)
        {
            if (searchValue < MatedInMaxPlayValue)
            {
                return searchValue - playFromRootNode;
            }
            else if (searchValue > MateInMaxPlayValue)
            {
                return searchValue + playFromRootNode;
            }
            else
            {
                return searchValue;
            }
        }

        private int threadId;
        private int callCount = 0;
        public long numSearchedNodes = 0;
        public int SelectiveDepth { get; set; } = 0;
    }
}
