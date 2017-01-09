using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;


namespace PushFight
{
    class ConsoleClient
    {
        public PushFightGame game;
        public Team team;

        public ConsoleClient(PushFightGame game)
        {
            this.game = game;
            this.team = game.CurrentTeam;
        }

        public void Command(String cmd)
        {
            var result = game.Input(cmd, team);
            Console.WriteLine(result);
            Console.WriteLine();
            team = game.CurrentTeam;
        }

        public void PrintPrompt()
        {
            Console.ForegroundColor = team == Team.Black ? ConsoleColor.DarkGray : ConsoleColor.White;
            Console.Write(team + " " + game.Phase + "> ");
        }

        public void Print()
        {
            if (game.Phase == GamePhase.Placement)
            {
                Console.Write("x:" + game.RemainingPieces.Where(r => r.Team == game.CurrentTeam && r.PawnType == PawnType.Square).First().Count);
                Console.Write(" o:" + game.RemainingPieces.Where(r => r.Team == game.CurrentTeam && r.PawnType == PawnType.Round).First().Count);
                Console.WriteLine();
            }
            else if (game.Phase == GamePhase.Push)
            {
                Console.WriteLine("Moves: " + game.RemainingMoves);
            }
            else
            {
                Console.WriteLine("Game Over");
                Console.WriteLine(game.Winner + " Wins!");
            }
            Console.WriteLine("  abcdefgh");
            for (int y = 1; y < game.Board.GetLength(1) - 1; y++)
            {
                Console.Write(y);
                for (int x = 0; x < game.Board.GetLength(0); x++)
                {
                    var cell = game.Board[x, y];
                    Console.ForegroundColor = cell.Contents.Team == Team.None ? ConsoleColor.DarkGray : cell.Contents.Team == Team.Black ? ConsoleColor.Black : ConsoleColor.White;
                    var cellColor = cell.Highlight ? ConsoleColor.DarkYellow : ConsoleColor.DarkGreen;

                    if (cell.Contents.Team != Team.None)
                    {
                        Console.BackgroundColor = cell.Anchored ? ConsoleColor.DarkRed : (cell.BoardType == CellType.Void && cell.Contents.Team != Team.None) ? ConsoleColor.Red : cellColor;
                        Console.Write(cell.Contents.Type == PawnType.Empty ? " " : cell.Contents.Type == PawnType.Square ? "x" : "o");
                    }
                    else
                    {
                        if ( cell.BoardType != CellType.Wall)
                        {
                            Console.ForegroundColor = cellColor;
                            Console.BackgroundColor = cell.BoardType == CellType.Void ? ConsoleColor.Black : cellColor;
                        }

                        Console.Write(cell.BoardType == CellType.Wall ? "|" : " ");
                    }

                    Console.ResetColor();
                }
                Console.Write("\r\n");
            }
            Console.Write("\r\n");
        }
    }

    class Program
    {
        static void Main(string[] args)
        {

            var game = new PushFightGame();

            var client = new ConsoleClient(game);

            client.Print();

            while (true)
            {
                client.PrintPrompt();
                var cmd = Console.ReadLine();
                Console.ResetColor();

                if (cmd == "quit")
                {
                    break;
                }

                if (cmd == "auto")
                {
                    var cmds = new List<string>
                    {
                        "place round d1",
                        "place round e1",
                        "place square d2",
                        "place square e2",
                        "place round d3",
                        "place round e3",
                        "place square d4",
                        "place square e4",
                        "place square c1",
                        "place square f4",
                        "push c1 right",
                        "push e2 left",
                        "push d4 right"

                    };

                    foreach (var autocmd in cmds)
                    {
                        client.PrintPrompt();
                        Console.WriteLine(autocmd);
                        Console.ResetColor();
                        client.Command(autocmd);
                        client.Print();
                    }

                    continue;
                }

                if (cmd == "switch")
                {
                    client.team = client.team == Team.White ? Team.Black : Team.White;
                    continue;
                }

                client.Command(cmd);
                client.Print();
            }

        }
    }
}
