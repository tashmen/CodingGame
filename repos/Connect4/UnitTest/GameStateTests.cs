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
        public void GameState_Wind_Test()
        {
            state = GameBuilder.BuildGameForWindTest();

            Move move = new Move();
            move.AddMove(HeroMove.CreateWindSpellMove(5000, 6000), 0);
            state.ApplyMove(move, true);

            move = new Move();
            move.AddMove(HeroMove.CreateWindSpellMove(5000, 5000), 0);
            state.ApplyMove(move, false);

            var windableMonster = state.board.monsters[0];
            var shieldedMonster = state.board.monsters[1];

            var maxHero = state.board.myHeroes[0];
            var minHero = state.board.opponentHeroes[0];

            Assert.Equal(5000, shieldedMonster.x);
            Assert.Equal(5200, shieldedMonster.y);

            Assert.Equal(5000, windableMonster.x);
            Assert.Equal(5500, windableMonster.y);

            Assert.Equal(5000, maxHero.x);
            Assert.Equal(2800, maxHero.y);

            Assert.Equal(5000, minHero.x);
            Assert.Equal(8200, minHero.y);
        }

        [Fact]
        public void GameState_Test_Monster_Movement()
        {
            state = GameBuilder.BuildGameWithSingleMonster();

            state.SetNextTurn(state.board, true);

            var monster = state.board.monsters[0];

            Assert.Equal(4700, monster.y);
            Assert.True(monster.isNearBase);

            Assert.Equal(0, monster.vx);
            Assert.Equal(-400, monster.vy);
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
            Console.Error.WriteLine("clone ms: " + watch.ElapsedMilliseconds);
        }
    }
}
