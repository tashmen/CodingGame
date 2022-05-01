using Algorithms.Space;
using System;
using System.Collections.Generic;
using UnitTest.Fixtures;
using Xunit;
using Xunit.Abstractions;

namespace UnitTest.Algorithms
{
    public class Point2dTests
    {
        public Point2dTests(ITestOutputHelper output)
        {
            var converter = new TestOutputFixture(output);
            Console.SetError(converter);
        }

        [Fact]
        public void AbsoluteRoundPoint_Test()
        {
            var point = new Point2d(-1, -24);
            var roundedPoint = point.GetRoundedAwayFromZeroPoint();
            Assert.True(point.Equals(roundedPoint));

            point = new Point2d(-1.5, 24.5);
            roundedPoint = point.GetRoundedAwayFromZeroPoint();
            Assert.Equal(-2, roundedPoint.x);
            Assert.Equal(25, roundedPoint.y);
        }

        
    }
}
