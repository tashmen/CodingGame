using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameSolution.Entities
{
    public class TreeState
    {
        private long emptyLocations = 274877906943;
        private long[] treeSizeLocations = new long[4] { 0, 0, 0, 0 };
        private long isMineLocations = 0;
        private long isOppLocations = 0;
        private long isDormant = 0;
        private long isSpookyShadow = 0;

        public bool Equals(TreeState otherState)
        {
            if(emptyLocations == otherState.emptyLocations && isMineLocations == otherState.isMineLocations && isOppLocations == otherState.isOppLocations && isDormant == otherState.isDormant && isSpookyShadow == otherState.isSpookyShadow)
            {
                for (int i = 0; i <= Constants.maxTreeSize; i++)
                {
                    if (treeSizeLocations[i] != otherState.treeSizeLocations[i])
                        return false;
                }
                return true;
            }
            return false;
        }

        public TreeState()
        {

        }

        public TreeState(TreeState treeState)
        {
            emptyLocations = treeState.emptyLocations;
            treeState.treeSizeLocations.CopyTo(treeSizeLocations, 0);
            isMineLocations = treeState.isMineLocations;
            isOppLocations = treeState.isOppLocations;
            isDormant = treeState.isDormant;
            isSpookyShadow = treeState.isSpookyShadow;
        }

        public void AddTree(Tree tree)
        {
            ref long location = ref GetLocation(tree.size);
            ref long player = ref GetPlayer(tree.isMine);

            location = BitFunctions.SetBit(location, tree.cellIndex);
            player = BitFunctions.SetBit(player, tree.cellIndex);
            emptyLocations = BitFunctions.ClearBit(emptyLocations, tree.cellIndex);
            isDormant = BitFunctions.SetOrClearBit(isDormant, tree.cellIndex, tree.isDormant);
            isSpookyShadow = BitFunctions.SetOrClearBit(isSpookyShadow, tree.cellIndex, tree.isSpookyShadow);
        }

        public void GrowTree(Tree tree)
        {
            ref long location = ref GetLocation(tree.size);
            ref long newLocation = ref GetLocation(tree.size + 1);

            location = BitFunctions.ClearBit(location, tree.cellIndex);
            newLocation = BitFunctions.SetBit(newLocation, tree.cellIndex);
            isDormant = BitFunctions.SetBit(isDormant, tree.cellIndex);
        }

        public void RemoveTree(Tree tree)
        {
            ref long location = ref GetLocation(tree.size);
            ref long player = ref GetPlayer(tree.isMine);

            location = BitFunctions.ClearBit(location, tree.cellIndex);
            player = BitFunctions.ClearBit(player, tree.cellIndex);
            emptyLocations = BitFunctions.SetBit(emptyLocations, tree.cellIndex);
        }

        public Tree GetTree(int cellIndex)
        {
            return new Tree(cellIndex, GetSize(cellIndex), GetIsMine(cellIndex), GetIsDormant(cellIndex));
        }

        public void SetDormant(Tree tree)
        {
            isDormant = BitFunctions.SetBit(isDormant, tree.cellIndex);
        }

        public void SetSpookyShadow(int cellIndex)
        {
            isSpookyShadow = BitFunctions.SetBit(isSpookyShadow, cellIndex);
        }

        public int GetSize(int cellIndex)
        {
            long cellMask = BitFunctions.GetBitMask(cellIndex);
            for(int i = 0; i<= Constants.maxTreeSize; i++)
            {
                if ((treeSizeLocations[i] & cellMask) == cellMask)
                    return i;
            }

            return -1;
        }

        public bool GetIsMine(int cellIndex)
        {
            long cellMask = BitFunctions.GetBitMask(cellIndex);
            if ((isMineLocations & cellMask) == cellMask)
                return true;
            return false;
        }

        public bool GetIsDormant(int cellIndex)
        {
            long cellMask = BitFunctions.GetBitMask(cellIndex);
            if ((isDormant & cellMask) == cellMask)
                return true;
            return false;
        }

        public int GetCountTrees(bool isMe)
        {
            int totalCount = 0;
            for(int i = 0; i<= Constants.maxTreeSize; i++)
            {
                totalCount += GetCount(i, isMe);
            }
            return totalCount;
        }

        public int GetCount(int treeSize, bool isMe)
        {
            long location = GetLocation(treeSize);
            long player = GetPlayer(isMe);

            return BitFunctions.NumberOfSetBits(location & player);
        }

        public int GetCountForSun(int treeSize, bool isMe)
        {
            long location = GetLocation(treeSize);
            long player = GetPlayer(isMe);

            return BitFunctions.NumberOfSetBits(location & player & ~isSpookyShadow);
        }

        public long GetCompleteActions(bool isMe)
        {
            long player = GetPlayer(isMe);
            return treeSizeLocations[3] & player & ~isDormant;
        }

        public long GetGrowActions(bool[] treeSizesThatCanGrow, bool isMe)
        {
            long player = GetPlayer(isMe);
            long growActions = 0;
            for(int i = 0; i<treeSizesThatCanGrow.Length; i++)
            {
                if (treeSizesThatCanGrow[i])
                {
                    growActions |= GetLocation(i) & player & ~isDormant;
                }
            }
            return growActions;
        }

        public long GetSeedActions(int treeSize, bool isMe)
        {
            long player = GetPlayer(isMe);
            return GetLocation(treeSize) & player & ~isDormant;
        }

        public long GetTrees(bool isMe)
        {
            long player = GetPlayer(isMe);
            long trees = GetTrees();
            return trees & player;
        }

        public long GetTrees()
        {
            long trees = 0;
            for (int i = 0; i < treeSizeLocations.Length; i++)
            {
                trees |= treeSizeLocations[i];
            }
            return trees;
        }

        public long GetTrees(int size)
        {
            return GetLocation(size);
        }

        public void ResetTrees()
        {
            isDormant = 0;
            isSpookyShadow = 0;
        }

        public void ChangeTreeOwnership()
        {
            long swap = isMineLocations;
            isMineLocations = isOppLocations;
            isOppLocations = swap;
        }

        public long GetSeedableSpaces()
        {
            return emptyLocations;
        }        

        

        private ref long GetPlayer(bool isMine)
        {
            if (isMine)
                return ref isMineLocations;
            return ref isOppLocations;
        }

        private ref long GetLocation(int size)
        {
            return ref treeSizeLocations[size];
        }

    }
}
