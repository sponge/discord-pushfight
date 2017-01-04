using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PushFight
{
    public enum Team : byte { None, White, Black }
    public enum PawnType : byte { Empty, Square, Round };
    public enum CellType : byte { Void, Solid, Wall };
    public enum Direction : byte { Up, Down, Left, Right };
    public enum GamePhase : byte { Placement, Push, Complete };
    public enum ECode : byte { Success, InvalidLocation, WrongTeam, WrongPhase, NotEnoughPieces, WrongHalf };

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
            /*x0*/ {0, 0, 1, 1, 1, 1, 1, 0, 0, 0},
                   {0, 1, 1, 1, 1, 1, 1, 1, 1, 0},
                   {0, 1, 1, 1, 1, 1, 1, 1, 1, 0},
            /*x3*/ {0, 0, 0, 1, 1, 1, 1, 1, 0, 0}
        };

        public Cell[,] Board = new Cell[4, 10];
        public GamePhase phase;
        public Team currentTeam = Team.White;

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
        
        public Team HasGameEnded()
        {
            return Team.None;
        }

        // this takes array indexes, make sure to subtract 1 from "human-readable" numbers
        public ECode ValidatedPlace(int x, int y, Team team, PawnType pawn)
        {
            if (phase != GamePhase.Placement)
            {
                return ECode.WrongPhase;
            }

            if (team != currentTeam)
            {
                return ECode.WrongTeam;
            }

            if ((team == Team.White && y > 4) || (team == Team.Black && y < 5))
            {
                return ECode.WrongHalf;
            }

            var remaining = (from rem in remainingPieces where rem.Team == team && rem.PawnType == pawn select rem.Count).First();

            if (remaining == 0)
            {
                return ECode.NotEnoughPieces;
            }

            Board[x, y].Contents = new Pawn(team, pawn);

            return ECode.Success;
        }

        bool ValidatedPush(int x, int y, Direction dir)
        {
            var cell = Board[x, y];
            // check for proper team and game phase
            if (cell.Contents.Team == Team.None || cell.Contents.Type != PawnType.Square)
            {
                return false;
            }

            return cell.Push(dir);
        }

        void Input(string input)
        {
            Console.WriteLine(input);
        }
    }
}
