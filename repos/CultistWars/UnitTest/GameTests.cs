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
            var endPoint = state.CheckBulletPath(new Point2d(3, 3), new Point2d(8, 4));
            Assert.Equal(new Point2d(4, 3), endPoint);
            endPoint = state.CheckBulletPath(new Point2d(8, 4), new Point2d(3, 3));
            Assert.Equal(new Point2d(7, 4), endPoint);

            state = GameHelper.ProblematicShooting2();
            endPoint = state.CheckBulletPath(new Point2d(0, 4), new Point2d(4, 3));
            Assert.True(new Point2d(2, 3).Equals(endPoint));
        }
    }
}
