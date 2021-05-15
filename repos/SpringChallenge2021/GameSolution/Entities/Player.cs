using GameSolution.Moves;
using System.Collections.Generic;
using System.Linq;

namespace GameSolution.Entities
{
    public class Player
    {
        public int sun;
        public int score;
        public bool isWaiting;

        //Calculated from the game board
        public List<Move> possibleMoves;

        public Player()
        {
            possibleMoves = new List<Move>();
        }

        public Player(Player player)
        {
            sun = player.sun;
            score = player.score;
            isWaiting = player.isWaiting;
            possibleMoves = new List<Move>(player.possibleMoves.Select(m => new Move(m)));
        }

        public void Reset()
        {
            possibleMoves = new List<Move>();
            isWaiting = false;
        }
    }
}
