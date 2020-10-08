using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RocketTanuki
{
    class Program
    {
        void Run()
        {
            Position.Initialize();
            Types.Initialize();

            var position = new Position();
            // 平手
            //position.Set("lnsgkgsnl/1r5b1/ppppppppp/9/9/9/PPPPPPPPP/1B5R1/LNSGKGSNL b - 1");
            //Console.WriteLine(position);

            // 指し手生成祭り
            //position.Set("l6nl/5+P1gk/2np1S3/p1p4Pp/3P2Sp1/1PPb2P1P/P5GS1/R8/LN4bKL w GR5pnsg 1");
            //var moves = MoveGenerator.Generate(position).ToList();
            //Console.WriteLine(moves.Count);

            // 最大合法手数局面
            //position.Set("8R/kSS1S1K2/4B4/9/9/9/9/9/3L1L1L1 b RBGSNLP3g3n17p 1");
            //var moves = MoveGenerator.Generate(position).ToList();
            //Console.WriteLine(moves.Count);

            // 平手
            //position.Set("lnsgkgsnl/1r5b1/ppppppppp/9/9/9/PPPPPPPPP/1B5R1/LNSGKGSNL b - 1");
            //foreach (var move in MoveGenerator.Generate(position))
            //{
            //    position.DoMove(move);
            //    Console.WriteLine(position);
            //    position.UndoMove(move);
            //    Console.WriteLine(position);
            //}
            //Console.WriteLine(position);

            //position.Set("l6nl/5+P1gk/2np1S3/p1p4Pp/3P2Sp1/1PPb2P1P/P5GS1/R8/LN4bKL w GR5pnsg 1");
            position.Set("8R/kSS1S1K2/4B4/9/9/9/9/9/3L1L1L1 b RBGSNLP3g3n17p 1");
            Console.WriteLine(position);
            foreach (var move in MoveGenerator.Generate(position))
            {
                Console.WriteLine(move.ToUsiString());
            }
        }

        static void Main(string[] args)
        {
            new Program().Run();
        }
    }
}
