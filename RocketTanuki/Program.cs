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
            position.Set("lnsgkgsnl/1r5b1/ppppppppp/9/9/9/PPPPPPPPP/1B5R1/LNSGKGSNL b - 1");
            Console.WriteLine(position);
        }

        static void Main(string[] args)
        {
            new Program().Run();
        }
    }
}
