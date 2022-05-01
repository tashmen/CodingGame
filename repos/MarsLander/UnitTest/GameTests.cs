using Algorithms.Space;
using GameSolution.Entities;
using System;
using System.Collections.Generic;
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
            Assert.True(b.ShipCollision(new Ship(25, -5, 0, 0, 0, 0, 0)));
            Assert.False(b.ShipCollision(new Ship(25, 5, 0, 0, 0, 0, 0)));
        }

        [Fact]
        public void SafeLandTest()
        {
            IList<Point2d> points = new List<Point2d>();
            points.Add(new Point2d(0, 0));
            points.Add(new Point2d(50, 0));
            Board b = new Board(points);
            Assert.True(b.ShipLanded(new Ship(25, 0, 14, 30, 0, 0, 4)));
            Assert.False(b.ShipLanded(new Ship(25, 20, 14, 30, 0, 0, 4)));
        }

      
    }
}
