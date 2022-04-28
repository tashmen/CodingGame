using Algorithms.GameComponent;
using Algorithms.Space;
using GameSolution.Entities;
using GameSolution.Game;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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

            state = GameBuilder.BuildEmptyGame();
        }

        [Fact]
        public void GameState_Test_Monster_Movement()
        {
            state = GameBuilder.BuildGameWithSingleMonster();

            state.SetNextTurn(state.board, true);

            Assert.Equal(4700, state.board.monsters[0].y);
            Assert.True(state.board.monsters[0].isNearBase);
        }

        [Fact]
        public void GameState_Clone_Test()
        {
            Stopwatch watch = new Stopwatch();
            watch.Start();
            IGameState[] states = new IGameState[100000];
            for (int i = 0; i < states.Length; i++)
            {
                states[i] = state.Clone();
            }
            watch.Stop();
            Console.Error.WriteLine("ms: " + watch.ElapsedMilliseconds);
        }
    }
}
