﻿using Algorithms.Trees;
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

            //Minimax myMinimaxSearch = new Minimax();
            //Minimax oppMinimaxSearch = new Minimax();

            do
            {
                //GC.Collect();
                int simulationTime = game.Turn > 1 ? 45 : 970;
                int maxTime = game.Turn > 1 ? 60 : 1000;
                //simulationTime = 5000;
                //maxTime = 6000;
                watch.Reset();
                watch.Start();


                search.SetState(game, true, false);
                Move move = (Move)search.GetNextMove(watch, simulationTime, -1, 20);

                /*
                myMinimaxSearch.SetState(game, true, false);
                Move move = (Move)myMinimaxSearch.GetNextMove(watch, simulationTime, -1);
                */

                watch.Stop();
                Console.Error.WriteLine($"Elapsed: {watch.ElapsedMilliseconds}");
                if (watch.ElapsedMilliseconds > maxTime)
                    Console.Error.WriteLine("Exceeded time period");

                game.ApplyMove(move, true);



                watch.Reset();
                watch.Start();


                oppSearch.SetState(game, false, false);
                move = (Move)oppSearch.GetNextMove(watch, simulationTime, -1, 20);

                /*
                oppMinimaxSearch.SetState(game, false, false);
                move = (Move)oppMinimaxSearch.GetNextMove(watch, simulationTime, -1);
                */


                watch.Stop();
                Console.Error.WriteLine($"Elapsed: {watch.ElapsedMilliseconds}");
                if (watch.ElapsedMilliseconds > maxTime)
                    Console.Error.WriteLine("Exceeded time period");

                game.ApplyMove(move, false);
                game.Print();
            }
            while (!game.GetWinner().HasValue);

            Console.Error.WriteLine(game.GetWinner().Value);
            Console.ReadLine();
        }

    }
}
