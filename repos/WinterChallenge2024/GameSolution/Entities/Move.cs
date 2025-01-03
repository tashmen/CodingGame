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
        public MoveType Type;
        public int OrganId;
        public Point2d Location;
        public EntityType EntityType;
        public int OrganRootId;
        public double Score;

        public OrganDirection OrganDirection;

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
            MoveAction action = new MoveAction(MoveType.GROW);
            action.OrganId = organId;
            action.Location = location;
            action.EntityType = type;
            action.OrganRootId = organRootId;
            action.OrganDirection = organDirection;
            return action;
        }


        public static MoveAction CreateWait()
        {
            MoveAction action = new MoveAction(MoveType.WAIT);
            action.EntityType = EntityType.NONE;
            action.Score = 0;
            return action;
        }

        public static MoveAction CreateSpore(int sporeOrganId, Point2d location)
        {
            MoveAction action = new MoveAction(MoveType.SPORE);
            action.OrganId = sporeOrganId;
            action.Location = location;
            action.EntityType = EntityType.ROOT;
            return action;
        }

        public override string ToString()
        {
            MoveAction move = this;
            switch (this.Type)
            {
                case MoveType.GROW:
                    return $"GROW {move.OrganId} {move.Location.x} {move.Location.y} {move.EntityType} {GetGrowDirection(move.OrganDirection)} {move.Score}";
                case MoveType.SPORE:
                    return $"SPORE {move.OrganId} {move.Location.x} {move.Location.y} {move.Score}";
                case MoveType.WAIT:
                    return $"WAIT {move.Score}";
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
        public MoveAction[] Actions;

        public double Score;

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
            Score = 0;
            foreach (MoveAction action in actions)
            {
                Score += action.Score;
            }
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
                foreach (MoveAction action in Actions)
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
                moveStr.Append(actionStr).Append(';');
            }
            return moveStr.ToString();
        }

        public void Print()
        {
            Console.Error.WriteLine(ToString());
        }

        public void Output()
        {
            foreach (MoveAction action in Actions)
            {
                Console.WriteLine(action.ToString());
            }
        }
    }
}
