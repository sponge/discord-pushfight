using System;
using System.Collections.Generic;
using System.Text;

namespace PushFight
{
    public enum TurnEventType : byte { Invalid, Move, Push, Place }
    public class TurnEvent
    {
        public TurnEventType Type;
        public int x1, x2, y1, y2;
        public Direction Direction;

        public TurnEvent(TurnEventType type, int x1, int y1)
        {
            Type = type;
            this.x1 = x1;
            this.y1 = y1;
        }

        public TurnEvent(TurnEventType type, int x1, int y1, Direction direction)
        {
            Type = type;
            this.x1 = x1;
            this.y1 = y1;
            Direction = direction;
        }

        public TurnEvent(TurnEventType type, int x1, int y1, int x2, int y2)
        {
            Type = type;
            this.x1 = x1;
            this.y1 = y1;
            this.x2 = x2;
            this.y2 = y2;
        }
    }
}
