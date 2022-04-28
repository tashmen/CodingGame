using Algorithms;
using Algorithms.Space;
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


    [Collection("MemoryAllocator")]
    public class GameTests
    {
        private readonly GameState game;
        private Converter converter;

        public GameTests(ITestOutputHelper output)
        {
            MemoryAllocator.Initialize();
            converter = new Converter(output);
            Console.SetError(converter);

            game = new GameState();

            List<BoardPiece> boardPieces = new List<BoardPiece>();
            boardPieces.Add(new Base(BoardPiece.MaxEntityId-1, 0, 0, true, 3, 0));
            boardPieces.Add(new Base(BoardPiece.MaxEntityId - 2, 17630, 9000, false, 3, 0));
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
            MemoryAllocator.AllowRecycle = true;
            for(int i = 0; i < 100; i++)
            {
                Minimax search = new Minimax();
                search.SetState(game, true, false);
            }
        }

        [Fact]
        public void PlaySelf_One_Monster()
        {
            Minimax search = new Minimax();
            var nextPoint = Space2d.TranslatePoint(new Point2d(3535, 3535), new Point2d(0, 0), Monster.Speed);
            game.board.boardPieces.Add(new Monster(20, 3535, 3535, null, 10, 0, false, nextPoint.GetTruncatedX() - 3535, nextPoint.GetTruncatedY() - 3535, true, true));
            game.board.SetupBoard();
            do
            {
                PlayGame(search);
            }
            while (!game.GetWinner().HasValue);
        }

        [Fact]
        public void PlaySelf_No_Monsters()
        {
            Minimax search = new Minimax();

            do
            {
                PlayGame(search);
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

        private void PlayGame(Minimax search)
        {
            int timeout = 5000000;

            Stopwatch watch = new Stopwatch();
            watch.Start();
            search.SetState(game, true, false);
            Console.Error.WriteLine($"ms: {watch.ElapsedMilliseconds}");
            Move move = (Move)search.GetNextMove(watch, timeout, 1);
            game.ApplyMove(move, true);
            watch.Stop();

            MemoryAllocator.Reset();

            if (watch.ElapsedMilliseconds > timeout)
                throw new Exception("timeout");

            watch.Start();
            search.SetState(game, false, false);
            Console.Error.WriteLine($"ms: {watch.ElapsedMilliseconds}");
            move = (Move)search.GetNextMove(watch, timeout, 1);
            game.ApplyMove(move, false);
            watch.Stop();

            MemoryAllocator.Reset();

            if (watch.ElapsedMilliseconds > timeout)
                throw new Exception("timeout");
        }
    }
}
