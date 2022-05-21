using Algorithms.Genetic;
using Algorithms.Space;
using Algorithms.Trees;
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



    public class GeneticAlgorithmTests
    {
        private Converter converter;

        public GeneticAlgorithmTests(ITestOutputHelper output)
        {
            converter = new Converter(output);
            Console.SetError(converter);
        }

        [Fact]
        public void EasyTest()
        {
            var state = GameHelper.CreateSimpleGame();
            GameHelper.PlayGame(state);
            Assert.True(state.GetWinner() >= 1);
        }

        [Fact]
        public void InitialSpeedCorrectSide()
        {
            var state = GameHelper.CreateInitialSpeedCorrectSide();
            GameHelper.PlayGame(state);
            Assert.True(state.GetWinner() >= 1);
        }

        [Fact]
        public void InitialSpeedWrongSide()
        {
            var state = GameHelper.CreateInitialSpeedWrongSide();
            GameHelper.PlayGame(state);
            Assert.True(state.GetWinner() >= 1);
        }

        [Fact]
        public void DeepCanyon()
        {
            var state = GameHelper.CreateDeepCanyon();
            GameHelper.PlayGame(state);
            Assert.True(state.GetWinner() >= 1);
        }

        [Fact]
        public void HighGround()
        {
            var state = GameHelper.CreateHighGround();
            GameHelper.PlayGame(state);
            Assert.True(state.GetWinner() >= 1);
        }


    }
}
