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

            Assert.Equal(64, moves.Distinct().Count());


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

        [Fact]
        public void Test_IsOpenSpace()
        {
            Board board = new Board(2, 2);
            board.SetEntities(new List<Entity>()
            {
                new Entity(new Point2d(0, 0, board.GetNodeIndex(0, 0)), EntityType.TENTACLE, true, 1, 1, 1, OrganDirection.South),
                new Entity(new Point2d(1, 0, board.GetNodeIndex(1, 0)), EntityType.WALL, null, 0, 0, 0, OrganDirection.None),
                new Entity(new Point2d(1, 1, board.GetNodeIndex(1, 1)), EntityType.WALL, null, 0, 0, 0, OrganDirection.None)
            });

            Assert.True(board.IsOpenSpace(board.GetNodeIndex(0, 1), true));

            Assert.Single(board.GetGrowMoveActions(1, true));

            board.Print();

            board = new Board(3, 3);
            board.SetEntities(new List<Entity>()
            {
                new Entity(new Point2d(0, 0, board.GetNodeIndex(0, 0)), EntityType.TENTACLE, true, 1, 1, 1, OrganDirection.South),
                new Entity(new Point2d(0, 1, board.GetNodeIndex(0, 1)), EntityType.TENTACLE, true, 2, 2, 2, OrganDirection.East),

                new Entity(new Point2d(1, 0, board.GetNodeIndex(1, 0)), EntityType.TENTACLE, true, 4, 1, 1, OrganDirection.South),

                new Entity(new Point2d(1, 2, board.GetNodeIndex(1, 2)), EntityType.TENTACLE, false, 3, 3, 3, OrganDirection.North),
                new Entity(new Point2d(2, 0, board.GetNodeIndex(2, 0)), EntityType.TENTACLE, true, 5, 1, 1, OrganDirection.West),
                new Entity(new Point2d(2, 1, board.GetNodeIndex(2, 1)), EntityType.WALL, null, 0, 0, 0, OrganDirection.None),
                new Entity(new Point2d(2, 2, board.GetNodeIndex(2, 2)), EntityType.BASIC, false, 6, 3, 3, OrganDirection.North)
            });

            Assert.Single(board.GetGrowMoveActions(2, true));

            board.Print();
        }
    }
}
