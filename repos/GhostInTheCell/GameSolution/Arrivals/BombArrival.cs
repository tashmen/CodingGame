using GameSolution.Entities;
using GameSolution.Utility;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GameSolution.Arrivals
{
    /// <summary>
    /// Class for determining the potential target of an enemy bomb
    /// </summary>
    public class BombArrival
    {
        //A value of 1 should mean the bomb arrives next turn
        public List<int> TurnsUntilArrival { get; private set; }

        public bool HasIncomingBomb
        {
            get
            {
                return TurnsUntilArrival.Where(a => a > 0).Any();
            }
        }
        public BombArrival(List<BombEntity> bombs, Dictionary<int, int> sentBombs, int factoryId, FactoryLinks links)
        {
            foreach(BombEntity bomb in bombs)
            {
                var distance = links.GetDistance(bomb.SourceFactoryId, factoryId);

                TurnsUntilArrival.Add(Math.Max(distance - sentBombs[bomb.Id], 0));
            }
        }
    }
}
