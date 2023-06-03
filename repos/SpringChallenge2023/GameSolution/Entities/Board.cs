using Algorithms.Graph;
using System.Collections.Generic;
using static Algorithms.Graph.GraphLinks;
using Node = Algorithms.Graph.Node;

namespace GameSolution.Entities
{
    public class Board
    {     
        public IDictionary<int, Cell> Cells { get; set; }
        public Graph Graph { get; set; }
        

        public Board()
        {
            Cells= new Dictionary<int, Cell>();
            Graph = new Graph();
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
            foreach(Cell cell in cells)
            {
                var node = new Node(cell.Index);
                Graph.AddNode(node);
                foreach (int index in cell.Neighbors)
                {
                    if (index != -1)
                    {
                        var neighborCell = GetCell(index);
                        var distance = neighborCell.ResourceAmount > 0 ? 1 : 1.001;
                        node.AddLink(new Link(node, new Node(index), distance));
                        //Console.Error.WriteLine($"adding line {cell.Index}, {index}, {distance}");
                    }
                }
            }
            Graph.CalculateShortestPaths();
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
