using System;
using System.Collections.Generic;
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
