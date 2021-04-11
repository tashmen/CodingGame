using GameSolution.Arrivals;
using GameSolution.Entities;
using GameSolution.Utility;
using System.Collections.Generic;
using Xunit;
using static GameSolution.Constants;

namespace UnitTest.Utilities
{
    public class BombArrivalTests
    {
        [Fact]
        public void Test_BombArrival()
        {
            FactoryLinks links;
            links = new FactoryLinks();
            links.AddLink(1, 2, 1);
            links.AddLink(1, 3, 2);
            links.AddLink(1, 4, 4);
            links.AddLink(2, 3, 1);
            links.AddLink(2, 4, 2);

            var bombs = new List<BombEntity>()
            {
                new BombEntity(1, (int)Owner.Me, 1, 2, 2, 0),
                new BombEntity(2, (int)Owner.Opponent, 2, -1, -1, 0),
                new BombEntity(3, (int)Owner.Opponent, 2, -1, -1, 0),
            };
            Dictionary<int, int> sentBombs = new Dictionary<int, int>();
            sentBombs[1] = 0;
            sentBombs[2] = 1;
            sentBombs[3] = 2;

            var arrivals = new BombArrival(bombs, sentBombs, 2, links);
            Assert.True(arrivals.HasIncomingBomb);
            Assert.Single(arrivals.TurnsUntilArrival);
            Assert.Equal(2, arrivals.TurnsUntilArrival[0]);

            arrivals = new BombArrival(bombs, sentBombs, 4, links);
            Assert.True(arrivals.HasIncomingBomb);
            Assert.Equal(2, arrivals.TurnsUntilArrival.Count);
            Assert.Contains(1, arrivals.TurnsUntilArrival);
            Assert.Contains(2, arrivals.TurnsUntilArrival);
        }
    }

    
}
