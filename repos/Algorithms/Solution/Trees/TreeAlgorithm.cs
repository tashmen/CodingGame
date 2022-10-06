using Algorithms.GameComponent;
using System;

namespace Algorithms.Trees
{
    public class TreeAlgorithm
    {
        protected GameTreeNode RootNode;
        public void SetState(IGameState rootState, bool isMax = true, bool findState = true)
        {
            if (RootNode != null && findState)
            {
                //if we have already started searching then continue to search as we go if possible; the search will scan two layers to see if only one move was played or if 2 moves were played to get back to the original players turn.

                //find the child that matches the new node
                bool isFound = false;
                //Expand any moves left in the root node (if any)
                for(int i = 0; i<RootNode.moves.Count; i++) 
                {
                    var move = RootNode.moves[i];
                    Expand(RootNode, move);
                }
                //Begin scanning the children
                for(int i = 0; i<RootNode.children.Count; i++)
                {
                    var child = RootNode.children[i];
                    if (child.state.Equals(rootState))
                    {
                        RootNode = child;
                        isFound = true;
                        break;
                    }

                    
                    for (int j = 0; j< child.moves.Count; j++)
                    {
                        var move = child.moves[j];
                        Expand(child, move);
                    }
                    for(int j = 0; j< child.children.Count; j++) 
                    {
                        var descendent = child.children[j];
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
                    RootNode = new GameTreeNode(rootState.Clone(), isMax);
                }
                else
                {
                    RootNode.parent = null;
                }
            }
            else
            {
                RootNode = new GameTreeNode(rootState.Clone(), isMax);
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
            GameTreeNode childNode = new GameTreeNode(nextState, !node.isMax, node);
            node.children.Add(childNode);

            return childNode;
        }
    }
}
