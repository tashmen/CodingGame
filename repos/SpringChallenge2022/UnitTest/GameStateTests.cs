using Algorithms.Space;
using GameSolution.Entities;
using GameSolution.Game;
using System;
using System.Collections.Generic;
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

            List<BoardPiece> boardPieces = new List<BoardPiece>();
            boardPieces.Add(new Base(BoardPiece.MaxEntityId-1, 0, 0, true, 3, 0));
            boardPieces.Add(new Base(BoardPiece.MaxEntityId - 2, 17630, 9000, false, 3, 0));
            boardPieces.Add(new Hero(0, 0, 0, true, 0, false, 0, 0, true));
            boardPieces.Add(new Hero(1, 0, 0, true, 0, false, 0, 0, true));
            boardPieces.Add(new Hero(2, 0, 0, true, 0, false, 0, 0, true));
            Board board = new Board(boardPieces);
            state.SetNextTurn(board);
        }

        [Fact]
        public void GameState_Test_Monster_Movement()
        {
            state.board.boardPieces.Add(new Monster(20, 0, 5000, null, 10, 0, false, 0, -300, true, true));

            state.board.SetupBoard();

            state.SetNextTurn(state.board, true);

            Assert.Equal(4700, state.board.monsters[0].y);
            Assert.True(state.board.monsters[0].isNearBase);
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
            Console.Error.WriteLine("ms: " + watch.ElapsedMilliseconds);
        }
    }
}
