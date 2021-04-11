using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameSolution.Utility
{
    public class Node
    {
        public int FactoryId { get; set; }
        public int Distance { get; set; }
        public Node(int factory, int distance)
        {
            FactoryId = factory;
            Distance = distance;
        }

        /// <summary>
        /// Creates a clone of the node from the current distance.  This is used while building the minimum spanning tree.
        /// </summary>
        /// <param name="currentDist">The current distance from the starting factory</param>
        /// <returns>A clone of the node with the proper distance</returns>
        public Node CreateAtDistance(int currentDist)
        {
            return new Node(FactoryId, currentDist + Distance + 1);//It takes an additional turn on every hop to re-route troops
        }
    }
}
