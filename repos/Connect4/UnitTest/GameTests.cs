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
    

    
    public class GameTests
    {
        private GameState game;
        private Converter converter;

        public GameTests(ITestOutputHelper output)
        {
            converter = new Converter(output);
            Console.SetError(converter);

            game = GameBuilder.BuildEmptyGame();
        }

        [Fact]
        public void RandomSimulationTest()
        {
            
        }

        [Fact]
        public void MonteCarloTreeSearch_SetState_Test()
        {
            for(int i = 0; i < 100; i++)
            {
                MonteCarloTreeSearch search = new MonteCarloTreeSearch();
                search.SetState(game, true, false);
            }
        }

        [Fact]
        public void PlaySelf_One_Monster()
        {
            MonteCarloTreeSearch search = new MonteCarloTreeSearch();
            
            game = GameBuilder.BuildGameWithSingleMonsterHeadingTowardMax();
            do
            {
                PlayGame(search);
            }
            while (!game.GetWinner().HasValue);
        }

        [Fact]
        public void PlaySelf_No_Monsters()
        {
            MonteCarloTreeSearch search = new MonteCarloTreeSearch();

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

        private void PlayGame(MonteCarloTreeSearch search)
        {
            int timeout = 50;

            Stopwatch watch = new Stopwatch();
            watch.Start();
            GC.Collect();
            search.SetState(game, true, false);
            //Console.Error.WriteLine($"max ms: {watch.ElapsedMilliseconds}");
            Move move = (Move)search.GetNextMove(watch, 45, 12, 20);
            game.ApplyMove(move, true);
            watch.Stop();

            watch.Reset();
            watch.Start();
            GC.Collect();
            search.SetState(game, false, false);
            //Console.Error.WriteLine($"min ms: {watch.ElapsedMilliseconds}");
            move = (Move)search.GetNextMove(watch, 45, 12, 20);
            game.ApplyMove(move, false);
            watch.Stop();
        }
    }
}
