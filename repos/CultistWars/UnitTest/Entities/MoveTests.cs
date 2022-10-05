using GameSolution.Entities;
using Xunit;

namespace UnitTest.Entities
{
    public class MoveTests
    {
        [Fact]
        public void TestMove()
        {
            long m = Move.Wait();
            Assert.Equal(MoveType.Wait, Move.GetMoveType(m));

            m = Move.MoveUnit(3, Board.ConvertPointToLocation(5, 4));
            Assert.Equal(MoveType.Move, Move.GetMoveType(m));
            Assert.Equal(3, Move.GetUnitId(m));
            Assert.Equal(Board.ConvertPointToLocation(5, 4), Move.GetLocation(m));

            m = Move.Convert(5, 13);
            Assert.Equal(MoveType.Convert, Move.GetMoveType(m));
            Assert.Equal(5, Move.GetUnitId(m));
            Assert.Equal(13, Move.GetTargetUnitId(m));
        }
    }
}
