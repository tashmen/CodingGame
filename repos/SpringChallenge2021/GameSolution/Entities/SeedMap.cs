using System.Collections.Generic;
using static GameSolution.Constants;

namespace GameSolution.Entities
{
    /// <summary>
    /// This map is intended to calculate all the indices where a seed can be placed at differing sizes
    /// </summary>
    public class SeedMap
    {
        public List<int>[][] seedMapByCellThenSize;

        public SeedMap(List<Cell> board)
        {
            seedMapByCellThenSize = new List<int>[board.Count][];
            foreach(Cell cell in board)
            {
                int currentCellIndex = cell.index;
                seedMapByCellThenSize[currentCellIndex] = new List<int>[maxTreeSize];
                seedMapByCellThenSize[currentCellIndex][0] = new List<int>(sunReset);
                seedMapByCellThenSize[currentCellIndex][1] = new List<int>(sunReset * 2);
                seedMapByCellThenSize[currentCellIndex][2] = new List<int>(sunReset * 3);

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
                            seedMapByCellThenSize[currentCellIndex][tSize].Add(index);

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
                                seedMapByCellThenSize[currentCellIndex][tempTSize].Add(cellIndex);
                        }
                    }
                }
            }
            
            
        }

        public List<int> GetSeedMap(int cellIndex, int treeSize)
        {
            return seedMapByCellThenSize[cellIndex][treeSize - 1];
        }
    }
}
