using System;

namespace PushFight
{
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
