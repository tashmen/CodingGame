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
            MonteCarloTreeSearch search = new MonteCarloTreeSearch(true, MonteCarloTreeSearch.SearchStrategy.Sequential, mathLogCacheSize: 50000);
            MonteCarloTreeSearch oppSearch = new MonteCarloTreeSearch(true, MonteCarloTreeSearch.SearchStrategy.Sequential, mathLogCacheSize: 50000);
            Stopwatch watch = new Stopwatch();

            do
            {
                //GC.Collect();
                int simulationTime = game.Turn > 1 ? 25 : 970;
                int maxTime = game.Turn > 1 ? 50 : 1000;
                //simulationTime = 5000;
                //maxTime = 6000;
                watch.Reset();
                watch.Start();
                search.SetState(game, true, false);
                Move move = (Move)search.GetNextMove(watch, simulationTime, 8, 1);
                watch.Stop();
                Console.Error.WriteLine($"Elapsed: {watch.ElapsedMilliseconds}");
                if (watch.ElapsedMilliseconds > maxTime)
                    throw new Exception();

                game.ApplyMove(move, true);



                watch.Reset();
                watch.Start();
                oppSearch.SetState(game, false, false);
                move = (Move)oppSearch.GetNextMove(watch, simulationTime, 4, 1);
                watch.Stop();
                Console.Error.WriteLine($"Elapsed: {watch.ElapsedMilliseconds}");
                if (watch.ElapsedMilliseconds > maxTime)
                    throw new Exception();

                game.ApplyMove(move, false);
                game.Print();
            }
            while (!game.GetWinner().HasValue);

            Console.Error.WriteLine(game.GetWinner().Value);
            Console.ReadLine();
        }

    }
}
