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

        private GameState game;

        public GameStateTests(ITestOutputHelper output)
        {
            var converter = new Converter(output);
            Console.SetError(converter);

            game = GameBuilder.BuildSampleGame();
        }



        [Fact]
        public void RunManyClonesOverOneRandomGame()
        {
            Random rand = new Random();
            Stopwatch watch = new Stopwatch();
            double totalClonesTested = 0;
            do
            {
                game.ApplyMoves(game.me.possibleMoves[rand.Next(0, game.me.possibleMoves.Count)], game.opponent.possibleMoves[rand.Next(0, game.opponent.possibleMoves.Count)]);
                watch.Start();
                double numberOfClones = 100000.0;
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
        public void RunManyApplyMovesRandomly()
        {
            Random rand = new Random();
            Stopwatch watch = new Stopwatch();
            double totalMovesPlayed = 0;
            GameState clone = game.Clone() as GameState;
            watch.Start();
            for (int i = 0; i < 50000; i++)
            {
                game.ApplyMoves(game.me.possibleMoves[rand.Next(0, game.me.possibleMoves.Count)], game.opponent.possibleMoves[rand.Next(0, game.opponent.possibleMoves.Count)]);
                totalMovesPlayed += 2;

                if(game.day == 24)
                {
                    game = clone.Clone() as GameState;
                }
            }
            watch.Stop();

            Console.Error.WriteLine($"Elapsed ms per move: {watch.ElapsedMilliseconds / totalMovesPlayed}");
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


        [Fact]
        public void RunManyGameUpdates()
        {
            Stopwatch watch = new Stopwatch();
            watch.Start();
            double numberOfUpdates = 100000.0;
            for (int i = 0; i < numberOfUpdates; i++)
            {
                game.UpdateGameState();
            }
            watch.Stop();

            Console.Error.WriteLine($"Elapsed ms per clone: {watch.ElapsedMilliseconds / numberOfUpdates}");
        }
    }
}
