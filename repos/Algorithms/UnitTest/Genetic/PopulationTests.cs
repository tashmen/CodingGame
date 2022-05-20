using Algorithms.Genetic;
using Algorithms.NeuralNetwork;
using System;
using System.Threading.Tasks;
using UnitTest.Fixtures;
using UnitTest.TwentyOneGame;
using Xunit;
using Xunit.Abstractions;

namespace UnitTest.Algorithms
{
    public class TestIndividual : Individual
    {
        public double Fitness {get;set;}

        public double CalculateFitness()
        {
            throw new NotImplementedException();
        }

        public Individual Clone()
        {
            throw new NotImplementedException();
        }

        public Individual CreateBaby(Individual parent1, Individual parent2, double crossOver)
        {
            throw new NotImplementedException();
        }

        public object GetNextMove()
        {
            throw new NotImplementedException();
        }

        public void Mutate(double mutationRate)
        {
            throw new NotImplementedException();
        }
    }

    public class PopulationTests
    {
        public PopulationTests(ITestOutputHelper output)
        {
            var converter = new TestOutputFixture(output);
            Console.SetError(converter);
        }
        
        [Fact]
        public void Sort_Test()
        {
            Population population = new Population();
            population.Add(new TestIndividual());
            population.Add(new TestIndividual());
            population[0].Fitness = -1;
            population[1].Fitness = 2;
            population.SortPopulationByFitness();
            Assert.Equal(2, population[0].Fitness);
            Assert.Equal(-1, population[1].Fitness);
        }
    }
}
