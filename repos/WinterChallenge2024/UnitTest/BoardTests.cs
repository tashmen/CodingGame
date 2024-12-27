using GameSolution.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
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
             new Entity(0, 0, board.GetNodeIndex(0,0), "WALL", -1, 0, "X", 0, 0),
             new Entity(1, 2, board.GetNodeIndex(1,2), "ROOT", 1, 1, "N", 1, 1),
             new Entity(1, 6, board.GetNodeIndex(1,6), "ROOT", 0, 2, "N", 2, 2),
          };

            board.SetEntities(entities);

            Assert.Equal(3, board.GlobalOrganId);
            Assert.Equal(1, board.GetMyEntityCount());
            Assert.Equal(1, board.GetOppEntityCount());
        }

        [Fact]
        public void Test_PrunedCartesianProduct()
        {
            MoveAction[][] moveActions = new MoveAction[3][];
            for (int i = 0; i < 3; i++)
            {
                moveActions[i] = new MoveAction[4];
                for (int j = 0; j < 4; j++)
                {
                    moveActions[i][j] = MoveAction.CreateGrow(j, new Point2d(i, j, 0), EntityType.BASIC, 1);
                }
            }

            var moves = board.PrunedCartesianProduct(moveActions, true, new int[4] { 2, 0, 0, 0 }).ToList();

            Assert.Equal(24, moves.Count);
        }
    }
}
