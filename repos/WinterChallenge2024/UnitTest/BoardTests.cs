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
        private readonly Board board;
        public BoardTests(ITestOutputHelper output)
        {
            Converter converter = new Converter(output);
            Console.SetError(converter);

            board = new Board(22, 11);
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

            board.SetEntities(entities, true);

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

            List<Move> moves = board.PrunedCartesianProduct(moveActions, true, new int[4] { 6, 0, 0, 0 }).ToList();

            Assert.Equal(64, moves.Distinct().Count());
        }

        [Fact]
        public void Test_PrunedCartesianProduct_Full_Small()
        {
            MoveAction[][] moveActions = new MoveAction[2][];
            for (int i = 0; i < 2; i++)
            {
                moveActions[i] = new MoveAction[2 - i];
                for (int j = 0; j < 2; j++)
                {
                    if (2 - i == 1 && j == 1)
                        continue;
                    moveActions[i][j] = MoveAction.CreateGrow(0, new Point2d(i, j, i * 4 + j), EntityType.BASIC, 1);
                }
            }

            List<Move> moves = board.PrunedCartesianProduct(moveActions, true, new int[4] { 6, 0, 0, 0 }).ToList();

            Assert.Equal(2, moves.Distinct().Count());
        }

        [Fact]
        public void Test_PrunedCartesianProduct_Full_Uneven()
        {
            MoveAction[][] moveActions = new MoveAction[3][];
            for (int i = 0; i < 3; i++)
            {
                moveActions[i] = new MoveAction[i == 1 ? 2 : 4];
                for (int j = 0; j < 4; j++)
                {
                    if (i == 1 && j >= 2)
                        continue;
                    moveActions[i][j] = MoveAction.CreateGrow(0, new Point2d(i, j, i * 4 + j), EntityType.BASIC, 1);
                }
            }

            List<Move> moves = board.PrunedCartesianProduct(moveActions, true, new int[4] { 6, 0, 0, 0 }).ToList();

            Assert.Equal(32, moves.Distinct().Count());
        }


        [Fact]
        public void Test_PrunedCartesianProduct_ProteinLimited()
        {
            MoveAction[][] moveActions = new MoveAction[3][];
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

            List<Move> moves = board.PrunedCartesianProduct(moveActions, false, new int[4] { 2, 0, 0, 0 }).ToList();

            Assert.Equal(37, moves.Count);
        }

        [Fact]
        public void Test_PrunedCartesianProduct_ProteinExtraLimited()
        {
            MoveAction[][] moveActions = new MoveAction[3][];
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

            List<Move> moves = board.PrunedCartesianProduct(moveActions, false, new int[4] { 1, 0, 0, 0 }).ToList();

            Assert.Equal(10, moves.Count);
        }

        [Fact]
        public void Test_PrunedCartesianProduct_LocationLimited()
        {
            MoveAction[][] moveActions = new MoveAction[3][];
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

            List<Move> moves = board.PrunedCartesianProduct(moveActions, true, new int[4] { 4, 0, 0, 0 }).ToList();

            Assert.Equal(34, moves.Count);
        }

        [Fact]
        public void Test_PrunedCartesianProduct_LocationExtraLimited()
        {
            MoveAction[][] moveActions = new MoveAction[3][];
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

            List<Move> moves = board.PrunedCartesianProduct(moveActions, true, new int[4] { 4, 0, 0, 0 }).ToList();

            Assert.Equal(10, moves.Count);
        }

        [Fact]
        public void Test_PrunedCartesianProduct_Specific_LimitedProtein()
        {
            /*
             * 9: GROW 15 11 8 BASIC N -27;
GROW 15 12 9 BASIC N -27;
WAIT -9999;
13: GROW 18 13 9 BASIC N -26;
GROW 13 15 10 BASIC N -26;
WAIT -9999;
2: GROW 2 19 10 BASIC N -28;
GROW 8 16 10 BASIC N -27;
WAIT -9999;
Proteins: 1,2,4,1
             * */

            MoveAction[][] moves = new MoveAction[3][];
            moves[0] = new MoveAction[] {
                MoveAction.CreateGrow(15, new Point2d(11, 8, board.GetNodeIndex(11, 8)), EntityType.BASIC, 1),
                MoveAction.CreateGrow(15, new Point2d(12, 9, board.GetNodeIndex(12, 9)), EntityType.BASIC, 1),
                MoveAction.CreateWait()
            };
            moves[1] = new MoveAction[] {
                MoveAction.CreateGrow(18, new Point2d(13, 9, board.GetNodeIndex(13, 9)), EntityType.BASIC, 1),
                MoveAction.CreateGrow(13, new Point2d(15, 0, board.GetNodeIndex(15, 0)), EntityType.BASIC, 1),
                MoveAction.CreateWait()
            };
            moves[2] = new MoveAction[] {
                MoveAction.CreateGrow(2, new Point2d(19, 10, board.GetNodeIndex(19, 10)), EntityType.BASIC, 1),
                MoveAction.CreateGrow(8, new Point2d(16, 10, board.GetNodeIndex(16, 10)), EntityType.BASIC, 1),
                MoveAction.CreateWait()
            };
            IEnumerable<Move> finalMoves = board.PrunedCartesianProduct(moves, false, new int[] { 1, 2, 4, 1 });

            Assert.Equal(7, finalMoves.Count());
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
            }, true);

            Assert.True(board.IsOpenSpace(board.GetNodeIndex(0, 1), true));

            Assert.Single(board.GetGrowMoveActions(1, true, new HashSet<int>(), new Board.ProteinInfo(new int[] { 1, 1, 1, 1 })));

            board.Print();

            board = new Board(3, 3);
            board.SetEntities(new List<Entity>()
            {
                new Entity(new Point2d(2, 1, board.GetNodeIndex(2, 1)), EntityType.WALL, null, 0, 0, 0, OrganDirection.None)
            }, true);
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

            board.Print();

            Assert.Single(board.GetGrowMoveActions(2, true, new HashSet<int>(), new Board.ProteinInfo(new int[] { 1, 1, 1, 1 })));

            board = new Board(2, 2);
            board.SetEntities(new List<Entity>()
            {

                new Entity(new Point2d(0, 1, board.GetNodeIndex(0, 1)), EntityType.TENTACLE, true, 2, 2, 2, OrganDirection.North),

                new Entity(new Point2d(1, 0, board.GetNodeIndex(1, 0)), EntityType.TENTACLE, false, 4, 1, 1, OrganDirection.East),
                new Entity(new Point2d(1, 1, board.GetNodeIndex(1, 1)), EntityType.WALL, null, 0, 0, 0, OrganDirection.None)
            }, true);

            board.Print();

            Assert.True(board.IsOpenSpace(board.GetNodeIndex(0, 0), true));
        }

        [Fact]
        public void Test_GetGrowMoves()
        {
            Board board = new Board(2, 1);
            board.SetEntities(new List<Entity>(), true);
            board.SetEntities(new List<Entity>()
            {
                new Entity(new Point2d(1, 0, board.GetNodeIndex(1, 0)), EntityType.HARVESTER, true, 3, 3, 3, OrganDirection.West),
                new Entity(0, 0, board.GetNodeIndex(0, 0), "A", -1, 0, "X", 0, 0)
            });

            List<MoveAction> moves = board.GetGrowMoveActions(3, true, new HashSet<int>(), new Board.ProteinInfo(new int[] { 1, 1, 1, 1 }));
            Assert.Single(moves);
            Assert.Equal(1000, moves[0].Score);


            board = new Board(2, 3);
            board.SetEntities(new List<Entity>() {
                new Entity(0, 0, board.GetNodeIndex(0, 0), "WALL", -1, 0, "X", 0, 0),
                new Entity(new Point2d(0, 1, board.GetNodeIndex(0, 1)), EntityType.D, null, 0, 0, 0, OrganDirection.None),
                new Entity(new Point2d(0, 2, board.GetNodeIndex(0, 2)), EntityType.BASIC, false, 4, 4, 4, OrganDirection.West),


                new Entity(new Point2d(1, 1, board.GetNodeIndex(1, 1)), EntityType.BASIC, true, 3, 3, 3, OrganDirection.West),
                new Entity(1, 2, board.GetNodeIndex(1, 2), "WALL", -1, 0, "X", 0, 0),
            }, true);

            board.Print();

            moves = board.GetGrowMoveActions(3, true, new HashSet<int>(), new Board.ProteinInfo(new int[] { 0, 0, 0, 0 }));
            Assert.Equal(2, moves.Count);
            Assert.Equal(-1020, moves[1].Score);

            moves = board.GetGrowBasicMoveActions(3, true, new HashSet<int>(), new Board.ProteinInfo(new int[] { 0, 0, 0, 0 }));
            Assert.Single(moves);
            Assert.Equal(-1050, moves[0].Score);
        }
    }
}
