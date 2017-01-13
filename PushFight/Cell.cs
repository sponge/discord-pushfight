using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace PushFight
{
    public class Cell
    {
        public int x, y;
        public CellType BoardType;
        public Pawn Contents;
        public bool Anchored;
        public PushFightGame Game;
        public bool Highlight = false;

        public Cell(PushFightGame game, int x, int y)
        {
            this.x = x;
            this.y = y;
            BoardType = (CellType)game.BoardBase[x, y];
            Contents = new Pawn(Team.None, PawnType.Empty);
            Anchored = false;
            Game = game;
        }

        public void ClearContents()
        {
             Contents = new Pawn(Team.None, PawnType.Empty);
        }

        public Cell GetNextCell(Direction dir)
        {
            switch (dir)
            {
                case Direction.Up:
                    if (y == 0) { return null; }
                    return Game.Board[x, y - 1];
                case Direction.Down:
                    if (y == Game.Board.GetLength(1) - 1) { return null; }
                    return Game.Board[x, y + 1];
                case Direction.Left:
                    if (x == 0) { return null; }
                    return Game.Board[x - 1, y];
                case Direction.Right:
                    if (x == Game.Board.GetLength(0) - 1) { return null; }
                    return Game.Board[x + 1, y];
            }
            return this;
        }

        public void MoveContents(int x, int y)
        {
            var newCell = Game.Board[x, y];
            newCell.Contents = Contents;
            ClearContents();
        }

        public ECode CanPush(Direction dir)
        {
            var next = GetNextCell(dir);
            if (next == null)
            {
                return ECode.CantPushNull;
            }

            if (next.BoardType == CellType.Wall)
            {
                return ECode.CantPushWall;
            }

            if (next.Contents.Type == PawnType.Empty)
            {
                return ECode.Success;
            }

            if (next.Anchored == true)
            {
                return ECode.CantPushAnchored;
            }

            var ecode = next.CanPush(dir);
            if (ecode != ECode.Success)
            {
                return ecode;
            }

            return ECode.Success;
        }

        public void Push(Direction dir)
        {
            var nextCell = GetNextCell(dir);
            if (nextCell.Contents.Team != Team.None)
            {
                nextCell.Push(dir);
            }

            MoveContents(nextCell.x, nextCell.y);
        }

        public List<Cell> ConnectedCells()
        {
            List<Cell> res = new List<Cell>();

            Queue<Cell> cellQueue = new Queue<Cell>();
            cellQueue.Enqueue(this);

            while (cellQueue.Count > 0)
            {
                Cell currentCell = cellQueue.Dequeue();
                if (!res.Contains(currentCell))
                {
                    res.Add(currentCell);
                    // check for a cell in each direction and queue it if it's a valid move
                    foreach (Direction dir in Enum.GetValues(typeof(Direction)))
                    {
                        Cell nextCell = currentCell.GetNextCell(dir);
                        if (nextCell != null && ValidMove(nextCell))
                        {
                            cellQueue.Enqueue(nextCell);
                        }
                    }
                }
            }

            return res;
        }

        // TODO put this somewhere it makes sense and can be re-used
        private bool ValidMove(Cell cell)
        {
            return cell.Contents.Type == PawnType.Empty && cell.BoardType != CellType.Wall && cell.BoardType != CellType.Void;
        }

        public bool IsConnectedTo(int x, int y)
        {
            // TODO could be faster but it doesn't really matter for this game
            return ConnectedCells()
                .Where(cell => cell.x == x && cell.y == y)
                .Any();
        }
    }
}
