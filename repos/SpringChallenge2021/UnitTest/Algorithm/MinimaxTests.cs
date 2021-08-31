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

            game = GameBuilder.BuildSampleGame();
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
