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
        public void Test_PrunedCartesianProduct_Full()
        {
            MoveAction[][] moveActions = new MoveAction[3][];
            for (int i = 0; i < 3; i++)
            {
                moveActions[i] = new MoveAction[4];
                for (int j = 0; j < 4; j++)
                {
                    moveActions[i][j] = MoveAction.CreateGrow(0, new Point2d(i, j, i * 4 + j), EntityType.BASIC, 1);
                }
            }

            var moves = board.PrunedCartesianProduct(moveActions, true, new int[4] { 6, 0, 0, 0 }).ToList();

            Assert.Equal(64, moves.Count);

        }


        [Fact]
        public void Test_PrunedCartesianProduct_ProteinLimited()
        {
            var moveActions = new MoveAction[3][];
            for (int i = 0; i < 3; i++)
            {
                moveActions[i] = new MoveAction[4];
                for (int j = 0; j < 4; j++)
                {
                    if (j == 0)
                    {
                        moveActions[i][j] = MoveAction.CreateWait();
                    }
                    else moveActions[i][j] = MoveAction.CreateGrow(0, new Point2d(i, j, i * 4 + j), EntityType.BASIC, 1);
                }
            }

            var moves = board.PrunedCartesianProduct(moveActions, false, new int[4] { 2, 0, 0, 0 }).ToList();

            Assert.Equal(37, moves.Count);
        }

        [Fact]
        public void Test_PrunedCartesianProduct_ProteinExtraLimited()
        {
            var moveActions = new MoveAction[3][];
            for (int i = 0; i < 3; i++)
            {
                moveActions[i] = new MoveAction[4];
                for (int j = 0; j < 4; j++)
                {
                    if (j == 0)
                    {
                        moveActions[i][j] = MoveAction.CreateWait();
                    }
                    else moveActions[i][j] = MoveAction.CreateGrow(0, new Point2d(i, j, i * 4 + j), EntityType.BASIC, 1);
                }
            }

            var moves = board.PrunedCartesianProduct(moveActions, false, new int[4] { 1, 0, 0, 0 }).ToList();

            Assert.Equal(10, moves.Count);
        }

        [Fact]
        public void Test_PrunedCartesianProduct_LocationLimited()
        {
            var moveActions = new MoveAction[3][];
            for (int i = 0; i < 3; i++)
            {
                moveActions[i] = new MoveAction[4];
                for (int j = 0; j < 4; j++)
                {
                    if (j == 0)
                    {
                        moveActions[i][j] = MoveAction.CreateWait();
                    }
                    else moveActions[i][j] = MoveAction.CreateGrow(0, new Point2d(0, j, 0 * 4 + j), EntityType.BASIC, 1);
                }
            }

            var moves = board.PrunedCartesianProduct(moveActions, true, new int[4] { 4, 0, 0, 0 }).ToList();

            Assert.Equal(34, moves.Count);
        }

        [Fact]
        public void Test_PrunedCartesianProduct_LocationExtraLimited()
        {
            var moveActions = new MoveAction[3][];
            for (int i = 0; i < 3; i++)
            {
                moveActions[i] = new MoveAction[4];
                for (int j = 0; j < 4; j++)
                {
                    if (j == 0)
                    {
                        moveActions[i][j] = MoveAction.CreateWait();
                    }
                    else moveActions[i][j] = MoveAction.CreateGrow(0, new Point2d(0, 0, 0 * 4 + 0), EntityType.BASIC, 1);
                }
            }

            var moves = board.PrunedCartesianProduct(moveActions, true, new int[4] { 4, 0, 0, 0 }).ToList();

            Assert.Equal(10, moves.Count);
        }
    }
}
