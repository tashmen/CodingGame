using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameSolution.Entities
{
    public class Move
    {
        public int Row;
        public int Col;

        public int BoardNumber;

        public Move(int row, int col, int boardNumber)
        {
            Row = row;
            Col = col;
            BoardNumber = boardNumber;
        }
        public Move(Move move)
        {
            Row = move.Row;
            Col = move.Col;
            BoardNumber = move.BoardNumber;
        }

        public bool Equals(Move move)
        {
            return Row == move.Row && Col == move.Col && BoardNumber == move.BoardNumber;
        }

        public Move Clone()
        {
            return new Move(this);
        }

        public override string ToString()
        {
            int row = BoardNumber / 3;
            int col = BoardNumber % 3;

            return $"{Row + 3 * row} {Col + 3 * col}";
        }
    }
}
