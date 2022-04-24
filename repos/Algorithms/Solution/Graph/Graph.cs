using System;
using System.Collections.Generic;
using System.Linq;

namespace Algorithms.Graph
{
    public class Graph
    {
        private List<INode> Nodes;
        //Will hold shortest paths from a start node id to an end node id
        private Dictionary<int, Dictionary<int, List<ILink>>> Paths { get; set; }

        public Graph()
        {
            Nodes = new List<INode>();
            
        }

        public void AddNode(INode node)
        {
            Nodes.Add(node);
        }

        /// <summary>
        /// Calculates all of the shortest paths in the node links
        /// </summary>
        public void CalculateShortestPaths()
        {
            Paths = new Dictionary<int, Dictionary<int, List<ILink>>>();

            foreach (INode vertex in Nodes)
            {
                InternalBuildShortestPathsFromStartNode(vertex);
            }
        }

        public void BuildShortestPathsFromStartNode(INode startNode, double maxDistance = double.MaxValue)
        {
            Paths = new Dictionary<int, Dictionary<int, List<ILink>>>();
            InternalBuildShortestPathsFromStartNode(startNode, maxDistance);
        }

        private void InternalBuildShortestPathsFromStartNode(INode startNode, double maxDistance = double.MaxValue)
        {
            foreach (INode node in Nodes)
            {
                node.IsExplored = false;
            }
            
            List<ILink> minimumSpanningTree = new List<ILink>();

            Paths[startNode.Id] = new Dictionary<int, List<ILink>>();
            Paths[startNode.Id][startNode.Id] = new List<ILink>();
            minimumSpanningTree.Add(new Link(startNode, startNode, 0));

            int vertexCount = Nodes.Count;
            double currentDist;

            while (minimumSpanningTree.Count < vertexCount)
            {
                double minDist = 99999;
                ILink bestLink = null;
                ILink parentLink = null;
                foreach (ILink currentLink in minimumSpanningTree)
                {
                    INode currentNode = currentLink.EndNode;
                    currentDist = currentLink.GetDistance(Paths[startNode.Id][currentNode.Id]);
                    foreach (ILink adjacent in currentNode.GetLinks())
                    {
                        INode adjacentNode = adjacent.EndNode;
                        if (adjacentNode.IsExplored)
                        {
                            continue;//skip nodes already in minimum spanning tree
                        }


                        double distance = currentDist + adjacent.Distance;
                        if (distance < minDist)
                        {
                            minDist = distance;
                            bestLink = adjacent;
                            parentLink = currentLink;
                        }
                        else if (distance == minDist)//When the distances are equivalent pick the one with the shortest path
                        {
                            Paths[startNode.Id].TryGetValue(currentNode.Id, out List<ILink> pathCurrent);
                            int lengthCurrent = pathCurrent == null ? 0 : pathCurrent.Count;
                            Paths[startNode.Id].TryGetValue(parentLink.EndNode.Id, out List<ILink> pathPrevious);
                            int lengthPrevious = pathPrevious == null ? 0 : pathPrevious.Count;
                            if (lengthCurrent < lengthPrevious)
                            {
                                minDist = distance;
                                bestLink = adjacent;
                                parentLink = currentLink;
                            }
                        }
                    }
                }
                if (parentLink == null)
                {
                    return;//no possible paths
                }
                minimumSpanningTree.Add(bestLink);
                bestLink.EndNode.IsExplored = true;
                List<ILink> currentPath = null;
                if (!parentLink.EndNode.Equals(startNode))
                {
                    Paths[startNode.Id].TryGetValue(parentLink.EndNode.Id, out currentPath);
                }
                if (currentPath == null)
                {
                    currentPath = new List<ILink>();
                }
                else
                {
                    currentPath = new List<ILink>(currentPath);
                }
                Paths[startNode.Id].Add(bestLink.EndNode.Id, currentPath);
                currentPath.Add(bestLink);
 
                if (minDist >= maxDistance)
                    return;
            }
        }

        /// <summary>
        /// Retrieves the next node along the path from start to end
        /// </summary>
        /// <param name="startId">The starting node id</param>
        /// <param name="endId">The ending node id</param>
        /// <returns>The next node in the path</returns>
        public INode GetNextNodeInShortestPath(INode startNode, INode endNode)
        {
            Paths.TryGetValue(startNode.Id, out Dictionary<int, List<ILink>> endPoints);
            if (endPoints == null)
            {
                Console.Error.WriteLine("|||Start not found: " + startNode.Id);
                throw new InvalidOperationException();
            }
            endPoints.TryGetValue(endNode.Id, out List<ILink> paths);
            if (paths == null)
            {
                Console.Error.WriteLine("|||End not found: " + endNode.Id + " start: " + startNode.Id);
                throw new InvalidOperationException();
            }

            INode shortest = paths.First().EndNode;
            Console.Error.WriteLine("|||Shortest: " + shortest + " from: " + startNode.Id + " to: " + endNode.Id);

            return shortest;
        }

        /// <summary>
        /// Retrieves the distance following the shortest path from start to end.
        /// </summary>
        /// <param name="startId">The starting node</param>
        /// <param name="endId">The ending node</param>
        /// <returns>The distance along the shortest path</returns>
        public double GetShortestPathDistance(INode startNode, INode endNode)
        {
            Paths.TryGetValue(startNode.Id, out Dictionary<int, List<ILink>> endPoints);
            if (endPoints == null)
            {
                return double.MaxValue;
            }
            endPoints.TryGetValue(endNode.Id, out List<ILink> paths);
            if (paths == null)
            {
                return double.MaxValue;
            }

            return paths.First().GetDistance(paths);
        }

        /// <summary>
        /// Retrieves the straight line distance from start to end
        /// </summary>
        /// <param name="startId">The starting node</param>
        /// <param name="endId">The ending node</param>
        /// <returns>The distance from start to end</returns>
        public double GetDistance(Node startNode, Node endNode)
        {
            return startNode.GetLinks().Where(l => l.EndNode.Equals(endNode)).First().Distance;
        }
    }
}
