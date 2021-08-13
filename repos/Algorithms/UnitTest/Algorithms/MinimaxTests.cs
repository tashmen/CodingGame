using Algorithms;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using UnitTest.Fixtures;
using UnitTest.TwentyOneGame;
using Xunit;
using Xunit.Abstractions;

namespace UnitTest.Algorithms
{
    public class MinimaxTests
    {
        Minimax search = new Minimax();
        GameState state = new GameState();
        public MinimaxTests(ITestOutputHelper output)
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

                IMove move = search.GetNextMove(watch, 100);
                state.ApplyMove(move, isMax);
                //isMax = !isMax;
                watch.Stop();

                Console.Error.WriteLine(" ");

                Console.Error.WriteLine(move.ToString());
                Console.Error.WriteLine(state.ToString());
                Console.Error.WriteLine("isMax: " + isMax);

                Console.Error.WriteLine(" ");
                Console.Error.WriteLine(" ");

                if(state.Total < 21)
                {
                    int sticks = state.Total % 4;
                    if (sticks == 3)
                    {
                        sticks = 2;
                    }
                    else if (sticks != 1)
                        sticks = sticks + 1;

                    move = new Move(sticks);
                    state.ApplyMove(move, !isMax);

                    Console.Error.WriteLine(" ");

                    Console.Error.WriteLine(move.ToString());
                    Console.Error.WriteLine(state.ToString());
                    Console.Error.WriteLine("isMax: " + !isMax);

                    Console.Error.WriteLine(" ");
                    Console.Error.WriteLine(" ");
                }
            } while (!state.GetWinner().HasValue);

            Console.Error.WriteLine("Winner: " + state.GetWinner().Value);

            Assert.Equal(1, state.GetWinner().Value);

        }
    }
}
