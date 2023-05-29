using System.Collections.Generic;

namespace GameSolution.Entities
{
    public class Board
    {     
        public IDictionary<int, Cell> Cells { get; set; }

        public Board()
        {
            Cells= new Dictionary<int, Cell>();
        }

        public Board(Board board)
        {
            Cells = new Dictionary<int, Cell>();
            foreach (var cell in board.Cells.Values)
            {
                board.Cells[cell.Index] = cell.Clone();
            }
            
        }

        public void SetCells(IList<Cell> cells)
        {
            foreach(Cell cell in cells) 
            { 
                Cells[cell.Index] = cell;
            }         
        }

        public Cell GetCell(int index)
        {
            if (index == -1)
                throw new System.Exception();
            return Cells[index];
        }

        public Board Clone()
        {
            return new Board(this);
        }

        public double? GetWinner()
        {
            return null;
        }

        public void UpdateBoard()
        {
           
        }
    }
}
