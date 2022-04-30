using System;
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

      
    }
}
