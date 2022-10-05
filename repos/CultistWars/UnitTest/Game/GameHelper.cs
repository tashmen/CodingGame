using Algorithms.Trees;
using GameSolution.Entities;
using System;
using System.Diagnostics;

namespace GameSolution.Game
{
    public class GameHelper
    {
        public static void PlayGame(GameState state)
        {

            MonteCarloTreeSearch monteCarlo = new MonteCarloTreeSearch();
            var limit = 995;
            bool isMax = true;
            do
            {
                Stopwatch watch = new Stopwatch();
                watch.Start();
                monteCarlo.SetState(state, isMax);
                long move = (long)monteCarlo.GetNextMove(watch, limit, 10, 5);
                state.ApplyMove(move, isMax);
                isMax = !isMax;

                watch.Stop();
                limit = 45;
            }
            while (state.GetWinner() == null);
        }

        public static void PlayGameAgainstRandom(GameState state)
        {

            MonteCarloTreeSearch monteCarlo = new MonteCarloTreeSearch();
            var limit = 995;
            //bool isMax = true;
            do
            {
                Stopwatch watch = new Stopwatch();
                watch.Start();
                monteCarlo.SetState(state, true);
                var move = (long)monteCarlo.GetNextMove(watch, limit, -1, 20);
                state.ApplyMove(move, true);

                var moves = state.GetPossibleMoves(false);
                state.ApplyMove(moves[Random.Shared.Next(0, moves.Count)], false);

                watch.Stop();
                limit = 45;
            }
            while (state.GetWinner() == null);
        }

        public static GameState ProblematicShooting()
        {
            string[] boardInput =
            {
                "x.xx.....xx.x",
                ".............",
                "....x...x....",
                ".x..x...x..x.",
                "..x.......x..",
                ".x.........x.",
                "...x.....x...",
            };

            Board board = new Board(boardInput);
            GameState state = new GameState(board);

            Entity[] entities = new Entity[]
            {
                 new Entity(0, 3, 2, 1, 10, 1),
                 new Entity(1, 8, 6, 1, 10, -1),
                 new Entity(2, 3, 1, 0, 10, 1),
                 new Entity(3, 0, 1, 0, 10, 1),
                 new Entity(4, 5, 6, 0, 10, 0),
                 null,
                 new Entity(6, 3, 3, 0, 10, 1),
                 null,
                 new Entity(8, 11, 6, 0, 8, -1),
                 new Entity(9, 11, 0, 0, 10, -1),
                 new Entity(10, 7, 6, 0, 9, -1),
                 new Entity(11, 8, 4, 0, 7, -1),
                 new Entity(12, 7, 4, 0, 10, -1),
                 new Entity(13, 7, 5, 0, 7, -1),
            };
            state.SetState(entities);
            return state;
        }

        public static GameState ProblematicShooting2()
        {
            string[] boardInput =
            {
                "x....x.x....x",
                ".....x.x.....",
                "...xx...xx...",
                "..x..x.x..x..",
                ".............",
                "xx.........xx",
                ".............",
            };

            Board board = new Board(boardInput);
            GameState state = new GameState(board);

            Entity[] entities = new Entity[]
            {
                 new Entity(0, 1, 1, 1, 6, 1),
                new Entity(1, 5, 6, 1, 2, -1),
                new Entity(2, 0, 4, 0, 10, 1),
                null,
                null,
                new Entity(5, 2, 2, 0, 10, 1),
                new Entity(6, 4, 3, 0, 1, -1),
                new Entity(7, 0, 6, 0, 10, 0),
                new Entity(8, 11, 3, 0, 10, -1),
                new Entity(9, 9, 3, 0, 10, -1),
                new Entity(10, 10, 0, 0, 10, -1),
                new Entity(11, 9, 0, 0, 10, -1),
                new Entity(12, 12, 4, 0, 10, -1),
                new Entity(13, 9, 5, 0, 10, -1),
            };
            state.SetState(entities);
            return state;
        }

        public static GameState CreateEasyStart()
        {
            string[] boardInput = {
                ".............",
                "x..x.....x..x",
                "...x.....x...",
                ".............",
                ".............",
                ".............",
                "....x...x...."
            };
            Board board = new Board(boardInput);
            GameState state = new GameState(board);

            Entity[] entities = new Entity[]
            {
                new Entity(0, 0, 3, 1, 10, 1),
                new Entity(1, 12, 3, 1, 10, -1),
                new Entity(2, 5, 5, 0, 10, 0),
                new Entity(3, 4, 0, 0, 10, 0),
                new Entity(4, 2, 0, 0, 10, 0),
                new Entity(5, 3, 4, 0, 10, 0),
                new Entity(6, 5, 4, 0, 10, 0),
                new Entity(7, 3, 3, 0, 10, 0),
                new Entity(8, 7, 5, 0, 10, 0),
                new Entity(9, 8, 0, 0, 10, 0),
                new Entity(10, 10, 0, 0, 10, 0),
                new Entity(11, 8, 2, 0, 10, 0),
                new Entity(12, 7, 4, 0, 10, 0),
                new Entity(13, 9, 3, 0, 10, 0),
            };
            state.SetState(entities);
            return state;
        }

        public static GameState IncorrectNeutralMove()
        {
            string[] boardInput = {
                "...x.....x...",
                ".x.x.....x.x.",
                "..x..x.x..x..",
                ".............",
                "x...........x",
                "...x.....x...",
                "x...........x"
            };
            Board board = new Board(boardInput);
            GameState state = new GameState(board);

            Entity[] entities = new Entity[]
            {
                new Entity(0, 1, 3, 1, 10, -1),
                new Entity(1, 12, 3, 1, 10, 1),
                new Entity(2, 4, 1, 0, 10, 0),
                new Entity(3, 1, 6, 0, 10, 0),
                new Entity(4, 2, 6, 0, 10, 0),
                new Entity(5, 4, 4, 0, 10, 0),
                new Entity(6, 5, 1, 0, 10, 0),
                new Entity(7, 4, 5, 0, 10, 0),
                new Entity(8, 8, 1, 0, 10, 0),
                new Entity(9, 11, 6, 0, 10, 0),
                new Entity(10, 10, 6, 0, 10, 0),
                new Entity(11, 8, 4, 0, 10, 0),
                new Entity(12, 7, 1, 0, 10, 0),
                new Entity(13, 8, 5, 0, 10, 0)
            };
            state.SetState(entities);
            return state;
        }
    }
}
