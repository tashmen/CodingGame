using Algorithms.Space;
using GameSolution.Game;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GameSolution.Entities
{
    public class Board
    {
        public int Height { get; set; }
        public int Width { get; set; }
        
        public IList<Cell> Cells { get; set; }

        public Board(int height, int width)
        {
            Height= height;
            Width= width;
        }

        public Board(Board board)
        {
            Height= board.Height;
            Width= board.Width;
            Cells= board.Cells.Select(c => c.Clone()).ToList();
        }

        public void SetCells(IList<Cell> cells)
        {
            Cells = cells;
        }

        public Board Clone()
        {
            return new Board(this);
        }

        public double? GetWinner()
        {
            var myTiles = Cells.Where(c => c.Owner == Owner.Me).Count();
            var oppTiles = Cells.Where(c => c.Owner == Owner.Opponent).Count();
            if(myTiles > oppTiles)
            {
                return 1;
            }
            else if (oppTiles > myTiles)
            {
                return -1;
            }

            return null;
        }
    }
}
