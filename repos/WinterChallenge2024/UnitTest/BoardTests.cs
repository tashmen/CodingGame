using GameSolution.Entities;
using System;
using System.Collections;
using System.Collections.Generic;
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

            board = new Board(18, 9);
        }

        [Fact]
        public void Test_SetEntities()
        {
            List<Entity> entities = new List<Entity>()
            {
                new Entity(0, 0, "WALL", -1, 0, "X", 0, 0),
                new Entity(1, 2, "ROOT", 1, 1, "N", 1, 1),
                new Entity(1, 6, "ROOT", 0, 2, "N", 2, 2),
            };

            board.SetEntities(entities);

            Assert.Equal(2, board.GlobalOrganId);
            Assert.Equal(1, board.GetMyEntityCount());
            Assert.Equal(1, board.GetOppEntityCount());
        }
    }
}
