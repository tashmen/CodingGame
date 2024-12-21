using GameSolution.Entities;
using System;
using System.Collections;
using Xunit;
using Xunit.Abstractions;

namespace UnitTest
{

    public class MoveTests
    {
        public MoveTests(ITestOutputHelper output)
        {
            var converter = new Converter(output);
            Console.SetError(converter);
        }
    }
}
