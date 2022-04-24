using Algorithms;
using GameSolution.Entities;
using GameSolution.Game;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using Xunit;
using Xunit.Abstractions;

namespace UnitTest
{
    

    
    public class GameTests
    {
        

        private readonly GameState game;

        public GameTests(ITestOutputHelper output)
        {
            var converter = new Converter(output);
            Console.SetError(converter);

            game = new GameState();

            Console.Error.WriteLine(game.ToString());
        }

        [Fact]
        public void RandomSimulationTest()
        {
            
        }

        [Fact]
        public void PlaySelf()
        {
            
        }
    }
}
