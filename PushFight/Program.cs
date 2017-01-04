using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PushFight
{
    class GamePrinter
    {
        static public void Print(PushFightGame game)
        {
            Console.WriteLine("  abcd");
            for (int y = 1; y < game.Board.GetLength(1) - 1; y++)
            {
                Console.Write(y);
                for (int x = 0; x < game.Board.GetLength(0); x++)
                {
                    var cell = game.Board[x, y];
                    Console.ForegroundColor = cell.Contents.Team == Team.None ? ConsoleColor.DarkGray : cell.Contents.Team == Team.Black ? ConsoleColor.Black : ConsoleColor.White;

                    if (cell.Contents.Team != Team.None)
                    {
                        Console.BackgroundColor = ConsoleColor.DarkYellow;
                        Console.Write(cell.Contents.Type == PawnType.Empty ? " " : cell.Contents.Type == PawnType.Square ? "■" : "o");
                    } else
                    {
                        Console.ForegroundColor = ConsoleColor.DarkYellow;
                        Console.Write(cell.BoardType == CellType.Solid ? "█" : cell.BoardType == CellType.Wall ? "│" : " ");
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

            var team = game.CurrentTeam;

            GamePrinter.Print(game);

            while (true)
            {
                Console.ForegroundColor = team == Team.Black ? ConsoleColor.DarkGray : ConsoleColor.White;
                Console.Write(team + "> ");
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
                        "place round a4",
                        "place round a5",
                        "place square b4",
                        "place square b5",
                        "place round c4",
                        "place round c5",
                        "place square d4",
                        "place square d5",
                        "place square c3",
                        "place square c6",
                    };

                    foreach (var autocmd in cmds)
                    {
                        var ecode = game.Input(autocmd, game.CurrentTeam);
                        Debug.Assert(ecode == ECode.Success);
                        GamePrinter.Print(game);
                    }
                    continue;
                }

                if (cmd == "switch")
                {
                    team = team == Team.White ? Team.Black : Team.White;
                    continue;
                }

                var result = game.Input(cmd, team);
                Console.WriteLine(result);
                team = game.CurrentTeam;

                GamePrinter.Print(game);
            }

        }
    }
}
