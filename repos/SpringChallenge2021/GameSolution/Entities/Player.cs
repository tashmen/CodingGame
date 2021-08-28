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
        public List<Move> possibleMoves { get; set; }
        public Move movePlayedForCurrentTurn;
        public Move movePlayedLastTurn;

        public Player(bool isMe)
        {
            possibleMoves = new List<Move>();
            this.isMe = isMe;
            movePlayedForCurrentTurn = null;
            movePlayedLastTurn = null;
        }

        public Player(Player player)
        {
            sun = player.sun;
            score = player.score;
            isWaiting = player.isWaiting;
            isMe = player.isMe;

            possibleMoves = new List<Move>(player.possibleMoves);
            movePlayedForCurrentTurn = player.movePlayedForCurrentTurn;
            movePlayedLastTurn = player.movePlayedLastTurn;
        }

        public void Reset()
        {
            possibleMoves = new List<Move>();
            isWaiting = false;
        }

        public void ResetMoves()
        {
            movePlayedLastTurn = movePlayedForCurrentTurn;
            movePlayedForCurrentTurn = null;
        }

        public int GetScore()
        {
            return score + sun / 3;
        }

        public bool Equals(Player player)
        {
            if (player.isMe == isMe && player.isWaiting == isWaiting && player.score == score && player.sun == sun && (player.movePlayedForCurrentTurn == movePlayedForCurrentTurn || (player.movePlayedForCurrentTurn != null && player.movePlayedForCurrentTurn.Equals(movePlayedForCurrentTurn))))
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
            string strMove = movePlayedLastTurn != null ? movePlayedLastTurn.ToString() : "";
            return $"sun: {sun}, score: {score}, wait: {isWaiting}, me: {isMe} \n {strMove}";
        }
    }
}
