using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GameSolution.Entities
{
    public enum MoveType
    {
        WAIT = 0,
        MESSAGE = 1,
        LINE = 2,
        BEACON = 3,
    };

    public class MoveAction
    {
        public MoveType Type { get; set; }
        public int Index1 { get; set; }
        public int Index2 { get; set; }
        public int Strength { get; set; }
        public string Message { get; set; }
        public MoveAction(MoveType moveType) 
        { 
            Type = moveType;
        }
        public static MoveAction CreateLine(int index1, int index2, int strength)
        {
            var action = new MoveAction(MoveType.LINE);
            action.Index1 = index1;
            action.Index2 = index2;
            action.Strength = strength;
            return action;
        }

        public static MoveAction CreateBeacon(int index1, int strength)
        {
            var action = new MoveAction(MoveType.BEACON);
            action.Index1 = index1;
            action.Strength = strength;
            return action;
        }
        public static MoveAction CreateMessage(string message)
        {
            var action = new MoveAction(MoveType.MESSAGE);
            action.Message = message;
            return action;
        }
        public static MoveAction CreateWait()
        {
            var action = new MoveAction(MoveType.WAIT);
            return action;
        }
    }

    public class Move
    {
        public List<MoveAction> Actions { get; set; }

        public Move()
        {
            Actions = new List<MoveAction>();
        }
        public Move(Move move)
        {
            Actions = move.Actions.Select(m => m).ToList();
        }

        public void AddAction(MoveAction move)
        {
            Actions.Add(move);
        }

        public Move Clone()
        {
            return new Move(this);
        }

        public override string ToString()
        {
            StringBuilder moveStr = new StringBuilder();
            foreach(MoveAction move in Actions)
            {
                switch (move.Type)
                {
                    case MoveType.LINE:
                        moveStr.Append("LINE " + move.Index1 + " " + move.Index2 + " " + move.Strength + ";");
                        break;
                    case MoveType.WAIT:
                        moveStr.Append("WAIT;");
                        break;
                    case MoveType.BEACON:
                        moveStr.Append("BEACON " + move.Index1 + " " + move.Strength + ";");
                        break;
                    case MoveType.MESSAGE:
                        moveStr.Append("MESSAGE " + move.Message + ";");
                        break;
                }
            }
            return moveStr.ToString().Substring(0, moveStr.Length - 1);
        }
    }
}
