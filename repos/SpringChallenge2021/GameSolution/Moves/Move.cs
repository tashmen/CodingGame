using GameSolution.Algorithm;
using static GameSolution.Constants;

namespace GameSolution.Moves
{
    public class Move : IMove
    {
        public string type;
        public int targetCellIdx;
        public int sourceCellIdx;

        public Move(string type, int sourceCellIdx, int targetCellIdx)
        {
            this.type = type;
            this.targetCellIdx = targetCellIdx;
            this.sourceCellIdx = sourceCellIdx;
        }

        public Move(Move move)
        {
            type = move.type;
            targetCellIdx = move.targetCellIdx;
            sourceCellIdx = move.sourceCellIdx;
        }

        public Move(string type, int targetCellIdx)
            : this(type, 0, targetCellIdx)
        {
        }

        public Move(string type)
            : this(type, 0, 0)
        {
        }

        public override string ToString()
        {
            if (type == Actions.WAIT)
            {
                return Actions.WAIT;
            }
            if (type == Actions.SEED)
            {
                return string.Format("{0} {1} {2}", Actions.SEED, sourceCellIdx, targetCellIdx);
            }
            return string.Format("{0} {1}", type, targetCellIdx);
        }

        public static Move Parse(string action)
        {
            string[] parts = action.Split(' ');
            switch (parts[0])
            {
                case Actions.WAIT:
                    return new Move(Actions.WAIT);
                case Actions.SEED:
                    return new Move(Actions.SEED, int.Parse(parts[1]), int.Parse(parts[2]));
                case Actions.GROW:
                case Actions.COMPLETE:
                default:
                    return new Move(parts[0], int.Parse(parts[1]));
            }
        }
    }
}
