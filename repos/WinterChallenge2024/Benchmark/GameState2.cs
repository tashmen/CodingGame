using Algorithms.Utility;
using Benchmark;
using GameSolution.Entities;

namespace GameSolution.Game
{
    public class GameState2 : PooledObject<GameState2>
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

        static GameState2()
        {
            SetInitialCapacity(20000);
        }

        public GameState2()
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

        protected override void Reset()
        {
            Board.Dispose();
            _myMoves.Clear();
            _oppMoves.Clear();
        }
    }
}
