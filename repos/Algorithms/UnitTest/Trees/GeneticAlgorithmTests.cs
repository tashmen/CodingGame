using Algorithms;
using GameSolution.Algorithms.Genetic;
using GameSolution.Algorithms.NeuralNetwork;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using UnitTest.Fixtures;
using UnitTest.TwentyOneGame;
using Xunit;
using Xunit.Abstractions;

namespace UnitTest.Algorithms
{
    public class GeneticAlgorithmTests
    {
        public GeneticAlgorithmTests(ITestOutputHelper output)
        {
            var converter = new TestOutputFixture(output);
            Console.SetError(converter);
        }
        
        [Fact]
        public void Test_Play_21_Game()
        {
            
            Population population = new Population(100);
            for(int i =0; i<100; i++)
            {
                population.addIndividual(new NeuralNetwork(4, new int[] { 1, 16, 4, 1 }, 1));
            }

            GeneticAlgorithm genetic = new GeneticAlgorithm(population, 0.01, 0.05, 0.7);
            
            do
            {
                population = genetic.runOnce();
                Parallel.For(0, 100, (p) =>
                {
                    Random rand = new Random();
                    for (int g = 0; g < 50; g++)
                    {
                        NeuralNetwork network = (NeuralNetwork)population.getIndividual(p);
                        GameState state = new GameState();
                        bool isMax = true;
                        do
                        {
                            var output = network.output(new double[] { 1 });
                            var addcount = (int)(output[0] * 3) + 1;
                            //addcount = rand.Next(1, 3);
                            Move move = new Move(addcount);
                            state.ApplyMove(move, isMax);

                            if (state.Total < 21)
                            {
                                int sticks;

                                if (false && rand.Next(0, 45) < network.GetFitness())
                                {
                                    sticks = state.Total % 4;
                                    if (sticks == 3)
                                    {
                                        sticks = 2;
                                    }
                                    else if (sticks != 1)
                                        sticks = sticks + 1;
                                }
                                else sticks = rand.Next(1, 3);

                                move = new Move(sticks);
                                state.ApplyMove(move, !isMax);
                            }
                        } while (!state.GetWinner().HasValue);

                        network.SetFitness(network.GetFitness() + (int)(state.GetWinner().Value) + 1);
                    }
                });
            }
            while (population.avgFitness() < 50);

            population.sortPopulation();
            Individual individual = population.getIndividual(0);
            Console.Error.WriteLine(genetic.generationCounter);
            Console.Error.WriteLine(individual.GetFitness());
        }
    }
}
