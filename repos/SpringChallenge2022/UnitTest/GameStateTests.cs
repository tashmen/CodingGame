using GameSolution.Entities;
using GameSolution.Game;
using System;
using System.Collections;
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
            boardPieces.Add(new Base(-1, 0, 0, true, 3, 0));
            boardPieces.Add(new Base(-2, 17630, 9000, false, 3, 0));
            boardPieces.Add(new Hero(0, 0, 0, true, 0, false, 0, 0, true));
            boardPieces.Add(new Hero(1, 0, 0, true, 0, false, 0, 0, true));
            boardPieces.Add(new Hero(2, 0, 0, true, 0, false, 0, 0, true));
            Board board = new Board(boardPieces);
            state.SetNextTurn(board);


        }

        [Fact]
        public void GameState_Clone_Test()
        {
            for (int i = 0; i < 100; i++)
            {
                state.Clone();
            }
        }
    }
}
