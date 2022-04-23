using Algorithms;
using GameSolution.Entities;
using GameSolution.Game;
using System;
using System.Diagnostics;

namespace TestSimulation
{
    class Program
    {
        public static GameState game;
        static void Main(string[] args)
        {
            try
            {
                MonteCarloTreeSearch search = new MonteCarloTreeSearch(false, MonteCarloTreeSearch.SearchStrategy.Sequential);
                game = new GameState();
                bool isMax = true;
                do
                {
                    Stopwatch watch = new Stopwatch();
                    watch.Start();
                    search.SetState(game, isMax);
                    object moveToPlay = search.GetNextMove(watch, 95, -1, 20, 1);
                    Move move = (Move)moveToPlay;
                    watch.Stop();

                    game.ApplyMove(move, isMax);

                    isMax = !isMax;
                    Console.Error.WriteLine(game.ToString());
                }
                while (!game.GetWinner().HasValue);
            }
            catch (Exception e)
            {
                Console.Error.WriteLine(e.ToString());
                Console.ReadKey();
            }
        }
    }
}
