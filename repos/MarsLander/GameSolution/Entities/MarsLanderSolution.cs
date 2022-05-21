using Algorithms.Genetic;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GameSolution.Entities
{
    public class MarsLanderSolution : Individual
    {
        public StaticMove[] Moves;
        private Random Rand;
        private static int TotalMoves = 160;
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

        public MarsLanderSolution(MarsLanderSolution solution)
        {
            Fitness = solution.Fitness;
            Rand = new Random();
            State = (GameState)solution.State.Clone();
            Moves = solution.Moves.Select(m => (StaticMove)m.Clone()).ToArray();
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

        public StaticMove CreateRandomMove(StaticMove move = null)
        {

            if(move == null)
                return new StaticMove(Rand.Next(-15, 16), Rand.Next(-1, 2));
            else
            {
                move.Rotation = Rand.Next(-15, 16);
                move.Power = Rand.Next(-1, 2);
            }
            return move;
        }

        public void AdvanceTurn(GameState updatedState)
        {
            State = (GameState)updatedState.Clone();
            Turn++;
        }


        public Individual CreateBaby(Individual parent1, Individual parent2, double crossOver)
        {
            var parentA = (MarsLanderSolution)parent1;
            var parentB = (MarsLanderSolution)parent2;
            var randomNum = Rand.NextDouble();
            State.Fill(((MarsLanderSolution)parent1).State);
            for (int i = 0; i < TotalMoves; i++)
            {
                var p1 = parentA.Moves[i];
                var p2 = parentB.Moves[i];
                Moves[i].Rotation = (int)Math.Round(randomNum * p1.Rotation + (1 - randomNum) * p2.Rotation);
                Moves[i].Power = (int)Math.Round(randomNum * p1.Power + (1 - randomNum) * p2.Power);
            }
            Turn = parentA.Turn;

            return this;
        }

        public void Mutate(double mutationRate)
        {
            for(int i = 0; i<TotalMoves * mutationRate; i++)
            {
                var index = Rand.Next(Turn, TotalMoves);
                Moves[index] = CreateRandomMove(Moves[index]);
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

        public Individual Clone()
        {
            return new MarsLanderSolution(this);
        }
    }
}
