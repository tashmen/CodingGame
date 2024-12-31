using Algorithms.Trees;
using GameSolution.Entities;
using GameSolution.Game;
using System;
using System.Diagnostics;

namespace TestSimulation
{
    internal class Program
    {
        static void Main(string[] args)
        {
            GameState game = GameBuilder.BuildSilverGame();
            MonteCarloTreeSearch search = new MonteCarloTreeSearch(mathLogCacheSize: 50000);
            MonteCarloTreeSearch oppSearch = new MonteCarloTreeSearch(mathLogCacheSize: 50000);
            Stopwatch watch = new Stopwatch();

            do
            {
                GC.Collect();
                int simulationTime = game.Turn > 1 ? 45 : 970;
                //simulationTime = 5000;
                watch.Reset();
                watch.Start();
                search.SetState(game, true, false);
                Move move = (Move)search.GetNextMove(watch, simulationTime, 4, 1);
                watch.Stop();

                game.ApplyMove(move, true);



                watch.Reset();
                watch.Start();
                oppSearch.SetState(game, false, false);
                move = (Move)oppSearch.GetNextMove(watch, simulationTime, 4, 1);
                watch.Stop();

                game.ApplyMove(move, false);
                game.Print();
            }
            while (!game.GetWinner().HasValue);

            Console.Error.WriteLine(game.GetWinner().Value);
        }
    }
}
