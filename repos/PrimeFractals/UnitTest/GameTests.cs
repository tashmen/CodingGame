using GameSolution;
using System;
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
        public void BasicPascal()
        {
            Pascal p = new Pascal();
            p.BuildTriangle(1);
            Assert.Equal((ulong)1, p.Triangle[0][0]);
        }

        [Fact]
        public void BasicPascal2()
        {
            Pascal p = new Pascal();
            p.BuildTriangle(2);
            Assert.Equal((ulong)1, p.Triangle[1][1]);
        }

        [Fact]
        public void BasicPascal3()
        {
            Pascal p = new Pascal();
            p.BuildTriangle(3);
            Assert.Equal((ulong)2, p.Triangle[2][1]);
        }

        [Fact]
        public void BasicPascal4()
        {
            Pascal p = new Pascal();
            p.BuildTriangle(4);
            Assert.Equal((ulong)3, p.Triangle[3][1]);
        }

        [Theory]
        [InlineData(3, 24, 6, 78)]
        [InlineData(3, 24, 24, 162)]
        [InlineData(5, 47, 47, 555)]
        [InlineData(5, 114, 114, 2625)]
        [InlineData(13, 13, 13, 91)]
        [InlineData(5, 3, 3, 6)]
        [InlineData(7, 20, 20, 147)]
        [InlineData(13, 61, 25, 724)]
        [InlineData(13, 12, 4, 42)]
        [InlineData(11, 63, 27, 831)]
        [InlineData(11, 63, 63, 1206)]
        [InlineData(5, 68, 68, 1017)]
        [InlineData(17, 43, 8, 260)]
        [InlineData(5, 136, 136, 3471)]
        //[InlineData(5, 930886, 692777, 0)]
        public void CountPascal1(long P, long R, long C, long expected)
        {
            Pascal p = new Pascal();
            p.BuildTriangle(R);
            var count = p.CountTriangle(R, C, P);
            Assert.Equal(expected, count);

            count = p.CountTriangleOptimized(R, C, P);
            Assert.Equal(expected, count);
        }
    }
}
