using GameSolution;
using System;
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
        public void TestGameStart()
        {
            Game game = new Game(6, 6, 80, 0, 0);
            game.SetState(BombDirection.Unknown);
            var result = game.GetNextMove();
            Assert.Equal(Tuple.Create(2, 0), result);


            game = new Game(7, 7, 80, 0, 0);
            game.SetState(BombDirection.Unknown);
            result = game.GetNextMove();
            Assert.Equal(Tuple.Create(3, 0), result);

            game = new Game(7, 7, 80, 3, 0);
            game.SetState(BombDirection.Unknown);
            result = game.GetNextMove();
            Assert.Equal(Tuple.Create(4, 0), result);
        }

        [Theory]
        [InlineData(21, 1, 2, 0, 0, 5, 0)]
        [InlineData(101, 1, 7, 0, 0, 32, 0)]
        [InlineData(101, 1, 6, 0, 0, 2, 0)]
        [InlineData(1000, 1000, 25, 501, 501, 998, 2)]

        public void TestGamePlay( int largestX, int largestY, int turns, int startX, int startY, int bombX, int bombY)
        {
            Game game = new Game(largestX, largestY, turns, startX, startY);
            GameHelper.PlayGame(game, bombX, bombY);

            Assert.Equal(0, game.Turns);
            Assert.Equal(bombX, game.StartX);
            Assert.Equal(bombY, game.StartY);
        }

        
    }
}
