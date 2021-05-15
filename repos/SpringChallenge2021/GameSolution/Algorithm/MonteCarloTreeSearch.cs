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

        public void SetState(IGameState rootState)
        {
            rootNode = new Node(rootState);
        }

        public IMove GetNextMove(Stopwatch watch, int timeLimit)
        {
            int count = 0;
            do
            {
                IMove move = SelectMoveAtRandom(rootNode);
                IGameState nextState = rootNode.state.Clone();
                nextState.ApplyMove(move);
                Node childNode = new Node(nextState);
                rootNode.children.Add(childNode);
                SimulateGame(childNode);
                count++;
            }
            while (watch.ElapsedMilliseconds < timeLimit);
            Console.Error.WriteLine($"Played {count} games!");
            

            Node bestChild = null;
            double bestScore = -1;
            foreach(Node child in rootNode.children)
            {
                double score = child.GetScore();
                if(bestScore < score)
                {
                    bestChild = child;
                    bestScore = score;
                }
            }

            Console.Error.WriteLine($"w: {bestChild.wins} l: {bestChild.loses} d: {bestChild.draws}");
            //Adjust nodes for next turn?

            return bestChild.state.GetMove();
        }

        private int? SimulateGame(Node node)
        {
            int? winner = node.GetWinner();
            node.ApplyWinner(winner);
            if (winner.HasValue)
            {
                return winner;
            }

            IMove move = SelectMoveAtRandom(node);
            IGameState nextState = node.state.Clone();
            nextState.ApplyMove(move);
            Node childNode = new Node(nextState);
            node.children.Add(childNode);

            winner = SimulateGame(childNode);
            node.ApplyWinner(winner);

            return winner;
        }

        private IMove SelectMoveAtRandom(Node node)
        {
            Node tempNode = node;
            while(tempNode.moves.Count == 0)
            {
                tempNode = node.children[rand.Next(0, node.children.Count - 1)];
            }
            int index = rand.Next(0, tempNode.moves.Count - 1);
            IMove move = tempNode.moves[index];
            tempNode.moves.RemoveAt(index);

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

            public Node(IGameState state)
            {
                this.state = state;
                moves = state.GetPossibleMoves();
                children = new List<Node>();
            }

            public double GetScore()
            {
                return (wins + draws * 0.5) / (wins + draws + loses);
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
