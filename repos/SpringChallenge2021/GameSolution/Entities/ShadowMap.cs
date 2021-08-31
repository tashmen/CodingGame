using System.Collections.Generic;
using static GameSolution.Constants;

namespace GameSolution.Entities
{
    public class ShadowMap
    {
        public List<int>[][] shadowMapByCellThenSunDirection;

        public ShadowMap(List<Cell> board)
        {
            shadowMapByCellThenSunDirection = new List<int>[board.Count][];

            foreach (Cell cell in board)
            {
                shadowMapByCellThenSunDirection[cell.index] = new List<int>[sunReset];
                for (int sunDirection = 0; sunDirection < sunReset; sunDirection++)
                {
                    shadowMapByCellThenSunDirection[cell.index][sunDirection] = new List<int>(maxTreeSize);
                    Cell current = cell;
                    int shadowDirection = (sunDirection + halfSunReset) % sunReset;
                    for(int treeSize = 0; treeSize < maxTreeSize; treeSize++)
                    {
                        int index = current.GetCellNeighbor(shadowDirection);
                        if (index == -1)
                        {
                            break;
                        }
                        current = board[index];
                        shadowMapByCellThenSunDirection[cell.index][sunDirection].Add(index);
                    }
                }
            }
        }

        public List<int> GetShadowMap(int cellIndex, int sunDirection)
        {
            return shadowMapByCellThenSunDirection[cellIndex][sunDirection];
        }
    }
}
