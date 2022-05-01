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
        /** Creates an individual from two parents*/
        Individual CreateBaby(Individual parent2, double crossOver);

        /** Mutates the individual */
        void Mutate(double mutationRate);

        /** Returns the fitness of the individual */
        double GetFitness();

        /** Sets the fitness of the individual */
        void SetFitness(double fit);
    }
}
