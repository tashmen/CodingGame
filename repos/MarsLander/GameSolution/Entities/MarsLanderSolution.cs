using Algorithms.Genetic;
using System;
using System.Collections.Generic;

namespace GameSolution.Entities
{
    public class MarsLanderSolution : Individual
    {
        public IList<StaticMove> Moves { get; set; }
        private Random Rand { get; set; }
        private static int TotalMoves { get; set; } = 100;
        public double Fitness { get; set; }
        public GameState State { get; set; }

        public MarsLanderSolution(GameState state)
        {
            Fitness = double.MinValue;
            Rand = new Random();
            State = (GameState)state.Clone();
            Moves = new List<StaticMove>(TotalMoves);
            for(int i = 0; i < TotalMoves; i++)
            {
                Moves.Add(CreateRandomMove());
            }
        }

        public MarsLanderSolution(MarsLanderSolution parentA, MarsLanderSolution parentB, double crossOver)
        {
            Rand = new Random();
            var randomNum = Rand.NextDouble();
            State = (GameState)parentA.State.Clone();
            Moves = new List<StaticMove>(TotalMoves);
            for (int i = 0; i < TotalMoves; i++)
            {
                
                var parent1 = parentA.Moves[i];
                var parent2 = parentB.Moves[i];
                Moves.Add(new StaticMove((int)(randomNum * parent1.Rotation + (1-randomNum) * parent2.Rotation), (int)(randomNum * parent1.Power + (1-randomNum) * parent2.Power)));
            }
        }

        public double CalculateFitness()
        {
            var clonedState = State.Clone();
            double? winner;
            int counter = 0;
            do
            {
                clonedState.ApplyMove(Moves[counter++], true);
                winner = clonedState.GetWinner();
            }
            while (winner == null);
            SetFitness(winner.Value);
            return Fitness;
        }

        public StaticMove CreateRandomMove()
        {
            return new StaticMove(Rand.Next(-15, 16), Rand.Next(-1, 2));
        }

        public void AdvanceTurn(GameState updatedState)
        {
            State = (GameState)updatedState.Clone();
            Moves.RemoveAt(0);
            Moves.Add(CreateRandomMove());
        }


        public Individual CreateBaby(Individual parent2, double crossOver)
        {
            return new MarsLanderSolution(this, (MarsLanderSolution)parent2, crossOver);
        }

        public void Mutate(double mutationRate)
        {
            
            for (int i = 0; i < TotalMoves; i++)
            {
                if (Rand.NextDouble() < mutationRate)
                {
                    Moves[i] = CreateRandomMove();
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

        public object GetNextMove()
        {
            return Moves[0];
        }
    }
}
