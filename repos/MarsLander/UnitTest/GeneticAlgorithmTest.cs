using Algorithms.Genetic;
using Algorithms.Space;
using Algorithms.Trees;
using GameSolution;
using GameSolution.Entities;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using Xunit;
using Xunit.Abstractions;

namespace UnitTest
{



    public class GeneticAlgorithmTests
    {
        private Converter converter;
        private Population Population { get; set; }

        public GeneticAlgorithmTests(ITestOutputHelper output)
        {
            converter = new Converter(output);
            Console.SetError(converter);

            Population = new Population();
        }

        [Fact]
        public void EasyTest()
        {
            IList<Point2d> points = new List<Point2d>();
            points.Add(new Point2d(0, 100));
            points.Add(new Point2d(7000, 100));
            Board board = new Board(points);
            GameState state = new GameState(board);
            Ship ship = new Ship(3500, 2900, 0, 0, 600, 0, 0);
            state.SetShip(ship);

            for (int i = 0; i < 100; i++)
            {
                Population.Add(new MarsLanderSolution(state));
            }
            GeneticAlgorithm genetic = new GeneticAlgorithm(Population, 0.01, 0.10, 0.2);

            do
            {
                Stopwatch watch = new Stopwatch();
                //watch.Start();
                var move = (Move)genetic.GetNextMove(watch, 98, genetic.GenerationCounter + 1000);
                Population = genetic.Population;

                Console.Error.WriteLine($"gen: {genetic.GenerationCounter}, move: {move}, score: {Population.GetBestIndividual().Fitness}");


                state.ApplyMove(move, true);

                foreach (Individual i in Population)
                {
                    var solution = (MarsLanderSolution)i;
                    solution.AdvanceTurn(state);
                }

                watch.Stop();
            }
            while (state.GetWinner() == null);

            Assert.True(state.GetWinner() >= 1);
        }

      
    }
}
