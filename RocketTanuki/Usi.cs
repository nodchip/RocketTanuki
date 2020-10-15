using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static RocketTanuki.Evaluator;

namespace RocketTanuki
{
    public class Usi
    {
        public static void OutputPv(BestMove bestMove, int alpha, int beta, int selectiveDepth)
        {
            var writer = new StringWriter();
            writer.Write("info");

            // depth
            writer.Write(" depth ");
            writer.Write(bestMove.Depth);

            // seldepth
            writer.Write(" seldepth ");
            writer.Write(selectiveDepth);

            // time
            writer.Write(" time ");
            writer.Write(TimeManager.Instance.ElapsedMs());

            // nodes
            writer.Write(" nodes ");
            writer.Write(Searchers.Instance.NumSearchedNodes());

            // score
            writer.Write(" score ");
            if (bestMove.Value < MatedInMaxPlayValue)
            {
                writer.Write("mate ");
                writer.Write(-MateValue - bestMove.Value);
            }
            else if (MateInMaxPlayValue < bestMove.Value)
            {
                writer.Write("mate +");
                writer.Write(MateValue - bestMove.Value);
            }
            else
            {
                writer.Write("cp ");
                writer.Write(bestMove.Value * 100 / PawnValue);
            }

            if (bestMove.Value <= alpha)
            {
                writer.Write(" upperbound");
            }
            else if (beta <= bestMove.Value)
            {
                writer.Write(" lowerbound");
            }

            // nps
            writer.Write(" nps ");
            writer.Write(Searchers.Instance.NumSearchedNodes() * 1000 / TimeManager.Instance.ElapsedMs());

            // pv
            writer.Write(" pv");
            BestMove current = bestMove;
            // stopコマンド受信時にcurrent.Moveがnullの場合があるため
            while (current != null && current.Move != null && current.Move != Move.Resign)
            {
                writer.Write(" ");
                writer.Write(current.Move.ToUsiString());
                current = current.Next;
            }

            Console.WriteLine(writer);
            Console.Out.Flush();
        }
    }
}
