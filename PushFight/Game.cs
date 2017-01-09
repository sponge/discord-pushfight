using System.Collections.Generic;
using System.Linq;

namespace PushFight
{
    public delegate ECode ErrCheck();

    public enum Team : byte { None, White, Black }
    public enum PawnType : byte { Empty, Square, Round };
    public enum CellType : byte { Void, Solid, Wall };
    public enum Direction : byte { None, Up, Down, Left, Right };
    public enum GamePhase : byte { Invalid, Placement, Push, Complete };
    public enum ECode : byte {
        Success,
        InvalidLocation,
        WrongTeam,
        WrongPhase,
        NotEnoughPieces,
        WrongHalf,
        CellIsEmpty,
        WrongPushType,
        WrongPawnTeam,
        CellNotEmpty,
        NoMoreMoves,
        CellNotConnected,
        CantPushWall,
        CantPushNull,
        CantPushAnchored,
        InputUnknownCommand,
        InputBadPawnType,
        InputBadCell,
        GameOver,
    };

    public struct Pawn
    {
        public Pawn(Team team, PawnType type)
        {
            Team = team;
            Type = type;
        }

        public PawnType Type;
        public Team Team;
    }

    public class RemainingPieces
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

    public class PushFightGame
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
            {0, 0, 0, 0, 0, 0},
            {0, 0, 1, 1, 0, 0},
            {0, 0, 1, 1, 1, 2},
            {2, 1, 1, 1, 1, 2},
            {2, 1, 1, 1, 1, 2},
            {2, 1, 1, 1, 1, 2},
            {2, 1, 1, 1, 1, 2},
            {2, 1, 1, 1, 0, 0},
            {0, 0, 1, 1, 0, 0},
            {0, 0, 0, 0, 0, 0}
        };

        public Cell[,] Board = new Cell[10, 6];
        public GamePhase Phase;
        public Team CurrentTeam = Team.White;
        public int RemainingMoves = 2;
        public Team Winner = Team.None;

        private Cell lastAnchored;

        private static Dictionary<ECode, string> errors = new Dictionary<ECode, string>()
        {
            {ECode.Success, "Success!"},
            {ECode.InvalidLocation, "Location is not valid."},
            {ECode.WrongTeam, "It is not your turn."},
            {ECode.WrongPhase, "Not a valid command for this phase."},
            {ECode.NotEnoughPieces, "Out of pawns of that type."},
            {ECode.WrongHalf, "Can only place pawns on your half of the board."},
            {ECode.CellIsEmpty, "The selected cell is empty"},
            {ECode.WrongPushType, "Can only push starting from a square pawn."},
            {ECode.WrongPawnTeam, "That pawn is not on your team."},
            {ECode.CellNotEmpty, "The selected cell is not empty."},
            {ECode.NoMoreMoves, "No more moves remaining."},
            {ECode.CellNotConnected, "Can only move cells to cells that are connected by empty spaces."},
            {ECode.CantPushWall, "A wall is blocking the push."},
            {ECode.CantPushNull, "A NULL is blocking the push???"},
            {ECode.CantPushAnchored, "Can't push a piece that is anchored."},
            {ECode.InputUnknownCommand, "Unknown command."},
            {ECode.InputBadPawnType, "Unknown pawn type, please specify either \"round\" or \"square\""},
            {ECode.InputBadCell, "Unknown cell. Cells are specified using a letter first, and then a number."},
            {ECode.GameOver, "Game Over!"},
        };
        static public string GetError(ECode err)
        {
            string errStr;
            if (!errors.TryGetValue(err, out errStr))
            {
                errStr = err.ToString();
            }
            return errStr;
        }

        public PushFightGame()
        {
            Phase = GamePhase.Placement;
            for (int x = 0; x < Board.GetLength(0); x++)
            {
                for (int y = 0; y < Board.GetLength(1); y++)
                {
                    Board[x, y] = new Cell(this, x, y);
                }
            }
        }

        public bool ScanGameEnded()
        {
            for (int x = 0; x < Board.GetLength(0); x++)
            {
                for (int y = 0; y < Board.GetLength(1); y++)
                {
                    var cell = Board[x, y];
                    if (cell.BoardType == CellType.Void && cell.Contents.Team != Team.None)
                    {
                        Phase = GamePhase.Complete;
                        Winner = cell.Contents.Team == Team.Black ? Team.White : Team.Black;
                        return true;
                    }
                }
            }

            return false;

        }

        public ECode ValidatedMove(int x, int y, int nx, int ny, Team team)
        {
            var checks = new List<ErrCheck>()
            {
                () => { return Phase != GamePhase.Push ? ECode.WrongPhase : ECode.Success; },
                () => { return team != CurrentTeam ? ECode.WrongTeam : ECode.Success; },
                () => { return RemainingMoves == 0 ? ECode.NoMoreMoves : ECode.Success; },

                // check start space
                () => { return x <= 0 || x >= Board.GetLength(0) ? ECode.InvalidLocation : ECode.Success; },
                () => { return y < 0 || y > Board.GetLength(1) ? ECode.InvalidLocation : ECode.Success; },
                () => { return Board[x,y].Contents.Type == PawnType.Empty ? ECode.CellIsEmpty : ECode.Success; },
                () => { return Board[x,y].Contents.Team != CurrentTeam ? ECode.WrongTeam : ECode.Success; ;},

                // check if new space is valid
                () => { return nx <= 0 || nx >= Board.GetLength(0) ? ECode.InvalidLocation : ECode.Success; },
                () => { return ny < 0 || ny > Board.GetLength(1) ? ECode.InvalidLocation : ECode.Success; },
                () => { return Board[nx, ny].BoardType == CellType.Void ? ECode.InvalidLocation : ECode.Success; },
                () => { return Board[nx, ny].Contents.Type != PawnType.Empty ? ECode.CellNotEmpty : ECode.Success; },

                // check connection to new space
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
            cell.MoveContents(nx, ny);
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
                () => { return x <= 0 || x >= Board.GetLength(0) ? ECode.InvalidLocation : ECode.Success; },
                () => { return y < 0 || y >= Board.GetLength(1) ? ECode.InvalidLocation : ECode.Success; },
                () => { return Board[x,y].BoardType != CellType.Solid ? ECode.InvalidLocation : ECode.Success; },
                () => { return (team == Team.White && x > 4) || (team == Team.Black && x < 5) ? ECode.WrongHalf : ECode.Success; },
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
                () => { return x <= 0 || x >= Board.GetLength(0) ? ECode.InvalidLocation : ECode.Success; },
                () => { return y < 0 || y > Board.GetLength(1) ? ECode.InvalidLocation : ECode.Success; },
                () => { return Board[x, y].Contents.Team != team ? ECode.WrongPawnTeam : ECode.Success; },
                () => { return Board[x, y].Contents.Type != PawnType.Square ? ECode.WrongPushType : ECode.Success; },

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

            var anchoredCell = cell.GetNextCell(dir);
            var ecode = cell.StartPush(dir);
            if (ecode != ECode.Success)
            {
                return ecode;
            }

            CurrentTeam = CurrentTeam == Team.White ? Team.Black : Team.White;
            RemainingMoves = 2;

            anchoredCell.Anchored = true;
            if (lastAnchored != null)
            {
                lastAnchored.Anchored = false;
            }
            lastAnchored = anchoredCell;

            ScanGameEnded();

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
            var cmd = input.ToLower().Split(' ');

            ResetHighlight();

            if (Phase == GamePhase.Complete)
            {
                return ECode.GameOver;
            }

            // allow "p" to overload between place and push
            if (cmd[0] == "p")
            {
                cmd[0] = Phase == GamePhase.Placement ? "place" : "push";
            }

            switch (cmd[0])
            {
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

                        var ecode = ValidatedPlace(x, y, team, pawn);

                        if (ecode == ECode.Success)
                        {
                            Board[x, y].Highlight = true;
                        }

                        return ecode;
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

                case "push":
                    {
                        var x = Parsers.X(cmd[1]);
                        var y = Parsers.Y(cmd[1]);
                        var dir = Parsers.Direction(cmd[2]);
                        return ValidatedPush(x, y, team, dir);
                    }


                default:
                    return ECode.InputUnknownCommand;
            }
        }
    }
}
