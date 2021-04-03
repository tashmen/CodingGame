using Xunit;
using GameSolution;
using GameSolution.Entities;

namespace UnitTests.Entities
{
    public class EntityTests
    {
        public EntityTests()
        {

        }

        [Fact]
        public void Test_Creation()
        {
            Entity e = new Entity(1, 1, 1,1,1, 1);
            Assert.True(true);
        }
    }
}
