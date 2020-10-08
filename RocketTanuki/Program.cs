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
            var position = new Position();

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
                        Console.Out.Flush();
                        break;

                    case "isready":
                        Console.WriteLine("readyok");
                        Console.Out.Flush();
                        break;

                    case "usinewgame":
                        break;

                    case "setoption":
                        Debug.Assert(split.Length == 5);
                        Debug.Assert(split[1] == "name");
                        Debug.Assert(split[3] == "value");
                        var id = split[2];
                        var x = split[4];
                        options.Add(id, x);
                        break;

                    case "position":
                        Debug.Assert(split.Length >= 2);
                        Debug.Assert(split[1] == "sfen" || split[1] == "startpos");
                        int nextIndex;
                        if (split[1] == "sfen")
                        {
                            Debug.Assert(command.Length >= 6);
                            var sfen = string.Join(" ", split.Skip(2).Take(4));
                            position.Set(sfen);
                            nextIndex = 6;
                        }
                        else if (split[1] == "startpos")
                        {
                            position.Set(Position.StartposSfen);
                            nextIndex = 2;
                        }
                        else
                        {
                            throw new Exception($"不正なpositionコマンドを受信しました。 line={line}");
                        }

                        foreach (var moveString in split.Skip(nextIndex))
                        {
                            if (moveString == "moves")
                            {
                                continue;
                            }

                            var move = Move.FromUsiString(position, moveString);
                            position.DoMove(move);
                        }

                        break;

                    case "go":
                        Console.WriteLine("bestmove resign");
                        Console.Out.Flush();
                        break;

                    default:
                        Console.WriteLine($"info string Unsupported command: command={command}");
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
