using GameSolution.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

            m = Move.MoveUnit(3, 5, 8);
            Assert.Equal(MoveType.Move, Move.GetMoveType(m));
            Assert.Equal(3, Move.GetUnitId(m));
            Assert.Equal(5, Move.GetX(m));
            Assert.Equal(8, Move.GetY(m));

            m = Move.Convert(5, 13);
            Assert.Equal(MoveType.Convert, Move.GetMoveType(m));
            Assert.Equal(5, Move.GetUnitId(m));
            Assert.Equal(13, Move.GetTargetUnitId(m));
        }
    }
}
