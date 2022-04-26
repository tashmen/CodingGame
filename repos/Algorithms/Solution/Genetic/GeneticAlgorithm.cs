using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Algorithms.Genetic
{
    public class GeneticAlgorithm
    {
        /**The population for the genetic algorithm to choose from*/
        private Population population;
        /**The rate at which mutations occur*/
        private double mutationRate;
        /**The percent of the original population that will be in the new population*/
        private double elitePercent;
        /**The percent of the chromosome to use from the first parent*/
        private double crossOver;

        public int generationCounter {get; set;}
        Random rand;

        /**Sets the initial population and the mutation rate*/
        public GeneticAlgorithm(Population initialPopulation, double mRate, double eP, double cO)
        {
            population = initialPopulation;
            mutationRate = mRate;
            elitePercent = eP;
            crossOver = cO;
            rand = new Random();
            generationCounter = 0;
        }

        /** Method to run the genetic algorithm once so that it does the following:
         * 1) Sorts the population based on the fitness of each individual 
         * 2) Kills off all of the population except for those that were in the top 5%
         * 3) Select two parents from the population
         * 4) Create a baby and add him to the new population
         * 5) Set the old population to the new one
         * @returns the new population
         */
        public Population runOnce()
        {
            generationCounter++;
            Population newPopulation = new Population(population.size);
            Individual individual1;
            Individual individual2;
            Individual child;
            //1) Sorts the population based on the fitness of each individual
            population.sortPopulation();
            //2) keep the top elite percent that are performing well
            for (int x = 0; x < (int)(population.size * elitePercent); x++)
            {
                population.getIndividual(x).SetFitness(0);
                newPopulation.addIndividual(population.getIndividual(x));
            }
            for (int x = (int)(population.size * elitePercent); x < population.size; x++)
            {
                //3) Select two parents from the population
                individual1 = population.selectRandomFromPopulation();
                individual2 = population.selectRandomFromPopulation();

                //4)Create a baby and add him to the new population
                child = individual1.CreateBaby(individual2, crossOver);
                child.Mutate(mutationRate);
                newPopulation.addIndividual(child);
                x++;
                if (x < population.size)
                {
                    child = individual2.CreateBaby(individual1, crossOver);
                    child.Mutate(mutationRate);
                    newPopulation.addIndividual(child);
                }
            }
            //5) Set the old population to the new one
            population = newPopulation;
            return newPopulation;
        }
    }
}
