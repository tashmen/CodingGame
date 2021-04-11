using GameSolution.Utility;
using Xunit;

namespace UnitTest.Utilities
{
    public class FactoryLinksTest
    {
        FactoryLinks _links;
        public FactoryLinksTest()
        {
            _links = new FactoryLinks();
            _links.AddLink(1, 2, 1);
            _links.AddLink(1, 3, 2);
            _links.AddLink(1, 4, 4);
            _links.AddLink(2, 3, 1);
            _links.AddLink(2, 4, 2);

            _links.CalculateShortestPaths();
        }

        [Fact]
        public void Test_ShortestPath()
        {
            Assert.Equal(3, _links.GetShortestPath(1, 3));
            Assert.Equal(2, _links.GetShortestPath(1, 4));

            Assert.Equal(3, _links.GetShortestPathDistance(1, 3));
            Assert.Equal(5, _links.GetShortestPathDistance(1, 4));
        }

        [Fact]
        public void Test_Distance()
        {
            Assert.Equal(3, _links.GetDistance(1, 3));
            Assert.Equal(5, _links.GetDistance(1, 4));
        }
    }
}
