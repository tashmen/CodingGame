using Algorithms.Graph;
using Xunit;

namespace UnitTest
{
    public class GraphLinksTests
    {
        GraphLinks _links;
        public GraphLinksTests()
        {
            _links = new GraphLinks();
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

            Assert.Equal(2, _links.GetShortestPathDistance(1, 3));
            Assert.Equal(3, _links.GetShortestPathDistance(1, 4));
        }

        [Fact]
        public void Test_Distance()
        {
            Assert.Equal(2, _links.GetDistance(1, 3));
            Assert.Equal(4, _links.GetDistance(1, 4));
        }
    }
}
