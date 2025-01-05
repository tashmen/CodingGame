using Benchmark;
using GameSolution.Entities;

namespace GameSolution.Game
{
    public class GameState4
    {
        public static int MaxTurns = 100;

        public Board2 Board;
        public int Turn;

        public int[] MyProtein;
        public int[] OppProtein;

        public Move? maxMove;
        public Move? minMove;

        private List<Move> _myMoves;
        private List<Move> _oppMoves;

        public GameState4()
        {
            Turn = 0;
            Board = Board2.Get();
            MyProtein = new int[4];
            OppProtein = new int[4];
            maxMove = null;
            minMove = null;
            _myMoves = new List<Move>();
            _oppMoves = new List<Move>();
        }

        public void Dispose()
        {
            Board.Dispose();
        }
    }
}
