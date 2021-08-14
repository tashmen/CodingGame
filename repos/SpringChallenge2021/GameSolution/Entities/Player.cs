using GameSolution.Moves;
using System.Collections.Generic;

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
        public Move movePlayed;

        public Player(bool isMe)
        {
            possibleMoves = new List<Move>();
            this.isMe = isMe;
            movePlayed = null;
        }

        public Player(Player player)
        {
            sun = player.sun;
            score = player.score;
            isWaiting = player.isWaiting;
            possibleMoves = new List<Move>();
            isMe = player.isMe;
            movePlayed = player.movePlayed;
        }

        public void Reset()
        {
            movePlayed = null;
            possibleMoves = new List<Move>();
            isWaiting = false;
        }

        public int GetScore()
        {
            return score + sun / 3;
        }

        public bool Equals(Player player)
        {
            if(player.isMe == isMe && player.isWaiting == isWaiting && player.score == score && player.sun == sun)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public override string ToString()
        {
            string strMove = movePlayed != null ? movePlayed.ToString() : "";
            return $"sun: {sun}, score: {score}, wait: {isWaiting}, me: {isMe} \n {strMove}";
        }
    }
}
