using System;
using System.Collections.Generic;
using System.Linq;

namespace Algorithms.Graph
{
    public class Graph
    {
        private Dictionary<int, INode> Nodes;
        //Will hold shortest paths from a start node id to an end node id
        private Dictionary<int, Dictionary<int, List<ILink>>> Paths;

        public Graph()
        {
            Nodes = new Dictionary<int, INode>();
            
        }

        public void AddNode(INode node)
        {
            Nodes[node.Id] = node;
        }

        /// <summary>
        /// Calculates all of the shortest paths in the node links
        /// </summary>
        public void CalculateShortestPaths()
        {
            Paths = new Dictionary<int, Dictionary<int, List<ILink>>>();

            foreach (INode vertex in Nodes.Values)
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
            foreach (INode node in Nodes.Values)
            {
                node.IsExplored = false;
            }
            
            List<ILink> minimumSpanningTree = new List<ILink>();

            Paths[startNode.Id] = new Dictionary<int, List<ILink>>();
            Paths[startNode.Id][startNode.Id] = new List<ILink>();
            minimumSpanningTree.Add(new Link(startNode.Id, startNode.Id, 0));
            startNode.IsExplored = true;

            int vertexCount = Nodes.Count;
            double currentDist;

            while (minimumSpanningTree.Count < vertexCount)
            {
                double minDist = 99999;
                ILink bestLink = null;
                ILink parentLink = null;
                foreach (ILink currentLink in minimumSpanningTree)
                {
                    INode currentNode = Nodes[currentLink.EndNodeId];
                    currentDist = currentLink.GetDistance(Paths[startNode.Id][currentNode.Id]);
                    foreach (ILink adjacent in currentNode.GetLinks())
                    {
                        INode adjacentNode = Nodes[adjacent.EndNodeId];
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
                            Paths[startNode.Id].TryGetValue(parentLink.EndNodeId, out List<ILink> pathPrevious);
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
                Nodes[bestLink.EndNodeId].IsExplored = true;
                List<ILink> currentPath = null;
                if (!parentLink.EndNodeId.Equals(startNode.Id))
                {
                    Paths[startNode.Id].TryGetValue(parentLink.EndNodeId, out currentPath);
                }
                if (currentPath == null)
                {
                    currentPath = new List<ILink>();
                }
                else
                {
                    currentPath = new List<ILink>(currentPath);
                }
                Paths[startNode.Id].Add(bestLink.EndNodeId, currentPath);
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

            INode shortest = Nodes[paths.First().EndNodeId];
            Console.Error.WriteLine("|||Shortest: " + shortest + " from: " + startNode.Id + " to: " + endNode.Id);

            return shortest;
        }

        /// <summary>
        /// Retrieves all nodes along the shortest path between two points
        /// </summary>
        /// <param name="startNodeId">Start node id</param>
        /// <param name="endNodeId">End node id</param>
        /// <returns>The full path from start to end</returns>
        /// <exception cref="InvalidOperationException"></exception>
        public IList<ILink> GetShortestPathAll(int startNodeId, int endNodeId)
        {
            Paths.TryGetValue(startNodeId, out Dictionary<int, List<ILink>> endPoints);
            if (endPoints == null)
            {
                Console.Error.WriteLine("|||Start not found: " + startNodeId);
                throw new InvalidOperationException();
            }
            endPoints.TryGetValue(endNodeId, out List<ILink> paths);
            if (paths == null)
            {
                Console.Error.WriteLine("|||End not found: " + endNodeId + " start: " + startNodeId);
                throw new InvalidOperationException();
            }

            return paths;
        }

        /// <summary>
        /// Retrieves the distance following the shortest path from start to end.
        /// </summary>
        /// <param name="startId">The starting node</param>
        /// <param name="endId">The ending node</param>
        /// <returns>The distance along the shortest path</returns>
        public double GetShortestPathDistance(INode startNode, INode endNode)
        {
            return GetShortestPathDistance(startNode.Id, endNode.Id);
        }

        /// <summary>
        /// Retrieves the distance following the shortest path from start to end.
        /// </summary>
        /// <param name="startId">The starting node id</param>
        /// <param name="endId">The ending node id</param>
        /// <returns>The distance along the shortest path</returns>
        public double GetShortestPathDistance(int startId, int endId)
        {
            Paths.TryGetValue(startId, out Dictionary<int, List<ILink>> endPoints);
            if (endPoints == null)
            {
                return double.MaxValue;
            }
            endPoints.TryGetValue(endId, out List<ILink> paths);
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
            return startNode.GetLinks().Where(l => l.EndNodeId.Equals(endNode.Id)).First().Distance;
        }
    }
}
