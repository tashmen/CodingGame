﻿using Algorithms.Space;
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
    public class GameTests
    {
        private Converter converter;

        public GameTests(ITestOutputHelper output)
        {
            converter = new Converter(output);
            Console.SetError(converter);

        }

        [Fact]
        public void CollisionTest()
        {
            IList<Point2d> points = new List<Point2d>();
            points.Add(new Point2d(0, 0));
            points.Add(new Point2d(50, 0));
            Board b = new Board(points);
            Assert.True(b.ShipCollision(new Ship(25, -5, 0, -10, 0, 0, 0)));
            Assert.False(b.ShipCollision(new Ship(25, 5, 0, 0, 0, 0, 0)));
            Assert.True(b.ShipCollision(new Ship(25, 0, 0, -5, 0, 0, 0)));
        }

        [Fact]
        public void CollisionTest_AtPeak()
        { 
            IList<Point2d> points = new List<Point2d>();
            points.Add(new Point2d(3200, 1000));
            points.Add(new Point2d(3500, 2000));
            points.Add(new Point2d(3800, 800));
            Board b = new Board(points);
            Assert.True(b.ShipCollision(new Ship(3451, 1698, 67, -41, 712, 26, 4)));
        }

        [Fact]
        public void ShipCollisionThroughLine()
        {
            IList<Point2d> points = new List<Point2d>();
            points.Add(new Point2d(500, 2100));
            points.Add(new Point2d(1500, 2100));
            points.Add(new Point2d(2000, 200));
            Board b = new Board(points);
            Assert.True(b.ShipCollision(new Ship(1482, 2102, -26, 3, 500, 0, 4)));
        }

        [Fact]
        public void ShipLanded_WhereShipIsUnderPoint()
        {
            IList<Point2d> points = new List<Point2d>();
            points.Add(new Point2d(500, 2100));
            points.Add(new Point2d(1500, 2100));
            Board b = new Board(points);
            Assert.False(b.ShipLanded(new Ship(1492, 2079, -20, -4, 300, 0, 4)));
        }

        [Fact]
        public void SafeLandTest()
        {
            IList<Point2d> points = new List<Point2d>();
            points.Add(new Point2d(0, 0));
            points.Add(new Point2d(50, 0));
            Board b = new Board(points);
            Assert.True(b.ShipLanded(new Ship(25, -1, 14, -30, 0, 0, 4)));
            Assert.False(b.ShipLanded(new Ship(25, 20, 14, -30, 0, 0, 4)));
            Assert.False(b.ShipLanded(new Ship(25, 0, 0, -5, 0, 0, 0)));
        }

        [Fact]
        public void MonteCarloEasyTest()
        {
            IList<Point2d> points = new List<Point2d>();
            points.Add(new Point2d(0, 100));
            points.Add(new Point2d(7000, 100));
            Board board = new Board(points);
            GameState state = new GameState(board);
            Ship ship = new Ship(3500, 2900, 0, 0, 600, 0, 0);
            state.SetShip(ship);
            MonteCarloTreeSearch search = new MonteCarloTreeSearch(true);
            search.SetState(state);
            do
            {
                Stopwatch watch = new Stopwatch();
                watch.Start();
                var move = search.GetNextMove(watch, 98, -1, 30);
                state.ApplyMove(move, true);

                watch.Stop();
            }
            while (state.GetWinner() == null);

            Assert.True(state.GetWinner() >= 1);
        }
    }
}
