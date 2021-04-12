using GameSolution.Arrivals;
using GameSolution.Utility;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GameSolution.Entities
{
    public class FactoryEntity : Entity
    {
        public int NumberOfCyborgs
        {
            get { return Arg2; }
            private set 
            { 
                Arg2 = value;
                if (NumberOfCyborgs < 0)
                {
                    throw new InvalidOperationException("Not enough troops!");
                }
            }
        }
        public int ProductionCount
        {
            get { return Arg3; }
            private set 
            { 
                Arg3 = value;
                if (ProductionCount > 3)
                {
                    throw new InvalidOperationException("At maximum production!");
                }
            }
        }

        public int TurnsTillProduction
        {
            get { return Arg4; }
        }

        public int CyborgsRequiredToDefend { get; set; }

        public TroopArrival TroopArrival { get; private set; }

        public BombArrival BombArrival { get; private set; }

        /// <summary>
        /// Creates a new Factory Entity
        /// </summary>
        /// <param name="id">Unique Identifier</param>
        /// <param name="arg1">Owner</param>
        /// <param name="arg2">Number of cyborgs in the factory</param>
        /// <param name="arg3">Factory Production</param>
        /// <param name="arg4">Number of turns till production</param>
        /// <param name="arg5">unused</param>
        public FactoryEntity(int id, int arg1, int arg2, int arg3, int arg4, int arg5)
        : base(id, arg1, arg2, arg3, arg4, arg5)
        {

        }

        public FactoryEntity(FactoryEntity entity) : base(entity)
        {
            
        }

        public void Move(int cyborgCount)
        {
            NumberOfCyborgs = NumberOfCyborgs - cyborgCount;
        }

        public void Upgrade()
        {
            ProductionCount++;
            NumberOfCyborgs -= 10;
        }

        public bool IsProducing()
        {
            return TurnsTillProduction == 0;
        }

        public void BuildArrivals(List<TroopEntity> troops, List<BombEntity> bombs, Dictionary<int, int> sentBombs, FactoryLinks links)
        {
            TroopArrival = new TroopArrival(troops, bombs.Where(b => b.IsFriendly()).ToList(), this);

            BombArrival = new BombArrival(bombs, sentBombs, Id, links);
        }
    }
}
