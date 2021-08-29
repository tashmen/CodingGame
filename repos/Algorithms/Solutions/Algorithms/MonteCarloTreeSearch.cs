using Algorithms.Trees;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Algorithms
{
    public class MonteCarloTreeSearch : TreeAlgorithm
    {
        private Random rand;
        private bool printErrors;
        public MonteCarloTreeSearch(bool showErrors = true) 
        {
            rand = new Random();
            printErrors = showErrors;
        }

        /// <summary>
        /// Get the next move
        /// </summary>
        /// <param name="watch">timer</param>
        /// <param name="timeLimit">The amount of time to give to the search in milliseconds</param>
        /// <param name="numRollouts">The number of roll outs to play per expansion</param>
        /// <returns></returns>
        public IMove GetNextMove(Stopwatch watch, int timeLimit, int depth = -1, int numRollouts = 1, double? exploration = null)
        {
            if(exploration == null)
            {
                exploration = Math.Sqrt(2);
            }
            int count = 0;
            do
            {
                GameTreeNode selectedNode = SelectNodeWithUnplayedMoves(RootNode, exploration.Value);
                if(selectedNode == null)
                {
                    if(printErrors)
                        Console.Error.WriteLine("Found no more moves!");
                    break;
                }
                IMove move = SelectMoveAtRandom(selectedNode);
                GameTreeNode childNode = Expand(selectedNode, move);
                int? winner = childNode.GetWinner();
                if (winner.HasValue)
                {
                    BackPropagate(childNode, winner);
                }
                else
                {
                    for (int i = 0; i<numRollouts; i++)
                    {
                        var clonedState = childNode.state.Clone();
                        winner = SimulateGame(clonedState, watch, timeLimit, depth, childNode.isMax);
                        BackPropagate(childNode, winner);
                        count++;
                    }
                }
            }
            while (watch.ElapsedMilliseconds < timeLimit);
            if(printErrors)
                Console.Error.WriteLine($"Played {count} games!");
            

            GameTreeNode bestChild = null;
            double bestScore = -1;
            foreach(GameTreeNode child in RootNode.children)
            {
                double score = child.GetScore(RootNode.isMax);
                if(bestScore < score)
                {
                    bestChild = child;
                    bestScore = score;
                }
                if(printErrors)
                    Console.Error.WriteLine($"w: {child.wins} l: {child.loses} d: {child.draws} move: {child.state.GetMove(RootNode.isMax)} score: {score} isMax: {RootNode.isMax}");
            }

            if(printErrors)
                Console.Error.WriteLine($"Best: w: {bestChild.wins} l: {bestChild.loses} d: {bestChild.draws}");

            return bestChild.state.GetMove(RootNode.isMax);
        }

        private void BackPropagate(GameTreeNode selectedNode, int? winner)
        {
            selectedNode.ApplyWinner(winner);
            GameTreeNode tempNode = selectedNode.parent;
            while(tempNode != null)
            {
                tempNode.ApplyWinner(winner);
                tempNode = tempNode.parent;
            }
        }

        private int? SimulateGame(IGameState state, Stopwatch watch, int timeLimit, int depth, bool isMax)
        {
            int? winner = state.GetWinner();
            if (winner.HasValue)
            {
                return winner;
            }
            if(depth == 0)
            {
                double eval = state.Evaluate(isMax);
                if (eval > 0)
                {
                    return 1;
                }
                else if (eval == 0)
                {
                    return 0;
                }
                else return -1;
            }

            IMove move = SelectMoveAtRandom(state, isMax);
            state.ApplyMove(move, isMax);

            if (watch.ElapsedMilliseconds >= timeLimit)
            {
                return null;
            }

            winner = SimulateGame(state, watch, timeLimit, depth - 1, !isMax);

            return winner;
        }

        private GameTreeNode SelectNodeWithUnplayedMoves(GameTreeNode node, double exploration)
        {
            Queue<GameTreeNode> queue = new Queue<GameTreeNode>();
            queue.Enqueue(node);

            GameTreeNode tempNode;
            GameTreeNode bestNode = null;
            double maxValue = -1;
            while (queue.Count > 0)
            {
                tempNode = queue.Dequeue();
                if (tempNode.moves.Count == 0)
                {
                    foreach (GameTreeNode child in tempNode.children)
                    {
                        queue.Enqueue(child);
                    }
                }
                else if(tempNode.parent != null)
                {
                    double wins = tempNode.isMax ? tempNode.wins : tempNode.loses;
                    double nodeTotal = tempNode.TotalPlays();
                    double parentTotal = tempNode.parent.TotalPlays();

                    double value = wins / nodeTotal + exploration * Math.Sqrt(Math.Log(parentTotal / nodeTotal));
                    if(value > maxValue)
                    {
                        maxValue = value;
                        bestNode = tempNode;
                    }
                }
                else return tempNode;
            }

            return bestNode;
        }

        private IMove SelectMoveAtRandom(IGameState state, bool isMax)
        {
            IList<IMove> moves = state.GetPossibleMoves(isMax);
            int index = rand.Next(0, moves.Count);
            return moves[index];
        }

        private IMove SelectMoveAtRandom(GameTreeNode node)
        {
            IMove move;
            if (node.moves.Count == 0)//If there are no more moves then that is a problem...
            {
                throw new Exception("No moves found!");
            }
            else
            {
                int index = rand.Next(0, node.moves.Count);
                move = node.moves[index];
                node.moves.RemoveAt(index);
            }
            
            return move;
        }
    }
}
