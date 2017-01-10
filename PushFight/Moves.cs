using System;
using System.Collections.Generic;
using System.Text;

namespace PushFight
{
    public class Move
    {
        public int x1, x2, y1, y2;

        public Move(int x1, int y1, int x2, int y2)
        {
            this.x1 = x1;
            this.y1 = y1;
            this.x2 = x2;
            this.y2 = y2;
        }

    }

    public class Moves
    {
        public int Remaining { get; private set; }
        public int Maximum { get; private set; }
        public List<Move> List { get; private set; }

        public Moves(int max)
        {
            Maximum = max;
            List = new List<Move>(Maximum);
            Remaining = max;
        }

        public void Track(int x1, int y1, int x2, int y2)
        {
            Remaining -= 1;
            List.Add(new Move(x1, y1, x2, y2));
        }

        public Move Pop()
        {
            if (List.Count == 0)
            {
                return null;
            }

            Remaining += 1;
            var popped = List[List.Count - 1];
            List.RemoveAt(List.Count - 1);
            return popped;
        }

        public void Reset()
        {
            List = new List<Move>(Maximum);
            Remaining = Maximum;
        }
    }
}
