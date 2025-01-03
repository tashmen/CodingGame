using Algorithms.GameComponent;
using System;

namespace Algorithms.Trees
{
    public class TreeAlgorithm
    {
        public TreeAlgorithm()
        {
            //_ = new GameTreeNode();
        }

        protected GameTreeNode RootNode;
        public void SetState(IGameState rootState, bool isMax = true, bool findState = true)
        {
            if (RootNode != null && findState)
            {
                //if we have already started searching then continue to search as we go if possible; the search will scan two layers to see if only one move was played or if 2 moves were played to get back to the original players turn.

                //find the child that matches the new node
                bool isFound = false;
                //Expand any moves left in the root node (if any)
                for (int i = 0; i < RootNode.moves.Count; i++)
                {
                    object move = RootNode.moves[i];
                    Expand(RootNode, move);
                }
                //Begin scanning the children
                for (int i = 0; i < RootNode.children.Count; i++)
                {
                    GameTreeNode child = RootNode.GetChild(i);
                    if (child.state.Equals(rootState))
                    {
                        RootNode = child;
                        isFound = true;
                        break;
                    }


                    for (int j = 0; j < child.moves.Count; j++)
                    {
                        object move = child.moves[j];
                        Expand(child, move);
                    }
                    for (int j = 0; j < child.children.Count; j++)
                    {
                        GameTreeNode descendent = child.GetChild(j);
                        if (descendent.state.Equals(rootState))
                        {
                            RootNode = descendent;
                            isFound = true;
                            break;
                        }
                    }
                }
                if (!isFound)
                {
                    Console.Error.WriteLine("Could not find the next state in tree!  Starting over...");
                    //RootNode.Dispose();
                    RootNode = GameTreeNode.GetGameTreeNode(rootState.Clone(), isMax);
                }
                else
                {
                    //Potential loss of non returned game tree node
                    RootNode.parent = null;
                }
            }
            else
            {
                //if (RootNode != null)
                //   RootNode.Dispose();
                RootNode = GameTreeNode.GetGameTreeNode(rootState.Clone(), isMax);
            }
        }

        /// <summary>
        /// Expands the given node by create a clone, applying the move and then adding it to the list of children.
        /// </summary>
        /// <param name="node">The node to expand</param>
        /// <param name="move">The move to play on the expanded node</param>
        /// <returns></returns>
        protected GameTreeNode Expand(GameTreeNode node, object move)
        {
            IGameState nextState = node.state.Clone();
            nextState.ApplyMove(move, node.isMax);
            GameTreeNode childNode = GameTreeNode.GetGameTreeNode(nextState, !node.isMax, node);
            node.children.Add(childNode);

            return childNode;
        }
    }
}
