namespace GameSolution.Entities
{
    public class Tree
    {
        public int cellIndex { get; private set; }
        public int size { get { return (int)(bitBoard & sizeValue); } }
        public bool isMine { get { return (bitBoard & isMineValue) == isMineValue; } }
        public bool isDormant { get { return (bitBoard & isDormantValue) == isDormantValue; } }

        //Calculated value
        public bool isSpookyShadow { get { return (bitBoard & isSpookyShadowValue) == isSpookyShadowValue; } }

        private long bitBoard = 0;
        private static long sizeValue = 3;
        private static long isMineValue = 4;
        private static long isDormantValue = 8;
        private static long isSpookyShadowValue = 16;

        public Tree(int cellIndex, int size, bool isMine, bool isDormant)
        {
            this.cellIndex = cellIndex;
            bitBoard += size;
            bitBoard += isMine ? isMineValue : 0;
            bitBoard += isDormant ? isDormantValue : 0;
        }

        public Tree(Tree tree)
        {
            cellIndex = tree.cellIndex;
            bitBoard = tree.bitBoard;
        }
        
        public void Reset()
        {
            bitBoard &= ~isDormantValue;
            bitBoard &= ~isSpookyShadowValue;
        }

        public void Grow()
        {
            bitBoard |= isDormantValue;
            bitBoard += 1;
        }

        public void UpdateSpookyShadow(int shadowSize)
        {
            if(size <= shadowSize)
            {
                bitBoard |= isSpookyShadowValue;
            }
            else
            {
                bitBoard &= ~isSpookyShadowValue;
            }
        }

        public void ChangeOwnership()
        {
            bitBoard ^= isMineValue;
        }

        public void SetDormant(bool isDormant)
        {
            if (isDormant)
            {
                bitBoard |= isDormantValue;
            }
            else
            {
                bitBoard &= ~isDormantValue;
            }
            
        }

        public override string ToString()
        {
            return $"i: {cellIndex} s: {size} me: {isMine} d: {isDormant}";
        }

        public bool Equals(Tree tree)
        {
            if(tree.cellIndex == cellIndex && tree.bitBoard == bitBoard)
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
