using System;

namespace RobotTanuki
{
    public class Program
    {
        public const string USI_Ponder = "USI_Ponder";
        public const string USI_Hash = "USI_Hash";

        void Run()
        {
            string line;
            while ((line = Console.ReadLine()) != null)
            {
                var split = line.Split();
                if (split.Length == 0) continue;

                var command = split[0];
                switch (command)
                {
                    case "usi":
                        Console.WriteLine("id name Robot Tanuki");
                        Console.WriteLine("id author onyx31");
                        Console.WriteLine($"option name {USI_Ponder} type check default true");
                        Console.WriteLine($"option name {USI_Hash} type spin default 256");
                        Console.WriteLine("usiok");
                        Console.Out.Flush();
                        break;

                    case "isready":
                        Console.WriteLine("readyok");
                        Console.Out.Flush();
                        break;

                    case "usinewgame":
                    case "setoption":
                    case "position":
                    case "stop":
                    case "ponderhit":
                    case "gameover":
                        break;

                    case "go":
                        Console.WriteLine("bestmove resign");
                        Console.Out.Flush();
                        break;

                    case "quit":
                        return;

                    default:
                        Console.WriteLine($"info string Unsupported command: {command}");
                        Console.Out.Flush();
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