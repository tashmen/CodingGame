using Algorithms.Utility;
using System.Collections.Generic;
using static GameSolution.Constants;

namespace GameSolution.Entities
{
    /// <summary>
    /// This map is intended to calculate all the indices where a seed can be placed at differing sizes
    /// </summary>
    public class SeedMapMask
    {
        public long[][] seedMapByCellThenSize;

        public SeedMapMask(List<Cell> board)
        {
            seedMapByCellThenSize = new long[board.Count][];
            foreach(Cell cell in board)
            {
                int currentCellIndex = cell.index;
                seedMapByCellThenSize[currentCellIndex] = new long[maxTreeSize] { 0, 0, 0 };

                for (int i = 0; i < sunReset; i++)
                {
                    Cell current = cell;
                    for (int tSize = 0; tSize < maxTreeSize; tSize++)
                    {
                        int index = current.GetCellNeighbor(i);
                        if (index == -1)
                        {
                            break;
                        }
                        current = board[index];

                        if(current.richness != (int)Richness.Unusable)
                            seedMapByCellThenSize[currentCellIndex][tSize] |= BitFunctions.GetBitMask(index);

                        Cell tempCurrent = current;
                        for (int tempTSize = tSize + 1; tempTSize < maxTreeSize; tempTSize++)
                        {
                            int cellIndex = tempCurrent.GetCellNeighbor((i + 1) % sunReset);
                            if (cellIndex == -1)
                            {
                                break;
                            }
                            tempCurrent = board[cellIndex];

                            if (tempCurrent.richness != (int)Richness.Unusable)
                                seedMapByCellThenSize[currentCellIndex][tempTSize] |= BitFunctions.GetBitMask(cellIndex);
                        }
                    }
                }
            }
        }

        public long GetSeedMap(int cellIndex, int treeSize, int startSize)
        {
            long seedMap = 0;
            for(int i = startSize - 1; i<treeSize; i++)
            {
                seedMap |= seedMapByCellThenSize[cellIndex][i];
            }
            return seedMap;
        }

        public long GetSeedMap(int cellIndex, int treeSize)
        {
            return seedMapByCellThenSize[cellIndex][treeSize - 1];
        }
    }
}
