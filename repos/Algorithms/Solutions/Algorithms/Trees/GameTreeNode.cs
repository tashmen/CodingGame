﻿using System.Collections.Generic;

namespace Algorithms.Trees
{
    public class GameTreeNode
    {
        public IGameState state;
        public IList<IMove> moves;
        public List<GameTreeNode> children;
        public int wins = 0;
        public int loses = 0;
        public int draws = 0;
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
            if (isMax)
            {
                return (wins + draws * 0.5) / (wins + draws + loses);
            }
            else
            {
                return (loses + draws * 0.5) / (wins + draws + loses);
            }
        }

        public int TotalPlays()
        {
            return wins + loses + draws;
        }

        public int? GetWinner()
        {
            return state.GetWinner();
        }

        public void ApplyWinner(int? winner)
        {
            switch (winner)
            {
                case 1:
                    wins++;
                    break;
                case 0:
                    draws++;
                    break;
                case -1:
                    loses++;
                    break;
                default:
                    break;
            }
        }

        public double Evaluate()
        {
            return state.Evaluate(isMax);
        }
    }
}