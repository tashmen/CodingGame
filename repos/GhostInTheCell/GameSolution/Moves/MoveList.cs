using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameSolution
{
    public class MoveList
    {
        public List<Move> Moves { get; set; }

        public MoveList()
        {
            Moves = new List<Move>();
        }
        public void AddMove(Move move)
        {
            Moves.Add(move);
        }

        public void PlayMoves()
        {
            bool isFirst = true;
            foreach (Move move in Moves)
            {
                if (!isFirst)
                {
                    Console.Write(";");
                }
                move.PlayMove();
                isFirst = false;
            }
            Console.WriteLine();
        }
    }
}
