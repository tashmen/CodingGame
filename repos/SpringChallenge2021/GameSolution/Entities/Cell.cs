using System;
using System.Collections.Generic;

namespace GameSolution.Entities
{
    public class Cell
    {
        public int index;
        public int richness;
        public List<int> neighbours;
        public Tree tree { get; private set; }

        public bool HasTree { get; private set; }

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
            neighbours = cell.neighbours;
            
            if (cell.HasTree)
            {
                tree = new Tree(cell.tree);
                HasTree = cell.HasTree;
            }
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
            if (HasTree)
            {
                tree.isSpookyShadow = tree.size <= shadowSize;
            }
        }

        /// <summary>
        /// Retrieves the cell neighbor index or -1 if one doesn't exist
        /// </summary>
        /// <param name="sunDirection">the direction of the sun</param>
        /// <returns>The cell in the direction of the sun from this cell</returns>
        public int GetCellNeighbor(int sunDirection)
        {
            return neighbours[sunDirection];
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
                HasTree = true;
            }
            
        }

        public void RemoveTree()
        {
            this.tree = null;
            HasTree = false;
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
