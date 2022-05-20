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
    public class GeneticAlgorithmTests
    {
        public GeneticAlgorithmTests(ITestOutputHelper output)
        {
            var converter = new TestOutputFixture(output);
            Console.SetError(converter);
        }
        
        [Fact]
        public void Test_Play_21_Game_Neural()
        {
            
            Population population = new Population();
            for(int i =0; i<100; i++)
            {
                population.Add(new NeuralNetwork(4, new int[] { 1, 16, 16, 3, 1 }, 1));
            }

            GeneticAlgorithm genetic = new GeneticAlgorithm(population, 0.01, 0.05, 0.7);
            
            do
            {
                population = genetic.GenerateNextGeneration();
                Parallel.For(0, 100, (p) =>
                {
                    Random rand = new Random();
                    for (int g = 0; g < 50; g++)
                    {
                        NeuralNetwork network = (NeuralNetwork)population[p];
                        if(g == 0)
                        {
                            network.Fitness = 0;
                        }
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

                                if (false && rand.Next(0, 45) < network.Fitness)
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

                        network.Fitness = network.Fitness + (int)(state.GetWinner().Value) + 1;
                    }
                });
            }
            while (population.AverageFitness() < 60);

            Individual individual = population.GetBestIndividual();
            Console.Error.WriteLine(genetic.GenerationCounter);
            Console.Error.WriteLine(individual.Fitness);
        }

        [Fact]
        public void Play_21_Game_Genetic()
        {
            
        }
    }
}
