using Algorithms.Genetic;
using Algorithms.Space;
using GameSolution.Entities;
using System;
using System.Collections.Generic;
using System.Diagnostics;

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
            GeneticAlgorithm genetic = new GeneticAlgorithm(Population, 0.01, 0.20, 0.2);
            var limit = 995;
            do
            {
                Stopwatch watch = new Stopwatch();
                watch.Start();
                var move = (Move)genetic.GetNextMove(watch, limit, genetic.GenerationCounter + 1000);
                Population = genetic.Population;

                Console.Error.WriteLine($"gen: {genetic.GenerationCounter}, move: {move}, score: {Population.GetBestIndividual().Fitness}");


                state.ApplyMove(move, true);

                foreach (Individual i in Population)
                {
                    var solution = (MarsLanderSolution)i;
                    solution.AdvanceTurn(state);
                }

                watch.Stop();
                limit = 45;
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

        public static GameState CreateInitialSpeedWrongSide()
        {
            /*
             * (0,100)
(1000,500)
(1500,1500)
(3000,1000)
(4000,150)
(5500,150)
(6999,800)
             * */

            IList<Point2d> points = new List<Point2d>();
            points.Add(new Point2d(0, 100));
            points.Add(new Point2d(1000, 500));
            points.Add(new Point2d(1500, 1500));
            points.Add(new Point2d(3000, 1000));
            points.Add(new Point2d(4000, 150));
            points.Add(new Point2d(5500, 150));
            points.Add(new Point2d(6999, 800));
            Board board = new Board(points);
            GameState state = new GameState(board);
            Ship ship = new Ship(6500, 2800, -90, 0, 750, 90, 0);
            state.SetShip(ship);

            return state;
        }

        public static GameState CreateDeepCanyon()
        {
            /*
             * (0,1000)
(300,1500)
(350,1400)
(500,2000)
(800,1800)
(1000,2500)
(1200,2100)
(1500,2400)
(2000,1000)
(2200,500)
(2500,100)
(2900,800)
(3000,500)
(3200,1000)
(3500,2000)
(3800,800)
(4000,200)
(5000,200)
(5500,1500)
(6999,2800)
             * */

            IList<Point2d> points = new List<Point2d>();
            points.Add(new Point2d(0, 1000));
            points.Add(new Point2d(300, 1500));
            points.Add(new Point2d(350, 1400));
            points.Add(new Point2d(500, 2000));
            points.Add(new Point2d(800, 1800));
            points.Add(new Point2d(1000, 2500));
            points.Add(new Point2d(1200, 2100));
            points.Add(new Point2d(1500, 2400));
            points.Add(new Point2d(2000, 1000));
            points.Add(new Point2d(2200, 500));
            points.Add(new Point2d(2500, 100));
            points.Add(new Point2d(2900, 800));
            points.Add(new Point2d(3000, 500));
            points.Add(new Point2d(3200, 1000));
            points.Add(new Point2d(3500, 2000));
            points.Add(new Point2d(3800, 800));
            points.Add(new Point2d(4000, 200));
            points.Add(new Point2d(5000, 200));
            points.Add(new Point2d(5500, 1500));
            points.Add(new Point2d(6999, 2800));
            Board board = new Board(points);
            GameState state = new GameState(board);
            Ship ship = new Ship(6500, 2800, -90, 0, 750, 90, 0);
            state.SetShip(ship);

            return state;
        }

        public static GameState CreateHighGround()
        {
            /*
             *(0,1000)
(300,1500)
(350,1400)
(500,2100)
(1500,2100)
(2000,200)
(2500,500)
(2900,300)
(3000,200)
(3200,1000)
(3500,500)
(3800,800)
(4000,200)
(4200,800)
(4800,600)
(5000,1200)
(5500,900)
(6000,500)
(6500,300)
(6999,500) 
             */
            IList<Point2d> points = new List<Point2d>();
            points.Add(new Point2d(0, 1000));
            points.Add(new Point2d(300, 1500));
            points.Add(new Point2d(350, 1400));
            points.Add(new Point2d(500, 2100));
            points.Add(new Point2d(1500, 2100));
            points.Add(new Point2d(2000, 200));
            points.Add(new Point2d(2500, 500));
            points.Add(new Point2d(2900, 300));
            points.Add(new Point2d(3000, 200));
            points.Add(new Point2d(3200, 1000));
            points.Add(new Point2d(3500, 500));
            points.Add(new Point2d(3800, 800));
            points.Add(new Point2d(4000, 200));
            points.Add(new Point2d(4200, 800));
            points.Add(new Point2d(4800, 600));
            points.Add(new Point2d(5000, 1200));
            points.Add(new Point2d(5500, 900));
            points.Add(new Point2d(6000, 500));
            points.Add(new Point2d(6500, 300));
            points.Add(new Point2d(6999, 500));
            Board board = new Board(points);
            GameState state = new GameState(board);
            Ship ship = new Ship(6500, 2800, -90, 0, 750, 90, 0);
            state.SetShip(ship);

            return state;

        }

        public static GameState CreateCaveWrongSide()
        {
            /*
             *(0,1800)
(300,1200)
(1000,1550)
(2000,1200)
(2500,1650)
(3700,220)
(4700,220)
(4750,1000)
(4700,1650)
(4000,1700)
(3700,1600)
(3750,1900)
(4000,2100)
(4900,2050)
(5100,1000)
(5500,500)
(6200,800)
(6999,600)
             */
            IList<Point2d> points = new List<Point2d>();
            points.Add(new Point2d(0, 1800));
            points.Add(new Point2d(300, 1200));
            points.Add(new Point2d(1000, 1550));
            points.Add(new Point2d(2000, 1200));
            points.Add(new Point2d(2500, 1650));
            points.Add(new Point2d(3700, 220));
            points.Add(new Point2d(4700, 220));
            points.Add(new Point2d(4750, 1000));
            points.Add(new Point2d(4700, 1650));
            points.Add(new Point2d(4000, 1700));
            points.Add(new Point2d(3700, 1600));
            points.Add(new Point2d(3750, 1900));
            points.Add(new Point2d(4000, 2100));
            points.Add(new Point2d(4900, 2050));
            points.Add(new Point2d(5100, 1000));
            points.Add(new Point2d(5500, 500));
            points.Add(new Point2d(6200, 800));
            points.Add(new Point2d(6999, 600));
            Board board = new Board(points);
            GameState state = new GameState(board);
            Ship ship = new Ship(6500, 2000, 0, 0, 1200, 0, 0);
            state.SetShip(ship);

            return state;

        }
    }
}
