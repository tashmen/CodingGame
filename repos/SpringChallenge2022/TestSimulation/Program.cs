using Algorithms.Trees;
using GameSolution.Entities;
using GameSolution.Game;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace TestSimulation
{
    internal class Program
    {
        static void Main(string[] args)
        {
            GameState game = GameBuilder.BuildEmptyGame();
            Minimax search = new Minimax();
            Stopwatch watch = new Stopwatch();

            do
            {
                watch.Reset();
                watch.Start();
                search.SetState(game, true, false);
                Move move = (Move)search.GetNextMove(watch, 48000, 4);
                game.ApplyMove(move, true);
                watch.Stop();


                watch.Reset();
                watch.Start();
                search.SetState(game, false, false);
                move = (Move)search.GetNextMove(watch, 48000, 4);
                game.ApplyMove(move, false);
                watch.Stop();
            }
            while (!game.GetWinner().HasValue);
        }
    }
}
