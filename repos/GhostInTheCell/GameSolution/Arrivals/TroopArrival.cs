using GameSolution.Entities;
using System.Collections.Generic;

namespace GameSolution.Arrivals
{
    public class TroopArrival
    {
        //# of Turns to Troop Count
        public Dictionary<int, int> myTroopArrival { get; private set; }
        public Dictionary<int, int> enemyTroopArrival { get; private set; }

        public TroopArrival(List<TroopEntity> troops, int targetFactoryId)
        {
            myTroopArrival = new Dictionary<int, int>();
            enemyTroopArrival = new Dictionary<int, int>();
            //Calculate the time until troops arrive at the target factory
            foreach (TroopEntity troop in troops)
            {
                if (troop.TargetFactory == targetFactoryId)
                {
                    if (troop.IsFriendly())
                    {
                        if (myTroopArrival.ContainsKey(troop.TurnsToArrive))
                        {
                            myTroopArrival[troop.TurnsToArrive] += troop.NumberOfCyborgs;
                        }
                        else
                        {
                            myTroopArrival[troop.TurnsToArrive] = troop.NumberOfCyborgs;
                        }
                    }
                    else if (troop.IsEnemy())
                    {
                        //Console.Error.WriteLine("Enemy troop count: " + troop.NumberOfCyborgs + " arrives: " + troop.TurnsToArrive);
                        if (enemyTroopArrival.ContainsKey(troop.TurnsToArrive))
                        {
                            enemyTroopArrival[troop.TurnsToArrive] += troop.NumberOfCyborgs;
                        }
                        else
                        {
                            enemyTroopArrival[troop.TurnsToArrive] = troop.NumberOfCyborgs;
                        }
                    }
                }
            }
        }
    }
}
