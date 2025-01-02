
using Algorithms.GameComponent;
using Algorithms.Genetic;
using Algorithms.Trees;
using Algorithms.Utility;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

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

namespace Algorithms.Genetic
{
public class GeneticAlgorithm
{

public Population Population;
private Population HiddenPopulation;

private double MutationRate;

private double ElitePercent;

private double CrossOver;



public int GenerationCounter;

public GeneticAlgorithm(Population initialPopulation, double mRate, double eP, double cO)
{
Population = initialPopulation;
HiddenPopulation = initialPopulation.Clone();
MutationRate = mRate;
ElitePercent = eP;
CrossOver = cO;
GenerationCounter = 0;
}







public object GetNextMove(Stopwatch watch, int timeLimit, int maxGeneration = -1)
{
do
{
var counter = 0;
foreach(Individual i in Population)
{
if (watch.ElapsedMilliseconds >= timeLimit && counter > 1)
{
break;
}
if(i.Fitness == double.MinValue)
i.CalculateFitness();
counter++;
}
GenerateNextGeneration();
}
while (watch.ElapsedMilliseconds < timeLimit && GenerationCounter != maxGeneration);
var bestIndividual = Population.GetBestIndividual();
return bestIndividual.GetNextMove();
}









public Population GenerateNextGeneration()
{
GenerationCounter++;
Individual individual1;
Individual individual2;
Individual child;

Population.SortPopulationByFitness();

for (int x = 0; x < (int)(Population.Count * ElitePercent); x++)
{
HiddenPopulation[x] = Population[x];
}
double totalFit = Population.GetTotalFitness();
for (int x = (int)(Population.Count * ElitePercent); x < Population.Count; x++)
{

individual1 = Population.SelectRandomFromPopulation(totalFit);
individual2 = Population.SelectRandomFromPopulation(totalFit, individual1);

child = HiddenPopulation[x].CreateBaby(individual1, individual2, CrossOver);
child.Mutate(MutationRate);
child.Fitness = double.MinValue;
x++;
if (x < Population.Count)
{
child = HiddenPopulation[x].CreateBaby(individual2, individual1, CrossOver);
child.Mutate(MutationRate);
child.Fitness = double.MinValue;
}
}

var swap = Population;
Population = HiddenPopulation;
HiddenPopulation = swap;
return Population;
}
}
}

namespace Algorithms.Genetic
{

public interface Individual
{



public double Fitness { get; set; }

Individual CreateBaby(Individual parent1, Individual parent2, double crossOver);

void Mutate(double mutationRate);
double CalculateFitness();
object GetNextMove();
Individual Clone();
}
}

namespace Algorithms.Genetic
{
public class Population : IEnumerable<Individual>, IList<Individual>
{
private List<Individual> Individuals;
private Random Rand;
public int Count => Individuals.Count;
public bool IsReadOnly => throw new NotImplementedException();
public Individual this[int index] { get { return Individuals[index]; }  set { Individuals[index] = value; } }

public Population()
{
Individuals = new List<Individual>();
Rand = new Random();
}
public Population(Population population)
{
Individuals = population.Individuals.Select(i => i.Clone()).ToList();
Rand = new Random();
}



public void SortPopulationByFitness()
{
Individuals.Sort(delegate (Individual i1, Individual i2)
{
if (i1 == null && i2 == null) return 0;
else if (i1 == null) return 1;
else if (i2 == null) return -1;
else if (i2.Fitness == i1.Fitness) return 0;
else return i1.Fitness > i2.Fitness ? -1 : 1;
});
}
public double GetTotalFitness()
{
return Individuals.Sum(i => i.Fitness);
}





public Individual SelectRandomFromPopulation(double totalFit, Individual i = null)
{
double randNum = Rand.NextDouble() * totalFit;
int y = 0;
double totalFitSoFar = Individuals[y].Fitness;
while (totalFitSoFar < randNum)
{
y++;
totalFitSoFar += Individuals[y].Fitness;
}
if(i != null && i == Individuals[y])
{
if (y == Individuals.Count - 1)
{
y--;
}
else y++;
}
return Individuals[y];
}
public Individual GetBestIndividual()
{
SortPopulationByFitness();
return Individuals.First();
}
public double MaximumFitness()
{
SortPopulationByFitness();
return Individuals[0].Fitness;
}
public double MinimumFitness()
{
SortPopulationByFitness();
return Individuals.Last().Fitness;
}
public double AverageFitness()
{
return Individuals.Average(i => i.Fitness);
}
public IEnumerator<Individual> GetEnumerator()
{
return Individuals.GetEnumerator();
}
IEnumerator IEnumerable.GetEnumerator()
{
return Individuals.GetEnumerator();
}
public int IndexOf(Individual item)
{
return Individuals.IndexOf(item);
}
public void Insert(int index, Individual item)
{
Individuals.Insert(index, item);
}
public void RemoveAt(int index)
{
Individuals.RemoveAt(index);
}
public void Add(Individual item)
{
Individuals.Add(item);
}
public void Clear()
{
Individuals.Clear();
}
public bool Contains(Individual item)
{
return Individuals.Contains(item);
}
public void CopyTo(Individual[] array, int arrayIndex)
{
Individuals.CopyTo(array, arrayIndex);
}
public bool Remove(Individual item)
{
return Individuals.Remove(item);
}
public Population Clone()
{
return new Population(this);
}
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
if(IsByDirectional)
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
foreach(ILink link in currentPath)
{
distance += link.Distance;
}
return distance;
}
}
}

namespace Algorithms.NeuralNetwork
{

public class NeuralNetwork : Individual
{

private NeuronLayer[] neuronLayers;

private int numLayers;

private int[] numNeurons;

private int[] numInputs;
private int nFirstInputs;

private int totalNumWeightsInNetwork;

public double Fitness { get; set; }
Random rand;

public NeuralNetwork(int nLayers, int[] nNeurons, int nInputs)
{
rand = new Random();
numLayers = nLayers;
numNeurons = nNeurons;
nFirstInputs = nInputs;
numInputs = new int[nLayers];
numInputs[0] = nInputs;
for (int x = 1; x < nLayers; x++)
{
numInputs[x] = numNeurons[x - 1];
}
neuronLayers = new NeuronLayer[nLayers];
for (int x = 0; x < nLayers; x++)
{
neuronLayers[x] = new NeuronLayer(numNeurons[x], numInputs[x]);
totalNumWeightsInNetwork += numNeurons[x] * numInputs[x];
}
Fitness = 0;
}
public NeuralNetwork(NeuralNetwork net)
{
rand = new Random();
numLayers = net.numLayers;
numNeurons = (int[])net.numNeurons.Clone();
nFirstInputs = net.nFirstInputs;
numInputs = (int[])net.numInputs.Clone();
neuronLayers = new NeuronLayer[numLayers];
for (int x = 0; x < numLayers; x++)
{
neuronLayers[x] = new NeuronLayer(numNeurons[x], numInputs[x]);
totalNumWeightsInNetwork += numNeurons[x] * numInputs[x];
}
setWeights(net.getWeights());
Fitness = 0;
}
public NeuralNetwork(BinaryReader reader)
{
numLayers = reader.ReadInt32();
numNeurons = new int[numLayers];
for (int x = 0; x < numLayers; x++)
{
numNeurons[x] = reader.ReadInt32();
}
numInputs = new int[numLayers];
for (int x = 0; x < numLayers; x++)
{
numInputs[x] = reader.ReadInt32();
}
nFirstInputs = numInputs[0];
neuronLayers=new NeuronLayer[numLayers];
for (int x = 0; x < numLayers; x++)
{
neuronLayers[x]=new NeuronLayer(numNeurons[x], numInputs[x]);
totalNumWeightsInNetwork += numNeurons[x] * numInputs[x];
}
double[] weights = new double[getNumWeights()];
for(int i = 0; i<weights.Length; i++)
{
weights[i] = reader.ReadDouble();
}
setWeights(weights);
Fitness = reader.ReadDouble();
}
public void Save(BinaryWriter writer)
{
writer.Write(numLayers);
for (int x = 0; x < numNeurons.Count(); x++)
{
writer.Write(numNeurons[x]);
}
for (int x = 0; x < numInputs.Count(); x++)
{
writer.Write(numInputs[x]);
}
double[] weights = getWeights();
foreach(double weight in weights)
{
writer.Write(weight);
}
writer.Write(Fitness);
}

public NeuronLayer getNeuronLayer(int location)
{
return neuronLayers[location];
}

public int getNumLayers()
{
return numLayers;
}

public int[] getNumNeurons()
{
return numNeurons;
}

public int[] getNumInputs()
{
return numInputs;
}

public int getNumWeights()
{
return totalNumWeightsInNetwork;
}

public double[] getWeights()
{
double[] weights = new double[totalNumWeightsInNetwork];
double[] layerWeights;
int count = 0;
for (int x = 0; x < getNumLayers(); x++)
{
layerWeights = getNeuronLayer(x).getWeights();
for (int y = 0; y < layerWeights.Count(); y++)
{
weights[count] = layerWeights[y];
count++;
}
}
return weights;
}

public void displayWeights()
{
double[] weights = getWeights();
for (int x = 0; x < weights.Count(); x++)
{
Console.Error.Write(weights[x] + ", ");
}
Console.Error.WriteLine();
}

public void setWeights(double[] weights)
{
int count = 0;
for (int x = 0; x < getNumLayers(); x++)
{
for (int y = 0; y < neuronLayers[x].getNumNeurons(); y++)
{
for (int z = 0; z < neuronLayers[x].getNeuron(y).getNumWeights(); z++)
{
neuronLayers[x].getNeuron(y).setWeight(z, weights[count]);
count++;
}
}
}
}

public double[] output(double[] inputs)
{
double[] output = inputs;


for (int x = 0; x < getNumLayers(); x++)
{
output = getNeuronLayer(x).output(output);
}
return output;
}


public Individual CreateBaby(Individual parent1, Individual parent2, double crossOver)
{
var p1 = (NeuralNetwork)parent1;
var p2 = (NeuralNetwork)parent2;
double[] weights = new double[p1.getNumWeights()];
double[] p1weights = p1.getWeights();
double[] p2weights = p2.getWeights();
for (int x = 0; x < p1.getNumWeights(); x++)
{
if (p1.getNumWeights() * crossOver < x)
weights[x] = p1weights[x];
else weights[x] = p2weights[x];
}
setWeights(weights);
return this;
}

public void Mutate(double mutationRate)
{
double[] weights = getWeights();
for(int i = 0; i < weights.Length; i++)
{
if(rand.NextDouble() < mutationRate)
{
weights[i] = (rand.NextDouble() * 2 - 1);
}
}
setWeights(weights);
}

public bool Equals(Individual i)
{
double[] weights1 = getWeights();
double[] weights2 = ((NeuralNetwork)i).getWeights();
for (int x = 0; x < weights1.Count(); x++)
{
if (weights1[x] != weights2[x])
return false;
}
return true;
}
public double CalculateFitness()
{
throw new NotImplementedException();
}
public object GetNextMove()
{
throw new NotImplementedException();
}
public Individual Clone()
{
return new NeuralNetwork(this);
}
}
}

namespace Algorithms.NeuralNetwork
{
public class Neuron
{

private double[] weights;

private int numWeights;
Random rand;

public Neuron(int numInputs)
{
rand = new Random();
numWeights = numInputs;
weights = new double[numInputs];
for (int x = 0; x < numInputs; x++)
{
weights[x] = rand.NextDouble() * 2 - 1;
}
}

public double getWeight(int location)
{
return weights[location];
}

public int getNumWeights()
{
return numWeights;
}

public void setWeight(int location, double weight)
{
weights[location] = weight;
}
}
}

namespace Algorithms.NeuralNetwork
{
public class NeuronLayer
{

private Neuron[] neurons;

private int numNeurons;

public NeuronLayer(int nNeurons, int numInputs)
{
numNeurons = nNeurons;
neurons = new Neuron[numNeurons];
for (int x = 0; x < numNeurons; x++)
{
neurons[x] = new Neuron(numInputs);
}
}

public Neuron getNeuron(int location)
{
return neurons[location];
}

public int getNumNeurons()
{
return numNeurons;
}

public double[] getWeights()
{
double[] weights = new double[neurons[0].getNumWeights() * numNeurons];
int count = 0;
for (int x = 0; x < getNumNeurons(); x++)
{
for (int y = 0; y < neurons[x].getNumWeights(); y++)
{
weights[count] = neurons[x].getWeight(y);
count++;
}
}
return weights;
}

public int getNumWeights()
{
return getNumNeurons() * getNeuron(0).getNumWeights();
}

public double[] output(double[] inputs)
{
double[] output = new double[getNumNeurons()];
double sum = 0;
int temp = 0;
for (int x = 0; x < getNumNeurons(); x++)
{
for (int y = 0; y < getNeuron(x).getNumWeights() - temp; y++)
{
sum = sum + inputs[y] * getNeuron(x).getWeight(y);
}

output[x] = sigmoid(sum);
sum = 0;
}
return output;
}

public double sigmoid(double value)
{
return (1.0 / (1.0 + Math.Exp(-value)));
}
}
}

namespace Algorithms.Space
{
public class Circle2d : Point2d
{
public double radius;
public Circle2d(double x, double y, double radius) : base(x, y)
{
this.radius = radius;
}
}
}

namespace Algorithms.Space
{
public class Point2d
{
public double x;
public double y;
public Point2d(double x, double y)
{
this.x = x; 
this.y = y;
}
public Point2d(Point2d point)
{
x = point.x;
y = point.y;
}
public override string ToString()
{
return $"({x},{y})";
}
public override bool Equals(object objPoint)
{
Point2d point = objPoint as Point2d;
return point.x == this.x && point.y == this.y;
}
public override int GetHashCode()
{
return Tuple.Create(x, y).GetHashCode();
}
public Point2d GetTruncatedPoint()
{
return new Point2d(Math.Truncate(this.x), Math.Truncate(this.y));
}
public Point2d GetRoundedPoint()
{
return new Point2d(Math.Round(this.x), Math.Round(this.y));
}
public Point2d GetCeilingPoint()
{
return new Point2d(Math.Ceiling(x), Math.Ceiling(y));
}
public int GetTruncatedX()
{
return (int)x;
}
public int GetTruncatedY()
{
return (int)y;
}
public double GetAngle(Point2d point)
{
return Math.Atan2(point.y - y, point.x - x);
}
public int GetManhattenDistance(Point2d point)
{
return (int)(Math.Abs(point.x - x) + Math.Abs(point.y - y));
}
public double GetDistance(Point2d point)
{
return GetDistance(point.x, point.y, x, y);
}
public Point2d GetMidPoint(Point2d point)
{
return GetMidPoint(point.x, point.y, x, y);
}
public double LengthSquared()
{
return x * x + y * y;
}
public double Length()
{
return Math.Sqrt(LengthSquared());
}
public Point2d Normalize()
{
var length = Length();
if (length == 0)
{
x = 0;
y = 0;
}
else
{
x /= length;
y /= length;
}
return this;
}
public Point2d Multiply(double scalar)
{
x *= scalar;
y *= scalar;
return this;
}
public Point2d Add(Point2d vector)
{
x += vector.x;
y += vector.y;
return this;
}
public Point2d Subtract(Point2d vector)
{
x -= vector.x;
y -= vector.y;
return this;
}
public Point2d Truncate()
{
x = GetTruncatedX();
y = GetTruncatedY();
return this;
}
public Point2d SymmetricTruncate(Point2d origin)
{
Subtract(origin).Truncate().Add(origin);
return this;
}
public Point2d GetRoundedAwayFromZeroPoint()
{
return new Point2d(Math.Round(x, MidpointRounding.AwayFromZero), Math.Round(y, MidpointRounding.AwayFromZero));
}
public Point2d Clone()
{
return new Point2d(x, y);
}
public void Fill(Point2d point)
{
x = point.x;
y = point.y;
}
public static double GetDistance(double x1, double y1, double x2, double y2)
{
return Math.Sqrt(Math.Pow(x1 - x2, 2) + Math.Pow(y1 - y2, 2));
}
public static Point2d GetMidPoint(double x1, double y1, double x2, double y2)
{
return new Point2d((x1 + x2)/2, (y1+y2)/2);
}
}
}

namespace Algorithms.Space
{
public class Space2d
{






public static Tuple<int, Point2d> FindCircleWithMaximumPoints(Point2d[] points, double radius)
{
Tuple<int, Point2d> maxPoint = null;
if (points == null)
return null;
if (radius <= 0)
return null;
var numberOfPoints = points.Count();
double[,] distance = new double[numberOfPoints, numberOfPoints];
for (int i = 0; i < numberOfPoints - 1; i++)
{
for (int j = i + 1; j < numberOfPoints; j++)
{
distance[i, j] = distance[j, i] = points[i].GetDistance(points[j]);
}
}
for (int i = 0; i < numberOfPoints; i++)
{
var currentAnswer = GetPointsInside(distance, points, i, radius, numberOfPoints);
var nextPoint = new Tuple<int, Point2d>(currentAnswer.Item1, new Point2d(points[i].x + (radius * Math.Round(Math.Cos(currentAnswer.Item2), 15)), points[i].y + (radius * Math.Round(Math.Sin(currentAnswer.Item2), 15))));
if (maxPoint == null || currentAnswer.Item1 > maxPoint.Item1 || (currentAnswer.Item1 == maxPoint.Item1 && IsInteger(nextPoint.Item2.x) && IsInteger(nextPoint.Item2.y)))
{
maxPoint = nextPoint;
}
}
return maxPoint;
}







public static Tuple<int, Point2d> FindCircleWithMaximumPoints(Point2d[] points, double radius, int i)
{
Tuple<int, Point2d> maxPoint = null;
if (points == null)
return null;
if (radius <= 0)
return null;
var numberOfPoints = points.Count();
double[,] distance = new double[numberOfPoints, numberOfPoints];
for (int j = 0; j < numberOfPoints; j++)
{
distance[i, j] = distance[j, i] = points[i].GetDistance(points[j]);
}
var currentAnswer = GetPointsInside(distance, points, i, radius, numberOfPoints);
maxPoint = new Tuple<int, Point2d>(currentAnswer.Item1, new Point2d(points[i].x + (radius * Math.Round(Math.Cos(currentAnswer.Item2), 15)), points[i].y + (radius * Math.Round(Math.Sin(currentAnswer.Item2), 15))));
return maxPoint;
}
public static double CalculateAreaOfCircle(double radius)
{
return Math.PI * Math.Pow(radius, 2);
}
public static double CalculateOverlappingArea(Circle2d circle, Circle2d circle2)
{
var d = circle.GetDistance(circle2);
if (d < circle.radius + circle2.radius)
{
var a = circle.radius * circle.radius;
var b = circle2.radius * circle2.radius;
var x = (a - b + d * d) / (2 * d);
var z = x * x;
var y = Math.Sqrt(a - z);
if (d <= Math.Abs(circle2.radius - circle.radius))
{
return Math.PI * Math.Min(a, b);
}
return a * Math.Asin(y / circle.radius) + b * Math.Asin(y / circle2.radius) - y * (x + Math.Sqrt(z + b - a));
}
return 0;
}







public static Point2d TranslatePoint(Point2d startPoint, Point2d targetPoint, double maximumDistance)
{
var vector = CreateVector(startPoint, targetPoint);
if (vector.LengthSquared() <= (maximumDistance * maximumDistance))
return targetPoint;
else
{
vector.Normalize();
vector.Multiply(maximumDistance);
return new Point2d(startPoint.x + vector.x, startPoint.y + vector.y);
}

}
public static Point2d CreateVector(Point2d startPoint, Point2d targetPoint)
{
var x = targetPoint.x - startPoint.x;
var y = targetPoint.y - startPoint.y;
return new Point2d(x, y);
}
private static bool IsInteger(double d)
{
return Math.Abs(d % 1) <= (Double.Epsilon * 100);
}
private static Tuple<int, double> GetPointsInside(double[,] distance, Point2d[] points, int i, double radius, int numberOfPoints)
{
List<Tuple<double, bool>> angles = new List<Tuple<double, bool>>();
for (int j = 0; j < numberOfPoints; j++)
{
if (i != j && distance[i, j] <= 2 * radius)
{
double B = Math.Acos(distance[i, j] / (2 * radius));
Complex c1 = new Complex(points[j].x - points[i].x, points[j].y - points[i].y);
double A = c1.Phase;
double alpha = A - B;
double beta = A + B;
angles.Add(new Tuple<double, bool>(alpha, true));
angles.Add(new Tuple<double, bool>(beta, false));
}
}
angles = angles.OrderBy(angle => angle.Item1).ToList();
int count = 1, res = 1;
double maxAngle = 0;
foreach (var angle in angles)
{
if (angle.Item2)
count++;
else
count--;
if (count > res)
{
res = count;
maxAngle = angle.Item1;
}
}
return new Tuple<int, double>(res, maxAngle);
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
