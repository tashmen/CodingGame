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
        public bool isMe;

        //Calculated from the game board
        public List<Move> possibleMoves;

        public Player(bool isMe)
        {
            possibleMoves = new List<Move>();
            this.isMe = isMe;
        }

        public Player(Player player)
        {
            sun = player.sun;
            score = player.score;
            isWaiting = player.isWaiting;
            possibleMoves = new List<Move>();
            isMe = player.isMe;
        }

        public void Reset()
        {
            possibleMoves = new List<Move>();
            isWaiting = false;
        }
    }
}
