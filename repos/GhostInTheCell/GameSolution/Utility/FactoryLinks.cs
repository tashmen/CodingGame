using System;
using System.Collections.Generic;
using System.Linq;

namespace GameSolution.Utility
{
    public class FactoryLinks
    {
        Dictionary<int, List<Node>> Links { get; set; }
        Dictionary<int, Dictionary<int, List<Node>>> Paths { get; set; }
        public FactoryLinks()
        {
            Links = new Dictionary<int, List<Node>>();
        }

        /// <summary>
        /// Adds a link to the list
        /// </summary>
        /// <param name="factory1">First factory id</param>
        /// <param name="factory2">Second factory id</param>
        /// <param name="distance">The distance between the two factories</param>
        public void AddLink(int factory1, int factory2, int distance)
        {
            Console.Error.WriteLine(factory1 + " " + factory2 + " " + distance);
            AddLinkInternal(factory1, factory2, distance);
            AddLinkInternal(factory2, factory1, distance);
        }

        /// <summary>
        /// Calculates all of the shortest paths in the factory links
        /// </summary>
        public void CalculateShortestPaths()
        {
            Paths = new Dictionary<int, Dictionary<int, List<Node>>>();

            List<int> vertices = Links.Keys.ToList();
            int vertexCount = vertices.Count;

            foreach (int vertex in vertices)
            {
                CalculateShortestPathFromStartNode(vertex, vertexCount);
            }
        }

        /// <summary>
        /// Calculates the shortest paths from the start node to all other nodes
        /// </summary>
        /// <param name="startNode">The starting factory id</param>
        /// <param name="vertexCount">The number of factories</param>
        private void CalculateShortestPathFromStartNode(int startNode, int vertexCount)
        {
            List<Node> minimumSpanningTree = new List<Node>();
            //Console.Error.WriteLine("Starting with " + startNode);
            int currentDist = 0;
            Paths[startNode] = new Dictionary<int, List<Node>>();
            minimumSpanningTree.Add(new Node(startNode, currentDist));
            while (minimumSpanningTree.Count < vertexCount)
            {
                int minDist = 99999;
                Node bestNode = null;
                Node parentNode = null;
                foreach (Node currentNode in minimumSpanningTree)
                {
                    currentDist = currentNode.Distance;
                    //Console.Error.WriteLine("Inspecting: " + currentNode.FactoryId + " distance " + currentDist);
                    foreach (Node adjacent in GetLinks(currentNode.FactoryId))
                    {
                        if (minimumSpanningTree.Where(n => n.FactoryId == adjacent.FactoryId).Any())
                        {
                            continue;//skip factories already in minimum spanning tree
                        }
                        int distance = currentDist + adjacent.Distance;
                        if (distance < minDist)
                        {
                            minDist = distance;
                            bestNode = adjacent.CreateAtDistance(currentDist);
                            parentNode = currentNode;
                        }
                        else if (distance == minDist)//When the distances are equivalent pick the one with the shortest path
                        {
                            Paths[startNode].TryGetValue(currentNode.FactoryId, out List<Node> pathCurrent);
                            int lengthCurrent = pathCurrent == null ? 0 : pathCurrent.Count;
                            Paths[startNode].TryGetValue(parentNode.FactoryId, out List<Node> pathPrevious);
                            int lengthPrevious = pathPrevious == null ? 0 : pathPrevious.Count;
                            if (lengthCurrent > lengthPrevious)
                            {
                                minDist = distance;
                                bestNode = adjacent.CreateAtDistance(currentDist);
                                parentNode = currentNode;
                            }
                        }
                    }
                }
                minimumSpanningTree.Add(bestNode);
                List<Node> currentPath = null;
                if (parentNode.FactoryId != startNode)
                {
                    Paths[startNode].TryGetValue(parentNode.FactoryId, out currentPath);
                }
                if (currentPath == null)
                {
                    currentPath = new List<Node>();
                }
                else
                {
                    currentPath = new List<Node>(currentPath);
                }
                Paths[startNode].Add(bestNode.FactoryId, currentPath);
                currentPath.Add(bestNode);
                /*
                if (startNode == 0)
                {
                    Console.Error.WriteLine("Parent node: " + parentNode.FactoryId + " distance: " + parentNode.Distance);
                    Console.Error.WriteLine("Shortest Node: " + bestNode.FactoryId + " distance: " + bestNode.Distance);
                }
                */
            }
        }

        /// <summary>
        /// Retrieves the links that are adjacent to the given factory
        /// </summary>
        /// <param name="factory">The factory id</param>
        /// <returns></returns>
        public List<Node> GetLinks(int factoryId)
        {
            return Links[factoryId];
        }

        /// <summary>
        /// Retrieves the straight line distance from start to end
        /// </summary>
        /// <param name="startId">The starting factory id</param>
        /// <param name="endId">The ending factory id</param>
        /// <returns>The distance from start to end</returns>
        public int GetDistance(int startId, int endId)
        {
            return GetLinks(startId).Where(l => l.FactoryId == endId).First().Distance + 1;//All commands are issued from this turn which is always turn 1.
        }

        /// <summary>
        /// Retrieves the distance following the shortest path from start to end.
        /// </summary>
        /// <param name="startId">The starting factory id</param>
        /// <param name="endId">The ending factory id</param>
        /// <returns>The distance along the shortest path</returns>
        public int GetShortestPathDistance(int startId, int endId)
        {
            Paths.TryGetValue(startId, out Dictionary<int, List<Node>> endPoints);
            endPoints.TryGetValue(endId, out List<Node> paths);

            /*
            if(endId == 2){
                foreach(Node n in paths){
                    Console.Error.WriteLine($"Path: {n.FactoryId}, dist: {n.Distance}");
                }
            }
            Console.Error.WriteLine($"From start {startId} to {endId} path length: {paths.Count}.");
            */

            return paths.Last().Distance;//Shortest path distance already takes into account the 1 turn to send a troop
        }


        /// <summary>
        /// Retrieves the next factory along the path from start to end
        /// </summary>
        /// <param name="startId">The starting factory id</param>
        /// <param name="endId">The ending factory id</param>
        /// <returns>The factory id that is first in the path</returns>
        public int GetShortestPath(int startId, int endId)
        {
            Paths.TryGetValue(startId, out Dictionary<int, List<Node>> endPoints);
            if (endPoints == null)
            {
                Console.Error.WriteLine("|||Start not found: " + startId);
                return endId;
            }
            endPoints.TryGetValue(endId, out List<Node> paths);
            if (paths == null)
            {
                Console.Error.WriteLine("|||End not found: " + endId + " start: " + startId);
                return endId;
            }

            int shortest = paths.First().FactoryId;
            Console.Error.WriteLine("|||Shortest: " + shortest + " from: " + startId + " to: " + endId);

            return shortest;
        }

        //Adds links to the factory links
        private void AddLinkInternal(int startFactory, int destinationFactory, int distance)
        {
            List<Node> factoryLinks = null;
            if (Links.ContainsKey(startFactory))
            {
                factoryLinks = Links[startFactory];
            }
            else
            {
                factoryLinks = new List<Node>();
                Links[startFactory] = factoryLinks;
            }
            factoryLinks.Add(new Node(destinationFactory, distance));
        }
    }
}
