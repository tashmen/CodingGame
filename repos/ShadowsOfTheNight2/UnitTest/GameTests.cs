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
        [InlineData(0, 0, 0, 0, 0, 0, "UNKOWN", 0, 0, 0, 0, 0, 0)]
        public void PlaySingleTurnTest(int largestX, int largestY, int startX, int startY, int leftX, int rightX, string bombDirection, int expectedX, int expectedY, int expectedLeftX, int expectedRightX, int expectedLeftY, int expectedRightY)
        {
            Game game = new Game(largestX, largestY, 0, startX, startY);
            game.leftX = leftX;
            game.rightX = rightX;
            game.SetState(bombDirection);
            var result = game.GetNextMove();
            Assert.Equal(Tuple.Create(expectedX, expectedY), result);
            Assert.Equal(Tuple.Create(expectedLeftX, expectedRightX), Tuple.Create(game.leftX, game.rightX));
            Assert.Equal(Tuple.Create(expectedLeftY, expectedRightY), Tuple.Create(game.leftY, game.rightY));
        }

        [Theory]
        [InlineData(21, 1, 10, 0, 0, 5, 0)]
        [InlineData(101, 1, 20, 0, 0, 32, 0)]
        [InlineData(101, 1, 20, 0, 0, 2, 0)]
        [InlineData(50, 50, 16, 17, 29, 48, 10)]
        [InlineData(1000, 1000, 27, 501, 501, 998, 2)]
        [InlineData(1000, 1000, 30, 501, 501, 998, 738)]
        [InlineData(1000, 1000, 27, 501, 501, 738, 745)]
        [InlineData(1000, 1000, 27, 501, 501, 501, 998)]
        [InlineData(1000, 1000, 27, 501, 501, 751, 719)]
        [InlineData(1000, 1000, 27, 501, 501, 998, 998)]

        public void TestGamePlay( int largestX, int largestY, int turns, int startX, int startY, int bombX, int bombY)
        {
            Game game = new Game(largestX, largestY, turns, startX, startY);
            GameHelper.PlayGame(game, bombX, bombY);

            Assert.True(game.Turns >= 0);
            Assert.Equal(bombX, game.StartX);
            Assert.Equal(bombY, game.StartY);
        }

        
    }
}
