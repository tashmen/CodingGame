using Algorithms.GameComponent;
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
            moves = new List<object>(50);
            var possibleMoves = state.GetPossibleMoves(isMax);
            for(int i = 0; i < possibleMoves.Count; i++)
            {
                var obj = possibleMoves[i];
                moves.Add(obj);
            }
            children = new List<GameTreeNode>(50);
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
                return (wins - loses) / totalPlays;
            }
            else
            {
                return (loses - wins) / totalPlays;
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
