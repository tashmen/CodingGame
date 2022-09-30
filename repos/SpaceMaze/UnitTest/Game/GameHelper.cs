using Algorithms.Genetic;
using Algorithms.Space;
using Algorithms.Trees;
using GameSolution.Entities;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace GameSolution.Game
{
    public class GameHelper
    {
        public static void PlayGame(GameState state)
        {

            MonteCarloTreeSearch monteCarlo = new MonteCarloTreeSearch();
            var limit = 995;
            do
            {
                Stopwatch watch = new Stopwatch();
                watch.Start();
                monteCarlo.SetState(state);
                var move = (Move)monteCarlo.GetNextMove(watch, limit, -1, 20);
                state.ApplyMove(move, true);

                watch.Stop();
                limit = 45;
            }
            while (state.GetWinner() == null);
        }

        public static GameState CreateEasyStart()
        {
            string[] boardInput = { 
"###################",
"###..######...#####",
"###..######...#####",
"###..######...#####",
"###..######..0#####",
"###..######...#####",
"###..######...#####",
"###..######...#####",
"###################",
"###################"
            };
            Board board = new Board(boardInput);
            GameState state = new GameState(board);

            List<Entity> entities = new List<Entity>()
            {
                new Entity(0,4,3,"CAR"),
                new Entity(1,5,3,"R")
            };
            state.SetState(entities);
            return state;
        }
    }
}
