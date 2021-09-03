using Algorithms;
using System;
using static GameSolution.Constants;

namespace GameSolution.Moves
{
    /// <summary>
    /// To Try: Change this to a static class and make the move a long and bit board the logic
    /// </summary>
    public static class Move
    {
        public static long waitBitValue = 1;
        public static long seedBitValue = 2;
        public static long growBitValue = 4;
        public static long completeBitValue = 8;
        public static long typeBitValue = 15;
        public static long sourceCellIndexValue = 1008;
        public static long targetCellIndexValue = 64512;
        public static long CreateMove(Actions type, long sourceCellIdx, long targetCellIdx)
        {
            long output = 0;
            switch (type)
            {
                case Actions.WAIT:
                    output |= waitBitValue;
                    break;
                case Actions.SEED:
                    output |= seedBitValue;
                    break;
                case Actions.GROW:
                    output |= growBitValue;
                    break;
                case Actions.COMPLETE:
                    output |= completeBitValue;
                    break;
            }

            if(sourceCellIdx == -1)
            {
                output |= sourceCellIndexValue;
            }
            else
            {
                output |= sourceCellIdx << 4;
            }

            if (targetCellIdx == -1)
            {
                output |= targetCellIndexValue;
            }
            else
            {
                output |= targetCellIdx << 10;
            }

            return output;
        }

        public static long CreateMove(Actions type, int targetCellIdx)
        {
            return CreateMove(type, -1, targetCellIdx);
        }

        public static long CreateMove(Actions type)
        {
            return CreateMove(type, -1, -1);
        }

        public static Actions GetType(long move)
        {
            return (Actions)(move & typeBitValue);
        }

        public static int GetSourceIndex(long move)
        {
            long value = move & sourceCellIndexValue;
            if (value == sourceCellIndexValue)
                return -1;
            else 
                return (int)value >> 4;
        }

        public static int GetTargetIndex(long move)
        {
            long value = move & targetCellIndexValue;
            if (value == targetCellIndexValue)
                return -1;
            else 
                return (int)value >> 10;
        }

        public static string ToString(long move)
        {
            if (move == 0)
                return "";
            Actions type = GetType(move);
            int sourceCellIdx = GetSourceIndex(move);
            int targetCellIdx = GetTargetIndex(move);
            if (type == Actions.WAIT)
            {
                return ActionsString.WAIT;
            }
            if (type == Actions.SEED)
            {
                return string.Format("{0} {1} {2}", ActionsString.SEED, sourceCellIdx, targetCellIdx);
            }
            return string.Format("{0} {1}", type, targetCellIdx);
        }

        public static long Parse(string action)
        {
            string[] parts = action.Split(' ');
            switch (parts[0])
            {
                case ActionsString.WAIT:
                    return CreateMove(Actions.WAIT);
                case ActionsString.SEED:
                    return CreateMove(Actions.SEED, int.Parse(parts[1]), int.Parse(parts[2]));
                case ActionsString.GROW:
                    return CreateMove(Actions.GROW, int.Parse(parts[1]));
                case ActionsString.COMPLETE:
                    return CreateMove(Actions.COMPLETE, int.Parse(parts[1]));    
            }

            throw new InvalidOperationException("invalid action type");
        }
    }
}
