using Algorithms.Genetic;
using System;
using System.Collections.Generic;

namespace GameSolution.Entities
{
    public class MarsLanderSolution : Individual
    {
        public IList<Move> Moves { get; set; }
        private Random Rand { get; set; }
        private static int TotalMoves { get; set; } = 200;
        public double Fitness { get; set; }
        public GameState State { get; set; }

        public MarsLanderSolution(GameState state)
        {
            Fitness = double.MinValue;
            Rand = new Random();
            State = (GameState)state.Clone();
            Moves = new List<Move>(TotalMoves);
            for(int i = 0; i < TotalMoves; i++)
            {
                if (i == 0)
                {
                    Moves.Add(CreateRandomMove(new Move(State.Ship.RotationAngle, State.Ship.Power)));
                }
                else Moves.Add(CreateRandomMove(Moves[i - 1]));
            }
        }

        public MarsLanderSolution(MarsLanderSolution parentA, MarsLanderSolution parentB, double crossOver)
        {
            Rand = new Random();
            State = (GameState)parentA.State.Clone();
            Moves = new List<Move>(TotalMoves);
            for (int i = 0; i < TotalMoves; i++)
            {
                if (i < TotalMoves * crossOver)
                {
                    Moves.Add(parentA.Moves[i]);
                }
                else
                {
                    Moves.Add(parentB.Moves[i]);
                }
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

        public Move CreateRandomMove(Move previousMove)
        {
            return new Move(Rand.Next(Math.Max(previousMove.Rotation - 15, -90), Math.Min(previousMove.Rotation + 15, 90) + 1), Rand.Next(Math.Max(previousMove.Power - 1, 0), Math.Min(previousMove.Power + 1, 4) + 1));
        }

        public void AdvanceTurn(GameState updatedState)
        {
            State = (GameState)updatedState.Clone();
            Moves.RemoveAt(0);
            Moves.Add(CreateRandomMove(Moves[Moves.Count - 1]));
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
                    if(i == 0)
                    {
                        Moves[i] = CreateRandomMove(new Move(State.Ship.RotationAngle, State.Ship.Power));
                    }
                    else Moves[i] = CreateRandomMove(Moves[i-1]);
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
