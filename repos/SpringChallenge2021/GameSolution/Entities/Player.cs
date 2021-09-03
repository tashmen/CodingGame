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
        public List<long> possibleMoves { get; set; }
        public long movePlayedForCurrentTurn;
        public long movePlayedLastTurn;

        public Player(bool isMe)
        {
            possibleMoves = new List<long>();
            this.isMe = isMe;
            movePlayedForCurrentTurn = 0;
            movePlayedLastTurn = 0;
        }

        public Player(Player player)
        {
            sun = player.sun;
            score = player.score;
            isWaiting = player.isWaiting;
            isMe = player.isMe;

            possibleMoves = new List<long>(player.possibleMoves);
            movePlayedForCurrentTurn = player.movePlayedForCurrentTurn;
            movePlayedLastTurn = player.movePlayedLastTurn;
        }

        public void Reset()
        {
            possibleMoves = new List<long>();
            isWaiting = false;
        }

        public void ResetMoves()
        {
            movePlayedLastTurn = movePlayedForCurrentTurn;
            movePlayedForCurrentTurn = 0;
        }

        public int GetScore()
        {
            return score + sun / 3;
        }

        public bool Equals(Player player)
        {
            if (player.isMe == isMe && player.isWaiting == isWaiting && player.score == score && player.sun == sun && player.movePlayedForCurrentTurn == movePlayedForCurrentTurn)
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
            string strMove = Move.ToString(movePlayedLastTurn);
            return $"sun: {sun}, score: {score}, wait: {isWaiting}, me: {isMe} \n {strMove}";
        }
    }
}
