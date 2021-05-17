using GameSolution.Utility;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameSolution.Algorithm
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
            //If rootnode is not null should we Adjust nodes for next turn?
            rootNode = new Node(rootState, isMax);
        }

        public IMove GetNextMove(Stopwatch watch, int timeLimit)
        {
            int count = 0;
            do
            {
                Node selectedNode = SelectNodeWithMoves(rootNode);
                int? winner = SimulateGame(selectedNode, watch, timeLimit, selectedNode.isMax);
                BackPropagate(selectedNode, winner);

                count++;
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
                Console.Error.WriteLine($"w: {child.wins} l: {child.loses} d: {child.draws} move: {child.state.GetMove(rootNode.isMax)}");
            }


            Console.Error.WriteLine($"Best: w: {bestChild.wins} l: {bestChild.loses} d: {bestChild.draws}");

            return bestChild.state.GetMove(rootNode.isMax);
        }

        private void BackPropagate(Node selectedNode, int? winner)
        {
            Node tempNode = selectedNode.parent;
            while(tempNode != null)
            {
                tempNode.ApplyWinner(winner);
                tempNode = tempNode.parent;
            }
        }

        private int? SimulateGame(Node node, Stopwatch watch, int timeLimit, bool isMax)
        {
            int? winner = node.GetWinner();
            if (winner.HasValue)
            {
                node.ApplyWinner(winner);
                return winner;
            }

            IMove move = SelectMoveAtRandom(node);
            IGameState nextState = node.state.Clone();
            nextState.ApplyMove(move, isMax);
            Node childNode = new Node(nextState, !isMax, node);
            node.children.Add(childNode);

            if (watch.ElapsedMilliseconds >= timeLimit)
            {
                childNode.ApplyWinner(0);
                node.ApplyWinner(0);
                return 0;
            }

            winner = SimulateGame(childNode, watch, timeLimit, !isMax);
            node.ApplyWinner(winner);

            return winner;
        }

        private Node SelectNodeWithMoves(Node node)
        {
            Node tempNode = node;
            while (tempNode.moves.Count == 0)
            {
                tempNode = tempNode.children[rand.Next(0, tempNode.children.Count - 1)];
            }

            return tempNode;
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
                int index = rand.Next(0, node.moves.Count - 1);
                move = node.moves[index];
                node.moves.RemoveAt(index);
            }
            

            return move;
        }

        internal class Node
        {
            public IGameState state;
            public List<IMove> moves;
            public List<Node> children;
            public int wins = 0;
            public int loses = 0;
            public int draws = 0;
            public int? winner = -8;
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

            public int? GetWinner()
            {
                if (winner == -8)
                {
                    winner = state.GetWinner();
                }

                return winner;
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
