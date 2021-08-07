using System;
using System.Collections.Generic;
using System.Linq;

namespace Algorithms.Graph
{
    public class GraphLinks
    {
        private Dictionary<int, List<Node>> Links { get; set; }
        private Dictionary<int, Dictionary<int, List<Node>>> Paths { get; set; }
        public GraphLinks()
        {
            Links = new Dictionary<int, List<Node>>();
        }

        /// <summary>
        /// Adds a link to the list
        /// </summary>
        /// <param name="id1">First id</param>
        /// <param name="id2">Second id</param>
        /// <param name="distance">The distance between the two nodes</param>
        public void AddLink(int id1, int id2, int distance)
        {
            Console.Error.WriteLine(id1 + " " + id2 + " " + distance);
            AddLinkInternal(id1, id2, distance);
            AddLinkInternal(id2, id1, distance);
        }

        public void RemoveLink(int id1, int id2)
        {
            Links[id1].RemoveAll(n => n.Id == id2);
            Links[id2].RemoveAll(n => n.Id == id1);
        }

        /// <summary>
        /// Calculates all of the shortest paths in the node links
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
        /// <param name="startNode">The starting id</param>
        /// <param name="vertexCount">The number of nodes</param>
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
                    foreach (Node adjacent in GetLinks(currentNode.Id))
                    {
                        if (minimumSpanningTree.Where(n => n.Id == adjacent.Id).Any())
                        {
                            continue;//skip nodes already in minimum spanning tree
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
                            Paths[startNode].TryGetValue(currentNode.Id, out List<Node> pathCurrent);
                            int lengthCurrent = pathCurrent == null ? 0 : pathCurrent.Count;
                            Paths[startNode].TryGetValue(parentNode.Id, out List<Node> pathPrevious);
                            int lengthPrevious = pathPrevious == null ? 0 : pathPrevious.Count;
                            if (lengthCurrent < lengthPrevious)
                            {
                                minDist = distance;
                                bestNode = adjacent.CreateAtDistance(currentDist);
                                parentNode = currentNode;
                            }
                        }
                    }
                }
                if (parentNode == null)
                {
                    return;//no possible paths
                }
                minimumSpanningTree.Add(bestNode);
                List<Node> currentPath = null;
                if (parentNode.Id != startNode)
                {
                    Paths[startNode].TryGetValue(parentNode.Id, out currentPath);
                }
                if (currentPath == null)
                {
                    currentPath = new List<Node>();
                }
                else
                {
                    currentPath = new List<Node>(currentPath);
                }
                Paths[startNode].Add(bestNode.Id, currentPath);
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
        /// <param name="id">The node id</param>
        /// <returns></returns>
        public List<Node> GetLinks(int id)
        {
            return Links[id];
        }

        /// <summary>
        /// Retrieves the straight line distance from start to end
        /// </summary>
        /// <param name="startId">The starting node id</param>
        /// <param name="endId">The ending node id</param>
        /// <returns>The distance from start to end</returns>
        public int GetDistance(int startId, int endId)
        {
            return GetLinks(startId).Where(l => l.Id == endId).First().Distance;
        }

        /// <summary>
        /// Retrieves the distance following the shortest path from start to end.
        /// </summary>
        /// <param name="startId">The starting node id</param>
        /// <param name="endId">The ending node id</param>
        /// <returns>The distance along the shortest path</returns>
        public int GetShortestPathDistance(int startId, int endId)
        {
            Paths.TryGetValue(startId, out Dictionary<int, List<Node>> endPoints);
            if(endPoints == null)
            {
                return 99999;
            }
            endPoints.TryGetValue(endId, out List<Node> paths);
            if (paths == null)
            {
                return 99999;
            }

            return paths.Last().Distance;
        }


        /// <summary>
        /// Retrieves the next node along the path from start to end
        /// </summary>
        /// <param name="startId">The starting node id</param>
        /// <param name="endId">The ending node id</param>
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

            int shortest = paths.First().Id;
            Console.Error.WriteLine("|||Shortest: " + shortest + " from: " + startId + " to: " + endId);

            return shortest;
        }

        //Adds links to the node links
        public void AddLinkInternal(int startNode, int endNode, int distance)
        {
            List<Node> factoryLinks;
            if (Links.ContainsKey(startNode))
            {
                factoryLinks = Links[startNode];
            }
            else
            {
                factoryLinks = new List<Node>();
                Links[startNode] = factoryLinks;
            }
            factoryLinks.Add(new Node(endNode, distance));
        }
    }
}
