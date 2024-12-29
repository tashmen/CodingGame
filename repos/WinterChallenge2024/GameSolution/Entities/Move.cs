using System;
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
        public double Score { get; set; }

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

        public override string ToString()
        {
            var move = this;
            switch (this.Type)
            {
                case MoveType.GROW:
                    return "GROW " + move.OrganId + " " + move.Location.x + " " + move.Location.y + " " + move.EntityType.ToString() + " " + GetGrowDirection(move.OrganDirection) + ";";
                case MoveType.SPORE:
                    return "SPORE " + move.OrganId + " " + move.Location.x + " " + move.Location.y + ";";
                case MoveType.WAIT:
                    return "WAIT;";
            }
            return "";
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
    }

    public class Move
    {
        public MoveAction[] Actions { get; set; }

        public Move()
        {
        }
        public Move(Move move)
        {
            Actions = move.Actions.Select(m => m).ToArray();
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

        public void SetActions(MoveAction[] actions)
        {
            Actions = actions;
        }

        public Move Clone()
        {
            return new Move(this);
        }

        public override bool Equals(object obj)
        {
            // Check if the object is the same reference or of the same type
            if (ReferenceEquals(this, obj))
                return true;

            // Check if the object is of the same type
            if (obj is Move otherMove)
            {
                // Ensure Actions are compared (null checks included)
                if (this.Actions == null && otherMove.Actions == null)
                    return true;

                if (this.Actions == null || otherMove.Actions == null)
                    return false;

                // Compare Actions arrays element-by-element
                if (this.Actions.Length != otherMove.Actions.Length)
                    return false;

                for (int i = 0; i < this.Actions.Length; i++)
                {
                    // Compare each MoveAction (assuming MoveAction also overrides Equals and GetHashCode)
                    if (!this.Actions[i].Equals(otherMove.Actions[i]))
                        return false;
                }

                return true;
            }

            return false;
        }

        // Override GetHashCode based on Actions
        public override int GetHashCode()
        {
            // Use a base hash code for the class and incorporate each Action into the hash calculation
            int hash = 17; // Arbitrary non-zero number
            if (Actions != null)
            {
                foreach (var action in Actions)
                {
                    hash = hash * 23 + (action?.GetHashCode() ?? 0); // Use 23 as another multiplier (common convention)
                }
            }

            return hash;
        }


        public override string ToString()
        {
            StringBuilder moveStr = new StringBuilder();
            foreach (MoveAction move in Actions)
            {
                string actionStr = move.ToString();
                moveStr.Append(actionStr);
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
