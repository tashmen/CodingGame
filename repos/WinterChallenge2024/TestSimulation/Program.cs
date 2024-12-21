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
            GameState game = GameBuilder.BuildWood3Game();
            MonteCarloTreeSearch search = new MonteCarloTreeSearch();
            MonteCarloTreeSearch oppSearch = new MonteCarloTreeSearch();
            Stopwatch watch = new Stopwatch();

            do
            {
                watch.Reset();
                watch.Start();
                search.SetState(game, true, true);
                Move move = (Move)search.GetNextMove(watch, 48, 80, 2);
                game.ApplyMove(move, true);
                watch.Stop();


                watch.Reset();
                watch.Start();
                oppSearch.SetState(game, false, true);
                move = (Move)oppSearch.GetNextMove(watch, 48, 80, 2);
                game.ApplyMove(move, false);
                watch.Stop();

                game.Print();
            }
            while (!game.GetWinner().HasValue);

            Console.Error.WriteLine(game.GetWinner().Value);
        }
    }
}
