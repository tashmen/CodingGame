using Algorithms.Space;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GameSolution.Entities
{
    public enum MoveType
    {
        WAIT = 0,
        GROW,
        SPORE
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

        public static int[][] EntityCosts = new int[][]
        {
          new int[]{0, 0, 0, 0 },
          new int[]{1, 1, 1, 1},
          new int[]{ 1, 0, 0, 0 },
          new int[]{0, 1, 1, 0},
          new int[]{0, 0, 1, 1 },
          new int[]{0, 1, 0, 1 }
        };

        public int[] GetCost()
        {
            return EntityCosts[EntityType - EntityType.NONE];
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
            action.EntityType = EntityType.NONE;
            return action;
        }

        public static MoveAction CreateSpore(int sporeOrganId, Point2d location)
        {
            var action = new MoveAction(MoveType.SPORE);
            action.OrganId = sporeOrganId;
            action.Location = location;
            action.EntityType = EntityType.ROOT;
            return action;
        }
    }

    public class Move : IEnumerable<MoveAction>
    {
        public MoveAction[] Actions { get; set; }
        private int _actionIndex = 0;

        public Move()
        {
            Actions = new MoveAction[50];
            _actionIndex = 0;
        }
        public Move(Move move)
        {
            Actions = move.Actions.Select(m => m).ToArray();
            _actionIndex = move.Actions.Count(a => a != null);
        }

        public IEnumerator<MoveAction> GetEnumerator()
        {
            if (Actions == null)
                yield break;

            for (int i = 0; i < _actionIndex; i++)
            {
                yield return Actions[i];
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        private int[] _costs = null;
        public int[] GetCost()
        {
            if (_costs == null)
            {
                _costs = new int[] { 0, 0, 0, 0 };

                foreach (MoveAction action in Actions)
                {
                    int[] actionCost = action.GetCost();
                    for (int i = 0; i < 4; i++)
                    {
                        _costs[i] += actionCost[i];
                    }
                }
            }

            return _costs;
        }

        public void AddAction(MoveAction move)
        {
            Actions[_actionIndex++] = move;
        }

        public void SetActions(MoveAction[] actions)
        {
            Actions = actions;
            _actionIndex = actions.Count(a => a != null);
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
                        moveStr.Append("GROW " + move.OrganId + " " + move.Location.x + " " + move.Location.y + " " + move.EntityType.ToString() + " " + GetGrowDirection(move.OrganDirection) + ";");
                        break;
                    case MoveType.SPORE:
                        moveStr.Append("SPORE " + move.OrganId + " " + move.Location.x + " " + move.Location.y + ";");
                        break;
                    case MoveType.WAIT:
                        moveStr.Append("WAIT;");
                        break;
                }
            }
            return moveStr.ToString().Substring(0, moveStr.Length - 1);
        }

        public char GetGrowDirection(OrganDirection direction)
        {
            switch (direction)
            {
                case OrganDirection.North:
                    return 'N';
                case OrganDirection.South:
                    return 'S';
                case OrganDirection.West:
                    return 'W';
                case OrganDirection.East:
                    return 'E';
            }
            throw new Exception("Invalid direction");
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
