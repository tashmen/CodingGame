using Algorithms.Genetic;
using Algorithms.Space;
using GameSolution.Entities;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace GameSolution.Game
{
    public class GameHelper
    {
        public static void PlayGame(GameState state)
        {
            Population Population = new Population();

            for (int i = 0; i < 100; i++)
            {
                Population.Add(new MarsLanderSolution(state));
            }
            GeneticAlgorithm genetic = new GeneticAlgorithm(Population, 0.05, 0.20, 0.2);

            do
            {
                Stopwatch watch = new Stopwatch();
                watch.Start();
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
        }

        public static GameState CreateSimpleGame()
        {
            IList<Point2d> points = new List<Point2d>();
            points.Add(new Point2d(0, 100));
            points.Add(new Point2d(7000, 100));
            Board board = new Board(points);
            GameState state = new GameState(board);
            Ship ship = new Ship(3500, 2900, 0, 0, 600, 0, 0);
            state.SetShip(ship);

            return state;
        }

        public static GameState CreateInitialSpeedCorrectSide()
        {
            /*
             * (0,100)
(1000,500)
(1500,100)
(3000,100)
(3500,500)
(3700,200)
(5000,1500)
(5800,300)
(6000,1000)
(6999,2000)
             * */
            IList<Point2d> points = new List<Point2d>();
            points.Add(new Point2d(0, 100));
            points.Add(new Point2d(1000, 500));
            points.Add(new Point2d(1500, 100));
            points.Add(new Point2d(3000, 100));
            points.Add(new Point2d(3500, 500));
            points.Add(new Point2d(3700, 200));
            points.Add(new Point2d(5000, 1500));
            points.Add(new Point2d(5800, 300));
            points.Add(new Point2d(6000, 1000));
            points.Add(new Point2d(6999, 2000));
            Board board = new Board(points);
            GameState state = new GameState(board);
            Ship ship = new Ship(6500, 2800, -100, 0, 600, 90, 0);
            state.SetShip(ship);

            return state;
        }
    }
}
