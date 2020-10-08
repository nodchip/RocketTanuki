using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RocketTanuki
{
    public class Program
    {
        public static void Initialize()
        {
            Position.Initialize();
            Types.Initialize();
        }

        void Run()
        {
            Initialize();

            var options = new Dictionary<string, string>();

            string line;
            while ((line = Console.ReadLine()) != null)
            {
                var split = line.Split();
                if (split.Length == 0)
                {
                    continue;
                }

                var command = split[0];
                switch (command)
                {
                    case "usi":
                        Console.WriteLine("id name Rocket Tanuki");
                        Console.WriteLine("id author nodchip");
                        Console.WriteLine("usiok");
                        break;

                    case "isready":
                        Console.WriteLine("readyok");
                        break;

                    case "usinewgame":
                        break;

                    case "setoption":
                        Debug.Assert(split[1] == "name");
                        Debug.Assert(split[3] == "value");
                        var id = split[2];
                        var x = split[4];
                        options.Add(id, x);
                        break;

                    default:
                        Console.WriteLine($"info string Unsupported command: command={command}");
                        break;
                }
            }
        }

        static void Main(string[] args)
        {
            new Program().Run();
        }
    }
}
