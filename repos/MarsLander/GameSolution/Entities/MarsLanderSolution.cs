using Algorithms.Genetic;
using System;
using System.Collections.Generic;

namespace GameSolution.Entities
{
    public class MarsLanderSolution : Individual
    {
        public StaticMove[] Moves;
        private Random Rand;
        private static int TotalMoves = 100;
        public double Fitness { get; set; }
        public GameState State;
        public int Turn = 0;

        public MarsLanderSolution(GameState state)
        {
            Fitness = double.MinValue;
            Rand = new Random();
            State = (GameState)state.Clone();
            Moves = new StaticMove[TotalMoves];
            for(int i = 0; i < TotalMoves; i++)
            {
                Moves[i] = CreateRandomMove();
            }
        }

        public MarsLanderSolution(MarsLanderSolution parentA, MarsLanderSolution parentB, double crossOver)
        {
            Rand = new Random();
            var randomNum = Rand.NextDouble();
            State = (GameState)parentA.State.Clone();
            Moves = new StaticMove[TotalMoves];
            for (int i = 0; i < TotalMoves; i++)
            {
                var parent1 = parentA.Moves[i];
                var parent2 = parentB.Moves[i];
                Moves[i] = new StaticMove((int)(randomNum * parent1.Rotation + (1-randomNum) * parent2.Rotation), (int)(randomNum * parent1.Power + (1-randomNum) * parent2.Power));
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
            while (winner == null && counter < TotalMoves);
            if (winner == null)
                winner = 0;
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
            Turn++;
        }


        public Individual CreateBaby(Individual parent2, double crossOver)
        {
            return new MarsLanderSolution(this, (MarsLanderSolution)parent2, crossOver);
        }

        public void Mutate(double mutationRate)
        {
            for(int i = 0; i<TotalMoves * mutationRate; i++)
            {
                var index = Rand.Next(0, TotalMoves);
                Moves[index] = CreateRandomMove();
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
            var ship = State.Ship;
            return StaticMove.ConvertToMove(ship, move);
        }
    }
}
