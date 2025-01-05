using Algorithms.GameComponent;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Algorithms.Trees
{
    public class GameTreeNode //: PooledObject<GameTreeNode>
    {
        public IGameState state;
        public IList moves;
        public List<GameTreeNode> children = new List<GameTreeNode>(50);
        public double wins = 0;
        public double loses = 0;
        public int totalPlays = 0;
        public GameTreeNode parent;
        public bool isMax;

        /*
        static GameTreeNode()
        {
            SetInitialCapacity(100000);
        }
        

        public GameTreeNode()
        {

        }*/

        public static GameTreeNode GetGameTreeNode(IGameState state, bool isMax, GameTreeNode parent = null)
        {
            GameTreeNode node = new GameTreeNode();//Get();
            node.state = state;
            node.moves = node.state.GetPossibleMoves(isMax);
            //Possible inconsistent state modification if GetPossibleMoves is cached
            /*
            moves = new List<object>();
            IList possibleMoves = state.GetPossibleMoves(isMax);
            for (int i = 0; i < possibleMoves.Count; i++)
            {
                moves.Add(possibleMoves[i]);
            }
            */
            node.isMax = isMax;
            node.parent = parent;
            return node;
        }

        /*
        protected override void Reset()
        {
            state.Dispose();
            moves.Clear();
            wins = 0;
            loses = 0;
            totalPlays = 0;
            foreach (GameTreeNode childNode in children)
            {
                childNode.Dispose();
            }
            children.Clear();
        }*/

        public GameTreeNode GetChild(int index)
        {
            return children[index];
        }

        public double GetScore(bool isMax)
        {
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

        public double? GetWinner()
        {
            return state.GetWinner();
        }

        public void ApplyWinner(double? winner)
        {
            if (winner.HasValue)
            {
                if (winner > 0)
                {
                    wins += winner.Value;
                }
                else if (winner < 0)
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
