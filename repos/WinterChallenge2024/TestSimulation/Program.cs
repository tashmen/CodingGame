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
                int simulationTime = game.Turn > 1 ? 45 : 970;
                //simulationTime = 5000;
                watch.Reset();
                watch.Start();
                search.SetState(game, true, false);
                Move move = (Move)search.GetNextMove(watch, simulationTime, 20, 1);
                game.ApplyMove(move, true);
                watch.Stop();


                watch.Reset();
                watch.Start();
                oppSearch.SetState(game, false, false);
                move = (Move)oppSearch.GetNextMove(watch, simulationTime, 20, 1);
                game.ApplyMove(move, false);
                watch.Stop();

                game.Print();
            }
            while (!game.GetWinner().HasValue);

            Console.Error.WriteLine(game.GetWinner().Value);
        }
    }
}
