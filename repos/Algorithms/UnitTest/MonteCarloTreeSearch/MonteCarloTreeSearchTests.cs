﻿using Algorithm;
using System;
using System.Diagnostics;
using UnitTest.Fixtures;
using UnitTest.TwentyOneGame;
using Xunit;
using Xunit.Abstractions;

namespace UnitTest
{
    public class MonteCarloTreeSearchTests
    {
        MonteCarloTreeSearch search = new MonteCarloTreeSearch();
        GameState state = new GameState();
        public MonteCarloTreeSearchTests(ITestOutputHelper output)
        {
            var converter = new TestOutputFixture(output);
            Console.SetError(converter);
        }
        
        [Fact]
        public void Test_Play_21_Game()
        {
            bool isMax = true;
            do
            {
                Stopwatch watch = new Stopwatch();
                watch.Start();
                search.SetState(state, isMax);

                IMove move = search.GetNextMove(watch, 10000, 10000);
                state.ApplyMove(move, isMax);
                //isMax = !isMax;
                watch.Stop();

                Console.Error.WriteLine(" ");

                Console.Error.WriteLine(move.ToString());
                Console.Error.WriteLine(state.ToString());
                Console.Error.WriteLine("isMax: " + isMax);

                Console.Error.WriteLine(" ");
                Console.Error.WriteLine(" ");

                int sticks = state.Total % 4;
                if(sticks == 3)
                {
                    sticks = 2;
                }
                else if(sticks != 1)
                    sticks = sticks + 1;

                move = new Move(sticks);
                state.ApplyMove(move, !isMax);

                Console.Error.WriteLine(" ");

                Console.Error.WriteLine(move.ToString());
                Console.Error.WriteLine(state.ToString());
                Console.Error.WriteLine("isMax: " + !isMax);

                Console.Error.WriteLine(" ");
                Console.Error.WriteLine(" ");


            } while (!state.GetWinner().HasValue);

            Console.Error.WriteLine("Winner: " + state.GetWinner().Value);

            Assert.Equal(1, state.GetWinner().Value);
            
        }
        
    }
}