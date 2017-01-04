using System.Collections.Generic;
using System.Linq;

namespace PushFight
{
    public delegate ECode ErrCheck();

    public enum Team : byte { None, White, Black }
    public enum PawnType : byte { Empty, Square, Round };
    public enum CellType : byte { Void, Solid, Wall };
    public enum Direction : byte { Up, Down, Left, Right };
    public enum GamePhase : byte { Placement, Push, Complete };
    public enum ECode : byte {
        Success,
        InvalidLocation,
        WrongTeam,
        WrongPhase,
        NotEnoughPieces,
        WrongHalf,
        CellIsEmpty,
        InvalidPushStart,
        CellNotEmpty,
        NoMoreMoves,
        CellNotConnected,
        InputUnknownCommand,
        InputBadPawnType,
        InputBadCell,
    };

    struct Pawn
    {
        public Pawn(Team team, PawnType type)
        {
            Team = team;
            Type = type;
        }

        public PawnType Type;
        public Team Team;
    }

    class RemainingPieces
    {
        public Team Team;
        public PawnType PawnType;
        public int Count;

        public RemainingPieces(Team team, PawnType pawnType, int count)
        {
            Team = team;
            PawnType = pawnType;
            Count = count;
        }
    }

    class PushFightGame
    {

        public List<RemainingPieces> RemainingPieces = new List<RemainingPieces>() 
        {
            new RemainingPieces(Team.Black, PawnType.Round, 2),
            new RemainingPieces(Team.Black, PawnType.Square, 3),
            new RemainingPieces(Team.White, PawnType.Round, 2),
            new RemainingPieces(Team.White, PawnType.Square, 3),
        };

        // rotated so [x,y] is how you access it
        public int[,] BoardBase = {
                   /*y0 .. y9*/
            /*x0*/ {0, 0, 2, 2, 2, 2, 2, 0, 0, 0},
                   {0, 0, 1, 1, 1, 1, 1, 0, 0, 0},
                   {0, 1, 1, 1, 1, 1, 1, 1, 1, 0},
                   {0, 1, 1, 1, 1, 1, 1, 1, 1, 0},
                   {0, 0, 0, 1, 1, 1, 1, 1, 0, 0},
            /*x5*/ {0, 0, 0, 2, 2, 2, 2, 2, 0, 0}
        };

        public Cell[,] Board = new Cell[6, 10];
        public GamePhase Phase;
        public Team CurrentTeam = Team.White;
        public int RemainingMoves = 2;
        public Team Winner = Team.None;

        public PushFightGame()
        {
            for (int x = 0; x < Board.GetLength(0); x++)
            {
                for (int y = 0; y < Board.GetLength(1); y++)
                {
                    Board[x, y] = new Cell(this, x, y);
                }
            }
        }

        public ECode ValidatedMove(int x, int y, int nx, int ny, Team team)
        {
            var checks = new List<ErrCheck>()
            {
                () => { return Phase != GamePhase.Push ? ECode.WrongPhase : ECode.Success; },
                () => { return team != CurrentTeam ? ECode.WrongTeam : ECode.Success; },
                () => { return RemainingMoves == 0 ? ECode.NoMoreMoves : ECode.Success; },
                () => { return x <= 0 || x >= 5 ? ECode.InvalidLocation : ECode.Success; },
                () => { return y < 0 || y > 9 ? ECode.InvalidLocation : ECode.Success; },
                () => { return Board[x,y].Contents.Type == PawnType.Empty ? ECode.CellIsEmpty : ECode.Success; },
                () => { return Board[x,y].Contents.Team != CurrentTeam ? ECode.WrongTeam : ECode.Success; ;},

                () => { return nx <= 0 || nx >= 5 ? ECode.InvalidLocation : ECode.Success; },
                () => { return ny< 0 || ny> 9 ? ECode.InvalidLocation : ECode.Success; },
                () => { return Board[nx, ny].BoardType == CellType.Void ? ECode.InvalidLocation : ECode.Success; },
                () => { return Board[nx, ny].Contents.Type != PawnType.Empty ? ECode.CellNotEmpty : ECode.Success; },
                () => { return Board[x, y].IsConnectedTo(nx, ny) == false ? ECode.CellNotConnected : ECode.Success; },
            };

            foreach (var check in checks)
            {
                var ret = check();
                if (ret != ECode.Success)
                {
                    return ret;
                }
            }

            var cell = Board[x, y];
            var newCell = Board[nx, ny];

            newCell.Contents = cell.Contents;
            cell.ClearContents();
            RemainingMoves -= 1;

            return ECode.Success;
        }

        public ECode ValidatedPlace(int x, int y, Team team, PawnType pawn)
        {
            var remaining = (from rem in RemainingPieces where rem.Team == team && rem.PawnType == pawn select rem).First();

            var checks = new List<ErrCheck>()
            {
                () => { return Phase != GamePhase.Placement ? ECode.WrongPhase : ECode.Success; },
                () => { return team != CurrentTeam ? ECode.WrongTeam : ECode.Success; },
                () => { return x <= 0 || x >= 5 ? ECode.InvalidLocation : ECode.Success; },
                () => { return y < 0 || y > 9 ? ECode.InvalidLocation : ECode.Success; },
                () => { return Board[x,y].BoardType != CellType.Solid ? ECode.InvalidLocation : ECode.Success; },
                () => { return (team == Team.White && y > 4) || (team == Team.Black && y < 5) ? ECode.WrongHalf : ECode.Success; },
                () => { return Board[x,y].Contents.Type != PawnType.Empty ? ECode.CellNotEmpty : ECode.Success; },
                () => { return remaining.Count == 0 ? ECode.NotEnoughPieces : ECode.Success; },
            };

            foreach (var check in checks)
            {
                var ret = check();
                if (ret != ECode.Success)
                {
                    return ret;
                }
            }

            Board[x, y].Contents = new Pawn(team, pawn);

            remaining.Count -= 1;
            CurrentTeam = CurrentTeam == Team.White ? Team.Black : Team.White;

            var piecesLeft = (from rem in RemainingPieces select rem.Count).Sum();

            if (piecesLeft == 0)
            {
                Phase = GamePhase.Push;
            }

            return ECode.Success;
        }

        public ECode ValidatedPush(int x, int y, Team team, Direction dir)
        {
            var checks = new List<ErrCheck> {
                () => { return Phase != GamePhase.Push ? ECode.WrongPhase : ECode.Success; },
                () => { return team != CurrentTeam ? ECode.WrongTeam : ECode.Success; },
                () => { return x <= 0 || x >= 5 ? ECode.InvalidLocation : ECode.Success; },
                () => { return y < 0 || y > 9 ? ECode.InvalidLocation : ECode.Success; },
                () => { return (team == Team.White && y > 4) || (team == Team.Black && y < 5) ? ECode.WrongHalf : ECode.Success; },
                () => { return Board[x, y].Contents.Team == Team.None ? ECode.CellIsEmpty : ECode.Success; },
                () => { return Board[x, y].Contents.Type != PawnType.Square ? ECode.InvalidPushStart : ECode.Success; },

            };
            foreach (var check in checks)
            {
                var ret = check();
                if (ret != ECode.Success)
                {
                    return ret;
                }
            }

            var cell = Board[x, y];

            var ecode = cell.Push(dir);
            if (ecode != ECode.Success)
            {
                return ecode;
            }

            CurrentTeam = CurrentTeam == Team.White ? Team.Black : Team.White;

            return ECode.Success;
        }

        private void ResetHighlight()
        {
            foreach(Cell cell in Board)
            {
                cell.Highlight = false;
            }
        }

        public ECode Input(string input, Team team)
        {
            var cmd = input.Split(' ');

            ResetHighlight();

            switch (cmd[0])
            {
                case "pl":
                case "place":
                    {
                        var pawn = Parsers.Pawn(cmd[1]);
                        if (pawn == PawnType.Empty)
                        {
                            return ECode.InputBadPawnType;
                        }
                        var x = Parsers.X(cmd[2]);
                        var y = Parsers.Y(cmd[2]);

                        if (x == -1 || y == -1)
                        {
                            return ECode.InputBadCell;
                        }

                        return ValidatedPlace(x, y, team, pawn);
                    }

                case "m":
                case "mv":
                case "move":
                    {
                        var x = Parsers.X(cmd[1]);
                        var y = Parsers.Y(cmd[1]);

                        if (cmd.Length == 2)
                        {
                            Board[x, y].ConnectedCells().ForEach(cell => cell.Highlight = true);
                            return ECode.Success;
                        }

                        var nx = Parsers.X(cmd[2]);
                        var ny = Parsers.Y(cmd[2]);

                        if ( x == -1 || y == -1 || nx == -1 || ny == -1)
                        {
                            return ECode.InputBadCell;
                        }

                        return ValidatedMove(x, y, nx, ny, team);
                    }

                default:
                    return ECode.InputUnknownCommand;
            }
        }
    }
}
