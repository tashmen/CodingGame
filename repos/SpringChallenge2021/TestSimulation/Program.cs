using Algorithms;
using GameSolution.Entities;
using GameSolution.Moves;
using GameSolution.Utility;
using System;
using System.Collections.Generic;
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
                game = GameBuilder.BuildSampleGame();
                Random rand = new Random();
                MonteCarloTreeSearch search = new MonteCarloTreeSearch(false, MonteCarloTreeSearch.SearchStrategy.Sequential);
                do
                {
                    Stopwatch watch = new Stopwatch();
                    watch.Start();
                    search.SetState(game);
                    object moveToPlay = search.GetNextMove(watch, 95, -1, 20);
                    long move = (long)moveToPlay;
                    watch.Stop();

                    long opponentMove = game.opponent.possibleMoves[rand.Next(0, game.opponent.possibleMoves.Count)];
                    game.ApplyMoves(move, opponentMove);
                }
                while (game.day < 24);
            }
            catch(Exception e)
            {
                Console.Error.WriteLine(e.ToString());
                Console.ReadKey();
            }
        }
    }
}
