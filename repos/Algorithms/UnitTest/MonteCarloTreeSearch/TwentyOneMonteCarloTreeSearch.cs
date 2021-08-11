using Algorithm.MonteCarloTreeSearch;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using UnitTest.Fixtures;
using Xunit;
using Xunit.Abstractions;

namespace UnitTest
{
    public class GameState : IGameState
    {
        int Total;
        Move LastMove;
        bool? LastMoveBy;

        public GameState()
        {
            Total = 0;
        }
        public void ApplyMove(IMove move, bool isMax)
        {
            Move m = move as Move;
            Total += m.Count;
            LastMove = m;
            LastMoveBy = isMax;
        }

        public IGameState Clone()
        {
            GameState state = new GameState();
            state.Total = Total;
            return state;
        }

        public bool Equals(IGameState state)
        {
            GameState s = state as GameState;
            return Total == s.Total;
        }

        public IMove GetMove(bool isMax)
        {
            return LastMove;
        }

        public List<IMove> GetPossibleMoves(bool isMax)
        {
            return new List<IMove>()
            {
                new Move(1),
                new Move(2),
                new Move(3)
            };
        }

        public int? GetWinner()
        {
            if(Total >= 21)
            {
                if(Total == 21)
                {
                    if (LastMoveBy.HasValue)
                    {
                        if (LastMoveBy.Value)
                            return 1;
                        else return -1;
                    }
                    return 0;
                }
                else if (LastMoveBy.HasValue)
                {
                    if (LastMoveBy.Value)
                        return -1;
                    else return 1;
                }

            }
            return null;
        }

        public override string ToString()
        {
            return "Total: " + Total;
        }
    }

    public class Move : IMove
    {
        public int Count;

        public Move(int count)
        {
            Count = count;
        }

        public override string ToString()
        {
            return "Move: " + Count.ToString();
        }
    }

    public class TwentyOneMonteCarloTreeSearch
    {
        MonteCarloTreeSearch search = new MonteCarloTreeSearch();
        GameState state = new GameState();
        public TwentyOneMonteCarloTreeSearch(ITestOutputHelper output)
        {
            var converter = new TestOutputFixture(output);
            Console.SetError(converter);
        }
        
        [Fact]
        public void Test_Play_Game()
        {
            bool isMax = true;
            do
            {
                Stopwatch watch = new Stopwatch();
                watch.Start();
                search.SetState(state, isMax);

                IMove move = search.GetNextMove(watch, 1000);
                state.ApplyMove(move, isMax);
                isMax = !isMax;
                watch.Stop();

                Console.Error.WriteLine(" ");

                Console.Error.WriteLine(move.ToString());
                Console.Error.WriteLine(state.ToString());
                Console.Error.WriteLine("isMax: " + isMax);

                Console.Error.WriteLine(" ");
                Console.Error.WriteLine(" ");


            } while (!state.GetWinner().HasValue);

            Console.Error.WriteLine("Winner: " + state.GetWinner().Value);
            
        }
        
    }
}
