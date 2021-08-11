using System.Collections.Generic;

namespace Algorithm.MonteCarloTreeSearch
{
    public interface IGameState
    {
        /// <summary>
        /// Retrieve the possible moves
        /// </summary>
        /// <param name="isMax">Whether or not to retrieve moves for max</param>
        /// <returns>list of all possible moves</returns>
        List<IMove> GetPossibleMoves(bool isMax);

        /// <summary>
        /// Applies a move to the game state.  The game state must remember this move so that it can be retrieves with GetMove.
        /// </summary>
        /// <param name="isMax">Whether or not the move is for max</param>
        /// <param name="move">the move to apply</param>
        void ApplyMove(IMove move, bool isMax);

        /// <summary>
        /// Retrieves the move that was played to reach this state.
        /// </summary>
        /// <param name="isMax">Whether or not the move is for max</param>
        /// <returns>The move</returns>
        IMove GetMove(bool isMax);

        /// <summary>
        /// Clones the game state
        /// </summary>
        /// <returns>The copy of the state</returns>
        IGameState Clone();

        /// <summary>
        /// Returns whether or not the game is over and who won (1 - max wins, 0 - draw, -1 - min wins, null - game is not over)
        /// </summary>
        /// <returns>Who won the game</returns>
        int? GetWinner();

        /// <summary>
        /// Determines if the game state is the same as this one
        /// </summary>
        /// <param name="">the state to compare against</param>
        /// <returns>true if equal</returns>
        bool Equals(IGameState state);
    }
}
