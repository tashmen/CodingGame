using Algorithms.Genetic;
using System;

namespace GameSolution.Entities
{
    public class MarsLanderSolution : Individual
    {
        public Move[] Moves { get; set; }
        private Random Rand { get; set; }
        private static int TotalMoves { get; set; } = 200;
        public double Fitness { get; set; }

        public MarsLanderSolution()
        {
            Rand = new Random();
            Moves = new Move[TotalMoves];
            for(int i = 0; i < TotalMoves; i++)
            {
                Moves[i] = new Move(Rand.Next(-90, 90), Rand.Next(0, 4));
            }
        }

        public MarsLanderSolution(MarsLanderSolution parentA, MarsLanderSolution parentB, double crossOver)
        {
            Rand = new Random();
            Moves = new Move[TotalMoves];
            for (int i = 0; i < TotalMoves; i++)
            {
                if (i < TotalMoves * crossOver)
                {
                    Moves[i] = parentA.Moves[i];
                }
                else
                {
                    Moves[i] = parentB.Moves[i];
                }
            }
        }


        public Individual CreateBaby(Individual parent2, double crossOver)
        {
            return new MarsLanderSolution(this, (MarsLanderSolution)parent2, crossOver);
        }

        public void Mutate(double mutationRate)
        {
            for (int i = 0; i < TotalMoves; i++)
            {
                if(Rand.NextDouble() < mutationRate)
                {
                    Moves[i] = new Move(Rand.Next(-90, 90), Rand.Next(0, 4));
                }
            }
        }

        public double GetFitness()
        {
            return Fitness;
        }

        public void SetFitness(double fit)
        {
            Fitness = fit;
        }

        public bool Equals(Individual i)
        {
            throw new NotImplementedException();
        }
    }
}
