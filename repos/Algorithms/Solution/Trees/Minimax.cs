﻿using Algorithms.Trees;
using System;
using System.Diagnostics;

namespace Algorithms.Trees
{
    public class Minimax : TreeAlgorithm
    {
        public object GetNextMove(Stopwatch watch, int timeLimit, int depth = int.MaxValue)
        {
            double val = 99999999;
            val *= RootNode.isMax ? -1 : 1;
            object bestMove = null;
            foreach (object move in RootNode.moves)
            {
                
                GameTreeNode child = Expand(RootNode, move);
                double currentVal = RunMinimax(child, depth, -999999, 999999, watch, timeLimit);
                if ((RootNode.isMax && currentVal > val) || (!RootNode.isMax && currentVal < val))
                {
                    bestMove = move;
                    val = currentVal;
                }
                if (watch.ElapsedMilliseconds >= timeLimit)
                {
                    break;
                }
            }

            return bestMove;
        }

        public double RunMinimax(GameTreeNode currentNode, int depth, double alpha, double beta, Stopwatch watch, int timeLimit)
        {
            if (depth == 0 || watch.ElapsedMilliseconds >= timeLimit)
            {
                double eval = currentNode.Evaluate();
                return eval;
            }
            double? winner = currentNode.GetWinner();
            if (winner.HasValue)
            {
                return winner.Value;
            }

            if (currentNode.isMax)
            {
                double value = -99999;
                double minMax;
                foreach (object move in currentNode.moves)
                {
                    GameTreeNode childNode = Expand(currentNode, move);
                    minMax = RunMinimax(childNode, depth - 1, alpha, beta, watch, timeLimit);

                    value = Math.Max(value, minMax);
                    alpha = Math.Max(alpha, value);

                    if (alpha >= beta)
                    {
                        break;
                    }
                }
                return value;
            }
            else
            {
                double value = 99999;
                double minMax;
                foreach (object move in currentNode.moves)
                {
                    GameTreeNode childNode = Expand(currentNode, move);
                    minMax = RunMinimax(childNode, depth - 1, alpha, beta, watch, timeLimit);

                    value = Math.Min(value, minMax);
                    beta = Math.Min(beta, value);
                    if (beta <= alpha)
                    {
                        break;
                    }
                }
                return value;
            }
        }
    }
}
