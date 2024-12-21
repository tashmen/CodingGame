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

        public static GameState BuildBasicGame()
        {
            GameState game = new GameState();

            Board board = new Board(18, 9);
            List<Entity> entities = new List<Entity>()
            {
                new Entity(0, 0, "WALL", -1, 0, "X", 0, 0),
                new Entity(1, 0, "WALL", -1, 0, "X", 0, 0),
                new Entity(2, 0, "WALL", -1, 0, "X", 0, 0),
                new Entity(3, 0, "WALL", -1, 0, "X", 0, 0),
                new Entity(4, 0, "WALL", -1, 0, "X", 0, 0),
                new Entity(5, 0, "WALL", -1, 0, "X", 0, 0),
                new Entity(6, 0, "WALL", -1, 0, "X", 0, 0),
                new Entity(7, 0, "WALL", -1, 0, "X", 0, 0),
                new Entity(8, 0, "WALL", -1, 0, "X", 0, 0),
                new Entity(9, 0, "WALL", -1, 0, "X", 0, 0),
                new Entity(10, 0, "WALL", -1, 0, "X", 0, 0),
                new Entity(11, 0, "WALL", -1, 0, "X", 0, 0),
                new Entity(12, 0, "WALL", -1, 0, "X", 0, 0),
                new Entity(13, 0, "WALL", -1, 0, "X", 0, 0),
                new Entity(14, 0, "WALL", -1, 0, "X", 0, 0),
                new Entity(15, 0, "WALL", -1, 0, "X", 0, 0),
                new Entity(16, 0, "WALL", -1, 0, "X", 0, 0),
                new Entity(17, 0, "WALL", -1, 0, "X", 0, 0),

                new Entity(0, 8, "WALL", -1, 0, "X", 0, 0),
                new Entity(1, 8, "WALL", -1, 0, "X", 0, 0),
                new Entity(2, 8, "WALL", -1, 0, "X", 0, 0),
                new Entity(3, 8, "WALL", -1, 0, "X", 0, 0),
                new Entity(4, 8, "WALL", -1, 0, "X", 0, 0),
                new Entity(5, 8, "WALL", -1, 0, "X", 0, 0),
                new Entity(6, 8, "WALL", -1, 0, "X", 0, 0),
                new Entity(7, 8, "WALL", -1, 0, "X", 0, 0),
                new Entity(8, 8, "WALL", -1, 0, "X", 0, 0),
                new Entity(9, 8, "WALL", -1, 0, "X", 0, 0),
                new Entity(10, 8, "WALL", -1, 0, "X", 0, 0),
                new Entity(11, 8, "WALL", -1, 0, "X", 0, 0),
                new Entity(12, 8, "WALL", -1, 0, "X", 0, 0),
                new Entity(13, 8, "WALL", -1, 0, "X", 0, 0),
                new Entity(14, 8, "WALL", -1, 0, "X", 0, 0),
                new Entity(15, 8, "WALL", -1, 0, "X", 0, 0),
                new Entity(16, 8, "WALL", -1, 0, "X", 0, 0),
                new Entity(17, 8, "WALL", -1, 0, "X", 0, 0),

                new Entity(0, 4, "WALL", -1, 0, "X", 0, 0),
                new Entity(1, 4, "WALL", -1, 0, "X", 0, 0),
                new Entity(2, 4, "WALL", -1, 0, "X", 0, 0),
                new Entity(3, 4, "WALL", -1, 0, "X", 0, 0),
                new Entity(4, 4, "WALL", -1, 0, "X", 0, 0),
                new Entity(5, 4, "WALL", -1, 0, "X", 0, 0),
                new Entity(6, 4, "WALL", -1, 0, "X", 0, 0),
                new Entity(7, 4, "WALL", -1, 0, "X", 0, 0),
                new Entity(8, 4, "WALL", -1, 0, "X", 0, 0),
                new Entity(9, 4, "WALL", -1, 0, "X", 0, 0),
                new Entity(10, 4, "WALL", -1, 0, "X", 0, 0),
                new Entity(11, 4, "WALL", -1, 0, "X", 0, 0),
                new Entity(12, 4, "WALL", -1, 0, "X", 0, 0),
                new Entity(13, 4, "WALL", -1, 0, "X", 0, 0),
                new Entity(14, 4, "WALL", -1, 0, "X", 0, 0),
                new Entity(15, 4, "WALL", -1, 0, "X", 0, 0),
                new Entity(17, 4, "WALL", -1, 0, "X", 0, 0),

                new Entity(0, 1, "WALL", -1, 0, "X", 0, 0),
                new Entity(0, 2, "WALL", -1, 0, "X", 0, 0),
                new Entity(0, 3, "WALL", -1, 0, "X", 0, 0),
                new Entity(0, 5, "WALL", -1, 0, "X", 0, 0),
                new Entity(0, 6, "WALL", -1, 0, "X", 0, 0),
                new Entity(0, 7, "WALL", -1, 0, "X", 0, 0),

                new Entity(17, 1, "WALL", -1, 0, "X", 0, 0),
                new Entity(17, 2, "WALL", -1, 0, "X", 0, 0),
                new Entity(17, 3, "WALL", -1, 0, "X", 0, 0),
                new Entity(17, 5, "WALL", -1, 0, "X", 0, 0),
                new Entity(17, 6, "WALL", -1, 0, "X", 0, 0),
                new Entity(17, 7, "WALL", -1, 0, "X", 0, 0),

                new Entity(1, 2, "ROOT", 1, 1, "N", 1, 1),
                new Entity(1, 6, "ROOT", 0, 2, "N", 2, 2),

                new Entity(4, 1, "A", -1, 0, "X", 0, 0),
                new Entity(6, 1, "A", -1, 0, "X", 0, 0),
                new Entity(8, 2, "A", -1, 0, "X", 0, 0),
                new Entity(12, 2, "A", -1, 0, "X", 0, 0),
                new Entity(16, 2, "A", -1, 0, "X", 0, 0),
                new Entity(3, 3, "A", -1, 0, "X", 0, 0),
                new Entity(5, 3, "A", -1, 0, "X", 0, 0),
                new Entity(10, 3, "A", -1, 0, "X", 0, 0),
                new Entity(14, 3, "A", -1, 0, "X", 0, 0),
                new Entity(4, 5, "A", -1, 0, "X", 0, 0),
                new Entity(6, 5, "A", -1, 0, "X", 0, 0),
                new Entity(8, 6, "A", -1, 0, "X", 0, 0),
                new Entity(12, 6, "A", -1, 0, "X", 0, 0),
                new Entity(16, 6, "A", -1, 0, "X", 0, 0),
                new Entity(3, 7, "A", -1, 0, "X", 0, 0),
                new Entity(4, 7, "A", -1, 0, "X", 0, 0),
                new Entity(10, 7, "A", -1, 0, "X", 0, 0),
                new Entity(14, 7, "A", -1, 0, "X", 0, 0)
            };
            board.SetEntities(entities);
            board.Print();

            game.SetNextTurn(board, new Dictionary<EntityType, int>() { { EntityType.A, 10 } }, new Dictionary<EntityType, int>() { { EntityType.A, 10 } });

            return game;
        }

        public static GameState BuildWood3Game()
        {
            GameState game = new GameState();

            Board board = new Board(18, 9);
            List<Entity> entities = new List<Entity>()
            {
                new Entity(0, 0, "WALL", -1, 0, "X", 0, 0),
                new Entity(1, 0, "WALL", -1, 0, "X", 0, 0),
                new Entity(2, 0, "WALL", -1, 0, "X", 0, 0),
                new Entity(3, 0, "WALL", -1, 0, "X", 0, 0),
                new Entity(4, 0, "WALL", -1, 0, "X", 0, 0),
                new Entity(5, 0, "WALL", -1, 0, "X", 0, 0),
                new Entity(6, 0, "WALL", -1, 0, "X", 0, 0),
                new Entity(7, 0, "WALL", -1, 0, "X", 0, 0),
                new Entity(8, 0, "WALL", -1, 0, "X", 0, 0),
                new Entity(9, 0, "WALL", -1, 0, "X", 0, 0),
                new Entity(10, 0, "WALL", -1, 0, "X", 0, 0),
                new Entity(11, 0, "WALL", -1, 0, "X", 0, 0),
                new Entity(12, 0, "WALL", -1, 0, "X", 0, 0),
                new Entity(13, 0, "WALL", -1, 0, "X", 0, 0),
                new Entity(14, 0, "WALL", -1, 0, "X", 0, 0),
                new Entity(15, 0, "WALL", -1, 0, "X", 0, 0),
                new Entity(16, 0, "WALL", -1, 0, "X", 0, 0),
                new Entity(17, 0, "WALL", -1, 0, "X", 0, 0),

                new Entity(0, 8, "WALL", -1, 0, "X", 0, 0),
                new Entity(1, 8, "WALL", -1, 0, "X", 0, 0),
                new Entity(2, 8, "WALL", -1, 0, "X", 0, 0),
                new Entity(3, 8, "WALL", -1, 0, "X", 0, 0),
                new Entity(4, 8, "WALL", -1, 0, "X", 0, 0),
                new Entity(5, 8, "WALL", -1, 0, "X", 0, 0),
                new Entity(6, 8, "WALL", -1, 0, "X", 0, 0),
                new Entity(7, 8, "WALL", -1, 0, "X", 0, 0),
                new Entity(8, 8, "WALL", -1, 0, "X", 0, 0),
                new Entity(9, 8, "WALL", -1, 0, "X", 0, 0),
                new Entity(10, 8, "WALL", -1, 0, "X", 0, 0),
                new Entity(11, 8, "WALL", -1, 0, "X", 0, 0),
                new Entity(12, 8, "WALL", -1, 0, "X", 0, 0),
                new Entity(13, 8, "WALL", -1, 0, "X", 0, 0),
                new Entity(14, 8, "WALL", -1, 0, "X", 0, 0),
                new Entity(15, 8, "WALL", -1, 0, "X", 0, 0),
                new Entity(16, 8, "WALL", -1, 0, "X", 0, 0),
                new Entity(17, 8, "WALL", -1, 0, "X", 0, 0),

                new Entity(0, 4, "WALL", -1, 0, "X", 0, 0),
                new Entity(1, 4, "WALL", -1, 0, "X", 0, 0),
                new Entity(2, 4, "WALL", -1, 0, "X", 0, 0),
                new Entity(3, 4, "WALL", -1, 0, "X", 0, 0),
                new Entity(4, 4, "WALL", -1, 0, "X", 0, 0),
                new Entity(5, 4, "WALL", -1, 0, "X", 0, 0),
                new Entity(6, 4, "WALL", -1, 0, "X", 0, 0),
                new Entity(7, 4, "WALL", -1, 0, "X", 0, 0),
                new Entity(8, 4, "WALL", -1, 0, "X", 0, 0),
                new Entity(9, 4, "WALL", -1, 0, "X", 0, 0),
                new Entity(10, 4, "WALL", -1, 0, "X", 0, 0),
                new Entity(11, 4, "WALL", -1, 0, "X", 0, 0),
                new Entity(12, 4, "WALL", -1, 0, "X", 0, 0),
                new Entity(13, 4, "WALL", -1, 0, "X", 0, 0),
                new Entity(14, 4, "WALL", -1, 0, "X", 0, 0),
                new Entity(15, 4, "WALL", -1, 0, "X", 0, 0),
                new Entity(17, 4, "WALL", -1, 0, "X", 0, 0),

                new Entity(0, 1, "WALL", -1, 0, "X", 0, 0),
                new Entity(0, 2, "WALL", -1, 0, "X", 0, 0),
                new Entity(0, 3, "WALL", -1, 0, "X", 0, 0),
                new Entity(0, 5, "WALL", -1, 0, "X", 0, 0),
                new Entity(0, 6, "WALL", -1, 0, "X", 0, 0),
                new Entity(0, 7, "WALL", -1, 0, "X", 0, 0),

                new Entity(17, 1, "WALL", -1, 0, "X", 0, 0),
                new Entity(17, 2, "WALL", -1, 0, "X", 0, 0),
                new Entity(17, 3, "WALL", -1, 0, "X", 0, 0),
                new Entity(17, 5, "WALL", -1, 0, "X", 0, 0),
                new Entity(17, 6, "WALL", -1, 0, "X", 0, 0),
                new Entity(17, 7, "WALL", -1, 0, "X", 0, 0),

                new Entity(1, 2, "ROOT", 1, 1, "N", 1, 1),
                new Entity(1, 6, "ROOT", 0, 2, "N", 2, 2),

                new Entity(4, 1, "A", -1, 0, "X", 0, 0),
                new Entity(4, 5, "A", -1, 0, "X", 0, 0)
            };
            board.SetEntities(entities);
            board.Print();

            game.SetNextTurn(board, new Dictionary<EntityType, int>() { { EntityType.A, 10 }, { EntityType.C, 1 }, { EntityType.D, 1 } }, new Dictionary<EntityType, int>() { { EntityType.A, 10 }, { EntityType.C, 1 }, { EntityType.D, 1 } });

            return game;
        }
    }
}
