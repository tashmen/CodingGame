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
    }
}
