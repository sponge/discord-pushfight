using System;

namespace PushFight
{
    class Parsers
    {
        public static int X(string pair)
        {
            var res = (int)pair[0];
            if (res < 'a' || res > 'h')
            {
                return -1;
            }
            return res - 96;
        }

        public static PawnType Pawn(string pawn)
        {
            var s_pawnType = pawn.ToLower();
            switch (s_pawnType)
            {
                case "round":
                case "r":
                case "c":
                    return PawnType.Round;

                case "square":
                case "s":
                    return PawnType.Square;

                default:
                    return PawnType.Empty;
            }
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

        public static Direction Direction(string dir)
        {
            var s_dir = dir.ToLower();
            switch (s_dir)
            {
                case "n":
                case "north":
                case "u":
                case "up":
                    return PushFight.Direction.Up;
                case "w":
                case "west":
                case "l":
                case "left":
                    return PushFight.Direction.Left;
                case "e":
                case "east":
                case "r":
                case "right":
                    return PushFight.Direction.Right;
                case "s":
                case "south":
                case "d":
                case "down":
                    return PushFight.Direction.Down;
                default:
                    return PushFight.Direction.None;
            }
        }
    }
}
