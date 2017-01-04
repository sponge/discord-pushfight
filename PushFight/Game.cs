using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ValidatorChecks = System.Collections.Generic.Dictionary<PushFight.ConditionalValidator.D, PushFight.ECode>;

namespace PushFight
{
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
    
    class ConditionalValidator
    {
        public delegate bool D();

        public ValidatorChecks Checks;
        public ConditionalValidator(ValidatorChecks d)
        {
            Checks = d;
        }
        
        public ECode Run()
        {
            foreach (var check in Checks)
            {
                if (check.Key() == true)
                {
                    return check.Value;
                }
            }

            return ECode.Success;
        }
    }

    class PushFightGame
    {

        public List<RemainingPieces> remainingPieces = new List<RemainingPieces>() 
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
            var checker = new ConditionalValidator(new ValidatorChecks() {
                { () => { return Phase != GamePhase.Push; }, ECode.WrongPhase },
                { () => { return team != CurrentTeam; }, ECode.WrongTeam },
                { () => { return x <= 0 || x >= 5; }, ECode.InvalidLocation },
                { () => { return y < 0 || y > 9; }, ECode.InvalidLocation },
                { () => { return RemainingMoves == 0; }, ECode.NoMoreMoves },
                { () => { return Board[x,y].Contents.Type == PawnType.Empty; }, ECode.CellIsEmpty },
                { () => { return Board[x, y].Contents.Team != CurrentTeam; }, ECode.WrongTeam },
                // more checks go here
            });
            var ecode = checker.Run();

            if (ecode != ECode.Success)
            {
                return ecode;
            }

            var cell = Board[x, y];

            return ECode.Success;
        }

        public ECode ValidatedPlace(int x, int y, Team team, PawnType pawn)
        {
            var remaining = (from rem in remainingPieces where rem.Team == team && rem.PawnType == pawn select rem).First();

            var checker = new ConditionalValidator(new ValidatorChecks() {
                { () => { return Phase != GamePhase.Placement; }, ECode.WrongPhase },
                { () => { return team != CurrentTeam; }, ECode.WrongTeam },
                { () => { return x <= 0 || x >= 5; }, ECode.InvalidLocation },
                { () => { return y < 0 || y > 9; }, ECode.InvalidLocation },
                { () => { return Board[x,y].BoardType != CellType.Solid; }, ECode.InvalidLocation },
                { () => { return (team == Team.White && y > 4) || (team == Team.Black && y < 5); }, ECode.WrongHalf },
                { () => { return Board[x,y].Contents.Type != PawnType.Empty; }, ECode.CellNotEmpty },
                { () => { return remaining.Count == 0; }, ECode.NotEnoughPieces },

            });
            var ecode = checker.Run();

            if (ecode != ECode.Success)
            {
                return ecode;
            }

            Board[x, y].Contents = new Pawn(team, pawn);

            remaining.Count -= 1;
            CurrentTeam = CurrentTeam == Team.White ? Team.Black : Team.White;

            var piecesLeft = (from rem in remainingPieces select rem.Count).Sum();

            if (piecesLeft == 0)
            {
                Phase = GamePhase.Push;
            }

            return ECode.Success;
        }

        public ECode ValidatedPush(int x, int y, Team team, Direction dir)
        {
            ConditionalValidator checker;
            ECode ecode = ECode.Success;

            checker = new ConditionalValidator(new ValidatorChecks() {
                { () => { return Phase != GamePhase.Push; }, ECode.WrongPhase },
                { () => { return team != CurrentTeam; }, ECode.WrongTeam },
                { () => { return x <= 0 || x >= 5; }, ECode.InvalidLocation },
                { () => { return y < 0 || y > 9; }, ECode.InvalidLocation },
                { () => { return (team == Team.White && y > 4) || (team == Team.Black && y < 5); }, ECode.WrongHalf },
                { () => { return Board[x, y].Contents.Team == Team.None; }, ECode.CellIsEmpty },
                { () => { return Board[x, y].Contents.Type != PawnType.Square; }, ECode.InvalidPushStart },

            });
            ecode = checker.Run();

            if (ecode != ECode.Success)
            {
                return ecode;
            }

            var cell = Board[x, y];

            ecode = cell.Push(dir);
            if (ecode != ECode.Success)
            {
                return ecode;
            }

            CurrentTeam = CurrentTeam == Team.White ? Team.Black : Team.White;

            return ECode.Success;
        }

        public ECode Input(string input, Team team)
        {
            var cmd = input.Split(' ');

            if (cmd[0] == "place")
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

            return ECode.InputUnknownCommand;
        }
    }

    class Parsers
    {
        public static int X(string pair)
        {
            var res = (int)pair[0];
            if (res < 'a' || res > 'd')
            {
                return -1;
            }
            return res - 96;
        }

        public static PawnType Pawn(string pawn)
        {
            var s_pawnType = pawn.ToLower();
            return s_pawnType == "round" ? PawnType.Round : s_pawnType == "square" ? PawnType.Square : PawnType.Empty;
        }

        public static int Y(string pair)
        {
            Int32 res = -1;
            var success = Int32.TryParse(pair.Substring(1), out res);
            if (!success)
            {
                return -1;
            }

            return res;
        }
    }
}
