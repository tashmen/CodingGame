using System;
using System.Collections.Generic;
using System.Linq;

namespace Algorithms.Graph
{
    public class Graph
    {
        public class DistancePath
        {
            public double Distance;
            public List<ILink> Paths;

            public DistancePath(double distance, List<ILink> paths)
            {
                Distance = distance;
                Paths = paths;
            }
        }

        private readonly Dictionary<int, INode> Nodes;
        //Will hold shortest paths from a start node id to an end node id
        private DistancePath[,] Paths;

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
            Paths = new DistancePath[Nodes.Count, Nodes.Count];

            foreach (INode vertex in Nodes.Values)
            {
                InternalBuildShortestPathsFromStartNode(vertex);
            }
        }

        public void BuildShortestPathsFromStartNode(INode startNode, double maxDistance = double.MaxValue)
        {
            Paths = new DistancePath[Nodes.Count, Nodes.Count];
            InternalBuildShortestPathsFromStartNode(startNode, maxDistance);
        }

        //With a little help from Chat GPT improved the performance significantly.
        private void InternalBuildShortestPathsFromStartNode(INode startNode, double maxDistance = double.MaxValue)
        {
            // Initialize exploration state and paths
            foreach (INode node in Nodes.Values)
            {
                node.IsExplored = false;
            }

            HashSet<ILink> minimumSpanningTree = new HashSet<ILink>();
            SortedSet<(double Distance, int StepCount, ILink Link)> priorityQueue = new SortedSet<(double Distance, int StepCount, ILink Link)>(Comparer<(double Distance, int StepCount, ILink Link)>.Create((a, b) =>
            {
                // Compare first by distance, then by step count (in case of tie)
                int result = a.Distance.CompareTo(b.Distance);
                if (result != 0) return result;
                result = a.StepCount.CompareTo(b.StepCount);
                if (result != 0) return result;
                return a.Link.EndNodeId.CompareTo(b.Link.EndNodeId);
            }));

            Paths[startNode.Id, startNode.Id] = new DistancePath(0.0, new List<ILink>());
            startNode.IsExplored = true;

            // Add initial links of the startNode to the priority queue
            foreach (ILink link in startNode.GetLinks())
            {
                priorityQueue.Add((link.Distance, 1, link));  // Distance, StepCount (1), Link
            }

            while (minimumSpanningTree.Count < Nodes.Count && priorityQueue.Count > 0)
            {
                // Get the link with the minimum distance and fewest steps
                (double currentDist, int stepCount, ILink bestLink) = priorityQueue.Min;
                priorityQueue.Remove(priorityQueue.Min);

                INode currentNode = Nodes[bestLink.StartNodeId];
                INode adjacentNode = Nodes[bestLink.EndNodeId];

                if (adjacentNode.IsExplored)
                {
                    continue; // Skip already explored nodes
                }

                adjacentNode.IsExplored = true;
                minimumSpanningTree.Add(bestLink);

                // Update paths
                DistancePath currentPath = Paths[startNode.Id, currentNode.Id];
                if (currentPath == null)
                {
                    currentPath = new DistancePath(0.0, new List<ILink>());
                }
                else
                {
                    currentPath = new DistancePath(currentDist, new List<ILink>(currentPath.Paths)); // Copy the existing path
                }

                // Add the new link to the current path
                currentPath.Paths.Add(bestLink);

                // Store the complete path from the start node to the adjacent node
                Paths[startNode.Id, bestLink.EndNodeId] = currentPath;

                // Exit if the distance exceeds the maximum allowed
                if (currentDist >= maxDistance)
                    return;

                // Add adjacent links of the newly explored node to the queue
                foreach (ILink adjacentLink in adjacentNode.GetLinks())
                {
                    INode nextNode = Nodes[adjacentLink.EndNodeId];
                    if (!nextNode.IsExplored)
                    {
                        // Calculate the new distance and step count for the adjacent link
                        double newDist = currentDist + adjacentLink.Distance;
                        int newStepCount = stepCount + 1;
                        priorityQueue.Add((newDist, newStepCount, adjacentLink));
                    }
                }
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
            DistancePath paths = Paths[startNode.Id, endNode.Id];
            if (paths == null)
            {
                Console.Error.WriteLine("Path not found, end: " + endNode.Id + " start: " + startNode.Id);
                throw new InvalidOperationException();
            }

            INode shortest = Nodes[paths.Paths.First().EndNodeId];
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
            DistancePath paths = Paths[startNodeId, endNodeId];
            if (paths == null)
            {
                Console.Error.WriteLine("Path not found, end: " + endNodeId + " start: " + startNodeId);
                throw new InvalidOperationException();
            }

            return paths.Paths;
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
            DistancePath path = Paths[startId, endId];
            if (path == null)
                return double.MaxValue;

            return path.Distance;
        }


        /// <summary>
        /// Gets the shortest distance path if one exists
        /// </summary>
        /// <param name="startId">The starting point</param>
        /// <param name="endId">The ending point</param>
        /// <param name="distancePath">The distance and the path</param>
        /// <returns>the shortest path and it's distance or null if no path exists</returns>
        public bool GetShortest(int startId, int endId, out DistancePath distancePath)
        {
            distancePath = null;
            DistancePath path = Paths[startId, endId];
            if (path == null)
                return false;

            distancePath = path;
            return true;
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
