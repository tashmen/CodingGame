using Algorithms.Utility;
using System.Collections.Generic;
using static GameSolution.Constants;

namespace GameSolution.Entities
{
    public class ShadowMapMask
    {
        public long[][] shadowMapDistanceThenSunDirection;

        public ShadowMapMask(List<Cell> board)
        {
            shadowMapDistanceThenSunDirection = new long[maxTreeSize][];
            shadowMapDistanceThenSunDirection[0] = new long[sunReset] { 0, 0, 0, 0, 0, 0 };
            shadowMapDistanceThenSunDirection[1] = new long[sunReset] { 0, 0, 0, 0, 0, 0 };
            shadowMapDistanceThenSunDirection[2] = new long[sunReset] { 0, 0, 0, 0, 0, 0 };

            foreach (Cell cell in board)
            {                
                for (int sunDirection = 0; sunDirection < sunReset; sunDirection++)
                {
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
                        shadowMapDistanceThenSunDirection[treeSize][sunDirection] |= BitFunctions.GetBitMask(index);
                    }
                }
            }
        }

        public long GetShadowMap(int treeSize, int sunDirection)
        {
            return shadowMapDistanceThenSunDirection[treeSize - 1][sunDirection];
        }
    }
}
