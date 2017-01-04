using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PushFight
{
    class LocMap
    {
        public LocMap(int x, int y, Team team, PawnType pawn)
        {
            this.x = x;
            this.y = y;
            this.team = team;
            this.pawn = pawn;
        }
        public int x, y;
        public Team team;
        public PawnType pawn;
    }

    class GamePrinter
    {
        static public void Print(PushFightGame game)
        {
            for (int y = 0; y < game.Board.GetLength(1); y++)
            {
                for (int x = 0; x < game.Board.GetLength(0); x++)
                {
                    var cell = game.Board[x, y];
                    Console.ForegroundColor = cell.Contents.Team == Team.None ? ConsoleColor.DarkGray : cell.Contents.Team == Team.Black ? ConsoleColor.Red : ConsoleColor.Blue;

                    if (cell.Contents.Team != Team.None)
                    {
                        Console.BackgroundColor = ConsoleColor.DarkGray;
                        Console.Write(cell.Contents.Type == PawnType.Empty ? " " : cell.Contents.Type == PawnType.Square ? "■" : "o");
                    } else
                    {
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

            var pieces = new List<LocMap>
            {
                new LocMap(1, 4, Team.White, PawnType.Round),
                new LocMap(1, 5, Team.Black, PawnType.Round),
                new LocMap(2, 4, Team.White, PawnType.Square),
                new LocMap(2, 5, Team.Black, PawnType.Square),
                new LocMap(3, 4, Team.White, PawnType.Round),
                new LocMap(3, 5, Team.Black, PawnType.Round),
                new LocMap(4, 4, Team.White, PawnType.Square),
                new LocMap(4, 5, Team.Black, PawnType.Square),
                new LocMap(3, 3, Team.White, PawnType.Square),
                new LocMap(3, 6, Team.Black, PawnType.Square),
            };

            foreach (var piece in pieces)
            {
                var ecode = game.ValidatedPlace(piece.x, piece.y, piece.team, piece.pawn);
                Debug.Assert(ecode == ECode.Success);
                GamePrinter.Print(game);
            }

            while (true)
            {
                Console.Write("> ");
                var cmd = Console.ReadLine();

                if (cmd == "quit")
                {
                    break;
                }

                game.Input(cmd);
                GamePrinter.Print(game);
            }

        }
    }
}
