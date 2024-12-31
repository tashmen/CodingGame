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
            MonteCarloTreeSearch search = new MonteCarloTreeSearch();
            MonteCarloTreeSearch oppSearch = new MonteCarloTreeSearch();
            Stopwatch watch = new Stopwatch();

            do
            {
                int simulationTime = game.Turn > 1 ? 35 : 970;
                //simulationTime = 5000;
                watch.Reset();
                watch.Start();
                GC.Collect();
                search.SetState(game, true, false);
                Move move = (Move)search.GetNextMove(watch, simulationTime, 14, 1);
                watch.Stop();
                if (game.Turn > 1 && watch.ElapsedMilliseconds > 50)
                    throw new Exception();

                game.ApplyMove(move, true);



                watch.Reset();
                watch.Start();
                GC.Collect();
                oppSearch.SetState(game, false, false);
                move = (Move)oppSearch.GetNextMove(watch, simulationTime, 14, 1);
                watch.Stop();
                if (game.Turn > 2 && watch.ElapsedMilliseconds > 50)
                    throw new Exception();

                game.ApplyMove(move, false);
                game.Print();
            }
            while (!game.GetWinner().HasValue);

            Console.Error.WriteLine(game.GetWinner().Value);
        }
    }
}
