using GameSolution.Algorithm;

namespace GameSolution.Moves
{
    public class MoveSimultaneous : IMove
    {
        public Move myMove;
        public Move opponentMove;
        public MoveSimultaneous(Move myMove, Move opponentMove)
        {
            this.myMove = myMove;
            this.opponentMove = opponentMove;
        }
    }
}
