using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Algorithm
{
    public class MonteCarloTreeSearch
    {
        private Node rootNode;
        private Random rand;
        public MonteCarloTreeSearch()
        {
            rand = new Random();
        }

        public void SetState(IGameState rootState, bool isMax = true)
        {   
            if (rootNode != null)
            {
                //if we have already started searching then continue to search as we go if possible; the search will scan two layers to see if only one move was played or if 2 moves were played to get back to the original players turn.

                //find the child that matches the new node
                bool isFound = false;
                //Expand any moves left in the root node (if any)
                foreach(IMove move in rootNode.moves)
                {
                    Expand(rootNode, move);
                }
                //Begin scanning the children
                foreach(Node child in rootNode.children)
                {   
                    if (child.state.Equals(rootState))
                    {
                        rootNode = child;
                        isFound = true;
                        break;
                    }

                    foreach (IMove move in child.moves)
                    {
                        Expand(child, move);
                    }
                    foreach (Node descendent in child.children)
                    {
                        if(descendent.state.Equals(rootState))
                        {
                            rootNode = descendent;
                            isFound = true;
                            break;
                        }
                    }
                }
                if (!isFound)
                {
                    Console.Error.WriteLine("Could not find the next state in tree!  Starting over...");
                    rootNode = new Node(rootState.Clone(), isMax);
                }
            }
            else
            {
                rootNode = new Node(rootState.Clone(), isMax);
            }
        }

        /// <summary>
        /// Get the next move
        /// </summary>
        /// <param name="watch">timer</param>
        /// <param name="timeLimit">The amount of time to give to the search in milliseconds</param>
        /// <param name="numRollouts">The number of roll outs to play per expansion</param>
        /// <returns></returns>
        public IMove GetNextMove(Stopwatch watch, int timeLimit, int numRollouts = 1, double? exploration = null)
        {
            if(exploration == null)
            {
                exploration = Math.Sqrt(2);
            }
            int count = 0;
            do
            {
                Node selectedNode = SelectNodeWithUnplayedMoves(rootNode, exploration.Value);
                if(selectedNode == null)
                {
                    break;
                }
                IMove move = SelectMoveAtRandom(selectedNode);
                Node childNode = Expand(selectedNode, move);
                int? winner = childNode.GetWinner();
                if (winner.HasValue)
                {
                    BackPropagate(childNode, winner);
                }
                else
                {
                    for(int i = 0; i<numRollouts; i++)
                    {
                        winner = SimulateGame(childNode.state.Clone(), watch, timeLimit, childNode.isMax);
                        BackPropagate(childNode, winner);
                        count++;
                    }
                }
            }
            while (watch.ElapsedMilliseconds < timeLimit);
            Console.Error.WriteLine($"Played {count} games!");
            

            Node bestChild = null;
            double bestScore = -1;
            foreach(Node child in rootNode.children)
            {
                double score = child.GetScore(rootNode.isMax);
                if(bestScore < score)
                {
                    bestChild = child;
                    bestScore = score;
                }
                Console.Error.WriteLine($"w: {child.wins} l: {child.loses} d: {child.draws} move: {child.state.GetMove(rootNode.isMax)} score: {score} isMax: {rootNode.isMax}");
            }


            Console.Error.WriteLine($"Best: w: {bestChild.wins} l: {bestChild.loses} d: {bestChild.draws}");

            return bestChild.state.GetMove(rootNode.isMax);
        }

        private void BackPropagate(Node selectedNode, int? winner)
        {
            selectedNode.ApplyWinner(winner);
            Node tempNode = selectedNode.parent;
            while(tempNode != null)
            {
                tempNode.ApplyWinner(winner);
                tempNode = tempNode.parent;
            }
        }

        private int? SimulateGame(IGameState state, Stopwatch watch, int timeLimit, bool isMax)
        {
            int? winner = state.GetWinner();
            if (winner.HasValue)
            {
                return winner;
            }

            IMove move = SelectMoveAtRandom(state, isMax);
            state.ApplyMove(move, isMax);

            if (watch.ElapsedMilliseconds >= timeLimit)
            {
                return 0;
            }

            winner = SimulateGame(state, watch, timeLimit, !isMax);

            return winner;
        }

        private Node Expand(Node node, IMove move)
        {
            IGameState nextState = node.state.Clone();
            nextState.ApplyMove(move, node.isMax);
            Node childNode = new Node(nextState, !node.isMax, node);
            node.children.Add(childNode);

            return childNode;
        }

        private Node SelectNodeWithUnplayedMoves(Node node, double exploration)
        {
            Queue<Node> queue = new Queue<Node>();
            queue.Enqueue(node);

            Node tempNode;
            Node bestNode = null;
            double maxValue = -1;
            while (queue.Count > 0)
            {
                tempNode = queue.Dequeue();
                if (tempNode.moves.Count == 0)
                {
                    foreach (Node child in tempNode.children)
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

        private IMove SelectMoveAtRandom(Node node)
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

        internal class Node
        {
            public IGameState state;
            public IList<IMove> moves;
            public List<Node> children;
            public int wins = 0;
            public int loses = 0;
            public int draws = 0;
            public Node parent;
            public bool isMax;

            public Node(IGameState state, bool isMax, Node parent = null)
            {
                this.state = state;
                moves = state.GetPossibleMoves(isMax);
                children = new List<Node>();
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
        }
    }
}
