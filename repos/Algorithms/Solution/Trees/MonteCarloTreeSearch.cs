using Algorithms.GameComponent;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

namespace Algorithms.Trees
{
    public class MonteCarloTreeSearch : TreeAlgorithm
    {
        private Random rand;
        private bool printErrors;
        private SearchStrategy strategy;

        public enum SearchStrategy
        {
            Random = 0,
            Sequential = 1
        }
        public MonteCarloTreeSearch(bool showErrors = true, SearchStrategy searchStrategy = SearchStrategy.Random)
        {
            rand = new Random();
            printErrors = showErrors;
            strategy = searchStrategy;
        }

        public IGameState GetRootState()
        {
            return RootNode.state;
        }

        /// <summary>
        /// Get the next move
        /// </summary>
        /// <param name="watch">timer</param>
        /// <param name="timeLimit">The amount of time to give to the search in milliseconds</param>
        /// <param name="depth">How deep to run the simulations; does not impact how deep it goes in the game tree.</param>
        /// <param name="numRollouts">The number of roll outs to play per expansion</param>
        /// <returns></returns>
        public object GetNextMove(Stopwatch watch, int timeLimit, int depth = -1, int numRollouts = 1, double? exploration = null)
        {
            if (exploration == null)
            {
                exploration = Math.Sqrt(2);
            }
            int count = 0;
            do
            {
                GameTreeNode selectedNode = SelectNodeWithUnplayedMoves(RootNode, exploration.Value);
                if (selectedNode == null)
                {
                    if (printErrors)
                        Console.Error.WriteLine("Found no more moves!");
                    break;
                }
                object move = SelectMove(selectedNode);
                GameTreeNode childNode = Expand(selectedNode, move);
                double? winner = childNode.GetWinner();
                if (winner.HasValue)
                {
                    BackPropagate(childNode, winner);
                    count++;
                }
                else
                {
                    for (int i = 0; i < numRollouts; i++)
                    {
                        var clonedState = childNode.state.Clone();
                        winner = SimulateGame(clonedState, watch, timeLimit, depth, childNode.isMax);
                        if (!winner.HasValue)
                            break;//We simulated a game, but it didn't end so we are out of time...
                        BackPropagate(childNode, winner);
                        count++;
                    }
                }
            }
            while (watch.ElapsedMilliseconds < timeLimit);
            if (printErrors)
                Console.Error.WriteLine($"Played {count} games!");


            GameTreeNode bestChild = null;
            double bestScore = double.MinValue;
            for (int i = 0; i < RootNode.children.Count; i++)
            {
                var child = RootNode.children[i];
                double score = child.GetScore(RootNode.isMax);
                if (bestScore < score)
                {
                    bestChild = child;
                    bestScore = score;
                }
                if (printErrors)
                    Console.Error.WriteLine($"w: {(RootNode.isMax ? child.wins : child.loses)} l: {(RootNode.isMax ? child.loses : child.wins)} total: {child.totalPlays} score: {score} isMax: {RootNode.isMax} move: {child.state.GetMove(RootNode.isMax)}");
            }

            if (printErrors)
                Console.Error.WriteLine($"Best: w: {(RootNode.isMax ? bestChild.wins : bestChild.loses)} l: {(RootNode.isMax ? bestChild.loses : bestChild.wins)} total: {bestChild.totalPlays} score: {bestScore} move: {bestChild.state.GetMove(RootNode.isMax)}");

            return bestChild.state.GetMove(RootNode.isMax);
        }

        private void BackPropagate(GameTreeNode selectedNode, double? winner)
        {
            selectedNode.ApplyWinner(winner);
            GameTreeNode tempNode = selectedNode.parent;
            while (tempNode != null)
            {
                tempNode.ApplyWinner(winner);
                tempNode = tempNode.parent;
            }
        }

        private double? SimulateGame(IGameState state, Stopwatch watch, int timeLimit, int depth, bool isMax)
        {
            double? winner;
            do
            {
                if (watch.ElapsedMilliseconds >= timeLimit)
                {
                    return null;
                }

                object move = SelectMoveAtRandom(state, isMax);
                if (watch.ElapsedMilliseconds >= timeLimit)
                {
                    return null;
                }

                state.ApplyMove(move, isMax);

                if (watch.ElapsedMilliseconds >= timeLimit)
                {
                    return null;
                }

                depth--;
                isMax = !isMax;

                winner = state.GetWinner();
            }
            while (!winner.HasValue && depth != 0);


            if (winner.HasValue)
            {
                return winner;
            }
            if (depth == 0)
            {
                double eval = state.Evaluate(isMax);
                if (eval > 1)
                {
                    return 1;
                }
                else if (eval < -1)
                    return -1;
                else return eval;
            }

            throw new InvalidOperationException("Could not find a winner for simulation!");
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
                    for (int i = 0; i < tempNode.children.Count; i++)
                    {
                        var child = tempNode.children[i];
                        queue.Enqueue(child);
                    }
                }
                else if (tempNode.parent != null)
                {
                    double wins = RootNode.isMax ? tempNode.wins : tempNode.loses;
                    double nodeTotal = tempNode.TotalPlays();
                    double parentTotal = tempNode.parent.TotalPlays();

                    double value = wins / nodeTotal + exploration * Math.Sqrt(Math.Log(parentTotal) / nodeTotal);
                    if (value > maxValue)
                    {
                        maxValue = value;
                        bestNode = tempNode;
                    }
                }
                else return tempNode;
            }

            return bestNode;
        }

        private object SelectMoveAtRandom(IGameState state, bool isMax)
        {
            IList moves = state.GetPossibleMoves(isMax);
            if (moves.Count == 0)
            {
                throw new Exception("No moves available!");
            }
            int index = rand.Next(0, moves.Count);
            return moves[index];
        }

        private object SelectMove(GameTreeNode node)
        {
            switch (strategy)
            {
                case SearchStrategy.Random:
                    return SelectMoveAtRandom(node);
                case SearchStrategy.Sequential:
                    return SelectMoveSequentially(node);
            }
            throw new InvalidOperationException("strategy not supported");
        }

        private object SelectMoveSequentially(GameTreeNode node)
        {
            object move;
            if (node.moves.Count == 0)//If there are no more moves then that is a problem...
            {
                throw new Exception("No moves found!");
            }
            else
            {
                move = node.moves[0];
                node.moves.RemoveAt(0);
            }
            return move;
        }

        private object SelectMoveAtRandom(GameTreeNode node)
        {
            object move;
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
