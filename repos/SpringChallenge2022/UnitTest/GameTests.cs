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
        private Converter converter;

        public GameTests(ITestOutputHelper output)
        {
            converter = new Converter(output);
            Console.SetError(converter);

            game = new GameState();

            List<BoardPiece> boardPieces = new List<BoardPiece>();
            boardPieces.Add(new Base(-1, 0, 0, true, 3, 0));
            boardPieces.Add(new Base(-2, 17630, 9000, false, 3, 0));
            boardPieces.Add(new Hero(0, 0, 0, true, 0, false, 0, 0, true));
            boardPieces.Add(new Hero(1, 0, 0, true, 0, false, 0, 0, true));
            boardPieces.Add(new Hero(2, 0, 0, true, 0, false, 0, 0, true));
            Board board = new Board(boardPieces);
            game.SetNextTurn(board);
        }

        [Fact]
        public void RandomSimulationTest()
        {
            
        }

        [Fact]
        public void Minimax_SetState_Test()
        {
            for(int i = 0; i < 100; i++)
            {
                Minimax search = new Minimax();
                search.SetState(game, true, false);
            }
        }

        [Fact]
        public void PlaySelf()
        {
            Minimax search = new Minimax();

            do
            {
                Stopwatch watch = new Stopwatch();
                watch.Start();
                search.SetState(game, true, false);
                Console.Error.WriteLine($"ms: {watch.ElapsedMilliseconds}");
                Move move = (Move) search.GetNextMove(watch, 45, 4);
                game.ApplyMove(move, true);
                watch.Stop();

                if (watch.ElapsedMilliseconds > 50)
                    throw new Exception("timeout");

                watch.Start();
                search.SetState(game, false, false);
                Console.Error.WriteLine($"ms: {watch.ElapsedMilliseconds}");
                move = (Move)search.GetNextMove(watch, 45, 4);
                game.ApplyMove(move, false);
                watch.Stop();

                if (watch.ElapsedMilliseconds > 50)
                    throw new Exception("timeout");
            }
            while (!game.GetWinner().HasValue);
        }

        [Fact]
        public void GameHelper_Test()
        {
            Stopwatch watch = new Stopwatch();
            watch.Start();
            GameHelper gameHelper = new GameHelper(game);
            watch.Stop();
            Console.Error.WriteLine("ms: " + watch.ElapsedMilliseconds);
        }
    }
}
