namespace GameSolution.Entities
{
    public class Tree
    {
        public int cellIndex;
        public int size;
        public bool isMine;
        public bool isDormant;

        //Calculated value
        public bool isSpookyShadow;

        public Tree(int cellIndex, int size, bool isMine, bool isDormant)
        {
            this.cellIndex = cellIndex;
            this.size = size;
            this.isMine = isMine;
            this.isDormant = isDormant;
            isSpookyShadow = false;
        }

        public Tree(Tree tree)
        {
            cellIndex = tree.cellIndex;
            size = tree.size;
            isMine = tree.isMine;
            isDormant = tree.isDormant;
            isSpookyShadow = tree.isSpookyShadow;
        }
        
        public void Reset()
        {
            isDormant = false;
        }

        public void Grow()
        {
            isDormant = true;
            size += 1;
        }

        public override string ToString()
        {
            return $"i: {cellIndex} s: {size} me: {isMine} d: {isDormant}";
        }

        public bool Equals(Tree tree)
        {
            if(tree.cellIndex == cellIndex && tree.isDormant == isDormant && tree.isMine == isMine && tree.size == size)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
