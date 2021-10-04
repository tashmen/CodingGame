using Algorithms;
using GameSolution.Entities;
using GameSolution.Game;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using Xunit;
using Xunit.Abstractions;

namespace UnitTest
{
    

    
    public class GameTests
    {
        

        private readonly GameState game;

        public GameTests(ITestOutputHelper output)
        {
            var converter = new Converter(output);
            Console.SetError(converter);

            game = new GameState();

            Console.Error.WriteLine(game.ToString());
        }

        [Fact]
        public void RandomSimulationTest()
        {
            Random rand = new Random();
            MonteCarloTreeSearch search = new MonteCarloTreeSearch();
            bool isMax = true;
            do
            {
                if (!isMax)
                {
                    Stopwatch watch = new Stopwatch();
                    watch.Start();
                    search.SetState(game);
                    object moveToPlay = search.GetNextMove(watch, 95, -1, 20, 1);
                    Move move = (Move)moveToPlay;
                    watch.Stop();

                    game.ApplyMove(move, true);
                }
                else
                {
                    IList moves = game.GetPossibleMoves(false);
                    game.ApplyMove(moves[rand.Next(0, moves.Count)], false);
                }

                Console.Error.WriteLine(game.ToString());
                isMax = !isMax;
            }
            while (!game.GetWinner().HasValue);

            Console.Error.WriteLine(game.ToString());
            Assert.True(game.GetWinner() > 0);
        }

        [Fact]
        public void PlaySelf()
        {
            MonteCarloTreeSearch search = new MonteCarloTreeSearch();
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
    }
}
