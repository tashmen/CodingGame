
using Algorithms.GameComponent;
using Algorithms.Trees;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Algorithms.GameComponent
{
    public interface IGameState
    {





        IList GetPossibleMoves(bool isMax);





        void ApplyMove(object move, bool isMax);





        object GetMove(bool isMax);




        IGameState Clone();




        double? GetWinner();





        bool Equals(IGameState state);





        double Evaluate(bool isMax);
    }
}

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

        private DistancePath[,] Paths;
        public Graph()
        {
            Nodes = new Dictionary<int, INode>();
        }
        public void AddNode(INode node)
        {
            Nodes[node.Id] = node;
        }



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

        private void InternalBuildShortestPathsFromStartNode(INode startNode, double maxDistance = double.MaxValue)
        {

            foreach (INode node in Nodes.Values)
            {
                node.IsExplored = false;
            }
            HashSet<ILink> minimumSpanningTree = new HashSet<ILink>();
            SortedSet<(double Distance, int StepCount, ILink Link)> priorityQueue = new SortedSet<(double Distance, int StepCount, ILink Link)>(Comparer<(double Distance, int StepCount, ILink Link)>.Create((a, b) =>
            {

                int result = a.Distance.CompareTo(b.Distance);
                if (result != 0) return result;
                result = a.StepCount.CompareTo(b.StepCount);
                if (result != 0) return result;
                return a.Link.EndNodeId.CompareTo(b.Link.EndNodeId);
            }));
            Paths[startNode.Id, startNode.Id] = new DistancePath(0.0, new List<ILink>());
            startNode.IsExplored = true;

            foreach (ILink link in startNode.GetLinks())
            {
                priorityQueue.Add((link.Distance, 1, link));
            }
            while (minimumSpanningTree.Count < Nodes.Count && priorityQueue.Count > 0)
            {

                (double currentDist, int stepCount, ILink bestLink) = priorityQueue.Min;
                priorityQueue.Remove(priorityQueue.Min);
                INode currentNode = Nodes[bestLink.StartNodeId];
                INode adjacentNode = Nodes[bestLink.EndNodeId];
                if (adjacentNode.IsExplored)
                {
                    continue;
                }
                adjacentNode.IsExplored = true;
                minimumSpanningTree.Add(bestLink);

                DistancePath currentPath = Paths[startNode.Id, currentNode.Id];
                if (currentPath == null)
                {
                    currentPath = new DistancePath(0.0, new List<ILink>());
                }
                else
                {
                    currentPath = new DistancePath(currentDist, new List<ILink>(currentPath.Paths));
                }

                currentPath.Paths.Add(bestLink);

                Paths[startNode.Id, bestLink.EndNodeId] = currentPath;

                if (currentDist >= maxDistance)
                    return;

                foreach (ILink adjacentLink in adjacentNode.GetLinks())
                {
                    INode nextNode = Nodes[adjacentLink.EndNodeId];
                    if (!nextNode.IsExplored)
                    {

                        double newDist = currentDist + adjacentLink.Distance;
                        int newStepCount = stepCount + 1;
                        priorityQueue.Add((newDist, newStepCount, adjacentLink));
                    }
                }
            }
        }






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







        public List<ILink> GetShortestPathAll(int startNodeId, int endNodeId)
        {
            DistancePath paths = Paths[startNodeId, endNodeId];
            if (paths == null)
            {
                Console.Error.WriteLine("Path not found, end: " + endNodeId + " start: " + startNodeId);
                throw new InvalidOperationException();
            }
            return paths.Paths;
        }






        public double GetShortestPathDistance(INode startNode, INode endNode)
        {
            return GetShortestPathDistance(startNode.Id, endNode.Id);
        }






        public double GetShortestPathDistance(int startId, int endId)
        {
            DistancePath path = Paths[startId, endId];
            if (path == null)
                return double.MaxValue;
            return path.Distance;
        }







        public bool GetShortest(int startId, int endId, out DistancePath distancePath)
        {
            distancePath = null;
            DistancePath path = Paths[startId, endId];
            if (path == null)
                return false;
            distancePath = path;
            return true;
        }






        public double GetDistance(Node startNode, Node endNode)
        {
            return startNode.GetLinks().Where(l => l.EndNodeId.Equals(endNode.Id)).First().Distance;
        }
    }
}

namespace Algorithms.Graph
{
    public class GraphLinks
    {
        public class Node
        {
            public int Id;
            public double Distance;
            public bool IsExplored;
            public Node(int id, double distance)
            {
                Id = id;
                Distance = distance;
            }





            public Node CreateAtDistance(double currentDist)
            {
                return new Node(Id, currentDist + Distance);
            }
        }
        private Dictionary<int, List<Node>> Links;
        private Dictionary<int, Dictionary<int, List<Node>>> Paths;
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






        public void AddLink(int id1, int id2, double distance)
        {

            if (ContainsLink(id1, id2))
                return;
            AddLinkInternal(id1, id2, distance);
            if (IsByDirectional)
                AddLinkInternal(id2, id1, distance);
        }
        public void RemoveLink(int id1, int id2)
        {
            Links[id1].RemoveAll(n => n.Id == id2);
            Links[id2].RemoveAll(n => n.Id == id1);
        }



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





        public void CalculateShortestPathsFromStartNode(int startNode, int maxDistance)
        {
            Paths = new Dictionary<int, Dictionary<int, List<Node>>>();
            CalculateShortestPathFromStartNode(startNode, Links.Keys.Count, maxDistance);
        }






        private void CalculateShortestPathFromStartNode(int startNode, int vertexCount, int maxDistance)
        {
            List<Node> minimumSpanningTree = new List<Node>();

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

                    foreach (Node adjacent in GetLinks(currentNode.Id))
                    {
                        if (adjacent.IsExplored || minimumSpanningTree.Where(n => n.Id == adjacent.Id).Any())
                        {
                            adjacent.IsExplored = true;
                            continue;
                        }
                        double distance = currentDist + adjacent.Distance;
                        if (distance < minDist)
                        {
                            minDist = distance;
                            bestNode = adjacent.CreateAtDistance(currentDist);
                            parentNode = currentNode;
                        }
                        else if (distance == minDist)
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
                    return;
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

                if (minDist >= maxDistance)
                    return;
            }
        }





        public List<Node> GetLinks(int id)
        {
            return Links[id];
        }
        public Dictionary<int, List<Node>> GetPaths(int startId)
        {
            return Paths[startId];
        }






        public double GetDistance(int startId, int endId)
        {
            return GetLinks(startId).Where(l => l.Id == endId).First().Distance;
        }






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
        int StartNodeId { get; }
        int EndNodeId { get; }
        double Distance { get; }
        double GetDistance(List<ILink> currentPath);
    }
    public class Link : ILink
    {
        public int StartNodeId { get; private set; }
        public int EndNodeId { get; private set; }
        public double Distance { get; private set; }
        public Link(int startNodeId, int endNodeId, double distance)
        {
            StartNodeId = startNodeId;
            EndNodeId = endNodeId;
            Distance = distance;
        }
        public Link(INode startNode, INode endNode, double distance)
        {
            StartNodeId = startNode.Id;
            EndNodeId = endNode.Id;
            Distance = distance;
        }
        public double GetDistance(List<ILink> currentPath)
        {
            double distance = 0;
            foreach (ILink link in currentPath)
            {
                distance += link.Distance;
            }
            return distance;
        }
    }
}

namespace Algorithms.Trees
{
    public class GameTreeNode
    {
        public IGameState state;
        public IList moves;
        public List<GameTreeNode> children = new List<GameTreeNode>(50);
        public double wins = 0;
        public double loses = 0;
        public int totalPlays = 0;
        public GameTreeNode parent;
        public bool isMax;

        public static GameTreeNode GetGameTreeNode(IGameState state, bool isMax, GameTreeNode parent = null)
        {
            GameTreeNode node = new GameTreeNode();
            node.state = state;
            node.moves = node.state.GetPossibleMoves(isMax);


            node.isMax = isMax;
            node.parent = parent;
            return node;
        }

        public GameTreeNode GetChild(int index)
        {
            return children[index];
        }
        public double GetScore(bool isMax)
        {
            if (totalPlays == 0)
                return 0;
            if (isMax)
            {
                return (wins - loses) / totalPlays;
            }
            else
            {
                return (loses - wins) / totalPlays;
            }
        }
        public double? GetWinner()
        {
            return state.GetWinner();
        }
        public void ApplyWinner(double? winner)
        {
            if (winner.HasValue)
            {
                if (winner > 0)
                {
                    wins += winner.Value;
                }
                else if (winner < 0)
                {
                    loses += Math.Abs(winner.Value);
                }
                totalPlays++;
            }
        }
        public double Evaluate()
        {
            return state.Evaluate(isMax);
        }
    }
}

namespace Algorithms.Trees
{
    public class Minimax : TreeAlgorithm
    {
        public object GetNextMove(Stopwatch watch, int timeLimit, int depth = int.MaxValue)
        {
            double val = 99999999;
            val *= RootNode.isMax ? -1 : 1;
            object bestMove = null;
            foreach (object move in RootNode.moves)
            {
                GameTreeNode child = Expand(RootNode, move);
                double currentVal = RunMinimax(child, depth, -999999, 999999, watch, timeLimit);
                if ((RootNode.isMax && currentVal > val) || (!RootNode.isMax && currentVal < val))
                {
                    bestMove = move;
                    val = currentVal;
                }
                if (watch.ElapsedMilliseconds >= timeLimit)
                {
                    break;
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
            double? winner = currentNode.GetWinner();
            if (winner.HasValue)
            {
                return winner.Value;
            }
            if (currentNode.isMax)
            {
                double value = -99999;
                double minMax;
                foreach (object move in currentNode.moves)
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
                foreach (object move in currentNode.moves)
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

namespace Algorithms.Trees
{
    public class MonteCarloTreeSearch : TreeAlgorithm
    {
        private readonly Random rand;
        private readonly bool printErrors;
        private readonly SearchStrategy strategy;
        private readonly double[] _mathLogCache;
        private static readonly double _DefaultExploration = Math.Sqrt(2);
        public enum SearchStrategy
        {
            Random = 0,
            Sequential = 1
        }
        public MonteCarloTreeSearch(bool showErrors = true, SearchStrategy searchStrategy = SearchStrategy.Random, int mathLogCacheSize = 1000)
        {
            rand = new Random();
            printErrors = showErrors;
            strategy = searchStrategy;
            _mathLogCache = new double[mathLogCacheSize];
            for (int i = 0; i < mathLogCacheSize; i++)
            {
                _mathLogCache[i] = Math.Log(i);
            }
        }
        public IGameState GetRootState()
        {
            return RootNode.state;
        }








        public object GetNextMove(Stopwatch watch, int timeLimit, int depth = -1, int numRollouts = 1, double? exploration = null)
        {
            if (exploration == null)
            {
                exploration = _DefaultExploration;
            }
            int count = 0;
            do
            {
                GameTreeNode selectedNode = SelectNodeWithUnplayedMoves(RootNode, exploration.Value, watch, timeLimit);
                if (selectedNode == null)
                {
                    if (printErrors)
                        Console.Error.WriteLine("Found no more moves!");
                    break;
                }
                object move = SelectMove(selectedNode);
                GameTreeNode childNode = Expand(selectedNode, move);
                if (watch.ElapsedMilliseconds >= timeLimit)
                    break;
                double? winner = childNode.GetWinner();
                if (winner.HasValue)
                {
                    BackPropagate(childNode, winner);
                    count++;
                }
                else
                {
                    for (int i = 0; i < numRollouts; i++)
                    {
                        IGameState clonedState = childNode.state.Clone();
                        winner = SimulateGame(clonedState, watch, timeLimit, depth, childNode.isMax);
                        if (!winner.HasValue)
                        {
                            break;
                        }
                        BackPropagate(childNode, winner);
                        count++;
                    }
                }
            }
            while (watch.ElapsedMilliseconds < timeLimit);
            if (printErrors)
                Console.Error.WriteLine($"Played {count} games!");
            GameTreeNode bestChild = null;
            double bestScore = double.MinValue;
            for (int i = 0; i < RootNode.children.Count; i++)
            {
                GameTreeNode child = RootNode.children[i];
                double score = child.GetScore(RootNode.isMax);
                if (bestScore < score)
                {
                    bestChild = child;
                    bestScore = score;
                }
                if (printErrors)
                    Console.Error.WriteLine($"w: {(RootNode.isMax ? child.wins : child.loses)} l: {(RootNode.isMax ? child.loses : child.wins)} total: {child.totalPlays} score: {score} isMax: {RootNode.isMax} move: {child.state.GetMove(RootNode.isMax)}");
            }
            if (printErrors)
                Console.Error.WriteLine($"Best: w: {(RootNode.isMax ? bestChild.wins : bestChild.loses)} l: {(RootNode.isMax ? bestChild.loses : bestChild.wins)} total: {bestChild.totalPlays} score: {bestScore} move: {bestChild.state.GetMove(RootNode.isMax)}");
            return bestChild.state.GetMove(RootNode.isMax);
        }
        private void BackPropagate(GameTreeNode selectedNode, double? winner)
        {
            selectedNode.ApplyWinner(winner);
            GameTreeNode tempNode = selectedNode.parent;
            while (tempNode != null)
            {
                tempNode.ApplyWinner(winner);
                tempNode = tempNode.parent;
            }
        }
        private double? SimulateGame(IGameState state, Stopwatch watch, int timeLimit, int depth, bool isMax)
        {
            double? winner;
            do
            {
                if (watch.ElapsedMilliseconds >= timeLimit)
                {
                    return null;
                }
                object move = SelectMoveAtRandom(state, isMax);
                state.ApplyMove(move, isMax);
                depth--;
                isMax = !isMax;
                if (watch.ElapsedMilliseconds >= timeLimit)
                {
                    return null;
                }
                winner = state.GetWinner();
            }
            while (!winner.HasValue && depth != 0);
            if (winner.HasValue)
            {
                return winner;
            }
            if (depth == 0)
            {
                double eval = state.Evaluate(isMax);
                if (eval > 1)
                {
                    return 1;
                }
                else if (eval < -1)
                    return -1;
                else return eval;
            }
            Console.Error.WriteLine("Could not find a winner for simulation!");
            throw new InvalidOperationException("Could not find a winner for simulation!");
        }
        Queue<GameTreeNode> _queue = new Queue<GameTreeNode>(100);
        private GameTreeNode SelectNodeWithUnplayedMoves(GameTreeNode node, double exploration, Stopwatch watch, int timeLimit)
        {

            if (node.moves.Count > 0 && node.parent == null)
                return node;

            for (int i = 0; i < node.children.Count; i++)
            {
                _queue.Enqueue(node.children[i]);
            }
            GameTreeNode bestNode = null;
            double maxValue = -1;
            while (_queue.Count > 0)
            {
                GameTreeNode tempNode = _queue.Dequeue();
                if (tempNode.moves.Count > 0)
                {
                    double value = CalculateExplorationValue(tempNode, exploration);
                    if (value > maxValue)
                    {
                        maxValue = value;
                        bestNode = tempNode;
                        if (watch.ElapsedMilliseconds >= timeLimit)
                        {
                            _queue.Clear();
                            break;
                        }
                    }
                }
                else
                {

                    for (int i = 0; i < tempNode.children.Count; i++)
                    {
                        _queue.Enqueue(tempNode.children[i]);
                    }
                }
            }
            return bestNode;
        }
        private double CalculateExplorationValue(GameTreeNode node, double exploration)
        {
            double wins = RootNode.isMax ? node.wins : node.loses;
            double nodeTotal = node.totalPlays;
            int parentTotal = node.parent.totalPlays;
            double parentLog = _mathLogCache[parentTotal];
            double value = wins / nodeTotal + exploration * Math.Sqrt(parentLog / nodeTotal);
            return value;
        }
        private object SelectMoveAtRandom(IGameState state, bool isMax)
        {
            IList moves = state.GetPossibleMoves(isMax);
            if (moves.Count == 0)
            {
                Console.Error.WriteLine("No moves available!");
                throw new Exception("No moves available!");
            }
            int index = rand.Next(0, moves.Count);
            return moves[index];
        }
        private object SelectMove(GameTreeNode node)
        {
            switch (strategy)
            {
                case SearchStrategy.Random:
                    return SelectMoveAtRandom(node);
                case SearchStrategy.Sequential:
                    return SelectMoveSequentially(node);
            }
            Console.Error.WriteLine("strategy not supported");
            throw new InvalidOperationException("strategy not supported");
        }
        private object SelectMoveSequentially(GameTreeNode node)
        {
            object move;
            if (node.moves.Count == 0)
            {
                Console.Error.WriteLine("No moves found!");
                throw new Exception("No moves found!");
            }
            else
            {
                move = node.moves[0];
                node.moves.RemoveAt(0);
            }
            return move;
        }
        private object SelectMoveAtRandom(GameTreeNode node)
        {
            object move;
            if (node.moves.Count == 0)
            {
                Console.Error.WriteLine("No moves found!");
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

namespace Algorithms.Trees
{
    public class TreeAlgorithm
    {
        public TreeAlgorithm()
        {

        }
        protected GameTreeNode RootNode;
        public void SetState(IGameState rootState, bool isMax = true, bool findState = true)
        {
            if (RootNode != null && findState)
            {


                bool isFound = false;

                for (int i = 0; i < RootNode.moves.Count; i++)
                {
                    object move = RootNode.moves[i];
                    Expand(RootNode, move);
                }

                for (int i = 0; i < RootNode.children.Count; i++)
                {
                    GameTreeNode child = RootNode.GetChild(i);
                    if (child.state.Equals(rootState))
                    {
                        RootNode = child;
                        isFound = true;
                        break;
                    }
                    for (int j = 0; j < child.moves.Count; j++)
                    {
                        object move = child.moves[j];
                        Expand(child, move);
                    }
                    for (int j = 0; j < child.children.Count; j++)
                    {
                        GameTreeNode descendent = child.GetChild(j);
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

                    RootNode = GameTreeNode.GetGameTreeNode(rootState.Clone(), isMax);
                }
                else
                {

                    RootNode.parent = null;
                }
            }
            else
            {


                RootNode = GameTreeNode.GetGameTreeNode(rootState.Clone(), isMax);
            }
        }






        protected GameTreeNode Expand(GameTreeNode node, object move)
        {
            IGameState nextState = node.state.Clone();
            nextState.ApplyMove(move, node.isMax);
            GameTreeNode childNode = GameTreeNode.GetGameTreeNode(nextState, !node.isMax, node);
            node.children.Add(childNode);
            return childNode;
        }
    }
}

namespace Algorithms.Utility
{
    public static class BitFunctions
    {
        public static bool IsBitSet(long value, int location)
        {
            long mask = GetBitMask(location);
            return (value & mask) == mask;
        }
        public static long SetBit(long value, int location)
        {
            return value | (GetBitMask(location));
        }
        public static long ClearBit(long value, int location)
        {
            return value & (~(GetBitMask(location)));
        }
        public static long SetOrClearBit(long value, int location, bool isSet)
        {
            if (isSet)
                return SetBit(value, location);
            return ClearBit(value, location);
        }
        public static int NumberOfSetBits(long i)
        {
            i = i - ((i >> 1) & 0x5555555555555555);
            i = (i & 0x3333333333333333) + ((i >> 2) & 0x3333333333333333);
            return (int)((((i + (i >> 4)) & 0xF0F0F0F0F0F0F0F) * 0x101010101010101) >> 56);
        }
        public static long GetBitMask(int index)
        {
            return (long)1 << index;
        }
    }
}

namespace Algorithms.Utility
{
    public class ObjectPool<T> where T : PooledObject<T>, new()
    {
        private readonly ConcurrentQueue<T> _pool;
        private readonly Func<T> _objectGenerator;
        private readonly bool _captureLeaks;
        private readonly HashSet<T> _loanedReferences;
        public ObjectPool(Func<T> objectGenerator, int initialSize = 0, bool captureLeaks = false)
        {
            _captureLeaks = captureLeaks;
            if (captureLeaks)
                _loanedReferences = new HashSet<T>(initialSize);
            _objectGenerator = objectGenerator;
            _pool = new ConcurrentQueue<T>();

            for (int i = 0; i < initialSize; i++)
            {
                _pool.Enqueue(Create());
            }
        }
        private T Create()
        {
            T obj = _objectGenerator();
            return obj;
        }

        public T Get()
        {
            if (!_pool.TryDequeue(out T item))
            {
                item = Create();
                Console.Error.WriteLine($"Created a new object of type: {item.GetType().FullName}, initial pool size may be too small");
            }
            if (_captureLeaks)
                _loanedReferences.Add(item);

            return item;
        }

        public void Return(T item)
        {
            if (_captureLeaks)
                _loanedReferences.Remove(item);
            _pool.Enqueue(item);
        }
        ~ObjectPool()
        {
            if (_captureLeaks && _loanedReferences.Count > 0)
                Console.Error.WriteLine("Potential Memory leak detected.  All loaned objects must be returned.");
        }
    }
    public abstract class PooledObject<T> : IDisposable where T : PooledObject<T>, new()
    {
        private static ObjectPool<T> _pool;
        public static void SetInitialCapacity(int capacity, bool captureLeaks = false)
        {
            _pool = new ObjectPool<T>(() => new T(), capacity, captureLeaks);
        }
        public static void DeletePool()
        {
            _pool = null;
        }
        public static T Get()
        {
            return _pool.Get();
        }
        abstract protected void Reset();
        public void Dispose()
        {
            Reset();
            _pool.Return((T)this);
        }
    }
}
