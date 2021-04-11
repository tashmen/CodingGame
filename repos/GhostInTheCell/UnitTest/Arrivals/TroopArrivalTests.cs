using GameSolution.Arrivals;
using GameSolution.Entities;
using GameSolution.Utility;
using System.Collections.Generic;
using Xunit;
using static GameSolution.Constants;

namespace UnitTest.Utilities
{
    public class TroopArrivalTests
    {
        [Fact]
        public void Test_TroopArrivalTable()
        {
            var troops = new List<TroopEntity>()
            {
                new TroopEntity(4, (int)Owner.Me, 2, 2, 1, 1),
                new TroopEntity(5, (int)Owner.Me, 2, 2, 1, 1),
                new TroopEntity(6, (int)Owner.Opponent, 2, 2, 2, 1),
                new TroopEntity(12, (int)Owner.Opponent, 2, 2, 3, 2),
                new TroopEntity(12, (int)Owner.Opponent, 2, 2, 3, 2),
                new TroopEntity(12, (int)Owner.Opponent, 2, 3, 3, 2)//not on target
            };
            var arrivals = new TroopArrival(troops, 2);
            Assert.Equal(2, arrivals.myTroopArrival[1]);
            Assert.Equal(2, arrivals.enemyTroopArrival[1]);
            Assert.Equal(6, arrivals.enemyTroopArrival[2]);
        }
    }

    
}
