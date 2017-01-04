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
    public enum ECode : byte { Success, InvalidLocation, WrongTeam, WrongPhase, NotEnoughPieces, WrongHalf, CellIsEmpty, InvalidPushStart, CellNotEmpty, NoMoreMoves };

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
            if (Phase != GamePhase.Push)
            {
                return ECode.WrongPhase;
            }

            if (team != CurrentTeam)
            {
                return ECode.WrongTeam;
            }

            if (RemainingMoves == 0)
            {
                return ECode.NoMoreMoves;
            }

            var cell = Board[x, y];

            if (cell.Contents.Team == Team.None)
            {
                return ECode.CellIsEmpty;
            }

            // more checks go here

            return ECode.Success;
        }

        public ECode ValidatedPlace(int x, int y, Team team, PawnType pawn)
        {
            ConditionalValidator checker;
            ECode ecode = ECode.Success;

            var remaining = (from rem in remainingPieces where rem.Team == team && rem.PawnType == pawn select rem).First();

            // FIXME bounds checking
            checker = new ConditionalValidator(new ValidatorChecks() {
                { () => { return Phase != GamePhase.Placement; }, ECode.WrongPhase },
                { () => { return team != CurrentTeam; }, ECode.WrongTeam },
                { () => { return x == 0 || x == 5; }, ECode.InvalidLocation },
                { () => { return (team == Team.White && y > 4) || (team == Team.Black && y < 5); }, ECode.WrongHalf },
                { () => { return remaining.Count == 0; }, ECode.NotEnoughPieces },
                { () => { return Board[x,y].Contents.Type != PawnType.Empty; }, ECode.CellNotEmpty },
            });
            ecode = checker.Run();

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

            // FIXME bounds checking
            checker = new ConditionalValidator(new ValidatorChecks() {
                { () => { return Phase != GamePhase.Push; }, ECode.WrongPhase },
                { () => { return team != CurrentTeam; }, ECode.WrongTeam },
                { () => { return x == 0 || x == 5; }, ECode.InvalidLocation },
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

        public ECode Input(string input)
        {
            Console.WriteLine(input);

            return ECode.Success;
        }
    }
}
