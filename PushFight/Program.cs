using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PushFight
{
    class LocMap
    {
        public LocMap(int x, int y, Pawn pawn)
        {
            this.x = x;
            this.y = y;
            this.pawn = pawn;
        }
        public int x, y;
        public Pawn pawn;
    }

    class Program
    {
        static void Main(string[] args)
        {

            var game = new PushFightGame();

            var pieces = new List<LocMap>
            {
                new LocMap(2, 2, new Pawn(Team.White, PawnType.Round)),
                new LocMap(1, 3, new Pawn(Team.White, PawnType.Square)),
                new LocMap(2, 3, new Pawn(Team.Black, PawnType.Round)),
                new LocMap(1, 4, new Pawn(Team.White, PawnType.Round)),
                new LocMap(2, 4, new Pawn(Team.Black, PawnType.Square)),
                new LocMap(0, 5, new Pawn(Team.White, PawnType.Square)),
                new LocMap(1, 5, new Pawn(Team.Black, PawnType.Round)),
                new LocMap(2, 5, new Pawn(Team.Black, PawnType.Square)),
                new LocMap(3, 5, new Pawn(Team.White, PawnType.Square)),
                new LocMap(3, 6, new Pawn(Team.Black, PawnType.Square)),
            };

            game.ValidatedPlace(2, 2, Team.White, PawnType.Round);
            /*
            foreach (var piece in pieces)
            {
                game.Board[piece.x, piece.y].Contents = piece.pawn;
            }
            game.Board[2, 5].Anchored = true;
            */

            var cells = game.Board[2, 2].Sweep(Direction.Left);
        }
    }
}
