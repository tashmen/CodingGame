using GameSolution.Entities;
using System;
using System.Collections;
using Xunit;
using Xunit.Abstractions;

namespace UnitTest
{

    public class BoardTests
    {
        private Board board;
        public BoardTests(ITestOutputHelper output)
        {
            var converter = new Converter(output);
            Console.SetError(converter);

            board = new Board(1);
        }

        [Fact]
        public void TestApplyMove()
        {
            board.ApplyMove(new Move(0, 0, 1), true);
            Assert.Equal($"X--{Environment.NewLine}---{Environment.NewLine}---{Environment.NewLine}", board.ToString());

            board.ApplyMove(new Move(0, 1, 1), false);
            Assert.Equal($"XO-{Environment.NewLine}---{Environment.NewLine}---{Environment.NewLine}", board.ToString());
        }

        [Fact]
        public void TestEmptySpaces()
        {
            IList moves = board.GetEmptySpaces();
            Assert.Equal(9, moves.Count);

            //board.ApplyMove(new Move(1, 1, 1), true);
            //Assert.Equal(8, board.GetEmptySpaces().Count);

            board.ApplyMove(new Move(0, 0, 1), true);
            board.ApplyMove(new Move(0, 1, 1), true);
            board.ApplyMove(new Move(0, 2, 1), true);
            Assert.Equal(6, board.GetEmptySpaces().Count);

            Board board2 = board.Clone();

            board2.ApplyMove(new Move(1, 0, 1), true);
            //board.ApplyMove(new Move(1, 1, 1), true);
            board2.ApplyMove(new Move(1, 2, 1), true);

            Assert.Equal(4, board2.GetEmptySpaces().Count);

            board2.ApplyMove(new Move(2, 0, 1), true);
            board2.ApplyMove(new Move(2, 1, 1), true);
            board2.ApplyMove(new Move(2, 2, 1), true);

            Assert.Equal(1, board2.GetEmptySpaces().Count);
        }
    }
}
