using GameSolution.Entities;
using System;
using System.Collections;
using Xunit;
using Xunit.Abstractions;

namespace UnitTest
{

    public class BoardTests
    {
        private Board board;
        public BoardTests(ITestOutputHelper output)
        {
            var converter = new Converter(output);
            Console.SetError(converter);

            //board = new Board();
        }

        [Fact]
        public void BoardPiece_Test()
        {
            Monster m = new Monster(250, 567, 789, null, 20, 0, false, 0, 300, true, true);

            Assert.Equal(250, m.id);
            Assert.Equal(567, m.x);
            Assert.Equal(789, m.y);
            Assert.Null(m.isMax);
            Assert.Equal(20, m.health);
            Assert.Equal(0, m.shieldLife);
            Assert.False(m.isControlled);
            Assert.Equal(0, m.vx);
            Assert.Equal(300, m.vy);
            Assert.True(m.isNearBase);
            Assert.True(m.threatForMax);

            Hero h = new Hero(600, 18000, 9000, false, 12, true, -6600, -6600, true);

            Assert.Equal(600, h.id);
            Assert.Equal(18000, h.x);
            Assert.Equal(9000, h.y);
            Assert.False(h.isMax);
            Assert.Equal(12, h.shieldLife);
            Assert.True(h.isControlled);
            Assert.Equal(-6600, h.vx);
            Assert.Equal(-6600, h.vy);
            Assert.True(h.isNearBase);

            Base b = new Base(BoardPiece.MaxEntityId - 1, 17620, 9000, true, 3, 2000);

            Assert.Equal(BoardPiece.MaxEntityId - 1, b.id);
            Assert.Equal(17620, b.x);
            Assert.Equal(9000, b.y);
            Assert.True(b.isMax);
            Assert.Equal(3, b.health);
            Assert.Equal(2000, b.mana);

            b = new Base(BoardPiece.MaxEntityId - 2, 0, 0, false, 3, 2000);
            Assert.False(b.isMax);
        }
    }
}
