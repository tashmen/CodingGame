using GameSolution.Algorithms.NeuralNetwork;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnitTest.Fixtures;
using Xunit;
using Xunit.Abstractions;

namespace UnitTest.Algorithms
{
    public class NeuralNetworkTests
    {
        public NeuralNetworkTests(ITestOutputHelper output)
        {
            var converter = new TestOutputFixture(output);
            Console.SetError(converter);
        }

        [Fact]
        public void TestSaveAndLoad()
        {
            NeuralNetwork network = new NeuralNetwork(4, new int[] { 3, 4, 5, 1 }, 10);
            using (var writer = new BinaryWriter(new FileStream("./test.data", FileMode.Create)))
            {
                network.Save(writer);
            }

            NeuralNetwork network2;
            using (var reader = new BinaryReader(new FileStream("./test.data", FileMode.Open)))
            {
                network2 = new NeuralNetwork(reader);
            }

            Assert.True(network.Equals(network2));

        }
    }
}
