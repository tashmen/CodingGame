using Algorithms.Genetic;
using System;
using System.Linq;

namespace GameSolution.Entities
{
    public class Solution : Individual
    {
        public Move[] Moves;
        private Random Rand;
        private static int TotalMoves = 200;
        public double Fitness { get; set; }
        public GameState State;
        public int Turn = 0;

        public Solution(GameState state)
        {
            Fitness = double.MinValue;
            Rand = new Random();
            State = (GameState)state.Clone();
            Moves = new Move[TotalMoves];
            for(int i = 0; i < TotalMoves; i++)
            {
                //Moves[i] = CreateRandomMove();
            }
        }

        public Solution(Solution solution)
        {
            Fitness = solution.Fitness;
            Rand = new Random();
            State = (GameState)solution.State.Clone();
            Moves = solution.Moves.Select(m => m.Clone()).ToArray();
            Turn = solution.Turn;
        }

        public double CalculateFitness()
        {
            var clonedState = State.Clone();
            double? winner;
            int counter = Turn;
            do
            {
                clonedState.ApplyMove(Moves[counter++], true);
                winner = clonedState.GetWinner();
            }
            while (winner == null && counter < TotalMoves);
            if (winner == null)
                winner = 0;
            SetFitness(winner.Value);
            return Fitness;
        }

        public void AdvanceTurn(GameState updatedState)
        {
            State = (GameState)updatedState.Clone();
            Turn++;
        }


        public Individual CreateBaby(Individual parent1, Individual parent2, double crossOver)
        {
            var parentA = (Solution)parent1;
            var parentB = (Solution)parent2;
            var randomNum = Rand.NextDouble();
            

            return this;
        }

        public void Mutate(double mutationRate)
        {
            for(int i = 0; i<TotalMoves * mutationRate; i++)
            {
                var index = Rand.Next(Turn, TotalMoves);
                //Moves[index] = CreateRandomMove(Moves[index]);
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
            var move = Moves[Turn];
            return move;
        }

        public Individual Clone()
        {
            return new Solution(this);
        }
    }
}
