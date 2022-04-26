using Algorithms;
using Algorithms.Trees;
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
            List<BoardPiece> boardPieces = new List<BoardPiece>();
            boardPieces.Add(new Base(-1, 0, 0, true, 3, 0));
            boardPieces.Add(new Base(-2, 17630, 9000, false, 3, 0));
            boardPieces.Add(new Hero(0, 0, 0, true, 0, false, 0, 0, true));
            boardPieces.Add(new Hero(1, 0, 0, true, 0, false, 0, 0, true));
            boardPieces.Add(new Hero(2, 0, 0, true, 0, false, 0, 0, true));
            Board board = new Board(boardPieces);
            game.SetNextTurn(board);
            Minimax search = new Minimax();

            do
            {
                Stopwatch watch = new Stopwatch();
                watch.Start();
                search.SetState(game, true, false);
                Move move = (Move) search.GetNextMove(watch, -1, 1);
                game.ApplyMove(move, true);
                watch.Stop();

                watch.Start();
                search.SetState(game, false, false);
                move = (Move)search.GetNextMove(watch, -1, 1);
                game.ApplyMove(move, false);
                watch.Stop();
            }
            while (!game.GetWinner().HasValue);
        }
    }
}
