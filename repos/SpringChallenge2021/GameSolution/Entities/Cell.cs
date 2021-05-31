using System;
using System.Collections.Generic;

namespace GameSolution.Entities
{
    public class Cell
    {
        public int index;
        public int richness;
        public List<int> neighbours;
        public Tree tree;
        
        private Dictionary<int, Cell> sunDirectionToCellNeighbour;

        //Calculated value that must be reset!
        public int shadowSize { get; private set; }

        public Cell(int index, int richness, List<int> neighbours)
        {
            this.index = index;
            this.richness = richness;
            this.neighbours = neighbours;
            Reset();
        }

        public Cell(Cell cell)
        {
            index = cell.index;
            richness = cell.richness;
            neighbours = new List<int>(cell.neighbours);
            if (cell.HasTree())
            {
                tree = new Tree(cell.tree);
            }
            shadowSize = cell.shadowSize;
        }

        public int GetBonusScore()
        {
            return (int)Math.Pow(2, richness) - 2;
        }

        public void Reset()
        {
            shadowSize = -1;
        }

        public void SetShadowSize(int size)
        {
            shadowSize = Math.Max(shadowSize, size);
        }

        public void SetCellNeighbors(Dictionary<int, Cell> neighbours)
        {
            sunDirectionToCellNeighbour = neighbours;
        }

        /// <summary>
        /// Retrieves the cell neighbor or null if one doesn't exist
        /// </summary>
        /// <param name="sunDirection">the direction of the sun</param>
        /// <returns>The cell in the direction of the sun from this cell</returns>
        public Cell GetCellNeighbor(int sunDirection)
        {
            return sunDirectionToCellNeighbour[sunDirection];
        }

        public bool IsCorner()
        {
            return index == 19 || index == 22 || index == 25 || index == 28 || index == 31 || index == 34;
        }

        public void AddTree(Tree tree)
        {
            if (tree.cellIndex == index)
            {
                this.tree = tree;
            }
        }

        public void RemoveTree()
        {
            this.tree = null;
        }

        public bool HasTree()
        {
            return tree != null;
        }

        public override string ToString()
        {
            return $"index: {index} rich: {richness} shadow:{shadowSize} tree: {tree?.ToString()}";
        }

        public bool Equals(Cell cell)
        {
            if(cell.index == index && richness == cell.richness)
            {
                if(tree == null && cell.tree == null)
                {
                    return true;
                }
                else if(tree != null && cell.tree != null && tree.Equals(cell.tree))
                {
                    return true;
                }   
            }
            return false;
        }
    }
}
