using Algorithms.Space;
using GameSolution;
using GameSolution.Entities;
using GameSolution.Game;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using Xunit;
using Xunit.Abstractions;

namespace UnitTest
{
    public class GameTests
    {
        private Converter converter;

        public GameTests(ITestOutputHelper output)
        {
            converter = new Converter(output);
            Console.SetError(converter);

        }

        [Fact]
        public void BasicTest()
        {
            var state = GameHelper.CreateEasyStart();
            GameHelper.PlayGame(state);
            //Assert.Equal(1, state.GetWinner());
        }

        [Fact]
        public void BasicTestAgainstRandom()
        {
            var state = GameHelper.CreateEasyStart();
            GameHelper.PlayGameAgainstRandom(state);
            Assert.Equal(1, state.GetWinner());
        }

        [Fact]
        public void TestBresenham()
        {
            var state = GameHelper.ProblematicShooting();
            var endLocation = state.CheckBulletPath(Board.ConvertPointToLocation(3, 3), Board.ConvertPointToLocation(8, 4));
            Assert.Equal(Board.ConvertPointToLocation(4, 3), endLocation);
            endLocation = state.CheckBulletPath(Board.ConvertPointToLocation(8, 4), Board.ConvertPointToLocation(3, 3));
            Assert.Equal(Board.ConvertPointToLocation(7,4), endLocation);

            state = GameHelper.ProblematicShooting2();
            endLocation = state.CheckBulletPath(Board.ConvertPointToLocation(0, 4), Board.ConvertPointToLocation(4, 3));
            Assert.Equal(Board.ConvertPointToLocation(2, 3), endLocation);
        }
    }
}
