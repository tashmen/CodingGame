using Algorithms;
using GameSolution.Entities;
using GameSolution.Moves;
using GameSolution.Utility;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using Xunit;
using Xunit.Abstractions;

namespace UnitTest
{


    public class GameStateTests
    {
        private class Converter : TextWriter
        {
            ITestOutputHelper _output;
            public Converter(ITestOutputHelper output)
            {
                _output = output;
            }
            public override Encoding Encoding
            {
                get { return Encoding.UTF8; }
            }
            public override void WriteLine(string message)
            {
                _output.WriteLine(message);
            }
            public override void WriteLine(string format, params object[] args)
            {
                _output.WriteLine(format, args);
            }
        }

        private readonly GameState game;

        public GameStateTests(ITestOutputHelper output)
        {
            var converter = new Converter(output);
            Console.SetError(converter);

            game = new GameState();

            game.board.Insert(0, new Cell(0, 3, new List<int>() { 1, 2, 3, 4, 5, 6 }));

            game.board.Insert(1, new Cell(1, 3, new List<int>() { 7, 8, 2, 0, 6, 18 }));
            game.board.Insert(2, new Cell(2, 3, new List<int>() { 8, 9, 10, 3, 0, 1 }));
            game.board.Insert(3, new Cell(3, 3, new List<int>() { 2, 10, 11, 12, 4, 0 }));
            game.board.Insert(4, new Cell(4, 3, new List<int>() { 0, 3, 12, 13, 14, 5 }));
            game.board.Insert(5, new Cell(5, 3, new List<int>() { 6, 0, 4, 14, 15, 16 }));
            game.board.Insert(6, new Cell(6, 3, new List<int>() { 18, 1, 0, 5, 16, 17 }));

            game.board.Insert(7, new Cell(7, 2, new List<int>() { 19, 20, 8, 1, 18, 36 }));
            game.board.Insert(8, new Cell(8, 2, new List<int>() { 20, 21, 9, 2, 1, 7 }));
            game.board.Insert(9, new Cell(9, 2, new List<int>() { 21, 22, 23, 10, 2, 8 }));
            game.board.Insert(10, new Cell(10, 2, new List<int>() { 9, 23, 24, 11, 3, 2 }));
            game.board.Insert(11, new Cell(11, 2, new List<int>() { 10, 24, 25, 26, 12, 3 }));
            game.board.Insert(12, new Cell(12, 2, new List<int>() { 3, 11, 26, 27, 13, 4 }));
            game.board.Insert(13, new Cell(13, 2, new List<int>() { 4, 12, 27, 28, 29, 14 }));
            game.board.Insert(14, new Cell(14, 2, new List<int>() { 5, 4, 13, 29, 30, 15 }));
            game.board.Insert(15, new Cell(15, 2, new List<int>() { 16, 5, 14, 30, 31, 32 }));
            game.board.Insert(16, new Cell(16, 2, new List<int>() { 17, 6, 5, 15, 32, 33 }));
            game.board.Insert(17, new Cell(17, 2, new List<int>() { 35, 18, 6, 16, 33, 34 }));
            game.board.Insert(18, new Cell(18, 2, new List<int>() { 36, 7, 1, 6, 17, 35 }));


            game.board.Insert(19, new Cell(19, 1, new List<int>() { -1, -1, 20, 7, 36, -1 }));
            game.board.Insert(20, new Cell(20, 1, new List<int>() { -1, -1, 21, 8, 7, 19 }));
            game.board.Insert(21, new Cell(21, 1, new List<int>() { -1, -1, 22, 9, 8, 20 }));
            game.board.Insert(22, new Cell(22, 1, new List<int>() { -1, -1, -1, 23, 9, 21 }));
            game.board.Insert(23, new Cell(23, 1, new List<int>() { 22, -1, -1, 24, 10, 9 }));
            game.board.Insert(24, new Cell(24, 1, new List<int>() { 23, -1, -1, 25, 11, 10 }));
            game.board.Insert(25, new Cell(25, 1, new List<int>() { 24, -1, -1, -1, 26, 11 }));
            game.board.Insert(26, new Cell(26, 1, new List<int>() { 11, 25, -1, -1, 27, 12 }));
            game.board.Insert(27, new Cell(27, 1, new List<int>() { 12, 26, -1, -1, 28, 13 }));
            game.board.Insert(28, new Cell(28, 1, new List<int>() { 13, 27, -1, -1, -1, 29 }));
            game.board.Insert(29, new Cell(29, 1, new List<int>() { 14, 13, 28, -1, -1, 30 }));
            game.board.Insert(30, new Cell(30, 1, new List<int>() { 15, 14, 29, -1, -1, 31 }));
            game.board.Insert(31, new Cell(31, 1, new List<int>() { 32, 15, 30, -1, -1, -1 }));
            game.board.Insert(32, new Cell(32, 1, new List<int>() { 33, 16, 15, 31, -1, -1 }));
            game.board.Insert(33, new Cell(33, 1, new List<int>() { 34, 17, 16, 32, -1, -1 }));
            game.board.Insert(34, new Cell(34, 1, new List<int>() { -1, 35, 17, 33, -1, -1 }));
            game.board.Insert(35, new Cell(35, 1, new List<int>() { -1, 36, 18, 17, 34, -1 }));
            game.board.Insert(36, new Cell(36, 1, new List<int>() { -1, 7, 18, 35, -1, -1 }));

            game.ResetTrees();
            game.ResetPlayers();

            game.day = 1;
            game.nutrients = 10;
            game.me.sun = 2;
            game.me.score = 0;
            game.me.isWaiting = false;

            game.opponent.sun = 2;
            game.opponent.score = 0;
            game.opponent.isWaiting = false;

            game.board[4].AddTree(new Tree(4, 1, true, false));
            game.board[1].AddTree(new Tree(1, 1, false, false));

            game.UpdateGameState();
        }



        [Fact]
        public void RunManyClonesOverOneRandomGame()
        {
            Random rand = new Random();
            Stopwatch watch = new Stopwatch();
            double totalClonesTested = 0;
            do
            {
                game.ApplyMoves(game.me.possibleMoves[rand.Next(0, game.me.possibleMoves.Count - 1)], game.opponent.possibleMoves[rand.Next(0, game.opponent.possibleMoves.Count - 1)]);
                
                watch.Start();
                double numberOfClones = 10000.0;
                totalClonesTested += numberOfClones;
                for (int i = 0; i < numberOfClones; i++)
                {
                    var clone = game.Clone();
                }
                watch.Stop();
            }
            while (game.day < 24);


           

            Console.Error.WriteLine($"Elapsed ms per clone: {watch.ElapsedMilliseconds/ totalClonesTested}");
        }

        [Fact]
        public void RunManyClones()
        {
            Stopwatch watch = new Stopwatch();
            watch.Start();
            double numberOfClones = 100000.0;
            for (int i = 0; i < numberOfClones; i++)
            {
                var clone = game.Clone();
            }
            watch.Stop();

            Console.Error.WriteLine($"Elapsed ms per clone: {watch.ElapsedMilliseconds / numberOfClones}");
        }
    }
}
