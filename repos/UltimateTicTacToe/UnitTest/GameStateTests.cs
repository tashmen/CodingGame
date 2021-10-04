using GameSolution.Entities;
using GameSolution.Game;
using System;
using System.Collections;
using Xunit;
using Xunit.Abstractions;

namespace UnitTest
{

    public class GameStateTests
    {
        private GameState state;
        public GameStateTests(ITestOutputHelper output)
        {
            var converter = new Converter(output);
            Console.SetError(converter);

            state = new GameState();

            
        }

        [Fact]
        public void TestWinO()
        {
            state.ApplyMove(new Move(0, 0, 0), true);
            state.ApplyMove(new Move(1, 0, 0), true);
            state.ApplyMove(new Move(2, 0, 0), true);

            state.ApplyMove(new Move(0, 0, 4), true);
            state.ApplyMove(new Move(1, 0, 4), true);
            state.ApplyMove(new Move(2, 0, 4), true);

            state.ApplyMove(new Move(0, 0, 6), true);
            state.ApplyMove(new Move(1, 0, 6), true);
            state.ApplyMove(new Move(2, 0, 6), true);

            state.ApplyMove(new Move(0, 0, 7), true);
            state.ApplyMove(new Move(1, 0, 7), true);
            state.ApplyMove(new Move(2, 0, 7), true);

            state.ApplyMove(new Move(0, 0, 1), false);
            state.ApplyMove(new Move(1, 0, 1), false);
            state.ApplyMove(new Move(2, 0, 1), false);

            state.ApplyMove(new Move(0, 0, 2), false);
            state.ApplyMove(new Move(1, 0, 2), false);
            state.ApplyMove(new Move(2, 0, 2), false);

            state.ApplyMove(new Move(0, 0, 3), false);
            state.ApplyMove(new Move(1, 0, 3), false);
            state.ApplyMove(new Move(2, 0, 3), false);

            state.ApplyMove(new Move(0, 0, 8), false);
            state.ApplyMove(new Move(1, 0, 8), false);
            state.ApplyMove(new Move(2, 0, 8), false);

            state.ApplyMove(new Move(0, 0, 5), false);
            state.ApplyMove(new Move(0, 2, 5), false);
            state.ApplyMove(new Move(2, 1, 5), false);

            state.ApplyMove(new Move(1, 2, 5), true);
            state.ApplyMove(new Move(0, 1, 5), false);

            Assert.True( state.GetWinner() <= -1);
        }

        [Fact]
        public void TestClone()
        {
            state.ApplyMove(new Move(0, 0, 0), true);

            IList moves = state.GetPossibleMoves(true);

            GameState state2 = state.Clone() as GameState;

            Assert.Equal(8, state2.GetPossibleMoves(true).Count);
        }

        
    }
}
