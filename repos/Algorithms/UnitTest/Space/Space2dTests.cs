﻿using GameSolution.Algorithms;
using System;
using System.Collections.Generic;
using UnitTest.Fixtures;
using Xunit;
using Xunit.Abstractions;

namespace UnitTest.Algorithms
{
    public class Space2dTests
    {
        public Space2dTests(ITestOutputHelper output)
        {
            var converter = new TestOutputFixture(output);
            Console.SetError(converter);
        }
        
        [Fact]
        public void TestFindCircleWithMaximumPoints()
        {
            Assert.Null(Space2d.FindCircleWithMaximumPoints(null, 0));
            Assert.Null(Space2d.FindCircleWithMaximumPoints(new Point2d[0], 1));

            var points = new List<Point2d>{ new Point2d(0,0) };

            var result = Space2d.FindCircleWithMaximumPoints(points.ToArray(), 0);
            Assert.Null(result);

            result = Space2d.FindCircleWithMaximumPoints(points.ToArray(), 0.1);
            Assert.Equal(1, result.Item1);
            Assert.True(new Point2d(0.1, 0).Equals(result.Item2));

            points.Add(new Point2d(0, 1));
            result = Space2d.FindCircleWithMaximumPoints(points.ToArray(), 0.1);
            Assert.Equal(1, result.Item1);
            Assert.True(new Point2d(0.1, 0).Equals(result.Item2));

            result = Space2d.FindCircleWithMaximumPoints(points.ToArray(), 0.5);
            Assert.Equal(2, result.Item1);
            Assert.True(new Point2d(0, 0.5).Equals(result.Item2));

            points.Add(new Point2d(1, 0));
            result = Space2d.FindCircleWithMaximumPoints(points.ToArray(), 1);
            Assert.Equal(3, result.Item1);
            Assert.True(new Point2d(0, 0).Equals(result.Item2));
        }
    }
}
