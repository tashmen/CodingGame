using System;
using System.Collections;
using System.Collections.Generic;

namespace Algorithms.Trees
{
    public class GameTreeNode
    {
        public IGameState state;
        public IList moves;
        public List<GameTreeNode> children;
        public double wins = 0;
        public double loses = 0;
        public int totalPlays = 0;
        public GameTreeNode parent;
        public bool isMax;

        public GameTreeNode(IGameState state, bool isMax, GameTreeNode parent = null)
        {
            this.state = state;
            moves = state.GetPossibleMoves(isMax);
            children = new List<GameTreeNode>();
            this.parent = parent;
            this.isMax = isMax;
        }

        public double GetScore(bool isMax)
        {
            double totalPlays = TotalPlays();
            if (totalPlays == 0)
                return 0;

            if (isMax)
            {
                return wins / totalPlays;
            }
            else
            {
                return loses / totalPlays;
            }
        }

        public int TotalPlays()
        {
            return totalPlays;
        }

        public double? GetWinner()
        {
            return state.GetWinner();
        }

        public void ApplyWinner(double? winner)
        {
            if (winner.HasValue)
            {
                if(winner > 0)
                {
                    wins += winner.Value;
                }
                else if(winner < 0)
                {
                    loses += Math.Abs(winner.Value);
                }
                totalPlays++;
            }
        }

        public double Evaluate()
        {
            return state.Evaluate(isMax);
        }
    }
}
