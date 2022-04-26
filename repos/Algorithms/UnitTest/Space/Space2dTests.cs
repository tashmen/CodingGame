using Algorithms.Space;
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
        public void TestFindCircleWithMaximumPoints_AtIndex()
        {
            var points = new List<Point2d>()
            {
                new Point2d(5197,4972),
                new Point2d(5311,4455),
                new Point2d(4991,3415)
            };
            var result = Space2d.FindCircleWithMaximumPoints(points.ToArray(), 799, 0);
            Assert.Equal(3, result.Item1);
            var point = new Point2d(5197, 4972);
            Assert.True(point.GetDistance(result.Item2) <= 800);
        }

        [Theory]
        [InlineData(0, 0, 0, 800, 800, 0, 800)]
        [InlineData(0, 0, 0, 1600, 800, 0, 800)]
        [InlineData(0, 0, 1600, 1600, 800, 565, 565)]
        [InlineData(0, 0, -1600, -1600, 800, -565, -565)]
        [InlineData(0, 0, -1600, 1600, 800, -565, 565)]
        [InlineData(0, 0, 1600, -1600, 800, 565, -565)]
        public void TestTranslatePoint(int x, int y, int x1, int y1, double maxDistance, int rx, int ry)
        {
            Point2d startPoint = new Point2d(x, y);
            Point2d targetPoint = new Point2d(x1, y1);

            Point2d resultPoint = Space2d.TranslatePoint(startPoint, targetPoint, maxDistance);
            Assert.Equal(rx, resultPoint.GetTruncatedX());
            Assert.Equal(ry, resultPoint.GetTruncatedY());
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
