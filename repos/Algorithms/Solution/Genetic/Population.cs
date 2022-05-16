using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Algorithms.Genetic
{
    public class Population : IEnumerable<Individual>, IList<Individual>
    {
        private List<Individual> Individuals;
        private Random Rand;

        public int Count => Individuals.Count;

        public bool IsReadOnly => throw new NotImplementedException();

        public Individual this[int index] { get { return Individuals[index]; }  set { Individuals[index] = value; } }

        /**
         * Creates an empty population
          * */
        public Population()
        {
            Individuals = new List<Individual>();
            Rand = new Random();
        }

        /// <summary>
        /// Sort the population based on fitness
        /// </summary>
        public void SortPopulationByFitness()
        {
            Individuals.Sort(delegate (Individual i1, Individual i2)
            {
                if (i1 == null && i2 == null) return 0;
                else if (i1 == null) return 1;
                else if (i2 == null) return -1;
                else if (i2.Fitness == i1.Fitness) return 0;
                else return i1.Fitness > i2.Fitness ? -1 : 1;
            });
        }

        public double GetTotalFitness()
        {
            return Individuals.Sum(i => i.Fitness);
        }



        /// <summary>
        /// Selects a random Individual from the population in a roulette wheel fashion with individuals who have a higher fitness having a higher chance of being selected.
        /// </summary>
        /// <param name="totalFit">The total fitness of the population</param>
        /// <returns>The selected Individual</returns>
        public Individual SelectRandomFromPopulation(double totalFit)
        {
            double randNum = Rand.NextDouble() * totalFit;
            int y = 0;
            double totalFitSoFar = Individuals[y].Fitness;
            while (totalFitSoFar < randNum)
            {
                y++;
                totalFitSoFar += Individuals[y].Fitness;
            }
            return Individuals[y];
        }

        public Individual GetBestIndividual()
        {
            SortPopulationByFitness();
            return Individuals.First();
        }

        public double MaximumFitness()
        {
            SortPopulationByFitness();
            return Individuals[0].Fitness;
        }

        public double MinimumFitness()
        {
            SortPopulationByFitness();
            return Individuals.Last().Fitness;
        }

        public double AverageFitness()
        {
            return Individuals.Average(i => i.Fitness);
        }

        public IEnumerator<Individual> GetEnumerator()
        {
            return Individuals.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return Individuals.GetEnumerator();
        }

        public int IndexOf(Individual item)
        {
            return Individuals.IndexOf(item);
        }

        public void Insert(int index, Individual item)
        {
            Individuals.Insert(index, item);
        }

        public void RemoveAt(int index)
        {
            Individuals.RemoveAt(index);
        }

        public void Add(Individual item)
        {
            Individuals.Add(item);
        }

        public void Clear()
        {
            Individuals.Clear();
        }

        public bool Contains(Individual item)
        {
            return Individuals.Contains(item);
        }

        public void CopyTo(Individual[] array, int arrayIndex)
        {
            Individuals.CopyTo(array, arrayIndex);
        }

        public bool Remove(Individual item)
        {
            return Individuals.Remove(item);
        }
    }
}
