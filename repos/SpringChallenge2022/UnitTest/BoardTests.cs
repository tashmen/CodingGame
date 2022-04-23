using GameSolution.Entities;
using System;
using System.Collections;
using Xunit;
using Xunit.Abstractions;

namespace UnitTest
{

    public class BoardTests
    {
        private Board board;
        public BoardTests(ITestOutputHelper output)
        {
            var converter = new Converter(output);
            Console.SetError(converter);

            //board = new Board();
        }
    }
}
