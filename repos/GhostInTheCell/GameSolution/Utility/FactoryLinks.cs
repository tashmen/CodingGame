using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameSolution
{
    public class FactoryLinks
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

            public Node CreateAtDistance(int currentDist)
            {
                return new Node(FactoryId, currentDist + Distance);
            }
        }

        Dictionary<int, List<Node>> Links { get; set; }
        Dictionary<int, Dictionary<int, List<Node>>> Paths { get; set; }
        public FactoryLinks()
        {
            Links = new Dictionary<int, List<Node>>();
        }

        public void AddLink(int factory1, int factory2, int distance)
        {
            Console.Error.WriteLine(factory1 + " " + factory2 + " " + distance);
            AddLinkInternal(factory1, factory2, distance);
            AddLinkInternal(factory2, factory1, distance);
        }

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
                            //When the distances are equivalent pick the one with the longest path
                            minDist = distance;
                            bestNode = adjacent.CreateAtDistance(currentDist);
                            parentNode = currentNode;
                        }
                        else if (distance == minDist)
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
                if (startNode == 0)
                {
                    Console.Error.WriteLine("Parent node: " + parentNode.FactoryId + " distance: " + parentNode.Distance);
                    Console.Error.WriteLine("Shortest Node: " + bestNode.FactoryId + " distance: " + bestNode.Distance);
                }
            }
        }

        public List<Node> GetLinks(int factory)
        {
            return Links[factory];
        }

        //Retrieves direct straight distance
        public int GetDistance(int startId, int endId)
        {
            return GetLinks(startId).Where(l => l.FactoryId == endId).First().Distance + 1;//All commands are issued from this turn which is always turn 1.
        }

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

            return paths.Last().Distance + 1;
        }



        public int ShortestPath(int startId, int endId)
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
