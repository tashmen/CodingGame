using Algorithms.GameComponent;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

namespace Algorithms.Trees
{
    public class MonteCarloTreeSearch : TreeAlgorithm
    {
        private readonly Random rand;
        private readonly bool printErrors;
        private readonly SearchStrategy strategy;
        private readonly double[] _mathLogCache;
        private static readonly double _DefaultExploration = Math.Sqrt(2);

        public enum SearchStrategy
        {
            Random = 0,
            Sequential = 1
        }
        public MonteCarloTreeSearch(bool showErrors = true, SearchStrategy searchStrategy = SearchStrategy.Random, int mathLogCacheSize = 1000)
        {
            rand = new Random();
            printErrors = showErrors;
            strategy = searchStrategy;
            _mathLogCache = new double[mathLogCacheSize];
            for (int i = 0; i < mathLogCacheSize; i++)
            {
                _mathLogCache[i] = Math.Log(i);
            }
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
                exploration = _DefaultExploration;
            }
            int count = 0;
            do
            {
                GameTreeNode selectedNode = SelectNodeWithUnplayedMoves(RootNode, exploration.Value, watch, timeLimit);
                if (selectedNode == null)
                {
                    if (printErrors)
                        Console.Error.WriteLine("Found no more moves!");
                    break;
                }
                object move = SelectMove(selectedNode);
                GameTreeNode childNode = Expand(selectedNode, move);
                if (watch.ElapsedMilliseconds >= timeLimit)
                    break;
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
                        using (IGameState clonedState = childNode.state.Clone())
                            winner = SimulateGame(clonedState, watch, timeLimit, depth, childNode.isMax);
                        if (!winner.HasValue)
                        {
                            break;//We simulated a game, but it didn't end so we are out of time...
                        }
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
                GameTreeNode child = RootNode.GetChild(i);
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
                state.ApplyMove(move, isMax);

                depth--;
                isMax = !isMax;

                if (watch.ElapsedMilliseconds >= timeLimit)
                {
                    return null;
                }
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

            Console.Error.WriteLine("Could not find a winner for simulation!");
            throw new InvalidOperationException("Could not find a winner for simulation!");
        }


        Queue<GameTreeNode> _queue = new Queue<GameTreeNode>(100);
        private GameTreeNode SelectNodeWithUnplayedMoves(GameTreeNode node, double exploration, Stopwatch watch, int timeLimit)
        {
            // If the node has unplayed moves, return it immediately
            if (node.moves.Count > 0 && node.parent == null)
                return node;

            // Enqueue all children for further processing
            for (int i = 0; i < node.children.Count; i++)
            {
                _queue.Enqueue(node.GetChild(i));
            }

            GameTreeNode bestNode = null;
            double maxValue = -1;

            while (_queue.Count > 0)
            {
                GameTreeNode tempNode = _queue.Dequeue();

                if (tempNode.moves.Count > 0)
                {
                    double value = CalculateExplorationValue(tempNode, exploration);
                    if (value > maxValue)
                    {
                        maxValue = value;
                        bestNode = tempNode;
                        if (watch.ElapsedMilliseconds >= timeLimit)
                        {
                            _queue.Clear();
                            break;
                        }
                    }
                }
                else
                {
                    // Enqueue all children for further processing
                    for (int i = 0; i < tempNode.children.Count; i++)
                    {
                        _queue.Enqueue(tempNode.GetChild(i));
                    }
                }
            }

            return bestNode;
        }

        private double CalculateExplorationValue(GameTreeNode node, double exploration)
        {
            double wins = RootNode.isMax ? node.wins : node.loses;
            double nodeTotal = node.totalPlays;
            int parentTotal = node.parent.totalPlays;

            double parentLog = _mathLogCache[parentTotal];

            double value = wins / nodeTotal + exploration * Math.Sqrt(parentLog / nodeTotal);
            return value;
        }

        private object SelectMoveAtRandom(IGameState state, bool isMax)
        {
            IList moves = state.GetPossibleMoves(isMax);
            if (moves.Count == 0)
            {
                Console.Error.WriteLine("No moves available!");
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
            Console.Error.WriteLine("strategy not supported");
            throw new InvalidOperationException("strategy not supported");
        }

        private object SelectMoveSequentially(GameTreeNode node)
        {
            object move;
            if (node.moves.Count == 0)//If there are no more moves then that is a problem...
            {
                Console.Error.WriteLine("No moves found!");
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
                Console.Error.WriteLine("No moves found!");
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
