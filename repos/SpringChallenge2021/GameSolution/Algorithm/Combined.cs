/*
 * File generated by SourceCombiner.exe using 9 source files.
 * Created On: 8/12/2021 9:47:00 PM
*/
using Algorithms.Trees;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
//*** SourceCombiner -> original file TreeAlgorithm.cs ***
namespace Algorithms
{
    public class TreeAlgorithm
    {
        protected GameTreeNode RootNode { get; private set; }
        public void SetState(IGameState rootState, bool isMax = true)
        {
            if (RootNode != null)
            {
                //if we have already started searching then continue to search as we go if possible; the search will scan two layers to see if only one move was played or if 2 moves were played to get back to the original players turn.
                //find the child that matches the new node
                bool isFound = false;
                //Expand any moves left in the root node (if any)
                foreach (IMove move in RootNode.moves)
                {
                    Expand(RootNode, move);
                }
                //Begin scanning the children
                foreach (GameTreeNode child in RootNode.children)
                {
                    if (child.state.Equals(rootState))
                    {
                        RootNode = child;
                        isFound = true;
                        break;
                    }
                    foreach (IMove move in child.moves)
                    {
                        Expand(child, move);
                    }
                    foreach (GameTreeNode descendent in child.children)
                    {
                        if (descendent.state.Equals(rootState))
                        {
                            RootNode = descendent;
                            isFound = true;
                            break;
                        }
                    }
                }
                if (!isFound)
                {
                    Console.Error.WriteLine("Could not find the next state in tree!  Starting over...");
                    RootNode = new GameTreeNode(rootState.Clone(), isMax);
                }
            }
            else
            {
                RootNode = new GameTreeNode(rootState.Clone(), isMax);
            }
        }
        /// <summary>
        /// Expands the given node by create a clone, applying the move and then adding it to the list of children.
        /// </summary>
        /// <param name="node">The node to expand</param>
        /// <param name="move">The move to play on the expanded node</param>
        /// <returns></returns>
        protected GameTreeNode Expand(GameTreeNode node, IMove move)
        {
            IGameState nextState = node.state.Clone();
            nextState.ApplyMove(move, node.isMax);
            GameTreeNode childNode = new GameTreeNode(nextState, !node.isMax, node);
            node.children.Add(childNode);
            return childNode;
        }
    }
}
//*** SourceCombiner -> original file GameTreeNode.cs ***
namespace Algorithms.Trees
{
    public class GameTreeNode
    {
        public IGameState state;
        public IList<IMove> moves;
        public List<GameTreeNode> children;
        public int wins = 0;
        public int loses = 0;
        public int draws = 0;
        public GameTreeNode parent;
        public bool isMax;
        public GameTreeNode(IGameState state, bool isMax, GameTreeNode parent = null)
        {
            this.state = state;
            moves = state.GetPossibleMoves(isMax);
            children = new List<GameTreeNode>();
            this.parent = parent;
            this.isMax = isMax;
        }
        public double GetScore(bool isMax)
        {
            if (isMax)
            {
                return (wins + draws * 0.5) / (wins + draws + loses);
            }
            else
            {
                return (loses + draws * 0.5) / (wins + draws + loses);
            }
        }
        public int TotalPlays()
        {
            return wins + loses + draws;
        }
        public int? GetWinner()
        {
            return state.GetWinner();
        }
        public void ApplyWinner(int? winner)
        {
            switch (winner)
            {
                case 1:
                    wins++;
                    break;
                case 0:
                    draws++;
                    break;
                case -1:
                    loses++;
                    break;
                default:
                    break;
            }
        }
        public double Evaluate()
        {
            return state.Evaluate(isMax);
        }
    }
}
//*** SourceCombiner -> original file Graph.cs ***
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
//*** SourceCombiner -> original file GraphLinks.cs ***
namespace Algorithms.Graph
{
    public class GraphLinks
    {
        public class Node
        {
            public int Id { get; set; }
            public double Distance { get; set; }
            public bool IsExplored { get; set; }
            public Node(int id, double distance)
            {
                Id = id;
                Distance = distance;
            }
            /// <summary>
            /// Creates a clone of the node from the current distance.  This is used while building the minimum spanning tree.
            /// </summary>
            /// <param name="currentDist">The current distance from the starting node</param>
            /// <returns>A clone of the node with the proper distance</returns>
            public Node CreateAtDistance(double currentDist)
            {
                return new Node(Id, currentDist + Distance);
            }
        }
        private Dictionary<int, List<Node>> Links { get; set; }
        private Dictionary<int, Dictionary<int, List<Node>>> Paths { get; set; }
        private bool IsByDirectional;
        public GraphLinks(bool isByDirectional = true)
        {
            Links = new Dictionary<int, List<Node>>();
            IsByDirectional = isByDirectional;
        }
        public bool ContainsLink(int id1, int id2)
        {
            return Links.ContainsKey(id1) && Links[id1].Where(n => n.Id == id2).Any();
        }
        /// <summary>
        /// Adds a link to the list
        /// </summary>
        /// <param name="id1">First id</param>
        /// <param name="id2">Second id</param>
        /// <param name="distance">The distance between the two nodes</param>
        public void AddLink(int id1, int id2, double distance)
        {
            //Console.Error.WriteLine(id1 + " " + id2 + " " + distance);
            AddLinkInternal(id1, id2, distance);
            if(IsByDirectional)
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
                CalculateShortestPathFromStartNode(vertex, vertexCount, 9999999);
            }
        }
        /// <summary>
        /// Calculates the shortest paths from a start node
        /// </summary>
        /// <param name="startNode">id of the start node</param>
        /// <param name="maxDistance">the fartheset distance to travel</param>
        public void CalculateShortestPathsFromStartNode(int startNode, int maxDistance)
        {
            Paths = new Dictionary<int, Dictionary<int, List<Node>>>();
            CalculateShortestPathFromStartNode(startNode, Links.Keys.Count, maxDistance);
        }
        /// <summary>
        /// Calculates the shortest paths from the start node to all other nodes
        /// </summary>
        /// <param name="startNode">The starting id</param>
        /// <param name="vertexCount">The number of nodes</param>
        /// <param name="maxDistance">the farthest distance to travel</param>
        private void CalculateShortestPathFromStartNode(int startNode, int vertexCount, int maxDistance)
        {
            List<Node> minimumSpanningTree = new List<Node>();
            //Console.Error.WriteLine("Starting with " + startNode);
            double currentDist = 0;
            Paths[startNode] = new Dictionary<int, List<Node>>();
            minimumSpanningTree.Add(new Node(startNode, currentDist));
            while (minimumSpanningTree.Count < vertexCount)
            {
                double minDist = 99999;
                Node bestNode = null;
                Node parentNode = null;
                foreach (Node currentNode in minimumSpanningTree)
                {
                    currentDist = currentNode.Distance;
                    //Console.Error.WriteLine("Inspecting: " + currentNode.FactoryId + " distance " + currentDist);
                    foreach (Node adjacent in GetLinks(currentNode.Id))
                    {
                        if (adjacent.IsExplored || minimumSpanningTree.Where(n => n.Id == adjacent.Id).Any())
                        {
                            adjacent.IsExplored = true;
                            continue;//skip nodes already in minimum spanning tree
                        }
                        double distance = currentDist + adjacent.Distance;
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
                if (minDist >= maxDistance)
                    return;
            }
        }
        /// <summary>
        /// Retrieves the links that are adjacent to the given node
        /// </summary>
        /// <param name="id">The node id</param>
        /// <returns></returns>
        public List<Node> GetLinks(int id)
        {
            return Links[id];
        }
        public Dictionary<int, List<Node>> GetPaths(int startId)
        {
            return Paths[startId];
        }
        /// <summary>
        /// Retrieves the straight line distance from start to end
        /// </summary>
        /// <param name="startId">The starting node id</param>
        /// <param name="endId">The ending node id</param>
        /// <returns>The distance from start to end</returns>
        public double GetDistance(int startId, int endId)
        {
            return GetLinks(startId).Where(l => l.Id == endId).First().Distance;
        }
        /// <summary>
        /// Retrieves the distance following the shortest path from start to end.
        /// </summary>
        /// <param name="startId">The starting node id</param>
        /// <param name="endId">The ending node id</param>
        /// <returns>The distance along the shortest path</returns>
        public double GetShortestPathDistance(int startId, int endId)
        {
            Paths.TryGetValue(startId, out Dictionary<int, List<Node>> endPoints);
            if (endPoints == null)
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
                throw new InvalidOperationException();
            }
            endPoints.TryGetValue(endId, out List<Node> paths);
            if (paths == null)
            {
                Console.Error.WriteLine("|||End not found: " + endId + " start: " + startId);
                throw new InvalidOperationException();
            }
            int shortest = paths.First().Id;
            Console.Error.WriteLine("|||Shortest: " + shortest + " from: " + startId + " to: " + endId);
            return shortest;
        }
        /// <summary>
        /// Retrieves the full path from start to end
        /// </summary>
        /// <param name="startId">the start id</param>
        /// <param name="endId">the end id</param>
        /// <returns>The full path</returns>
        public List<Node> GetShortestPathAll(int startId, int endId)
        {
            Paths.TryGetValue(startId, out Dictionary<int, List<Node>> endPoints);
            if (endPoints == null)
            {
                Console.Error.WriteLine("|||Start not found: " + startId);
                throw new InvalidOperationException();
            }
            endPoints.TryGetValue(endId, out List<Node> paths);
            if (paths == null)
            {
                Console.Error.WriteLine("|||End not found: " + endId + " start: " + startId);
                throw new InvalidOperationException();
            }
            return paths;
        }
        //Adds links to the node links
        private void AddLinkInternal(int startNode, int endNode, double distance)
        {
            List<Node> nodeLinks;
            if (Links.ContainsKey(startNode))
            {
                nodeLinks = Links[startNode];
            }
            else
            {
                nodeLinks = new List<Node>();
                Links[startNode] = nodeLinks;
            }
            nodeLinks.Add(new Node(endNode, distance));
        }
    }
}
//*** SourceCombiner -> original file INode.cs ***
namespace Algorithms.Graph
{
    public interface INode
    {
        int Id { get; }
        bool IsExplored { get; set; }
        List<ILink> GetLinks();
    }
    public class Node : INode
    {
        public int Id { get; private set; }
        public bool IsExplored { get; set; }
        private List<ILink> Links;
        public Node(int id)
        {
            Id = id;
            IsExplored = false;
            Links = new List<ILink>();
        }
        public void AddLink(ILink link)
        {
            Links.Add(link);
        }
        public List<ILink> GetLinks()
        {
            return Links;
        }
        public bool Equals(INode node)
        {
            return node.Id == Id;
        }
    }
    public interface ILink
    {
        INode StartNode { get; }
        INode EndNode { get; }
        long Distance { get; }
        long GetDistance(List<ILink> currentPath);
    }
    public class Link : ILink
    {
        public INode StartNode { get; private set; }
        public INode EndNode { get; private set; }
        public long Distance { get; private set; }
        public Link(INode startNode, INode endNode, long distance)
        {
            StartNode = startNode;
            EndNode = endNode;
            Distance = distance;
        }
        public long GetDistance(List<ILink> currentPath)
        {
            long distance = 0;
            foreach(ILink link in currentPath)
            {
                distance += link.Distance;
            }
            return distance;
        }
    }
}
//*** SourceCombiner -> original file Minimax.cs ***
namespace Algorithms
{
    public class Minimax : TreeAlgorithm
    {
        public IMove GetNextMove(Stopwatch watch, int timeLimit, int depth = int.MaxValue)
        {
            double val = 99999999;
            val *= RootNode.isMax ? -1 : 1;
            IMove bestMove = null;
            foreach (IMove move in RootNode.moves)
            {
                GameTreeNode child = Expand(RootNode, move);
                double currentVal = RunMinimax(child, depth, -999999, 999999, watch, timeLimit);
                if ((RootNode.isMax && currentVal > val) || (!RootNode.isMax && currentVal < val))
                {
                    bestMove = move;
                    val = currentVal;
                }
            }
            return bestMove;
        }
        public double RunMinimax(GameTreeNode currentNode, int depth, double alpha, double beta, Stopwatch watch, int timeLimit)
        {
            if (depth == 0 || watch.ElapsedMilliseconds >= timeLimit)
            {
                double eval = currentNode.Evaluate();
                return eval;
            }
            int? winner = currentNode.GetWinner();
            if (winner.HasValue)
            {
                return winner.Value;
            }
            if (currentNode.isMax)
            {
                double value = -99999;
                double minMax;
                foreach (IMove move in currentNode.moves)
                {
                    GameTreeNode childNode = Expand(currentNode, move);
                    minMax = RunMinimax(childNode, depth - 1, alpha, beta, watch, timeLimit);
                    value = Math.Max(value, minMax);
                    alpha = Math.Max(alpha, value);
                    if (alpha >= beta)
                    {
                        break;
                    }
                }
                return value;
            }
            else
            {
                double value = 99999;
                double minMax;
                foreach (IMove move in currentNode.moves)
                {
                    GameTreeNode childNode = Expand(currentNode, move);
                    minMax = RunMinimax(childNode, depth - 1, alpha, beta, watch, timeLimit);
                    value = Math.Min(value, minMax);
                    beta = Math.Min(beta, value);
                    if (beta <= alpha)
                    {
                        break;
                    }
                }
                return value;
            }
        }
    }
}
//*** SourceCombiner -> original file IGameState.cs ***
namespace Algorithms
{
    public interface IGameState
    {
        /// <summary>
        /// Retrieve the possible moves
        /// </summary>
        /// <param name="isMax">Whether or not to retrieve moves for max</param>
        /// <returns>list of all possible moves</returns>
        IList<IMove> GetPossibleMoves(bool isMax);
        /// <summary>
        /// Applies a move to the game state.  The game state must remember this move so that it can be retrieves with GetMove.
        /// </summary>
        /// <param name="isMax">Whether or not the move is for max</param>
        /// <param name="move">the move to apply</param>
        void ApplyMove(IMove move, bool isMax);
        /// <summary>
        /// Retrieves the move that was played to reach this state.
        /// </summary>
        /// <param name="isMax">Whether or not the move is for max</param>
        /// <returns>The move</returns>
        IMove GetMove(bool isMax);
        /// <summary>
        /// Clones the game state
        /// </summary>
        /// <returns>The copy of the state</returns>
        IGameState Clone();
        /// <summary>
        /// Returns whether or not the game is over and who won (1 - max wins, 0 - draw, -1 - min wins, null - game is not over)
        /// </summary>
        /// <returns>Who won the game</returns>
        int? GetWinner();
        /// <summary>
        /// Determines if the game state is the same as this one
        /// </summary>
        /// <param name="">the state to compare against</param>
        /// <returns>true if equal</returns>
        bool Equals(IGameState state);
        /// <summary>
        /// Evaluates the current game board closer to 1 is max wins closer to -1 is min wins
        /// </summary>
        /// <param name="isMax">true if it is max's turn else false</param>
        /// <returns>A number between [-1, 1]</returns>
        double Evaluate(bool isMax);
    }
}
//*** SourceCombiner -> original file IMove.cs ***
namespace Algorithms
{
    public interface IMove
    {
        public string ToString();
    }
}
//*** SourceCombiner -> original file MonteCarloTreeSearch.cs ***
namespace Algorithms
{
    public class MonteCarloTreeSearch : TreeAlgorithm
    {
        private Random rand;
        public MonteCarloTreeSearch() 
        {
            rand = new Random();
        }
        /// <summary>
        /// Get the next move
        /// </summary>
        /// <param name="watch">timer</param>
        /// <param name="timeLimit">The amount of time to give to the search in milliseconds</param>
        /// <param name="numRollouts">The number of roll outs to play per expansion</param>
        /// <returns></returns>
        public IMove GetNextMove(Stopwatch watch, int timeLimit, int numRollouts = 1, double? exploration = null)
        {
            if(exploration == null)
            {
                exploration = Math.Sqrt(2);
            }
            int count = 0;
            do
            {
                GameTreeNode selectedNode = SelectNodeWithUnplayedMoves(RootNode, exploration.Value);
                if(selectedNode == null)
                {
                    break;
                }
                IMove move = SelectMoveAtRandom(selectedNode);
                GameTreeNode childNode = Expand(selectedNode, move);
                int? winner = childNode.GetWinner();
                if (winner.HasValue)
                {
                    BackPropagate(childNode, winner);
                }
                else
                {
                    for(int i = 0; i<numRollouts; i++)
                    {
                        winner = SimulateGame(childNode.state.Clone(), watch, timeLimit, childNode.isMax);
                        BackPropagate(childNode, winner);
                        count++;
                    }
                }
            }
            while (watch.ElapsedMilliseconds < timeLimit);
            Console.Error.WriteLine($"Played {count} games!");
            GameTreeNode bestChild = null;
            double bestScore = -1;
            foreach(GameTreeNode child in RootNode.children)
            {
                double score = child.GetScore(RootNode.isMax);
                if(bestScore < score)
                {
                    bestChild = child;
                    bestScore = score;
                }
                Console.Error.WriteLine($"w: {child.wins} l: {child.loses} d: {child.draws} move: {child.state.GetMove(RootNode.isMax)} score: {score} isMax: {RootNode.isMax}");
            }
            Console.Error.WriteLine($"Best: w: {bestChild.wins} l: {bestChild.loses} d: {bestChild.draws}");
            return bestChild.state.GetMove(RootNode.isMax);
        }
        private void BackPropagate(GameTreeNode selectedNode, int? winner)
        {
            selectedNode.ApplyWinner(winner);
            GameTreeNode tempNode = selectedNode.parent;
            while(tempNode != null)
            {
                tempNode.ApplyWinner(winner);
                tempNode = tempNode.parent;
            }
        }
        private int? SimulateGame(IGameState state, Stopwatch watch, int timeLimit, bool isMax)
        {
            int? winner = state.GetWinner();
            if (winner.HasValue)
            {
                return winner;
            }
            IMove move = SelectMoveAtRandom(state, isMax);
            state.ApplyMove(move, isMax);
            if (watch.ElapsedMilliseconds >= timeLimit)
            {
                return 0;
            }
            winner = SimulateGame(state, watch, timeLimit, !isMax);
            return winner;
        }
        private GameTreeNode SelectNodeWithUnplayedMoves(GameTreeNode node, double exploration)
        {
            Queue<GameTreeNode> queue = new Queue<GameTreeNode>();
            queue.Enqueue(node);
            GameTreeNode tempNode;
            GameTreeNode bestNode = null;
            double maxValue = -1;
            while (queue.Count > 0)
            {
                tempNode = queue.Dequeue();
                if (tempNode.moves.Count == 0)
                {
                    foreach (GameTreeNode child in tempNode.children)
                    {
                        queue.Enqueue(child);
                    }
                }
                else if(tempNode.parent != null)
                {
                    double wins = tempNode.isMax ? tempNode.wins : tempNode.loses;
                    double nodeTotal = tempNode.TotalPlays();
                    double parentTotal = tempNode.parent.TotalPlays();
                    double value = wins / nodeTotal + exploration * Math.Sqrt(Math.Log(parentTotal / nodeTotal));
                    if(value > maxValue)
                    {
                        maxValue = value;
                        bestNode = tempNode;
                    }
                }
                else return tempNode;
            }
            return bestNode;
        }
        private IMove SelectMoveAtRandom(IGameState state, bool isMax)
        {
            IList<IMove> moves = state.GetPossibleMoves(isMax);
            int index = rand.Next(0, moves.Count);
            return moves[index];
        }
        private IMove SelectMoveAtRandom(GameTreeNode node)
        {
            IMove move;
            if (node.moves.Count == 0)//If there are no more moves then that is a problem...
            {
                throw new Exception("No moves found!");
            }
            else
            {
                int index = rand.Next(0, node.moves.Count);
                move = node.moves[index];
                node.moves.RemoveAt(index);
            }
            return move;
        }
    }
}
