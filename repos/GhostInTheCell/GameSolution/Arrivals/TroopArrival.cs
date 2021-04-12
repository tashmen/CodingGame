using GameSolution.Entities;
using GameSolution.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using static GameSolution.Constants;

namespace GameSolution.Arrivals
{
    public class TroopArrival
    {
        //# of Turns to Troop Count
        public Dictionary<int, int> MyTroopArrival { get; private set; }
        public Dictionary<int, int> EnemyTroopArrival { get; private set; }

        /// <summary>
        /// Tracks who will own the factory by the specified time with current troop movements
        /// </summary>
        public Dictionary<int, Tuple<Owner, int>> TimeTofactoryOwnershipAndCyborgCount { get; private set; }

        public TroopArrival(List<TroopEntity> troops, List<BombEntity> friendlyBombs, FactoryEntity targetFactory)
        {
            MyTroopArrival = new Dictionary<int, int>();
            EnemyTroopArrival = new Dictionary<int, int>();
            //Calculate the time until troops arrive at the target factory
            foreach (TroopEntity troop in troops)
            {
                if (troop.TargetFactory == targetFactory.Id)
                {
                    if (troop.IsFriendly())
                    {
                        if (MyTroopArrival.ContainsKey(troop.TurnsToArrive))
                        {
                            MyTroopArrival[troop.TurnsToArrive] += troop.NumberOfCyborgs;
                        }
                        else
                        {
                            MyTroopArrival[troop.TurnsToArrive] = troop.NumberOfCyborgs;
                        }
                    }
                    else if (troop.IsEnemy())
                    {
                        //Console.Error.WriteLine("Enemy troop count: " + troop.NumberOfCyborgs + " arrives: " + troop.TurnsToArrive);
                        if (EnemyTroopArrival.ContainsKey(troop.TurnsToArrive))
                        {
                            EnemyTroopArrival[troop.TurnsToArrive] += troop.NumberOfCyborgs;
                        }
                        else
                        {
                            EnemyTroopArrival[troop.TurnsToArrive] = troop.NumberOfCyborgs;
                        }
                    }
                }
            }

            TimeTofactoryOwnershipAndCyborgCount = new Dictionary<int, Tuple<Owner, int>>();
            int cyborgsInFactory = targetFactory.NumberOfCyborgs;
            Owner currentOwner = targetFactory.Owner;
            int bombCount = 0;
            var friendlyBomb = friendlyBombs.FirstOrDefault(b => b.TargetFactoryId == targetFactory.Id);
            for (int time = 1; time <= 20; time++)
            {
                MyTroopArrival.TryGetValue(time, out int friendlyCount);
                EnemyTroopArrival.TryGetValue(time, out int enemyCount);
                currentOwner = GameHelper.DetermineFactoryOwnership(ref cyborgsInFactory, ref bombCount, currentOwner, targetFactory, friendlyBomb, time, friendlyCount, enemyCount);
                TimeTofactoryOwnershipAndCyborgCount[time] = new Tuple<Owner, int>(currentOwner, cyborgsInFactory);
            }
        }
    }
}
