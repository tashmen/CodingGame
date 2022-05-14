using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Algorithms.Genetic
{
    public class GeneticAlgorithm
    {
        /**The population for the genetic algorithm to choose from*/
        public Population Population;
        /**The rate at which mutations occur*/
        private double MutationRate;
        /**The percent of the original population that will be in the new population*/
        private double ElitePercent;
        /**The percent of the chromosome to use from the first parent*/
        private double CrossOver;
        public int GenerationCounter;

        /**Sets the initial population and the mutation rate*/
        public GeneticAlgorithm(Population initialPopulation, double mRate, double eP, double cO)
        {
            Population = initialPopulation;
            MutationRate = mRate;
            ElitePercent = eP;
            CrossOver = cO;
            GenerationCounter = 0;
        }

        /// <summary>
        /// Retrieves the next best move
        /// </summary>
        /// <param name="watch">Timer that is counting the time limit</param>
        /// <param name="timeLimit">How long to run the algorithm for</param>
        /// <param name="maxGeneration">The highest generation to reach</param>
        /// <returns>The best move</returns>
        public object GetNextMove(Stopwatch watch, int timeLimit, int maxGeneration = -1)
        {
            do
            {
                var counter = 0;
                foreach(Individual i in Population)
                {
                    if (watch.ElapsedMilliseconds >= timeLimit && counter > 1)
                    {
                        break;
                    }
                    if(i.Fitness == double.MinValue)
                        i.CalculateFitness();
                    counter++;
                }
                GenerateNextGeneration();
            }
            while (watch.ElapsedMilliseconds < timeLimit && GenerationCounter != maxGeneration);
            
            var bestIndividual = Population.GetBestIndividual();
            return bestIndividual.GetNextMove();
        }

        /// <summary>
        /// Method to run the genetic algorithm once so that it does the following:
        /// 1) Sorts the population based on the fitness of each individual 
        /// 2) Kills off all of the population except for those that were in the top ElitePercent
        /// 3) Select two parents from the population
        /// 4) Create a baby and add him to the new population
        /// 5) Set the old population to the new one
        /// </summary>
        /// <returns>the new population</returns>
        public Population GenerateNextGeneration()
        {
            GenerationCounter++;
            Population newPopulation = new Population();
            Individual individual1;
            Individual individual2;
            Individual child;
            //1) Sorts the population based on the fitness of each individual
            Population.SortPopulationByFitness();
            //2) keep the top elite percent that are performing well
            for (int x = 0; x < (int)(Population.Count * ElitePercent); x++)
            {
                newPopulation.Add(Population[x]);
            }
            for (int x = (int)(Population.Count * ElitePercent); x < Population.Count; x++)
            {
                //3) Select two parents from the population
                individual1 = Population.SelectRandomFromPopulation();
                individual2 = Population.SelectRandomFromPopulation();

                //4)Create a baby and add him to the new population
                child = individual1.CreateBaby(individual2, CrossOver);
                child.Mutate(MutationRate);
                child.Fitness = double.MinValue;
                newPopulation.Add(child);
                x++;
                if (x < Population.Count)
                {
                    child = individual2.CreateBaby(individual1, CrossOver);
                    child.Mutate(MutationRate);
                    child.Fitness = double.MinValue;
                    newPopulation.Add(child);
                }
            }
            //5) Set the old population to the new one
            Population = newPopulation;
            return newPopulation;
        }
    }
}
