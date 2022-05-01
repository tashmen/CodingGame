using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Algorithms.Genetic
{
    /* Interface for the individuals within the population for a genetic algorithm */
    public interface Individual
    {
        /// <summary>
        /// Fitness should be a postive value (> 0)
        /// </summary>
        public double Fitness { get; set; }
        /** Creates an individual from two parents*/
        Individual CreateBaby(Individual parent2, double crossOver);

        /** Mutates the individual */
        void Mutate(double mutationRate);

        double CalculateFitness();

        object GetNextMove();
    }
}
