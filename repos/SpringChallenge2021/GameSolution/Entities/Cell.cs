namespace GameSolution.Entities
{
    public class Cell
    {
        public int index { get; private set; }
        public int richness { get; private set; }
        public int[] neighbours { get; private set; }

        public Cell(int index, int richness, int[] neighbours)
        {
            this.index = index;
            this.richness = richness;
            this.neighbours = neighbours;
        }

        public Cell(Cell cell)
        {
            index = cell.index;
            richness = cell.richness;
            neighbours = cell.neighbours;
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

        public override string ToString()
        {
            return $"index: {index} rich: {richness}";
        }

        public bool Equals(Cell cell)
        {
            if(cell.index == index && richness == cell.richness)
            {
                return true;
            }
            return false;
        }
    }
}
