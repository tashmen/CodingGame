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


    public class MinimaxTests
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

        public MinimaxTests(ITestOutputHelper output)
        {
            var converter = new Converter(output);
            Console.SetError(converter);

            game = new GameState();

            game.board.Insert(0, new Cell(0, 2, new int[] { 1, 2, 3, 4, 5, 6 }));
            game.board.Insert(1, new Cell(1, 1, new int[] { -1, -1, 2, 0, 6, -1 }));
            game.board.Insert(2, new Cell(2, 1, new int[] { -1, -1, -1, 3, 0, 1 }));
            game.board.Insert(3, new Cell(3, 1, new int[] { 2, -1, -1, -1, 4, 0 }));
            game.board.Insert(4, new Cell(4, 1, new int[] { 0, 3, -1, -1, -1, 5 }));
            game.board.Insert(5, new Cell(5, 1, new int[] { 6, 0, 4, -1, -1, -1 }));
            game.board.Insert(6, new Cell(6, 1, new int[] { -1, 1, 0, 5, -1, -1 }));

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
        public void SimulationTest()
        {
            Random rand = new Random();
            Minimax search = new Minimax();
            do
            {
                Stopwatch watch = new Stopwatch();
                watch.Start();
                search.SetState(game);
                IMove moveToPlay = search.GetNextMove(watch, 100);
                Move move = moveToPlay as Move;
                Console.Error.WriteLine(move.ToString());

                watch.Stop();
                Console.Error.WriteLine($"ms: {watch.ElapsedMilliseconds}");

                game.ApplyMoves(move, game.opponent.possibleMoves[rand.Next(0, game.opponent.possibleMoves.Count)]);
            }
            while (game.day < 24);

            Console.Error.WriteLine(game.ToString());
            Assert.Equal(1, game.GetWinner());
        }
    }
}
