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
                watch.Reset();
                watch.Start();
                search.SetState(game, true, false);
                Move move = (Move)search.GetNextMove(watch, game.Turn > 1 ? 45 : 970, 20, 1);
                game.ApplyMove(move, true);
                watch.Stop();


                watch.Reset();
                watch.Start();
                oppSearch.SetState(game, false, false);
                move = (Move)oppSearch.GetNextMove(watch, game.Turn > 1 ? 45 : 970, 20, 1);
                game.ApplyMove(move, false);
                watch.Stop();

                game.Print();
            }
            while (!game.GetWinner().HasValue);

            Console.Error.WriteLine(game.GetWinner().Value);
        }
    }
}
