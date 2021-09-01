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

            game = GameBuilder.BuildSampleGame();
        }

        [Fact]
        public void StrongOpponentSimulationTest()
        {
            MonteCarloTreeSearch search = new MonteCarloTreeSearch(true, MonteCarloTreeSearch.SearchStrategy.Sequential);
            //Setup a second game so that both players can play as max
            GameState game2 = game.Clone() as GameState;
            foreach (Tree tree in game2.TreeEnumeration)//Invert tree ownership
            {
                tree.ChangeOwnership();
            }
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

                Stopwatch watch = new Stopwatch();
                watch.Start();
                search.SetState(game2, true);
                object moveToPlay = search.GetNextMove(watch, 95, -1, 20);
                Move move = moveToPlay as Move;

                game.ApplyMove(myMove, true);
                game.ApplyMove(move, false);

                game2.ApplyMove(move, true);
                game2.ApplyMove(myMove, false);

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
            MonteCarloTreeSearch search = new MonteCarloTreeSearch(true, MonteCarloTreeSearch.SearchStrategy.Sequential);
            do
            {
                Stopwatch watch = new Stopwatch();
                watch.Start();
                search.SetState(game);
                object moveToPlay = search.GetNextMove(watch, 95, -1, 20);
                Move move = moveToPlay as Move;
                Console.Error.WriteLine(move.ToString());

                watch.Stop();
                Console.Error.WriteLine($"ms: {watch.ElapsedMilliseconds}");
                Move opponentMove = game.opponent.possibleMoves[rand.Next(0, game.opponent.possibleMoves.Count)];
                Console.Error.WriteLine($"MCTS: {move.ToString()}, Random: {opponentMove.ToString()}");
                game.ApplyMoves(move, opponentMove);
            }
            while (game.day < 24);

            Console.Error.WriteLine(game.ToString());
            Assert.Equal(1, game.GetWinner());
        }
    }
}
