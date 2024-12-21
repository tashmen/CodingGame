using Algorithms.Space;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GameSolution.Entities
{
    public enum MoveType
    {
        WAIT = 0,
        GROW,
    };

    public class MoveAction
    {
        public MoveType Type { get; set; }
        public int OrganId { get; set; }
        public Point2d Location { get; set; }
        public EntityType EntityType { get; set; }
        public int OrganRootId { get; set; }

        public OrganDirection OrganDirection { get; set; }

        public MoveAction(MoveType moveType)
        {
            Type = moveType;
        }

        public static MoveAction CreateGrow(int organId, Point2d location, EntityType type, int organRootId, OrganDirection organDirection = OrganDirection.North)
        {
            var action = new MoveAction(MoveType.GROW);
            action.OrganId = organId;
            action.Location = location;
            action.EntityType = type;
            action.OrganRootId = organRootId;
            action.OrganDirection = organDirection;
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
            foreach (MoveAction move in Actions)
            {
                switch (move.Type)
                {
                    case MoveType.GROW:
                        moveStr.Append("GROW " + move.OrganId + " " + move.Location.x + " " + move.Location.y + " " + move.EntityType.ToString() + ";");
                        break;
                    case MoveType.WAIT:
                        moveStr.Append("WAIT;");
                        break;
                }
            }
            return moveStr.ToString().Substring(0, moveStr.Length - 1);
        }

        public void Print()
        {
            foreach (string action in ToString().Split(';'))
            {
                Console.WriteLine(action);
            }
        }
    }
}
