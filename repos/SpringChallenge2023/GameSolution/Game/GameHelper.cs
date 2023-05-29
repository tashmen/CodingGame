using Algorithms.Graph;
using GameSolution.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using static Algorithms.Graph.GraphLinks;
using Node = Algorithms.Graph.Node;

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

            foreach (Cell myBase in State.MyBaseDictionary.Values)
            {
                foreach (Cell eggCell in State.EggCells)
                {
                    var distance = State.Graph.GetShortestPathDistance(myBase.Index, eggCell.Index);
                    Console.Error.WriteLine($"Found egg cell: {eggCell.Index} with distance: {distance}");
                    if (IsMyBaseCloserThanOpponents(distance, State.OppBaseDictionary, eggCell))
                    {
                        Console.Error.WriteLine($"distance is within range");
                        var path = GetShortestPathFromExistingBeacons(move, myBase.Index, eggCell.Index);
                        move.AddAction(MoveAction.CreateBeacon(path.First().StartNodeId, 1));
                        foreach (var node in path)
                        {
                            move.AddAction(MoveAction.CreateBeacon(node.EndNodeId, 1));
                        }
                    }
                }
            }

            if(move.Actions.Count == 0 || State.TotalMyAnts > State.TotalOppAnts)
            {
                foreach (Cell myBase in State.MyBaseDictionary.Values)
                {
                    foreach (Cell crystalCell in State.CrystalCells)
                    {
                        var distance = State.Graph.GetShortestPathDistance(myBase.Index, crystalCell.Index);
                        if (distance < State.TotalMyAnts)
                        {
                            var path = GetShortestPathFromExistingBeacons(move, myBase.Index, crystalCell.Index);
                            move.AddAction(MoveAction.CreateBeacon(path.First().StartNodeId, 1));
                            foreach (var node in path)
                            {
                                move.AddAction(MoveAction.CreateBeacon(node.EndNodeId, 1));
                            }
                        }
                    }
                }
            }
            
            
            

            if (move.Actions.Count == 0)
                move.AddAction(MoveAction.CreateWait());

            return move;
        }

        public IList<ILink> GetShortestPathFromExistingBeacons(Move move, int startId, int endId)
        {
            var minDist = State.Graph.GetShortestPathDistance(startId, endId);
            IList<ILink> bestPath = State.Graph.GetShortestPathAll(startId, endId);

            foreach(MoveAction moveAction in move.Actions)
            {
                if (endId != moveAction.Index1)
                {
                    var distance = State.Graph.GetShortestPathDistance(moveAction.Index1, endId);
                    if (minDist > distance)
                    {
                        minDist = distance;
                        bestPath = State.Graph.GetShortestPathAll(moveAction.Index1, endId);
                    }
                }
            }

            return bestPath;
        }

        public bool IsMyBaseCloserThanOpponents(double distance, Dictionary<int, Cell> opponentBases, Cell targetLocation)
        {
            foreach(Cell oppBase in opponentBases.Values)
            {
                var oppDistance = State.Graph.GetShortestPathDistance(oppBase.Index, targetLocation.Index);
                if (oppDistance < distance)
                    return false;
            }
            return true;
        }
    }
}

