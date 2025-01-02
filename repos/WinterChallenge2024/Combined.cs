
using Algorithms.GameComponent;
using Algorithms.Graph;
using Algorithms.Trees;
using Algorithms.Utility;
using GameSolution.Entities;
using GameSolution.Game;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;


class Player
{
static void Main(string[] args)
{
bool submit = false;
bool showMove = false;
MonteCarloTreeSearch search = new MonteCarloTreeSearch(!submit, mathLogCacheSize: 5000);
GameState gameState = new GameState();
string[] inputs;
inputs = Console.ReadLine().Split(' ');
int width = int.Parse(inputs[0]); 
int height = int.Parse(inputs[1]); 
Stopwatch watch = new Stopwatch();
watch.Start();
Board board = new Board(width, height);
Console.Error.WriteLine($"after board ms: {watch.ElapsedMilliseconds}");

while (true)
{
int entityCount = int.Parse(Console.ReadLine());
List<Entity> entities = new List<Entity>();
for (int i = 0; i < entityCount; i++)
{
inputs = Console.ReadLine().Split(' ');
int x = int.Parse(inputs[0]);
int y = int.Parse(inputs[1]);
string type = inputs[2];
int owner = int.Parse(inputs[3]);
int organId = int.Parse(inputs[4]);
string organDir = inputs[5];
int organParentId = int.Parse(inputs[6]);
int organRootId = int.Parse(inputs[7]);
Entity entity = Entity.GetEntity(new Point2d(x, y, board.GetNodeIndex(x, y)), Entity.GetType(type), Entity.GetOwner(owner), organId, organParentId, organRootId, Entity.GetOrganDirection(organDir));
entities.Add(entity);
}
inputs = Console.ReadLine().Split(' ');
int myA = int.Parse(inputs[0]);
int myB = int.Parse(inputs[1]);
int myC = int.Parse(inputs[2]);
int myD = int.Parse(inputs[3]);
inputs = Console.ReadLine().Split(' ');
int oppA = int.Parse(inputs[0]);
int oppB = int.Parse(inputs[1]);
int oppC = int.Parse(inputs[2]);
int oppD = int.Parse(inputs[3]);
int requiredActionsCount = int.Parse(Console.ReadLine());
int[] myProtein = new int[] { myA, myB, myC, myD };
int[] oppProtein = new int[] { oppA, oppB, oppC, oppD };
watch.Start();
board.SetEntities(entities, gameState.Turn == 0);
Console.Error.WriteLine($"after entities ms: {watch.ElapsedMilliseconds}");
gameState.SetNextTurn(board, myProtein, oppProtein);
Console.Error.WriteLine($"after turn ms: {watch.ElapsedMilliseconds}");
search.SetState(gameState, true, false);
Console.Error.WriteLine($"after state ms: {watch.ElapsedMilliseconds}");
if (showMove)
{
board.GetMoves(gameState.MyProtein, true, true);
Console.Error.WriteLine($"after moves ms: {watch.ElapsedMilliseconds}");
}
Move move = (Move)search.GetNextMove(watch, gameState.Turn > 1 ? 25 : 970, 4, 1);
Console.Error.WriteLine($"after move ms: {watch.ElapsedMilliseconds}");
if (gameState.Turn <= 1)
{
search.SetState(gameState, true, false);
Console.Error.WriteLine($"after state ms: {watch.ElapsedMilliseconds}");
}
if (!submit)
{
if (watch.ElapsedMilliseconds < 48)
{
gameState.Print();
Console.Error.WriteLine($"after print ms: {watch.ElapsedMilliseconds}");
}
}
move.Print();
Console.Error.WriteLine($"after print move ms: {watch.ElapsedMilliseconds}");
watch.Stop();
watch.Reset();
move.Output();
}
}
}



namespace Algorithms.GameComponent
{
public interface IGameState : IDisposable
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

private Dictionary<int, Dictionary<int, DistancePath>> Paths;
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
Paths = new Dictionary<int, Dictionary<int, DistancePath>>();
foreach (INode vertex in Nodes.Values)
{
InternalBuildShortestPathsFromStartNode2(vertex);
}
}
public void BuildShortestPathsFromStartNode(INode startNode, double maxDistance = double.MaxValue)
{
Paths = new Dictionary<int, Dictionary<int, DistancePath>>();
InternalBuildShortestPathsFromStartNode2(startNode, maxDistance);
}

private void InternalBuildShortestPathsFromStartNode2(INode startNode, double maxDistance = double.MaxValue)
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
Paths[startNode.Id] = new Dictionary<int, DistancePath>();
Paths[startNode.Id][startNode.Id] = new DistancePath(0.0, new List<ILink>());
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

if (!Paths[startNode.Id].TryGetValue(currentNode.Id, out DistancePath currentPath))
{
currentPath = new DistancePath(0.0, new List<ILink>());
}
else
{
currentPath = new DistancePath(currentDist, new List<ILink>(currentPath.Paths)); 
}

currentPath.Paths.Add(bestLink);

Paths[startNode.Id][bestLink.EndNodeId] = currentPath;

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
Paths.TryGetValue(startNode.Id, out Dictionary<int, DistancePath> endPoints);
if (endPoints == null)
{
Console.Error.WriteLine("|||Start not found: " + startNode.Id);
throw new InvalidOperationException();
}
endPoints.TryGetValue(endNode.Id, out DistancePath paths);
if (paths == null)
{
Console.Error.WriteLine("|||End not found: " + endNode.Id + " start: " + startNode.Id);
throw new InvalidOperationException();
}
INode shortest = Nodes[paths.Paths.First().EndNodeId];
Console.Error.WriteLine("|||Shortest: " + shortest + " from: " + startNode.Id + " to: " + endNode.Id);
return shortest;
}







public IList<ILink> GetShortestPathAll(int startNodeId, int endNodeId)
{
Paths.TryGetValue(startNodeId, out Dictionary<int, DistancePath> endPoints);
if (endPoints == null)
{
Console.Error.WriteLine("|||Start not found: " + startNodeId);
throw new InvalidOperationException();
}
endPoints.TryGetValue(endNodeId, out DistancePath paths);
if (paths == null)
{
Console.Error.WriteLine("|||End not found: " + endNodeId + " start: " + startNodeId);
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

if (!Paths.TryGetValue(startId, out Dictionary<int, DistancePath> endPoints) || endPoints == null)
{
return double.MaxValue;
}

if (!endPoints.TryGetValue(endId, out DistancePath path) || path == null)
{
return double.MaxValue;
}
return path.Distance;
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
public class GameTreeNode : PooledObject<GameTreeNode>
{
public IGameState state;
public IList moves;
public List<GameTreeNode> children = new List<GameTreeNode>(50);
public double wins = 0;
public double loses = 0;
public int totalPlays = 0;
public GameTreeNode parent;
public bool isMax;
static GameTreeNode()
{
SetInitialCapacity(100000);
}
public GameTreeNode()
{
}
public static GameTreeNode GetGameTreeNode(IGameState state, bool isMax, GameTreeNode parent = null)
{
GameTreeNode node = Get();
node.state = state;
node.moves = node.state.GetPossibleMoves(isMax);
node.isMax = isMax;
node.parent = parent;
return node;
}
protected override void Reset()
{
state.Dispose();
moves.Clear();
wins = 0;
loses = 0;
totalPlays = 0;
foreach (GameTreeNode childNode in children)
{
childNode.Dispose();
}
children.Clear();
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
using (IGameState clonedState = childNode.state.Clone())
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
GameTreeNode child = RootNode.GetChild(i);
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
_queue.Enqueue(node.GetChild(i));
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
_queue.Enqueue(tempNode.GetChild(i));
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
_ = new GameTreeNode();
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
RootNode.Dispose();
RootNode = GameTreeNode.GetGameTreeNode(rootState.Clone(), isMax);
}
else
{
RootNode.parent = null;
}
}
else
{
if (RootNode != null)
RootNode.Dispose();
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
private readonly Queue<T> _pool;
private readonly Func<T> _objectGenerator;
private readonly bool _captureLeaks;
private readonly HashSet<T> _loanedReferences;
public ObjectPool(Func<T> objectGenerator, int initialSize = 0, bool captureLeaks = false)
{
_captureLeaks = captureLeaks;
if (captureLeaks)
_loanedReferences = new HashSet<T>(initialSize);
_objectGenerator = objectGenerator;
_pool = new Queue<T>(initialSize);

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

namespace GameSolution.Entities
{
public class Board : PooledObject<Board>
{
public int Width { get; private set; }
public int Height { get; private set; }
private Entity[] Entities { get; set; }
public int GlobalOrganId { get; set; } = -1;
public Graph Graph { get; set; }
public static OrganDirection[] PossibleDirections = new OrganDirection[] { OrganDirection.North, OrganDirection.South, OrganDirection.East, OrganDirection.West };
static Board()
{
SetInitialCapacity(20000);
}
public Board()
{
}
public Board(int width, int height)
{
Width = width;
Height = height;
Entities = new Entity[Width * Height];
Graph = new Graph();
InitializeBoard();
}
protected override void Reset()
{
for (int i = 0; i < Entities.Length; i++)
{
Entities[i]?.Dispose();
}
UpdateBoard();
}
public Board CopyFrom(Board board)
{
Width = board.Width;
Height = board.Height;
if (Entities != null)
{
Array.Copy(board.Entities, Entities, board.Entities.Length);
}
else
{
Entities = board.Entities.ToArray();
}
GlobalOrganId = board.GlobalOrganId;
Graph = board.Graph;
foreach (string key in board._entityCache.Keys)
{
_entityCache[key] = board._entityCache[key];
}
foreach (string key in board._moveActionCache.Keys)
{
_moveActionCache[key] = board._moveActionCache[key];
}
foreach (string key in board._locationCheckCache.Keys)
{
_locationCheckCache[key] = board._locationCheckCache[key];
}
_myEntityCount = board._myEntityCount;
_oppEntityCount = board._oppEntityCount;
_locationCache = board._locationCache;
_locationIndexCache = board._locationIndexCache;
_locationNeighbors = board._locationNeighbors;
_intToStringCache = board._intToStringCache;
_isOpenSpaceInitial = board._isOpenSpaceInitial;
return this;
}
public bool Equals(Board board)
{
if (Width != board.Width || Height != board.Height)
return false;
for (int x = 0; x < Width; x++)
{
for (int y = 0; y < Height; y++)
{
board.GetEntity(GetNodeIndex(x, y), out Entity entity);
GetEntity(GetNodeIndex(x, y), out Entity currentEntity);
if (entity == null && currentEntity == null)
continue;
if (entity == null && currentEntity != null)
return false;
if (entity != null && currentEntity == null)
return false;
else if (!entity.Equals(currentEntity))
return false;
}
}
return true;
}
public void Attack()
{
List<Entity> tentacles = GetTentacleEntities();
List<Entity> deadEntities = new List<Entity>();
foreach (Entity tentacle in tentacles)
{
if (GetEntityWithDirection(tentacle.Location, tentacle.OrganDirection, out Entity entity) && entity.IsMine.HasValue && entity.IsMine != tentacle.IsMine)
{
deadEntities.Add(entity);
}
}
foreach (Entity deadEntity in deadEntities)
{
if (deadEntity.Type == EntityType.ROOT)
{
IEnumerable<Entity> deathToRoot = GetEntitiesList().Where(e => e.OrganRootId == deadEntity.OrganId);
foreach (Entity kill in deathToRoot)
{
Entities[kill.Location.index]?.Dispose();
Entities[kill.Location.index] = null;
}
}
else
{
IEnumerable<Entity> deathToChildren = GetEntitiesList().Where(e => e.OrganParentId == deadEntity.OrganId);
foreach (Entity kill in deathToChildren)
{
Entities[kill.Location.index]?.Dispose();
Entities[kill.Location.index] = null;
}
}
}
}
public void ApplyMove(Move myMove, Move oppMove)
{
foreach (MoveAction action in myMove.Actions)
{
if (action.Type != MoveType.WAIT)
{
IEnumerable<MoveAction> collisionActions = oppMove.Actions.Where(a => (a.Type == MoveType.GROW || a.Type == MoveType.SPORE) && a.Location.Equals(action.Location));
if (collisionActions.Count() > 0)
{
Entities[action.Location.index]?.Dispose();
Entities[action.Location.index] = Entity.GetEntity(action.Location, EntityType.WALL, null, 0, 0, 0, OrganDirection.None);
}
else
{
ApplyAction(action, true);
}
}
}
foreach (MoveAction action in oppMove.Actions)
{
ApplyAction(action, false);
}
}
public void ApplyAction(MoveAction action, bool isMine)
{
switch (action.Type)
{
case MoveType.GROW:
Entity growEntity = GetEntityByLocation(action.Location);
if (growEntity == null || growEntity.IsOpenSpace())
{
Entities[action.Location.index]?.Dispose();
Entities[action.Location.index] = Entity.GetEntity(action.Location, action.EntityType, isMine, GlobalOrganId++, action.OrganId, action.OrganRootId, action.OrganDirection);
}
break;
case MoveType.SPORE:
Entity sporeEntity = GetEntityByLocation(action.Location);
if (sporeEntity == null || sporeEntity.IsOpenSpace())
{
int organId = GlobalOrganId++;
Entities[action.Location.index]?.Dispose();
Entities[action.Location.index] = Entity.GetEntity(action.Location, action.EntityType, isMine, organId, organId, organId, action.OrganDirection);
}
break;
}
}
public class ProteinInfo
{
public int[] Proteins;
public bool[] HasProteins;
public bool HasHarvestProteins;
public bool HasBasicProteins;
public bool HasTentacleProteins;
public bool HasSporerProteins;
public bool HasRootProteins;
public bool[] HasManyProteins;
public bool HasManyRootProteins;
public bool HasManyTentacleProteins;
public bool HasManySporerProteins;
public bool HasAtLeastTwoMany;
public ProteinInfo(int[] proteins)
{
Proteins = proteins;
HasProteins = proteins.Select(p => p > 0).ToArray();
HasHarvestProteins = HasProteins[2] && HasProteins[3];
HasBasicProteins = HasProteins[0];
HasTentacleProteins = HasProteins[1] && HasProteins[2];
HasSporerProteins = HasProteins[1] && HasProteins[3];
HasRootProteins = HasProteins.All(m => m);
HasManyProteins = proteins.Select(p => p > 10).ToArray();
HasManyRootProteins = HasManyProteins.All(m => m);
HasManyTentacleProteins = HasManyProteins[1] && HasManyProteins[2];
HasManySporerProteins = HasManyProteins[1] && HasManyProteins[3];
HasAtLeastTwoMany = HasManyProteins.Count(value => value) > 1;
}
}

readonly int[] _maxActionsPerOrganism = new int[5] { 10, 3, 2, 1, 1 };
Stopwatch _watch = new Stopwatch();
public List<Move> GetMoves(int[] proteins, bool isMine, bool debug = false)
{
_watch.Reset();
_watch.Start();
List<Move> moves = new List<Move>();
List<Entity> rootEntities = GetRootEntities(isMine);
int organismCount = rootEntities.Count;
bool[] organismHasMoves = new bool[organismCount];
MoveAction[][] organismToMoveActions = new MoveAction[organismCount][];
ProteinInfo proteinInfo = new ProteinInfo(proteins);
int i = 0;
int maxOrgans = 5;
List<Entity> limitedRootEntities = rootEntities;
if (organismCount > maxOrgans)
{
limitedRootEntities = rootEntities.Take(maxOrgans).ToList();
}
int selectedOrgans = limitedRootEntities.Count;
bool hasSufficientProteins = (proteins[0] >= selectedOrgans || proteins[0] == 0) && (proteins[1] >= selectedOrgans || proteins[1] == 0) && (proteins[2] >= selectedOrgans || proteins[2] == 0) && (proteins[3] >= selectedOrgans || proteins[3] == 0);
HashSet<int> locationsTaken = new HashSet<int>();
foreach (Entity root in limitedRootEntities)
{
List<MoveAction> moveActions = new List<MoveAction>();
bool sporeHasPriority = organismCount < 2 || proteinInfo.HasManyRootProteins;
if (proteinInfo.HasRootProteins && sporeHasPriority)
{
AddSporeMoveActions(moveActions, root.OrganRootId, isMine, locationsTaken);
if (debug)
Console.Error.WriteLine($"Spore: {root.OrganRootId}: " + string.Join('\n', moveActions));
if (moveActions.Count > 0)
{
moveActions = moveActions.OrderBy(m => m.Score).ToList();
if (moveActions[0].Score < 0)
{
moveActions = moveActions.Where(m => m.Score < 0).ToList();
organismHasMoves[i] = true;
}
}
}
if (moveActions.Count <= 0 || !sporeHasPriority)
{
AddGrowMoveActions(moveActions, root.OrganRootId, isMine, proteinInfo, locationsTaken, debug);
if (moveActions.Count > 0)
{
moveActions = moveActions.OrderBy(m => m.Score).Take(_maxActionsPerOrganism[limitedRootEntities.Count - 1]).ToList();
if (moveActions[0].Score < 0)
{
moveActions = moveActions.Where(m => m.Score < 0).ToList();
organismHasMoves[i] = true;
}
}
}
locationsTaken.UnionWith(moveActions.Select(m => m.Location.index));
if (organismCount == 1 && moveActions.Count > 0)
{
}
else
{
if (moveActions.Count > 0 && hasSufficientProteins)
{
}
else
moveActions.Add(MoveAction.CreateWait());
}
organismToMoveActions[i] = moveActions.ToArray();
if (debug)
Console.Error.WriteLine($"{root.OrganId}: " + string.Join('\n', moveActions));
i++;
if (_watch.ElapsedMilliseconds > 5)
{
Console.Error.WriteLine("Move generation took too long");
break;
}
}
_watch.Stop();
for (; i < organismCount; i++)
{
organismToMoveActions[i] = new MoveAction[]
{
MoveAction.CreateWait()
};
}
if (organismCount > 1)
{

bool hasAtLeastOneMove = organismHasMoves.Any(m => m);
if (hasAtLeastOneMove)
{
for (int a = 0; a < organismCount; a++)
{
if (!organismHasMoves[a])
{
if (debug)
Console.Error.WriteLine($"Pruning moves for {a}");
organismToMoveActions[a] = new MoveAction[]
{
MoveAction.CreateWait()
};
}
}
}
if (debug)
{
foreach (MoveAction[] organMoves in organismToMoveActions)
{
Console.Error.WriteLine($"{organMoves[0].OrganRootId}: " + string.Join('\n', organMoves.AsEnumerable()));
}
Console.Error.WriteLine($"Proteins: " + string.Join(",", proteins));
}
IEnumerable<Move> theMoves = PrunedCartesianProduct(organismToMoveActions, hasSufficientProteins, proteins);
moves = theMoves.ToList();
if (debug)
Console.Error.WriteLine($"Final set: {moves.Count}");
}
else if (organismCount == 1)
{
foreach (MoveAction action in organismToMoveActions[0])
{
Move move = new Move();
move.SetActions(new MoveAction[] { action });
moves.Add(move);
}
}
else
{

Move move = new Move();
move.SetActions(new MoveAction[]
{
MoveAction.CreateWait()
});
moves.Add(move);
}
if (moves.Count == 0)
{
if (!debug)
GetMoves(proteins, isMine, true);
}
return moves;
}
public bool ValidateCost(int[] proteins, Move move)
{
int[] theCosts = move.GetCost();
for (int i = 0; i < 4; i++)
{
if (proteins[i] < theCosts[i])
return false;
}
return true;
}

public IEnumerable<Move> PrunedCartesianProduct(MoveAction[][] sequences, bool hasSufficientProteins, int[] proteins)
{
if (sequences == null || sequences.Length == 0)
yield break;
MoveAction[][] sequenceArrays = sequences;
int dimensions = sequenceArrays.Length;

int[] indices = new int[dimensions];

int[][] partialCosts = new int[dimensions][];
HashSet<int>[] partialCollision = new HashSet<int>[dimensions];
for (int i = 0; i < dimensions; i++)
{
partialCosts[i] = new int[4]; 
partialCollision[i] = new HashSet<int>();
}
int[] initialCost = new int[4] { 0, 0, 0, 0 };
HashSet<int> initialCollision = new HashSet<int>();
MoveAction[] currentCombination = new MoveAction[dimensions];
int position = 0;
while (true)
{
bool hasCollision = false;
bool hasProteins = true;

int i;
for (i = position; i < dimensions; i++)
{

if (i > 0)
{
Array.Copy(partialCosts[i - 1], partialCosts[i], partialCosts[i - 1].Length);
partialCollision[i] = new HashSet<int>(partialCollision[i - 1]);
}
else
{
Array.Copy(initialCost, partialCosts[i], initialCost.Length);
partialCollision[i] = new HashSet<int>(initialCollision);
}
currentCombination[i] = sequenceArrays[i][indices[i]];

if (!hasSufficientProteins)
{
int[] cost = currentCombination[i].GetCost();
for (int j = 0; j < proteins.Length; j++)
{
partialCosts[i][j] += cost[j];
if (partialCosts[i][j] > proteins[j])
{
hasProteins = false;
break;
}
}
if (!hasProteins)
{
break;
}
}

if (ValidateLocation(currentCombination[i].Location))
{
if (!partialCollision[i].Add(currentCombination[i].Location.index))
{
hasCollision = true;
break;
}
}
}

if (!hasCollision && (hasSufficientProteins || hasProteins))
{
Move move = new Move();
move.SetActions(currentCombination.ToArray());
yield return move;
position = dimensions - 1;
}
else
{
if (position < i)
position = i;
}

while (position >= 0)
{
indices[position]++;
if (indices[position] < sequenceArrays[position].Length)
break;

indices[position] = 0;
position--;
}

if (position < 0)
break;
}
}
public List<MoveAction> AddSporeMoveActions(List<MoveAction> moveActions, int organRootId, bool isMine, HashSet<int> locationsTaken)
{
IEnumerable<Entity> sporers = GetSporerEntities(organRootId, isMine);
foreach (Entity sporer in sporers)
{
Point2d location = sporer.Location;
int distance = 0;
while (true)
{
distance++;
location = GetNextLocation(location, sporer.OrganDirection);
if (ValidateLocation(location) && IsOpenSpace(location.index, isMine))
{
if (distance <= 3 || locationsTaken.Contains(location.index))
{
continue;
}
bool? isHarvesting = IsHarvesting(location, isMine);
MoveAction sporeMove = MoveAction.CreateSpore(sporer.OrganId, location);
moveActions.Add(sporeMove);
if (!(isHarvesting.HasValue && isHarvesting.Value) && IsHarvestExactly2spaces(location))
{
sporeMove.Score = -1000;
}
else
{
sporeMove.Score = 1000;
}
}
else
{
break;
}
if (moveActions.Count > 4)
{
break;
}
}
}
return moveActions;
}
public List<MoveAction> AddGrowMoveActions(List<MoveAction> moveActions, int organRootId, bool isMine, ProteinInfo proteinInfo, HashSet<int> locationsTaken, bool debug = false)
{
List<MoveAction> growMoveActions = GetGrowMoveActions(organRootId, isMine, locationsTaken, proteinInfo);
if (debug)
{
Console.Error.WriteLine($"All: {organRootId}: " + string.Join('\n', growMoveActions));
}
bool canHarvest = false;
List<MoveAction> harvestActions = new List<MoveAction>();
if (proteinInfo.HasHarvestProteins)
{
harvestActions = GetHarvestMoveActions(organRootId, isMine, locationsTaken, proteinInfo);
moveActions.AddRange(harvestActions);
canHarvest = harvestActions.Count > 0;
if (debug)
Console.Error.WriteLine($"Harvest: {organRootId}: " + string.Join('\n', harvestActions));
}
List<MoveAction> tentacleActions = new List<MoveAction>();
if (proteinInfo.HasTentacleProteins)
{
tentacleActions = GetTentacleMoveActions(organRootId, isMine, locationsTaken, proteinInfo);
moveActions.AddRange(tentacleActions);
if (debug)
Console.Error.WriteLine($"Tentacle: {organRootId}: " + string.Join('\n', tentacleActions));
}
if (proteinInfo.HasBasicProteins)
{
List<MoveAction> basicActions = GetGrowBasicMoveActions(organRootId, isMine, locationsTaken, proteinInfo);
if (harvestActions.Any(m => m.Score < 0))
basicActions.ForEach(m => m.Score += 100);
if (tentacleActions.Any(m => m.Score < 0))
basicActions.ForEach(m => m.Score += 100);
if (proteinInfo.HasManyTentacleProteins)
basicActions.Join(tentacleActions, m1 => m1.Location, m2 => m2.Location, (m1, m2) => m1).ToList().ForEach(a => a.Score += 100);
moveActions.AddRange(basicActions);
if (debug)
Console.Error.WriteLine($"Basic: {organRootId}: " + string.Join('\n', basicActions));
}
if (proteinInfo.HasSporerProteins)
{
List<MoveAction> sporerActions = GetSporerMoveActions(organRootId, isMine, locationsTaken, proteinInfo);
if (harvestActions.Any(m => m.Score < 0))
sporerActions.ForEach(m => m.Score += 100);
if (!proteinInfo.HasManyTentacleProteins && tentacleActions.Any(m => m.Score < 0))
sporerActions.ForEach(m => m.Score += 100);
if (!proteinInfo.HasRootProteins)
sporerActions.ForEach(m => m.Score += 100);
moveActions.AddRange(sporerActions);
if (debug)
Console.Error.WriteLine($"Sporer: {organRootId}: " + string.Join('\n', sporerActions));
}
return moveActions;
}
public List<MoveAction> GetSporerMoveActions(int organRootId, bool isMine, HashSet<int> locationsTaken, ProteinInfo proteinInfo)
{
List<MoveAction> moveActions = new List<MoveAction>();
List<MoveAction> growMoveActions = GetGrowMoveActions(organRootId, isMine, locationsTaken, proteinInfo);
foreach (MoveAction growAction in growMoveActions)
{
List<MoveAction> sporerMoves = new List<MoveAction>();
foreach (LocationNeighbor locationNeighbor in GetLocationNeighbors(growAction.Location))
{
MoveAction sporerAction = MoveAction.CreateGrow(growAction.OrganId, growAction.Location, EntityType.SPORER, growAction.OrganRootId, locationNeighbor.direction);
sporerAction.Score = growAction.Score;
Point2d location = growAction.Location;
bool isOpen = true;
for (int i = 0; i < 4; i++)
{
location = GetNextLocation(location, locationNeighbor.direction);
if (!ValidateLocation(location) || !IsOpenSpace(location.index, isMine))
{
isOpen = false;
break;
}
}
if (isOpen)
{
sporerAction.Score -= 100;
sporerMoves.Add(sporerAction);
}
}
if (sporerMoves.Count > 0)
{
moveActions.AddRange(sporerMoves);
}
else
{
MoveAction moveAction = MoveAction.CreateGrow(growAction.OrganId, growAction.Location, EntityType.SPORER, growAction.OrganRootId);
moveAction.Score = growAction.Score;
moveAction.Score += 1000;
moveActions.Add(moveAction);
}
}
return moveActions;
}
public List<MoveAction> GetTentacleMoveActions(int organRootId, bool isMine, HashSet<int> locationsTaken, ProteinInfo proteinInfo)
{
List<MoveAction> moveActions = new List<MoveAction>();
List<MoveAction> growMoveActions = GetGrowMoveActions(organRootId, isMine, locationsTaken, proteinInfo);
foreach (MoveAction growAction in growMoveActions)
{
List<MoveAction> tentacleMoveActions = new List<MoveAction>();

bool isOppIn3Spaces = IsOpponentWithin3Spaces(growAction.Location, isMine);
foreach (LocationNeighbor locationNeighbor in GetLocationNeighbors(growAction.Location))
{
MoveAction tentacleAction = MoveAction.CreateGrow(growAction.OrganId, growAction.Location, EntityType.TENTACLE, growAction.OrganRootId, locationNeighbor.direction);
tentacleAction.Score = growAction.Score;
if (IsOpponentOrEmptySpace(locationNeighbor.point.index, isMine))
{
tentacleMoveActions.Add(tentacleAction);
if (proteinInfo.HasManyTentacleProteins)
tentacleAction.Score -= 50;
if (isOppIn3Spaces)
{
tentacleAction.Score -= 50;

if (GetEntity(locationNeighbor.point.index, out Entity entity) && entity.IsMine.HasValue && entity.IsMine != isMine)
{
tentacleAction.Score -= 50000;
if (entity.Type == EntityType.ROOT && GetEntitiesByRoot(entity.OrganId, !isMine).Count > 1)
{
tentacleAction.Score -= 50000;
return new List<MoveAction> { tentacleAction };
}
}
}
}
}
if (tentacleMoveActions.Count > 0)
{
double best = tentacleMoveActions.Min(m => m.Score);
moveActions.AddRange(tentacleMoveActions.Where(m => m.Score == best));
}
else
{
MoveAction moveAction = MoveAction.CreateGrow(growAction.OrganId, growAction.Location, EntityType.TENTACLE, growAction.OrganRootId, OrganDirection.North);
moveAction.Score = growAction.Score;
moveAction.Score += 10000;
moveActions.Add(moveAction);
}
}
return moveActions;
}
private Dictionary<string, List<MoveAction>> _moveActionCache = new Dictionary<string, List<MoveAction>>();
private const int _maxMoves = 10;
public List<MoveAction> GetGrowMoveActions(int organRootId, bool isMine, HashSet<int> locationsTaken, ProteinInfo proteinInfo)
{
string key = (isMine ? "grow1_" : "grow0_") + organRootId.ToString();
if (!_moveActionCache.TryGetValue(key, out List<MoveAction> moveActions))
{
List<Entity> oppRootEntities = GetRootEntities(!isMine);
List<Entity> harvestableEntities = GetHarvestableEntities();
List<Entity> harvestingEntities = GetHarvestedEntities(isMine);
HashSet<EntityType> harvestedTypes = harvestingEntities.Select(e => e.Type).ToHashSet();
List<Entity> toHarvestEntities = harvestedTypes.Count < 4 ? harvestableEntities.Where(e => !harvestedTypes.Contains(e.Type)).ToList() : new List<Entity>();
bool hasOppRoot = oppRootEntities.Any();
bool hasHarvestable = toHarvestEntities.Any();
moveActions = new List<MoveAction>();
List<Entity> entities = GetEntitiesByRoot(organRootId, isMine);
HashSet<int> locationsChecked = new HashSet<int>(locationsTaken);
foreach (Entity entity in entities)
{
foreach (LocationNeighbor locationNeighbor in GetLocationNeighbors(entity.Location))
{
if (locationsChecked.Add(locationNeighbor.point.index) && IsOpenSpace(locationNeighbor.point.index, isMine))
{
MoveAction moveAction = MoveAction.CreateGrow(entity.OrganId, locationNeighbor.point, EntityType.NONE, entity.OrganRootId);
moveAction.Score = 0;
if (hasHarvestable)
{
moveAction.Score += toHarvestEntities.Min(r => Graph.GetShortestPathDistance(r.Location.index, moveAction.Location.index));
}
else if (hasOppRoot)
{
moveAction.Score += oppRootEntities.Min(r => Graph.GetShortestPathDistance(r.Location.index, moveAction.Location.index));
}
bool? isHarvesting = IsHarvesting(locationNeighbor.point.index, isMine);
if (isHarvesting.HasValue)
{
if (GetEntity(locationNeighbor.point.index, out Entity harvestEntity) && !proteinInfo.HasHarvestProteins && (harvestEntity.Type == EntityType.C || harvestEntity.Type == EntityType.D))
{
moveAction.Score -= 1000;
}
bool isOppIn3Spaces = IsOpponentWithin3Spaces(moveAction.Location, isMine);
if (isHarvesting.Value && !isOppIn3Spaces)
{
moveAction.Score += 1000;
}
if (isOppIn3Spaces)
{
moveAction.Score -= 20;
}
}
moveActions.Add(moveAction);
}
}
}
if (moveActions.Count > _maxMoves)
{
moveActions = moveActions.OrderBy(m => m.Score).Take(_maxMoves).ToList();
}
_moveActionCache[key] = moveActions;
}
return moveActions;
}
public List<MoveAction> GetGrowBasicMoveActions(int organRootId, bool isMine, HashSet<int> locationsTaken, ProteinInfo proteinInfo)
{
List<MoveAction> moveActions = new List<MoveAction>();
List<MoveAction> growMoveActions = GetGrowMoveActions(organRootId, isMine, locationsTaken, proteinInfo);
List<MoveAction> growBasicActions = new List<MoveAction>();
foreach (MoveAction growAction in growMoveActions)
{
MoveAction moveAction = MoveAction.CreateGrow(growAction.OrganId, growAction.Location, EntityType.BASIC, growAction.OrganRootId);
moveAction.Score = growAction.Score;
moveAction.Score -= 30;
growBasicActions.Add(moveAction);
}
if (growBasicActions.Count > 0)
{
double best = growBasicActions.Min(m => m.Score);
moveActions.AddRange(growBasicActions.Where(m => m.Score == best));
}
return moveActions;
}
public List<MoveAction> GetHarvestMoveActions(int organRootId, bool isMine, HashSet<int> locationsTaken, ProteinInfo proteinInfo)
{
List<MoveAction> moveActions = new List<MoveAction>();
List<MoveAction> growMoveActions = GetGrowMoveActions(organRootId, isMine, locationsTaken, proteinInfo);
foreach (MoveAction growAction in growMoveActions)
{
bool isOppIn3Spaces = IsOpponentWithin3Spaces(growAction.Location, isMine);
List<MoveAction> harvestActions = new List<MoveAction>();
foreach (LocationNeighbor locationNeighbor in GetLocationNeighbors(growAction.Location))
{
bool? isHarvesting = IsHarvesting(locationNeighbor.point.index, isMine);
MoveAction harvestAction = MoveAction.CreateGrow(growAction.OrganId, growAction.Location, EntityType.HARVESTER, growAction.OrganRootId, locationNeighbor.direction);
harvestAction.Score = growAction.Score;
if (isOppIn3Spaces)
{
harvestAction.Score += 1000;
}
if (isHarvesting.HasValue && !isHarvesting.Value)
{
harvestAction.Score -= 500;
}
if (harvestAction.Score < 0)
{
harvestActions.Add(harvestAction);
}
}
if (harvestActions.Count > 0)
{
moveActions.AddRange(harvestActions);
}
else
{
MoveAction moveAction = MoveAction.CreateGrow(growAction.OrganId, growAction.Location, EntityType.HARVESTER, growAction.OrganRootId);
moveAction.Score = growAction.Score;
moveAction.Score += 1000;
moveActions.Add(moveAction);
}
}
return moveActions;
}
public bool? IsHarvesting(int location, bool isMine)
{
if (IsHarvestSpace(location))
{
foreach (LocationNeighbor locationNeighbor in GetLocationNeighbors(location))
{
if (GetEntity(locationNeighbor.point.index, out Entity entity) && entity.Type == EntityType.HARVESTER && entity.IsMine.HasValue && entity.IsMine == isMine && entity.OrganDirection == GetOpposingDirection(locationNeighbor.direction))
{
return true;
}
}
return false;
}
return null;
}
public bool? IsHarvesting(Point2d location, bool isMine)
{
if (ValidateLocation(location))
{
return IsHarvesting(location.index, isMine);
}
return null;
}
public bool IsFull()
{
foreach (Entity entity in Entities)
{
if (entity == null)
{
return false;
}
}
return true;
}
public int[] GetHarvestProteins(bool isMine)
{
List<Entity> harvesters = GetHarvesterEntities(isMine);
int[] harvestedProteins = new int[4] { 0, 0, 0, 0 };
foreach (Entity harvester in harvesters)
{
if (GetEntityWithDirection(harvester.Location, harvester.OrganDirection, out Entity entity) && entity.IsOpenSpace())
{
harvestedProteins[entity.Type - EntityType.A]++;
}
}
return harvestedProteins;
}
public OrganDirection GetOpposingDirection(OrganDirection direction)
{
switch (direction)
{
case OrganDirection.North:
return OrganDirection.South;
case OrganDirection.South:
return OrganDirection.North;
case OrganDirection.East:
return OrganDirection.West;
case OrganDirection.West:
return OrganDirection.East;
}
throw new Exception("No direction given");
}
public bool IsHarvestExactly2spaces(Point2d location)
{
List<Entity> harvestEntities = GetHarvestableEntities();
foreach (Entity entity in harvestEntities)
{
if (location.Equals(entity.Location))
continue;
double distance = Graph.GetShortestPathDistance(location.index, entity.Location.index);
if (distance == 2)
{
return true;
}
}
return false;
}
Dictionary<string, bool> _locationCheckCache = new Dictionary<string, bool>();
public bool IsOpponentWithin3Spaces(Point2d location, bool isMine)
{
string key = (isMine ? "opponentclose_1" : "opponentclose_2") + location.index.ToString();
if (!_locationCheckCache.TryGetValue(key, out bool result))
{
List<Entity> oppEntities = GetEntities(!isMine);
foreach (Entity oppEntity in oppEntities)
{
double distance = Graph.GetShortestPathDistance(location.index, oppEntity.Location.index);
if (distance <= 3)
{
result = true;
_locationCheckCache[key] = result;
return result;
}
}
result = false;
_locationCheckCache[key] = result;
}
return result;
}
public bool IsOpponentOrEmptySpace(int index, bool isMine)
{
return !GetEntity(index, out Entity entity) || entity.IsOpenSpace() || (entity.IsMine.HasValue && entity.IsMine != isMine);
}
public bool IsOpponentSpace(Point2d location, OrganDirection direction, bool isMine)
{
return GetEntityWithDirection(location, direction, out Entity entity) && entity.IsMine != isMine;
}
public bool IsHarvestSpace(int location)
{
return GetEntity(location, out Entity entity) && entity.IsOpenSpace();
}
public bool IsHarvestSpace(Point2d location, OrganDirection nextDirection)
{
return GetEntityWithDirection(location, nextDirection, out Entity entity) && entity.IsOpenSpace();
}
private bool IsOpenSpace(int location)
{
return !GetEntity(location, out Entity entity) || entity.IsOpenSpace();
}
private string[] _intToStringCache;
private bool[] _isOpenSpaceInitial;
public bool IsOpenSpace(int location, bool isMine)
{
if (!_isOpenSpaceInitial[location] || !IsOpenSpace(location))
{
return false;
}
string key = $"{(isMine ? "openspace_1" : "openspace_2")}{_intToStringCache[location]}";

if (_locationCheckCache.TryGetValue(key, out bool result))
{
return result;
}
result = !HasOpposingTentacle(location, isMine);

_locationCheckCache[key] = result;
return result;
}

private bool HasOpposingTentacle(int location, bool isMine)
{
foreach (LocationNeighbor neighbor in GetLocationNeighbors(location))
{
if (GetEntity(neighbor.point.index, out Entity entity) &&
entity.Type == EntityType.TENTACLE &&
entity.IsMine != isMine &&
GetOpposingDirection(entity.OrganDirection) == neighbor.direction)
{
return true;
}
}
return false;
}
public bool ValidateLocation(Point2d location, OrganDirection nextDirection)
{
return ValidateLocation(GetNextLocation(location, nextDirection));
}
public bool ValidateLocation(Point2d location)
{
return location != null;
}
public bool GetEntityWithDirection(Point2d currentLocation, OrganDirection nextDirection, out Entity entity)
{
Point2d location = GetNextLocation(currentLocation, nextDirection);
entity = null;
return ValidateLocation(location) && GetEntity(location, out entity);
}
public Point2d GetNextLocation(Point2d currentLocation, OrganDirection nextDirection)
{
return _locationCache[(int)currentLocation.x + 1][(int)currentLocation.y + 1][(int)nextDirection];
}
private Point2d GetNextLocationInternl(int x, int y, OrganDirection nextDirection)
{
switch (nextDirection)
{
case OrganDirection.North:
return new Point2d(x, y - 1, GetNodeIndexInternal(x, y - 1));
case OrganDirection.South:
return new Point2d(x, y + 1, GetNodeIndexInternal(x, y + 1));
case OrganDirection.East:
return new Point2d(x + 1, y, GetNodeIndexInternal(x + 1, y));
case OrganDirection.West:
return new Point2d(x - 1, y, GetNodeIndexInternal(x - 1, y));
default:
return new Point2d(x, y, GetNodeIndexInternal(x, y));
}
}
public void Harvest(bool isMine, int[] proteins)
{
foreach (Entity entity in GetHarvestedEntities(isMine))
{
proteins[entity.Type - EntityType.A]++;
}
}
public List<Entity> GetHarvestedEntities(bool isMine)
{
string key = isMine ? "harvested_1" : "harvested_0";
if (!_entityCache.TryGetValue(key, out List<Entity>? entities))
{
entities = new List<Entity>();
List<Entity> harvesters = GetHarvesterEntities(isMine);
HashSet<int> harvestedLocations = new HashSet<int>();
foreach (Entity harvester in harvesters)
{
if (GetEntityWithDirection(harvester.Location, harvester.OrganDirection, out Entity entity) && entity.IsOpenSpace() && harvestedLocations.Add(entity.Location.index))
{
entities.Add(entity);
}
}
_entityCache[key] = entities;
}
return entities;
}
public bool GetEntity(int entityIndex, out Entity entity)
{
entity = Entities[entityIndex];
return entity != null;
}
public bool GetEntity(Point2d location, out Entity entity)
{
return GetEntity(GetNodeIndex(location), out entity);
}
public List<Entity> GetHarvestableEntities()
{
string key = "harvestable";
if (!_entityCache.TryGetValue(key, out List<Entity>? entities))
{
entities = GetEntitiesList().Where(e => e.IsOpenSpace()).ToList();
_entityCache[key] = entities;
}
return entities;
}
public bool GetEntityByLocation(Point2d location, out Entity entity)
{
return GetEntity(location, out entity);
}
public Entity GetEntityByLocation(Point2d location)
{
Entity entity = null;
GetEntity(location, out entity);
return entity;
}
public List<Entity> GetHarvesterEntities(bool isMine)
{
string key = isMine ? "harvest1" : "harvest0";
if (!_entityCache.TryGetValue(key, out List<Entity>? entities))
{
entities = GetEntities(isMine).Where(e => e.Type == EntityType.HARVESTER).ToList();
_entityCache[key] = entities;
}
return entities;
}
public List<Entity> GetRootEntities(bool isMine)
{
string key = isMine ? "root1" : "root0";
if (!_entityCache.TryGetValue(key, out List<Entity>? entities))
{
entities = GetEntities(isMine).Where(e => e.Type == EntityType.ROOT).OrderBy(e => e.OrganId).ToList();
_entityCache[key] = entities;
}
return entities;
}
public List<Entity> GetSporerEntities(int organRootId, bool isMine)
{
string key = (isMine ? "sporer1" : "sporer0") + organRootId.ToString();
if (!_entityCache.TryGetValue(key, out List<Entity>? entities))
{
entities = GetEntitiesByRoot(organRootId, isMine).Where(e => e.Type == EntityType.SPORER).ToList();
_entityCache[key] = entities;
}
return entities;
}
public List<Entity> GetTentacleEntities(bool isMine)
{
string key = isMine ? "tentacles1" : "tentacles0";
if (!_entityCache.TryGetValue(key, out List<Entity>? entities))
{
entities = GetTentacleEntities().Where(e => e.IsMine == isMine).ToList();
_entityCache[key] = entities;
}
return entities;
}
public List<Entity> GetTentacleEntities()
{
string key = "tentacles";
if (!_entityCache.TryGetValue(key, out List<Entity>? entities))
{
entities = GetEntitiesList().Where(e => e.Type == EntityType.TENTACLE).ToList();
_entityCache[key] = entities;
}
return entities;
}
public List<Entity> GetEntitiesList()
{
string key = "all";
if (!_entityCache.TryGetValue(key, out List<Entity> entities))
{
entities = Entities.Where(e => e != null).Select(e => e).ToList();
_entityCache[key] = entities;
}
return entities;
}
private Dictionary<string, List<Entity>> _entityCache = new Dictionary<string, List<Entity>>();
public List<Entity> GetEntities(bool isMine)
{
string key = isMine ? "e1" : "e0";
if (!_entityCache.TryGetValue(key, out List<Entity> entities))
{
entities = GetEntitiesList().Where(e => e.IsMine.HasValue && e.IsMine == isMine).ToList();
_entityCache[key] = entities;
}
return entities;
}
public List<Entity> GetEntitiesByRoot(int organRootId, bool isMine)
{
string key = $"{organRootId}_{isMine}";
if (!_entityCache.TryGetValue(key, out List<Entity> entities))
{
entities = GetEntities(isMine).Where(e => e.OrganRootId == organRootId).ToList();
_entityCache[key] = entities;
}
return entities;
}
public Entity[] GetEntities()
{
return Entities;
}
private int _myEntityCount = -1;
public int GetMyEntityCount()
{
if (_myEntityCount < 0)
{
_myEntityCount = GetEntities(true).Count;
}
return _myEntityCount;
}
private int _oppEntityCount = -1;
public int GetOppEntityCount()
{
if (_oppEntityCount < 0)
{
_oppEntityCount = GetEntities(false).Count;
}
return _oppEntityCount;
}
public void SetEntities(IList<Entity> entities, bool isFirstTurn = false)
{
Array.Clear(Entities);
foreach (Entity entity in entities)
{
Entities[entity.Location.index]?.Dispose();
Entities[entity.Location.index] = entity;
if (entity.OrganId > 0 && GlobalOrganId <= entity.OrganId)
{
GlobalOrganId = entity.OrganId + 1;
}
}
UpdateBoard();
if (isFirstTurn)
{
CalculatePathsAndNeighbors();
}
}
public Board Clone()
{
Board cleanState = Get();
cleanState.CopyFrom(this);
return cleanState;
}
public double? GetWinner()
{
return null;
}
public void Print()
{
for (int y = 0; y < Height; y++)
{
StringBuilder stringBuilder = new StringBuilder();
for (int x = 0; x < Width; x++)
{
if (!GetEntity(GetNodeIndex(x, y), out Entity entity))
{
stringBuilder.Append(' ');
}
else
{
stringBuilder.Append(GetCharacter(entity.Type, entity.IsMine));
}
}
Console.Error.WriteLine(stringBuilder.ToString());
}
}
public char GetCharacter(EntityType type, bool? isMine)
{
bool isMineInt = isMine.HasValue && isMine.Value;
switch (type)
{
case EntityType.WALL:
return 'X';
case EntityType.ROOT:
return isMineInt ? 'R' : 'r';
case EntityType.BASIC:
return isMineInt ? 'B' : 'b';
case EntityType.TENTACLE:
return isMineInt ? 'T' : 't';
case EntityType.HARVESTER:
return isMineInt ? 'H' : 'h';
case EntityType.SPORER:
return isMineInt ? 'S' : 's';
case EntityType.A:
return 'A';
case EntityType.B:
return 'B';
case EntityType.C:
return 'C';
case EntityType.D:
return 'D';
}
throw new ArgumentException($"Type: {type} not supported");
}
public void UpdateBoard()
{
_myEntityCount = -1;
_oppEntityCount = -1;
_moveActionCache.Clear();
_entityCache.Clear();
_locationCheckCache.Clear();
}
public readonly struct LocationNeighbor
{
public readonly Point2d point;
public readonly OrganDirection direction;
public LocationNeighbor(int x, int y, int index, OrganDirection direction)
{
this.point = new Point2d(x, y, index);
this.direction = direction;
}
}
private Point2d[][][] _locationCache;
private int[][] _locationIndexCache = null;
private List<LocationNeighbor>[] _locationNeighbors = null;
public void InitializeBoard()
{
_intToStringCache = new string[Width * Height];
_locationIndexCache = new int[Width][];
for (int x = 0; x < Width; x++)
{
_locationIndexCache[x] = new int[Height];
for (int y = 0; y < Height; y++)
{
int index = GetNodeIndexInternal(x, y);
_locationIndexCache[x][y] = index;
_intToStringCache[index] = index.ToString();
}
}
_locationCache = new Point2d[Width + 2][][];
for (int x = 0; x < Width + 2; x++)
{
_locationCache[x] = new Point2d[Height + 2][];
for (int y = 0; y < Height + 2; y++)
{
_locationCache[x][y] = new Point2d[4];
for (int z = 0; z < 4; z++)
{
Point2d nextLocation = GetNextLocationInternl(x - 1, y - 1, PossibleDirections[z]);
if (nextLocation.x > -1 && nextLocation.y > -1 && nextLocation.x < Width && nextLocation.y < Height)
{
_locationCache[x][y][z] = nextLocation;
}
else
{
_locationCache[x][y][z] = null;
}
}
}
}
}
private void CalculatePathsAndNeighbors()
{
_isOpenSpaceInitial = new bool[Width * Height];
Array.Fill(_isOpenSpaceInitial, true);
_locationNeighbors = new List<LocationNeighbor>[Width * Height];
for (int x = 0; x < Width; x++)
{
for (int y = 0; y < Height; y++)
{
int index = GetNodeIndex(x, y);
Node node = new Node(index);
Graph.AddNode(node);
_locationNeighbors[index] = new List<LocationNeighbor>();
for (int z = 0; z < 4; z++)
{
Point2d location = GetNextLocation(new Point2d(x, y, GetNodeIndex(x, y)), PossibleDirections[z]);
if (ValidateLocation(location))
{
if (IsOpenSpace(location.index) || (GetEntity(location.index, out Entity entity) && entity.Type != EntityType.WALL))
{
int nextIndex = GetNodeIndex(location);
node.AddLink(new Link(node, new Node(nextIndex), 1));
_locationNeighbors[index].Add(new LocationNeighbor(location.x, location.y, nextIndex, PossibleDirections[z]));
}
else _isOpenSpaceInitial[location.index] = false;
}
}
}
}
Graph.CalculateShortestPaths();
}
private int GetNodeIndexInternal(int x, int y)
{
return x * Height + y;
}
public List<LocationNeighbor> GetLocationNeighbors(int nodeIndex)
{
return _locationNeighbors[nodeIndex];
}
public List<LocationNeighbor> GetLocationNeighbors(Point2d location)
{
return _locationNeighbors[location.index];
}
public int GetNodeIndex(int x, int y)
{
return _locationIndexCache[x][y];
}
public int GetNodeIndex(Point2d location)
{
return location.index;
}
}
}

namespace GameSolution.Entities
{

public enum EntityType
{
WALL = 0,
NONE,
ROOT,
BASIC,
TENTACLE,
HARVESTER,
SPORER,
A,
B,
C,
D,
}
public enum OrganDirection
{
North = 0,
South,
East,
West,
None
}
public class Entity : PooledObject<Entity>
{
public bool? IsMine { get; private set; }
public Point2d Location { get; private set; }
public EntityType Type { get; private set; }
public int OrganId { get; private set; }
public OrganDirection OrganDirection { get; private set; }
public int OrganParentId { get; private set; }
public int OrganRootId { get; private set; }
private bool _IsOpenSpace { get; set; }
static Entity()
{
SetInitialCapacity(100000);
}
public Entity()
{
}
public Entity(int x, int y, int index, string type, int owner, int organId, string organDir, int organParentId, int organRootId)
{
Location = new Point2d(x, y, index);
IsMine = GetOwner(owner);
Type = GetType(type);
OrganDirection = GetOrganDirection(organDir);
OrganId = organId;
OrganParentId = organParentId;
OrganRootId = organRootId;
_IsOpenSpace = IsOpenSpaceInternal();
}
public static bool? GetOwner(int owner)
{
return owner == 1 ? true : owner == -1 ? (bool?)null : false;
}
public Entity(Point2d location, EntityType type, bool? isMine, int organId, int organParentId, int organRootId, OrganDirection organDirection)
{
Location = location;
IsMine = isMine;
Type = type;
OrganId = organId;
OrganParentId = organParentId;
OrganRootId = organRootId;
OrganDirection = organDirection;
_IsOpenSpace = IsOpenSpaceInternal();
}
protected override void Reset()
{
}
public static Entity GetEntity(Point2d location, EntityType type, bool? isMine, int organId, int organParentId, int organRootId, OrganDirection organDirection)
{
Entity entity = Get();
entity.Location = location;
entity.IsMine = isMine;
entity.Type = type;
entity.OrganId = organId;
entity.OrganParentId = organParentId;
entity.OrganRootId = organRootId;
entity.OrganDirection = organDirection;
entity._IsOpenSpace = entity.IsOpenSpaceInternal();
return entity;
}
public Entity(Entity entity)
{
this.IsMine = entity.IsMine;
this.Type = entity.Type;
this.OrganDirection = entity.OrganDirection;
this.Location = entity.Location.Clone();
this.OrganId = entity.OrganId;
this.OrganParentId = entity.OrganParentId;
this.OrganRootId = entity.OrganRootId;
_IsOpenSpace = entity._IsOpenSpace;
}
public static OrganDirection GetOrganDirection(string organDir)
{
switch (organDir)
{
case "N":
return OrganDirection.North;
case "E":
return OrganDirection.East;
case "W":
return OrganDirection.West;
case "S":
return OrganDirection.South;
case "X":
return OrganDirection.None;
}
throw new ArgumentException($"Invalid direction: {organDir}", nameof(organDir));
}
public static EntityType GetType(string type)
{
switch (type)
{
case "WALL":
return EntityType.WALL;
case "ROOT":
return EntityType.ROOT;
case "BASIC":
return EntityType.BASIC;
case "TENTACLE":
return EntityType.TENTACLE;
case "HARVESTER":
return EntityType.HARVESTER;
case "SPORER":
return EntityType.SPORER;
case "A":
return EntityType.A;
case "B":
return EntityType.B;
case "C":
return EntityType.C;
case "D":
return EntityType.D;
}
throw new ArgumentException($"Invalid type: {type}", nameof(type));
}
public bool IsOpenSpace()
{
return _IsOpenSpace;
}
private bool IsOpenSpaceInternal()
{
return Type == EntityType.A || Type == EntityType.B || Type == EntityType.C || Type == EntityType.D;
}
public Entity Clone()
{
return new Entity(this);
}
public bool Equals(Entity entity)
{
return entity.IsMine == IsMine && entity.OrganParentId == OrganParentId && entity.OrganRootId == OrganRootId && entity.Type == Type && entity.Location.Equals(Location) && entity.OrganId == OrganId && entity.OrganDirection == OrganDirection;
}
public override string ToString()
{
return "";
}
}
}

namespace GameSolution.Entities
{
public enum MoveType
{
WAIT = 0,
GROW,
SPORE
};
public class MoveAction
{
public MoveType Type { get; set; }
public int OrganId { get; set; }
public Point2d Location { get; set; }
public EntityType EntityType { get; set; }
public int OrganRootId { get; set; }
public double Score { get; set; }
public OrganDirection OrganDirection { get; set; }
public MoveAction(MoveType moveType)
{
Type = moveType;
}
public static int[][] EntityCosts = new int[][]
{
  new int[]{0, 0, 0, 0 },
  new int[]{1, 1, 1, 1},
  new int[]{ 1, 0, 0, 0 },
  new int[]{0, 1, 1, 0},
  new int[]{0, 0, 1, 1 },
  new int[]{0, 1, 0, 1 }
};
public int[] GetCost()
{
return EntityCosts[EntityType - EntityType.NONE];
}
public static MoveAction CreateGrow(int organId, Point2d location, EntityType type, int organRootId, OrganDirection organDirection = OrganDirection.North)
{
MoveAction action = new MoveAction(MoveType.GROW);
action.OrganId = organId;
action.Location = location;
action.EntityType = type;
action.OrganRootId = organRootId;
action.OrganDirection = organDirection;
return action;
}
public static MoveAction CreateWait()
{
MoveAction action = new MoveAction(MoveType.WAIT);
action.EntityType = EntityType.NONE;
action.Score = -9999;
return action;
}
public static MoveAction CreateSpore(int sporeOrganId, Point2d location)
{
MoveAction action = new MoveAction(MoveType.SPORE);
action.OrganId = sporeOrganId;
action.Location = location;
action.EntityType = EntityType.ROOT;
return action;
}
public override string ToString()
{
MoveAction move = this;
switch (this.Type)
{
case MoveType.GROW:
return $"GROW {move.OrganId} {move.Location.x} {move.Location.y} {move.EntityType} {GetGrowDirection(move.OrganDirection)} {move.Score}";
case MoveType.SPORE:
return $"SPORE {move.OrganId} {move.Location.x} {move.Location.y} {move.Score}";
case MoveType.WAIT:
return $"WAIT {move.Score}";
}
return "";
}
public char GetGrowDirection(OrganDirection direction)
{
switch (direction)
{
case OrganDirection.North:
return 'N';
case OrganDirection.South:
return 'S';
case OrganDirection.West:
return 'W';
case OrganDirection.East:
return 'E';
}
throw new Exception("Invalid direction");
}
}
public class Move
{
public MoveAction[] Actions { get; set; }
public Move()
{
}
public Move(Move move)
{
Actions = move.Actions.Select(m => m).ToArray();
}
private int[] _costs = null;
public int[] GetCost()
{
if (_costs == null)
{
_costs = new int[] { 0, 0, 0, 0 };
foreach (MoveAction action in Actions)
{
int[] actionCost = action.GetCost();
for (int i = 0; i < 4; i++)
{
_costs[i] += actionCost[i];
}
}
}
return _costs;
}
public void SetActions(MoveAction[] actions)
{
Actions = actions;
}
public Move Clone()
{
return new Move(this);
}
public override bool Equals(object obj)
{

if (ReferenceEquals(this, obj))
return true;

if (obj is Move otherMove)
{

if (this.Actions == null && otherMove.Actions == null)
return true;
if (this.Actions == null || otherMove.Actions == null)
return false;

if (this.Actions.Length != otherMove.Actions.Length)
return false;
for (int i = 0; i < this.Actions.Length; i++)
{

if (!this.Actions[i].Equals(otherMove.Actions[i]))
return false;
}
return true;
}
return false;
}

public override int GetHashCode()
{

int hash = 17; 
if (Actions != null)
{
foreach (MoveAction action in Actions)
{
hash = hash * 23 + (action?.GetHashCode() ?? 0); 
}
}
return hash;
}
public override string ToString()
{
StringBuilder moveStr = new StringBuilder();
foreach (MoveAction move in Actions)
{
string actionStr = move.ToString();
moveStr.Append(actionStr).Append(';');
}
return moveStr.ToString();
}
public void Print()
{
Console.Error.WriteLine(ToString());
}
public void Output()
{
foreach (MoveAction action in Actions)
{
Console.WriteLine(action.ToString());
}
}
}
}

namespace GameSolution.Entities
{
public class Point2d
{
public readonly int x, y, index; 

public Point2d(int x, int y, int index)
{
this.x = x;
this.y = y;
this.index = index;
}

public Point2d(Point2d point) : this(point.x, point.y, point.index) { }

public Point2d Clone() => new Point2d(this);

public override bool Equals(object obj)
{
return obj is Point2d point && point.x == this.x && point.y == this.y;
}

public override int GetHashCode()
{

unchecked 
{
return (x * 397) ^ y; 
}
}
}
}

namespace GameSolution.Game
{
public class GameHelper
{
GameState State { get; set; }
public GameHelper(GameState state)
{
State = state;
}
public Move GetMove()
{
Move move = new Move();
move.SetActions(new MoveAction[] { MoveAction.CreateWait() });
return move;
}
}
}

namespace GameSolution.Game
{
public class GameState : PooledObject<GameState>, IGameState
{
public static int MaxTurns = 100;
public Board Board;
public int Turn;
public int[] MyProtein;
public int[] OppProtein;
public Move? maxMove;
public Move? minMove;
static GameState()
{
SetInitialCapacity(20000);
}
public GameState()
{
Turn = 0;
MyProtein = new int[4];
OppProtein = new int[4];
maxMove = null;
minMove = null;
_myMoves = new List<Move>();
_oppMoves = new List<Move>();
}
protected override void Reset()
{
Board.Dispose();
_myMoves.Clear();
_oppMoves.Clear();
}
private GameState CopyFrom(GameState state)
{
Board = state.Board.Clone();
Turn = state.Turn;
Array.Copy(state.MyProtein, MyProtein, state.MyProtein.Length);
Array.Copy(state.OppProtein, OppProtein, state.OppProtein.Length);
maxMove = state.maxMove;
minMove = state.minMove;
_myMoves.AddRange(state._myMoves);
_oppMoves.AddRange(state._oppMoves);
return this;
}
public void SetNextTurn(Board board, int[] myProtein, int[] oppProtein)
{
Turn++;
Board = board;
MyProtein = myProtein;
OppProtein = oppProtein;
_myMoves.Clear();
_oppMoves.Clear();
UpdateGameState();
}
public void UpdateGameState()
{
Board.UpdateBoard();
}
public void ApplyMove(object move, bool isMax)
{
Move m = (Move)move;
if (isMax)
{
maxMove = m;
minMove = null;
}
else
{
if (maxMove == null)
throw new Exception("Expected max to play first.");
minMove = m;
}
if (maxMove != null && minMove != null)
{
ApplyMove(maxMove, MyProtein);
ApplyMove(minMove, OppProtein);
Board.ApplyMove(maxMove, minMove);


Board.Harvest(true, MyProtein);
Board.Harvest(false, OppProtein);
Board.Attack();
SetNextTurn(Board, MyProtein, OppProtein);
}
}
public void ApplyMove(Move move, int[] proteins)
{
foreach (MoveAction action in move.Actions)
{
if (action.Type == MoveType.GROW || action.Type == MoveType.SPORE)
{
ApplyCost(proteins, action.GetCost());
if (action.EntityType == EntityType.BASIC)
{
Entity entity = Board.GetEntityByLocation(action.Location);
if (entity != null)
{
proteins[entity.Type - EntityType.A] += 3;
}
}
}
}
}
public void ApplyCost(int[] proteins, int[] cost)
{
for (int i = 0; i < 4; i++)
{
proteins[i] -= cost[i];
if (proteins[i] < 0)
{
throw new Exception("Invalid move played; proteins can't be negative");
}
}
}
public IGameState Clone()
{
GameState cleanState = Get();
cleanState.CopyFrom(this);
return cleanState;
}
public bool Equals(IGameState state)
{
GameState gameState = state as GameState;
if (this.Turn != gameState.Turn)
return false;
if ((maxMove == null && gameState.maxMove != null) || (maxMove != null && gameState.maxMove == null))
return false;
if ((minMove == null && gameState.minMove != null) || (minMove != null && gameState.minMove == null))
return false;
for (int i = 0; i < 4; i++)
{
if (this.MyProtein[i] != gameState.MyProtein[i])
return false;
if (this.OppProtein[i] != gameState.OppProtein[i])
return false;
}
if (!this.Board.Equals(gameState.Board))
return false;
return true;
}
public int GetGlobalOrganId()
{
return Board.GlobalOrganId;
}
public double Evaluate(bool isMax)
{
double value;
int myEntities = Board.GetMyEntityCount();
int oppEntities = Board.GetOppEntityCount();
int[] myHarvestProteins = Board.GetHarvestProteins(true);
double myHarvestProteinsSum = myHarvestProteins.Sum();
int[] oppHarvestProteins = Board.GetHarvestProteins(false);
double oppHarvestProteinsSum = oppHarvestProteins.Sum();
int myNumUniqueProteins = myHarvestProteins.Where(p => p > 1).Count();
int oppNumUniqueProteins = oppHarvestProteins.Where(p => p > 1).Count();
int myProteinBoost = myNumUniqueProteins * 5;
int oppProteinBoost = myNumUniqueProteins * 5;
double proteinValue = (myHarvestProteinsSum + myProteinBoost - oppProteinBoost - oppHarvestProteinsSum) / (myHarvestProteinsSum + oppHarvestProteinsSum + 1 + myProteinBoost + oppProteinBoost) * 0.2;
int myProtein = MyProtein.Sum();
int oppProtein = OppProtein.Sum();
value = (((double)myEntities - oppEntities) / (myEntities + oppEntities + 1) * 0.2) + (((double)myProtein - oppProtein) / (myProtein + oppProtein + 1) * 0.0001) + proteinValue;
if (value >= 1 || value <= -1)
Console.Error.WriteLine("Evaluation too high");
return value;
}
public object GetMove(bool isMax)
{
return isMax ? maxMove : minMove;
}
public int[] GetProteins(bool isMine)
{
return isMine ? MyProtein : OppProtein;
}
private List<Move> _myMoves;
private List<Move> _oppMoves;
private List<Move> GetMoves(bool isMax)
{
return isMax ? _myMoves : _oppMoves;
}
private void SetMoves(List<Move> moves, bool isMax)
{
if (isMax)
{
_myMoves = moves;
}
else
{
_oppMoves = moves;
}
}
public IList GetPossibleMoves(bool isMax)
{
if (Turn <= 100)
{
int[] proteins = GetProteins(isMax);
List<Move> moves = GetMoves(isMax);
if (moves.Count == 0)
{
Stopwatch watch = new Stopwatch();
watch.Start();
moves = Board.GetMoves(proteins, isMax);
SetMoves(moves, isMax);
watch.Stop();
if (watch.ElapsedMilliseconds > 20)
{
Console.Error.WriteLine($"Move generation: {watch.ElapsedMilliseconds}ms");
foreach (Move move in moves)
{
Console.Error.WriteLine($"{move}");
}
}
}
return moves;
}
else return new List<Move>();
}
public double? GetWinner()
{
int myEntitiesCount = Board.GetMyEntityCount();
int oppEntitiesCount = Board.GetOppEntityCount();
if (Turn < 100)
{
if (myEntitiesCount == 0)
return CheckGameEnd(myEntitiesCount, oppEntitiesCount);
else if (oppEntitiesCount == 0)
return CheckGameEnd(myEntitiesCount, oppEntitiesCount);
bool hasNoMyProteinsToBuild = MyProtein[0] == 0 && ((MyProtein[1] == 0 && MyProtein[2] == 0) || (MyProtein[1] == 0 && MyProtein[3] == 0) || (MyProtein[2] == 0 && MyProtein[3] == 0));
bool hasNoOppProteinsToBuild = OppProtein[0] == 0 && ((OppProtein[1] == 0 && OppProtein[2] == 0) || (OppProtein[1] == 0 && OppProtein[3] == 0) || (OppProtein[2] == 0 && OppProtein[3] == 0));
if (hasNoMyProteinsToBuild && myEntitiesCount < oppEntitiesCount)
return -1;
if (hasNoOppProteinsToBuild && oppEntitiesCount < myEntitiesCount)
return 1;
if (hasNoMyProteinsToBuild && hasNoOppProteinsToBuild)
return CheckGameEnd(myEntitiesCount, oppEntitiesCount);
double currentWinner = CheckGameEnd(myEntitiesCount, oppEntitiesCount);
bool hasOppMoves = HasMoves(GetPossibleMoves(false));
if (!hasOppMoves && currentWinner == 1)
return 1;
bool hasMyMoves = HasMoves(GetPossibleMoves(true));
if (!hasMyMoves && currentWinner == -1)
return -1;
}
if (Turn >= 100 || Board.IsFull())
{
return CheckGameEnd(myEntitiesCount, oppEntitiesCount);
}
return null;
}
private bool HasMoves(IList moves)
{
if (moves.Count == 1)
{
Move? theMove = moves[0] as Move;
bool isAllWait = true;
foreach (MoveAction action in theMove.Actions)
{
if (action.Type != MoveType.WAIT)
{
isAllWait = false;
break;
}
}
return !isAllWait;
}
return true;
}
private double CheckGameEnd(int myEntitiesCount, int oppEntitiesCount)
{
if (myEntitiesCount > oppEntitiesCount)
{
return 1;
}
else if (myEntitiesCount < oppEntitiesCount)
{
return -1;
}
else
{
int myProteinTotal = MyProtein.Sum();
int oppProteinTotal = OppProtein.Sum();
if (myProteinTotal > oppProteinTotal)
{
return 1;
}
else if (myProteinTotal < oppProteinTotal)
{
return -1;
}
else return 0;
}
}
public void Print()
{
Console.Error.WriteLine(Turn);
Console.Error.WriteLine(string.Join(',', MyProtein));
Console.Error.WriteLine(string.Join(',', OppProtein));
Board.Print();
}
}
}
