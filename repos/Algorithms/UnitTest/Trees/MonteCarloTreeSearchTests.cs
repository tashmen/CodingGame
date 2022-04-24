using Algorithms.Trees;
using System;
using System.Diagnostics;
using UnitTest.Fixtures;
using UnitTest.TwentyOneGame;
using Xunit;
using Xunit.Abstractions;

namespace UnitTest.Algorithms
{
    public class MonteCarloTreeSearchTests
    {
        MonteCarloTreeSearch search = new MonteCarloTreeSearch(true, MonteCarloTreeSearch.SearchStrategy.Sequential);
        GameState state = new GameState();
        public MonteCarloTreeSearchTests(ITestOutputHelper output)
        {
            var converter = new TestOutputFixture(output);
            Console.SetError(converter);
        }
        
        [Fact]
        public void Test_Play_21_Game()
        {
            Random rand = new Random();
            bool isMax = true;
            do
            {
                Stopwatch watch = new Stopwatch();
                watch.Start();
                search.SetState(state, isMax);

                object move = search.GetNextMove(watch, 100,  -1, 500);
                state.ApplyMove(move, isMax);
                //isMax = !isMax;
                watch.Stop();

                Console.Error.WriteLine(" ");

                Console.Error.WriteLine(move.ToString());
                Console.Error.WriteLine(state.ToString());
                Console.Error.WriteLine("isMax: " + isMax);

                Console.Error.WriteLine(" ");
                Console.Error.WriteLine(" ");

                if (state.Total < 21)
                {
                    int sticks = state.Total % 4;
                    if (sticks == 3)
                    {
                        sticks = 2;
                    }
                    else if (sticks != 1)
                        sticks = sticks + 1;

                    if (true)
                        sticks = rand.Next(1, 3);

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

        [Fact]
        public void RunManyClones()
        {
            Stopwatch watch = new Stopwatch();
            watch.Start();
            double numberOfClones = 1000000.0;
            for (int i = 0; i < numberOfClones; i++)
            {
                var clone = state.Clone();
            }
            watch.Stop();

            Console.Error.WriteLine($"Elapsed ms per clone: {watch.ElapsedMilliseconds / numberOfClones}");
        }

    }
}
