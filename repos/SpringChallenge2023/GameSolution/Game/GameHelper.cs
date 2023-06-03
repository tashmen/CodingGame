using Algorithms.Graph;
using GameSolution.Entities;
using System;
using System.Collections.Generic;
using System.Linq;

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
            
            SortedDictionary<double, List<Cell>> distanceToTargets= new SortedDictionary<double, List<Cell>>();

            int totalEggTargets = 0;
            int totalCrystalTargets = 0;
            if (State.TotalCrystals > (State.TotalMyAnts + State.TotalOppAnts) / 6 * 10)
            {
                foreach (Cell eggCell in State.EggCells)
                {
                    var closestBase = GetClosestBase(State.MyBaseDictionary, eggCell);

                    if (IsMyBaseCloserThanOpponents(closestBase.Item1, State.OppBaseDictionary, eggCell))
                    {
                        Console.Error.WriteLine($"Found egg cell: {eggCell.Index} with minimum distance: {closestBase.Item1}");

                        if (!distanceToTargets.TryGetValue(closestBase.Item1, out List<Cell> targets))
                        {
                            targets = distanceToTargets[closestBase.Item1] = new List<Cell>();
                        }
                        targets.Add(eggCell);
                        totalEggTargets++;
                    }
                }
            }
            
            
            foreach (Cell crystalCell in State.CrystalCells)
            {
                var closestBase = GetClosestBase(State.MyBaseDictionary, crystalCell);

                if (IsMyBaseCloserThanOpponents(closestBase.Item1, State.OppBaseDictionary, crystalCell))
                {
                    if (!distanceToTargets.TryGetValue(closestBase.Item1, out List<Cell> targets))
                    {
                        targets = distanceToTargets[closestBase.Item1] = new List<Cell>();
                    }
                    targets.Add(crystalCell);
                    totalCrystalTargets++;
                }
            }


            

            HashSet<int> beacons = new HashSet<int>();
            var totalResources = 0;
            double previousDistance = 0;
            var eggsTargeted = 0;
            foreach(double key in distanceToTargets.Keys)
            {
                if (((key - previousDistance) < 2) && totalResources / (State.TotalMyAnts / key) > 4)
                    break;
                foreach(Cell target in distanceToTargets[key])
                {
                    
                    var closestBase = GetClosestBase(State.MyBaseDictionary, target);
                    Console.Error.WriteLine($"Processing target: {target.Index} at distance: {key}");
                    var path = GetShortestPathFromExistingBeacons(beacons, closestBase.Item2.Index, target.Index);
                    if ((totalEggTargets > eggsTargeted && target.ResourceType == ResourceType.Crystal && ((State.TotalCrystals / (State.TotalOppAnts + State.TotalMyAnts) > 5) || (target.ResourceAmount / key < 3)) && (eggsTargeted == 0 || path.Count > 1)))
                    {
                        continue;
                    }

                    
                    var totalCount = path.Count + beacons.Count;
                    Console.Error.WriteLine($"{totalCount} vs {State.TotalMyAnts}");
                    if (totalCount > State.TotalMyAnts)
                    { 
                        continue; 
                    }

                    beacons.Add(path[0].StartNodeId);
                    foreach(var link in path)
                    {
                        beacons.Add(link.EndNodeId);
                    }
                    totalResources += target.ResourceAmount;
                    if (target.ResourceType == ResourceType.Egg)
                    {
                        eggsTargeted++;
                    }
                }
                previousDistance = key;
            }
            
            foreach(var beacon in beacons)
            {
                move.AddAction(MoveAction.CreateBeacon(beacon, 1));
            }


            if (move.Actions.Count == 0)
                move.AddAction(MoveAction.CreateWait());

            return move;
        }

        public Tuple<double, Cell> GetClosestBase(Dictionary<int, Cell> bases, Cell target)
        {
            double minDistance = 999999;
            Cell closestBase = null;
            foreach (var myBaseTarget in bases.Values)
            {
                var distance = State.Board.Graph.GetShortestPathDistance(myBaseTarget.Index, target.Index);
                if (minDistance > distance)
                {
                    minDistance = distance;
                    closestBase = myBaseTarget;
                }
            }

            return Tuple.Create(minDistance, closestBase);
        }

        public IList<ILink> GetShortestPathFromExistingBeacons(HashSet<int> beacons, int startId, int endId)
        {
            var minDist = State.Board.Graph.GetShortestPathDistance(startId, endId);
            IList<ILink> bestPath = State.Board.Graph.GetShortestPathAll(startId, endId);

            foreach(int id in beacons)
            {
                if (endId != id)
                {
                    var distance = State.Board.Graph.GetShortestPathDistance(id, endId);
                    if (minDist > distance)
                    {
                        minDist = distance;
                        bestPath = State.Board.Graph.GetShortestPathAll(id, endId);
                    }
                }
            }

            return bestPath;
        }

        public bool IsMyBaseCloserThanOpponents(double distance, Dictionary<int, Cell> opponentBases, Cell targetLocation)
        {
            foreach(Cell oppBase in opponentBases.Values)
            {
                var oppDistance = State.Board.Graph.GetShortestPathDistance(oppBase.Index, targetLocation.Index);
                if (oppDistance * 2.5 < distance || ((oppDistance < distance) && (targetLocation.ResourceAmount / distance) < 2))
                    return false;
            }
            return true;
        }
    }
}

