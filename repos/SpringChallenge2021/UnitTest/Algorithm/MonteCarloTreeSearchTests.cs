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


    public class MonteCarloTreeSearchTests
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

        public MonteCarloTreeSearchTests(ITestOutputHelper output)
        {
            var converter = new Converter(output);
            Console.SetError(converter);

            game = new GameState();

            game.board.Insert(0, new Cell(0, 2, new List<int>() { 1, 2, 3, 4, 5, 6 }));
            game.board.Insert(1, new Cell(1, 1, new List<int>() { -1, -1, 2, 0, 6, -1 }));
            game.board.Insert(2, new Cell(2, 1, new List<int>() { -1, -1, -1, 3, 0, 1 }));
            game.board.Insert(3, new Cell(3, 1, new List<int>() { 2, -1, -1, -1, 4, 0 }));
            game.board.Insert(4, new Cell(4, 1, new List<int>() { 0, 3, -1, -1, -1, 5 }));
            game.board.Insert(5, new Cell(5, 1, new List<int>() { 6, 0, 4, -1, -1, -1 }));
            game.board.Insert(6, new Cell(6, 1, new List<int>() { -1, 1, 0, 5, -1, -1 }));

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
        public void StrongOpponentSimulationTest()
        {
            Random rand = new Random();
            MonteCarloTreeSearch search = new MonteCarloTreeSearch();
            do
            {
                Move myMove;
                if (!game.me.isWaiting)
                {
                    GameState clonedState = game.Clone() as GameState;
                    GameHelper gameHelper = new GameHelper(clonedState, clonedState.me.possibleMoves);
                    myMove = gameHelper.GetNextMove();
                }
                else
                {
                    myMove = game.GetMove(true) as Move;
                }
                game.ApplyMove(myMove, true);


                Stopwatch watch = new Stopwatch();
                watch.Start();
                search.SetState(game, false);
                IMove moveToPlay = search.GetNextMove(watch, 95, 20);
                Move move = moveToPlay as Move;

                game.ApplyMove(move, false);

                watch.Stop();
                Console.Error.WriteLine($"ms: {watch.ElapsedMilliseconds}");
                Console.Error.WriteLine($"MCTS: {move.ToString()}, Strong: {myMove.ToString()}");
                Console.Error.WriteLine(game.ToString());
            }
            while (game.day < 24);

            Console.Error.WriteLine(game.ToString());
            Assert.Equal(-1, game.GetWinner());
        }
        [Fact]
        public void RandomSimulationTest()
        {
            Random rand = new Random();
            MonteCarloTreeSearch search = new MonteCarloTreeSearch();
            do
            {
                Stopwatch watch = new Stopwatch();
                watch.Start();
                search.SetState(game);
                IMove moveToPlay = search.GetNextMove(watch, 95, 20);
                Move move = moveToPlay as Move;
                Console.Error.WriteLine(move.ToString());

                watch.Stop();
                Console.Error.WriteLine($"ms: {watch.ElapsedMilliseconds}");
                Move opponentMove = game.opponent.possibleMoves[rand.Next(0, game.opponent.possibleMoves.Count - 1)];
                Console.Error.WriteLine($"MCTS: {move.ToString()}, Random: {opponentMove.ToString()}");
                game.ApplyMoves(move, opponentMove);
            }
            while (game.day < 24);

            Console.Error.WriteLine(game.ToString());
            Assert.Equal(1, game.GetWinner());
        }
    }
}
