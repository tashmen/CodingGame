using System;
using System.Collections;

namespace Algorithms.GameComponent
{
    public interface IGameState : IDisposable
    {
        /// <summary>
        /// Retrieve the possible moves
        /// </summary>
        /// <param name="isMax">Whether or not to retrieve moves for max</param>
        /// <returns>list of all possible moves</returns>
        IList GetPossibleMoves(bool isMax);

        /// <summary>
        /// Applies a move to the game state.  The game state must remember this move so that it can be retrieves with GetMove.
        /// </summary>
        /// <param name="isMax">Whether or not the move is for max</param>
        /// <param name="move">the move to apply</param>
        void ApplyMove(object move, bool isMax);

        /// <summary>
        /// Retrieves the move that was played to reach this state.
        /// </summary>
        /// <param name="isMax">Whether or not the move is for max</param>
        /// <returns>The move</returns>
        object GetMove(bool isMax);

        /// <summary>
        /// Clones the game state
        /// </summary>
        /// <returns>The copy of the state</returns>
        IGameState Clone();

        /// <summary>
        /// Returns whether or not the game is over and who won (1 - max wins, 0 - draw, -1 - min wins, null - game is not over)
        /// </summary>
        /// <returns>Who won the game</returns>
        double? GetWinner();

        /// <summary>
        /// Determines if the game state is the same as this one
        /// </summary>
        /// <param name="">the state to compare against</param>
        /// <returns>true if equal</returns>
        bool Equals(IGameState state);

        /// <summary>
        /// Evaluates the current game board closer to 1 is max wins closer to -1 is min wins
        /// </summary>
        /// <param name="isMax">true if it is max's turn else false</param>
        /// <returns>A number between [-1, 1]</returns>
        double Evaluate(bool isMax);
    }
}
