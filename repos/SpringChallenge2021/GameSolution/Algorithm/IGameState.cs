using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameSolution.Algorithm
{
    public interface IGameState
    {
        /// <summary>
        /// Retrieve the possible moves
        /// </summary>
        /// <returns>list of all possible moves</returns>
        List<IMove> GetPossibleMoves();

        /// <summary>
        /// Applies a move to the game state.  The game state must remember this move so that it can be retrieves with GetMove.
        /// </summary>
        /// <param name="move">the move to apply</param>
        void ApplyMove(IMove move);

        /// <summary>
        /// Retrieves the move that was played to reach this state.
        /// </summary>
        /// <returns>The move</returns>
        IMove GetMove();

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
    }
}
