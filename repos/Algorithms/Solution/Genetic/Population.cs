using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Algorithms.Genetic
{
    public class Population
    {
        //The individuals within this population
        private Individual[] individuals;
        //The size of the population
        public int size { get; private set; }
        Random rand;

        /**Creates an empty population
          * @param size- The size of the population
          * */
        public Population(int size)
        {
            individuals = new Individual[size];
            size = 0;
            rand = new Random();
        }

        /**Sorts the population based on the individuals Fitness
          * Sorts using a simple insertion sort.  Could probably exchange this
          * sort for another one at a later point in time.
          * */
        public void sortPopulation()
        {
            Individual temp;
            for (int x = 1; x < size; x++)
            {
                for (int y = 0; y < x; y++)
                {
                    if (individuals[x].GetFitness() > individuals[y].GetFitness())
                    {
                        temp = individuals[x];
                        for (int z = x; z > y; z--)
                        {
                            individuals[z] = individuals[z - 1];
                        }
                        individuals[y] = temp;
                        y = x;
                    }
                }
            }
        }

        /**Selects a random Individual from the population in a roulette wheel fashion with individuals
          * who have a higher fitness having a higher chance of being selected.
          * @return The selected Individual
          * */
        public Individual selectRandomFromPopulation()
        {
            double totalFit = 0;
            for (int x = 0; x < this.size; x++)
            {
                totalFit = totalFit + Math.Abs(individuals[x].GetFitness());
            }
            double randNum = (rand.NextDouble() * totalFit);
            int y = 0;
            double totalFitSoFar = Math.Abs(individuals[y].GetFitness());
            while (totalFitSoFar < randNum)
            {
                y++;
                totalFitSoFar = totalFitSoFar + Math.Abs(individuals[y].GetFitness());
            }
            return individuals[y];
        }

        /**Calculates the maximum fitness of the population
          * @return the highest fitness value of the population
          * */
        public double maxFitness()
        {
            double maxFit = individuals[0].GetFitness();
            for (int x = 1; x < size; x++)
            {
                if (maxFit < individuals[x].GetFitness())
                    maxFit = individuals[x].GetFitness();
            }
            return maxFit;
        }

        /**Calculates the minimum fitness value of the population
          * @return The lowest fitness value of the population
          * */
        public double minFitness()
        {
            double minFit = individuals[0].GetFitness();
            for (int x = 1; x < size; x++)
            {
                if (minFit > individuals[x].GetFitness())
                    minFit = individuals[x].GetFitness();
            }
            return minFit;
        }

        /**Calculates the average fitness of the population
          * @return The average fitness of the population
          * */
        public double avgFitness()
        {
            double avgFit = 0;
            for (int x = 0; x < size; x++)
            {
                avgFit = avgFit + individuals[x].GetFitness();
            }
            return avgFit / size;
        }

        /**Gets an individual at index location
          * @param location- The location of the desired Individual
          * @return The desired individual from index location
          * */
        public Individual getIndividual(int location)
        {
            return individuals[location];
        }

        /**Adds an individual to the population
          * @param i- The individual to add
          * */
        public void addIndividual(Individual i)
        {
            individuals[size] = i;
            size++;
        }
    }
}
