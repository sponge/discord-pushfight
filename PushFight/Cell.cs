using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PushFight
{
    class Cell
    {

        public int x, y;
        public CellType BoardType;
        public Pawn Contents;
        public bool Anchored;
        public PushFightGame Game;

        public Cell(PushFightGame game, int x, int y)
        {
            this.x = x;
            this.y = y;
            BoardType = (CellType)game.BoardBase[x, y];
            Contents = new Pawn(Team.None, PawnType.Empty);
            Anchored = false;
            Game = game;
        }

        public Cell Up {
            get {
                if (y == 0) { return null; }
                return Game.Board[x, y - 1];
            }
        }

        public Cell Down {
            get {
                if (y == Game.Board.GetLength(1) - 1) { return null; }
                return Game.Board[x, y + 1];
            }
        }

        public Cell Left {
            get {
                if (x == 0) { return null; }
                return Game.Board[x - 1, y];
            }
        }

        public Cell Right {
            get {
                if (x == Game.Board.GetLength(0) - 1) { return null; }
                return Game.Board[x + 1, y];
            }
        }

        public List<Cell> Sweep(Direction dir)
        {
            var res = new List<Cell>();

            var dirStr = dir == Direction.Up ? "Up" : dir == Direction.Left ? "Left" : dir == Direction.Right ? "Right" : "Down";

            var cell = GetType().GetProperty(dirStr).GetValue(this, null) as Cell;
            while (cell != null)
            {
                res.Add(cell);
                cell = GetType().GetProperty(dirStr).GetValue(cell, null) as Cell;
            }

            return res;
        }

        public ECode Push(Direction dir)
        {
            var sweep = Sweep(dir);

            // need a whole lot of checks here

            // set anchor and remove old anchor

            return ECode.Success;
        }
    }
}
