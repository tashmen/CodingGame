namespace Algorithms.Graph
{
    public class Node
    {
        public int Id { get; set; }
        public int Distance { get; set; }

        public bool IsExplored { get; set; }
        public Node(int id, int distance)
        {
            Id = id;
            Distance = distance;
        }



        /// <summary>
        /// Creates a clone of the node from the current distance.  This is used while building the minimum spanning tree.
        /// </summary>
        /// <param name="currentDist">The current distance from the starting node</param>
        /// <returns>A clone of the node with the proper distance</returns>
        public Node CreateAtDistance(int currentDist)
        {
            return new Node(Id, currentDist + Distance);
        }
    }
}
