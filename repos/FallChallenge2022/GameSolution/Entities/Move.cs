using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GameSolution.Entities
{
    public enum MoveType
    {
        WAIT = 0,
        MOVE = 1,
        BUILD = 2,
        SPAWN = 3,
    };

    public class MoveAction
    {
        public MoveType Type { get; set; }
        public int Amount { get; set; }
        public int FromX { get; set; }
        public int FromY { get; set; }
        public int ToX { get; set; }
        public int ToY { get; set; }
        public MoveAction(MoveType moveType) 
        { 
            Type = moveType;
        }
        public static MoveAction CreateMove(int amount, int fromX, int fromY, int toX, int toY)
        {
            var action = new MoveAction(MoveType.MOVE);
            action.FromX= fromX;
            action.FromY= fromY;
            action.ToX= toX;
            action.ToY= toY;
            action.Amount= amount;
            return action;
        }

        public static MoveAction CreateBuild(int x, int y)
        {
            var action = new MoveAction(MoveType.BUILD);
            action.FromX = x;
            action.FromY= y;
            return action;
        }
        public static MoveAction CreateSpawn(int x, int y, int amount)
        {
            var action = new MoveAction(MoveType.SPAWN);
            action.FromX = x;
            action.FromY = y;
            action.Amount= amount;
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
        public List<MoveAction> UnitActions { get; set; }

        public Move()
        {
            UnitActions= new List<MoveAction>();
        }
        public Move(Move move)
        {
            UnitActions = move.UnitActions.Select(m => m).ToList();
        }

        public Move Clone()
        {
            return new Move(this);
        }

        public override string ToString()
        {
            StringBuilder moveStr = new StringBuilder();
            foreach(MoveAction move in UnitActions)
            {
                switch (move.Type)
                {
                    case MoveType.MOVE:
                        moveStr.Append("MOVE " + move.Amount + " " + move.FromX + " " + move.FromY + " " + move.ToX + " " + move.ToY + ";");
                        break;
                    case MoveType.WAIT:
                        moveStr.Append("WAIT;");
                        break;
                    case MoveType.BUILD:
                        moveStr.Append("BUILD " + move.FromX + " " + move.FromY + ";");
                        break;
                    case MoveType.SPAWN:
                        moveStr.Append("SPAWN " + move.Amount + " " + move.FromX + " " + move.FromY + ";");
                        break;
                }
            }
            return moveStr.ToString().Substring(0, moveStr.Length - 1);
        }
    }
}
