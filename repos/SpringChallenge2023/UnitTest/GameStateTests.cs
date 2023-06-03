using GameSolution.Game;
using System;
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
        public void TestGameState()
        {
            state = GameBuilder.BuildBasicGame();
            var path = state.Board.Graph.GetShortestPathAll(13, 25);

            Assert.Equal(25, path[0].EndNodeId);

            path = state.Board.Graph.GetShortestPathAll(13, 26);
            Assert.Equal(2, path.Count);

        }

    }
}
