using Algorithms.Space;
using GameSolution.Entities;
using Xunit;

namespace UnitTest.Entities
{
    public class BoardTest
    {

        [Fact]
        public void Test_PointToLocationConversion()
        {
            var location = Board.ConvertPointToLocation(2, 3);
            var point = Board.ConvertLocationToPoint(location);

            Assert.Equal(new Point2d(2, 3), point);
        }
    }
}
