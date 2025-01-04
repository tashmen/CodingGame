using GameSolution.Entities;
using System.Collections.Generic;

namespace GameSolution.Game
{
    public static class GameBuilder
    {
        public static GameState BuildEmptyGame(bool setNextTurn = true)
        {
            GameState game = new GameState();


            return game;
        }

        public static GameState BuildWood4Game()
        {
            GameState game = new GameState();

            Board board = new Board(18, 9);

            List<Entity> entities = new List<Entity>()
            {
                new Entity(1, 2, board.GetNodeIndex(1, 2), "ROOT", 1, 1, "N", 1, 1),
                new Entity(1, 6,board.GetNodeIndex(1, 6), "ROOT", 0, 2, "N", 2, 2),

                new Entity(4, 1, board.GetNodeIndex(4, 1), "A", -1, 0, "X", 0, 0),
                new Entity(6, 1, board.GetNodeIndex(6, 1), "A", -1, 0, "X", 0, 0),
                new Entity(8, 2, board.GetNodeIndex(8, 2), "A", -1, 0, "X", 0, 0),
                new Entity(12, 2, board.GetNodeIndex(12, 2), "A", -1, 0, "X", 0, 0),
                new Entity(16, 2, board.GetNodeIndex(16, 2), "A", -1, 0, "X", 0, 0),
                new Entity(3, 3, board.GetNodeIndex(3, 3), "A", -1, 0, "X", 0, 0),
                new Entity(5, 3, board.GetNodeIndex(5, 3), "A", -1, 0, "X", 0, 0),
                new Entity(10, 3, board.GetNodeIndex(10, 3), "A", -1, 0, "X", 0, 0),
                new Entity(14, 3, board.GetNodeIndex(14, 3), "A", -1, 0, "X", 0, 0),
                new Entity(4, 5, board.GetNodeIndex(4, 5), "A", -1, 0, "X", 0, 0),
                new Entity(6, 5, board.GetNodeIndex(6, 5), "A", -1, 0, "X", 0, 0),
                new Entity(8, 6, board.GetNodeIndex(8, 6), "A", -1, 0, "X", 0, 0),
                new Entity(12, 6, board.GetNodeIndex(12, 6), "A", -1, 0, "X", 0, 0),
                new Entity(16, 6, board.GetNodeIndex(16, 6), "A", -1, 0, "X", 0, 0),
                new Entity(3, 7, board.GetNodeIndex(3, 7), "A", -1, 0, "X", 0, 0),
                new Entity(4, 7, board.GetNodeIndex(4, 7), "A", -1, 0, "X", 0, 0),
                new Entity(10, 7, board.GetNodeIndex(10, 7), "A", -1, 0, "X", 0, 0),
                new Entity(14, 7, board.GetNodeIndex(14, 7), "A", -1, 0, "X", 0, 0)
            };

            for (int x = 0; x < 18; x++)
            {
                entities.Add(new Entity(x, 0, board.GetNodeIndex(x, 0), "WALL", -1, 0, "X", 0, 0));
                entities.Add(new Entity(x, 8, board.GetNodeIndex(x, 8), "WALL", -1, 0, "X", 0, 0));
                if (x != 16)
                {
                    entities.Add(new Entity(0, 4, board.GetNodeIndex(x, 4), "WALL", -1, 0, "X", 0, 0));
                }
            }
            for (int y = 0; y < 8; y++)
            {
                if (y != 4)
                {
                    entities.Add(new Entity(0, y, board.GetNodeIndex(0, y), "WALL", -1, 0, "X", 0, 0));
                    entities.Add(new Entity(17, y, board.GetNodeIndex(17, y), "WALL", -1, 0, "X", 0, 0));
                }
            }


            board.SetEntities(entities.ToArray(), true);
            board.Print();

            game.SetNextTurn(board, new int[] { 10, 0, 0, 0 }, new int[] { 10, 0, 0, 0 });

            return game;
        }

        public static GameState BuildWood3Game()
        {
            GameState game = new GameState();

            Board board = new Board(18, 9);
            List<Entity> entities = new List<Entity>()
            {
                new Entity(1, 2, board.GetNodeIndex(1, 2), "ROOT", 1, 1, "N", 1, 1),
                new Entity(1, 6, board.GetNodeIndex(1, 6), "ROOT", 0, 2, "N", 2, 2),

                new Entity(4, 1, board.GetNodeIndex(4, 1), "A", -1, 0, "X", 0, 0),
                new Entity(4, 5, board.GetNodeIndex(4, 5), "A", -1, 0, "X", 0, 0)
            };

            for (int x = 0; x < 18; x++)
            {
                entities.Add(new Entity(x, 0, board.GetNodeIndex(x, 0), "WALL", -1, 0, "X", 0, 0));
                entities.Add(new Entity(x, 8, board.GetNodeIndex(x, 8), "WALL", -1, 0, "X", 0, 0));
                if (x != 16)
                {
                    entities.Add(new Entity(0, 4, board.GetNodeIndex(x, 4), "WALL", -1, 0, "X", 0, 0));
                }
            }
            for (int y = 0; y < 8; y++)
            {
                if (y != 4)
                {
                    entities.Add(new Entity(0, y, board.GetNodeIndex(0, y), "WALL", -1, 0, "X", 0, 0));
                    entities.Add(new Entity(17, y, board.GetNodeIndex(17, y), "WALL", -1, 0, "X", 0, 0));
                }
            }


            board.SetEntities(entities.ToArray(), true);
            board.Print();

            game.SetNextTurn(board, new int[] { 10, 0, 1, 1 }, new int[] { 10, 0, 1, 1 });

            return game;
        }

        public static GameState BuildWood2Game()
        {
            GameState game = new GameState();

            Board board = new Board(18, 8);
            List<Entity> entities = new List<Entity>()
          {
             new Entity(1, 2, board.GetNodeIndex(1, 2), "ROOT", 1, 1, "N", 1, 1),
             new Entity(16, 5, board.GetNodeIndex(16, 5), "ROOT", 0, 2, "N", 2, 2)
          };

            for (int x = 0; x < 18; x++)
            {
                entities.Add(new Entity(x, 0, board.GetNodeIndex(x, 0), "WALL", -1, 0, "X", 0, 0));
                entities.Add(new Entity(x, 1, board.GetNodeIndex(x, 1), "WALL", -1, 0, "X", 0, 0));
                entities.Add(new Entity(x, 6, board.GetNodeIndex(x, 6), "WALL", -1, 0, "X", 0, 0));
                entities.Add(new Entity(x, 7, board.GetNodeIndex(x, 7), "WALL", -1, 0, "X", 0, 0));
            }
            for (int y = 0; y < 8; y++)
            {

                entities.Add(new Entity(0, y, board.GetNodeIndex(0, y), "WALL", -1, 0, "X", 0, 0));
                entities.Add(new Entity(17, y, board.GetNodeIndex(17, y), "WALL", -1, 0, "X", 0, 0));
            }


            board.SetEntities(entities.ToArray(), true);
            board.Print();

            game.SetNextTurn(board, new int[] { 50, 05, 05, 0 }, new int[] { 50, 05, 05, 0 });

            return game;
        }

        public static GameState BuildWood1Game()
        {
            GameState game = new GameState();

            Board board = new Board(18, 9);
            List<Entity> entities = new List<Entity>()
            {
                new Entity(1, 2, board.GetNodeIndex(1, 2), "ROOT", 1, 1, "N", 1, 1),
                new Entity(1, 6, board.GetNodeIndex(1, 6), "ROOT", 0, 2, "N", 2, 2),

                new Entity(16, 1, board.GetNodeIndex(16, 1), "A", -1, 0, "X", 0, 0),
                new Entity(16, 5, board.GetNodeIndex(16, 5), "A", -1, 0, "X", 0, 0)
            };
            for (int x = 0; x < 18; x++)
            {
                entities.Add(new Entity(x, 0, board.GetNodeIndex(x, 0), "WALL", -1, 0, "X", 0, 0));
                entities.Add(new Entity(x, 8, board.GetNodeIndex(x, 8), "WALL", -1, 0, "X", 0, 0));
                if (x != 16)
                    entities.Add(new Entity(x, 4, board.GetNodeIndex(x, 4), "WALL", -1, 0, "X", 0, 0));
            }
            for (int y = 1; y < 8; y++)
            {
                if (y != 4)
                {
                    entities.Add(new Entity(0, y, board.GetNodeIndex(0, y), "WALL", -1, 0, "X", 0, 0));
                    entities.Add(new Entity(17, y, board.GetNodeIndex(17, y), "WALL", -1, 0, "X", 0, 0));
                }
            }

            board.SetEntities(entities.ToArray(), true);
            board.Print();

            game.SetNextTurn(board, new int[] { 6, 2, 2, 3 }, new int[] { 6, 2, 2, 3 });

            return game;
        }

        public static GameState BuildSilverGame()
        {
            GameState game = new GameState();

            Board board = new Board(16, 8);
            List<Entity> entities = new List<Entity>()
          {
             new Entity(1, 2, board.GetNodeIndex(1,2), "WALL", -1, 0, "X", 0, 0),
             new Entity(2, 2, board.GetNodeIndex(2, 2), "WALL", -1, 0, "X", 0, 0),
             new Entity(6, 1, board.GetNodeIndex(6, 1), "WALL", -1, 0, "X", 0, 0),
             new Entity(9, 1, board.GetNodeIndex(9, 1), "WALL", -1, 0, "X", 0, 0),
             new Entity(13, 1, board.GetNodeIndex(13,1), "WALL", -1, 0, "X", 0, 0),
             new Entity(14, 1, board.GetNodeIndex(14,1), "WALL", -1, 0, "X", 0, 0),
             new Entity(12, 2, board.GetNodeIndex(12,2), "WALL", -1, 0, "X", 0, 0),
             new Entity(5, 3, board.GetNodeIndex(5,3), "WALL", -1, 0, "X", 0, 0),
             new Entity(8, 3, board.GetNodeIndex(8,3), "WALL", -1, 0, "X", 0, 0),
             new Entity(7, 4, board.GetNodeIndex(7,4), "WALL", -1, 0, "X", 0, 0),
             new Entity(10, 4, board.GetNodeIndex(10,4), "WALL", -1, 0, "X", 0, 0),
             new Entity(3, 5, board.GetNodeIndex(3,5), "WALL", -1, 0, "X", 0, 0),
             new Entity(13, 5, board.GetNodeIndex(13,5), "WALL", -1, 0, "X", 0, 0),
             new Entity(14, 5, board.GetNodeIndex(14,5), "WALL", -1, 0, "X", 0, 0),
             new Entity(1, 6, board.GetNodeIndex(1,6), "WALL", -1, 0, "X", 0, 0),
             new Entity(2, 6, board.GetNodeIndex(2,6), "WALL", -1, 0, "X", 0, 0),
             new Entity(6, 6, board.GetNodeIndex(6,6), "WALL", -1, 0, "X", 0, 0),
             new Entity(9, 6, board.GetNodeIndex(9,6), "WALL", -1, 0, "X", 0, 0),

             new Entity(3, 1, board.GetNodeIndex(3,1), "ROOT", 1, 1, "N", 1, 1),
             new Entity(12, 6, board.GetNodeIndex(12,6), "ROOT", 0, 2, "N", 2, 2),

             new Entity(10, 1, board.GetNodeIndex(10,1), "A", -1, 0, "X", 0, 0),
             new Entity(0, 2, board.GetNodeIndex(0,2), "A", -1, 0, "X", 0, 0),
             new Entity(15, 5, board.GetNodeIndex(15,5), "A", -1, 0, "X", 0, 0),
             new Entity(5, 6, board.GetNodeIndex(5,6), "A", -1, 0, "X", 0, 0),

             new Entity(6, 0, board.GetNodeIndex(6,0), "B", -1, 0, "X", 0, 0),
             new Entity(4, 2, board.GetNodeIndex(4,2), "B", -1, 0, "X", 0, 0),
             new Entity(11, 5, board.GetNodeIndex(11,5), "B", -1, 0, "X", 0, 0),
             new Entity(9, 7, board.GetNodeIndex(9,7), "B", -1, 0, "X", 0, 0),

             new Entity(2, 1, board.GetNodeIndex(2,1), "C", -1, 0, "X", 0, 0),
             new Entity(6, 2, board.GetNodeIndex(6,2), "C", -1, 0, "X", 0, 0),
             new Entity(9, 5, board.GetNodeIndex(9,5), "C", -1, 0, "X", 0, 0),
             new Entity(13, 6, board.GetNodeIndex(13,6), "C", -1, 0, "X", 0, 0),

             new Entity(5, 0, board.GetNodeIndex(5,0), "D", -1, 0, "X", 0, 0),
             new Entity(13, 0, board.GetNodeIndex(13,0), "D", -1, 0, "X", 0, 0),
             new Entity(2, 7, board.GetNodeIndex(2,7), "D", -1, 0, "X", 0, 0),
             new Entity(10, 7, board.GetNodeIndex(10,7), "D", -1, 0, "X", 0, 0)
          };
            board.SetEntities(entities.ToArray(), true);
            board.Print();

            game.SetNextTurn(board, new int[] { 6, 2, 2, 3 }, new int[] { 6, 2, 2, 3 });

            return game;
        }

        public static GameState BuildNoMovesGame()
        {
            GameState game = new GameState();

            Board board = new Board(2, 1);
            List<Entity> entities = new List<Entity>()
          {
             new Entity(0, 0, board.GetNodeIndex(0,0), "ROOT", 1, 1, "N", 1, 1),
             new Entity(1, 0, board.GetNodeIndex(1,0), "ROOT", 0, 2, "N", 2, 2),
          };
            board.SetEntities(entities.ToArray(), true);
            board.Print();

            game.SetNextTurn(board, new int[] { 6, 2, 2, 3 }, new int[] { 6, 2, 2, 3 });

            return game;
        }
    }
}
