using Algorithms;
using GameSolution.Entities;
using GameSolution.Moves;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using static GameSolution.Constants;

namespace GameSolution.Utility
{
    public class GameState : IGameState
    {
        public int day;
        public int nutrients;
        public List<Cell> board;
        public Player me;
        public Player opponent;
        public SeedMap seedMap;
        public ShadowMap shadowMap;
        
        //Calculated from the trees on the board
        public IEnumerable<Tree> TreeEnumeration
        { 
            get
            {
                return treeCache;
            }
        }

        private List<Tree> treeCache;
        private Tree[] treeState;

        //Calcualted from the day
        public int sunDirection;

        public int mySunPowerGenerationToday;
        public int opponentSunPowerGenerationToday;

        //lazy loaded cache
        private List<int> myTreeSizeKeyToCount;
        private List<int> opponentTreeSizeKeyToCount;

        public GameState(int boardSize = 37)
        {
            board = new List<Cell>(boardSize);
            me = new Player(true);
            opponent = new Player(false);
            treeState = new Tree[boardSize];
            treeCache = new List<Tree>(boardSize);
            myTreeSizeKeyToCount = new List<int>(4) { 0, 0, 0, 0 };
            opponentTreeSizeKeyToCount = new List<int>(4) { 0, 0, 0, 0 };
        }

        public GameState(GameState state)
        {
            day = state.day;
            nutrients = state.nutrients;
            board = state.board;
            treeState = new Tree[board.Count];
            treeCache = new List<Tree>(board.Count);
            for (int i = 0; i < board.Count; i++) 
            {
                Tree tree = state.treeState[i];
                if (tree != null)
                {
                    Tree newTree = new Tree(tree);
                    treeState[i] = newTree;
                    treeCache.Add(newTree);
                }
            }
            
            me = new Player(state.me);
            opponent = new Player(state.opponent);

            myTreeSizeKeyToCount = new List<int>(state.myTreeSizeKeyToCount);
            opponentTreeSizeKeyToCount = new List<int>(state.opponentTreeSizeKeyToCount);
            seedMap = state.seedMap;
            shadowMap = state.shadowMap;
        }

        public void AddTree(Tree tree)
        {
            if (tree.isMine)
                myTreeSizeKeyToCount[tree.size]++;
            else
                opponentTreeSizeKeyToCount[tree.size]++;
            treeState[tree.cellIndex] = tree;
            treeCache.Add(tree);
        }

        public void GrowTree(Tree tree)
        {

            if (tree.isMine)
            {
                myTreeSizeKeyToCount[tree.size]--;
                tree.Grow();
                myTreeSizeKeyToCount[tree.size]++;
            }
            else
            {
                opponentTreeSizeKeyToCount[tree.size]--;
                tree.Grow();
                opponentTreeSizeKeyToCount[tree.size]++;
            }
        }

        public void RemoveTree(Tree tree)
        {
            if (tree.isMine)
            {
                myTreeSizeKeyToCount[tree.size]--;
            }
            else opponentTreeSizeKeyToCount[tree.size]--;
            treeState[tree.cellIndex] = null;
            treeCache.Remove(tree);
        }

        public Tree GetTree(int cellIndex)
        {
            return treeState[cellIndex];
        }

        public void UpdateGameState(bool applySun = false)
        {
            sunDirection = day % sunReset;

            if (applySun)
            {
                CalculateShadows();
                CalculateSunGeneration(true);
            }

            CalculateMoves();
        }

        private void CalculateMoves()
        {
            List<long> myPossibleMoves = me.possibleMoves;
            List<long> oppPossibleMoves = opponent.possibleMoves;
            bool meWaiting = me.isWaiting;
            bool oppWaiting = opponent.isWaiting;
            int mySun = me.sun;
            int oppSun = opponent.sun;

            myPossibleMoves.Clear();
            oppPossibleMoves.Clear();

            if (day == 24)
            {
                return;
            }

            int costToSeedMe;
            int size3TreeToCut = 4;
            bool canCutMe = false;
            bool canSeedMe = false;

            if (day > 14)
            {
                size3TreeToCut--;
            }
            if (day > 18)
            {
                size3TreeToCut--;
            }
            if (day > 20)
            {
                size3TreeToCut--;
            }

            if (meWaiting)
            {
                myPossibleMoves.Add(Move.CreateMove(Actions.WAIT));
            }
            else
            {
                costToSeedMe = GetCostToSeed(true);

                canCutMe = day > 11 && mySun >= treeCompleteCost && (day > 22 || GetNumberOfTrees(true, (int)TreeSize.Large) > size3TreeToCut);
                canSeedMe = mySun >= costToSeedMe && costToSeedMe < 1;//only seed when cost is 0
            }

            int costToSeedOpp;
            bool canSeedOpp = false;
            bool canCutOpp = false;
            if (oppWaiting)
            {
                oppPossibleMoves.Add(Move.CreateMove(Actions.WAIT));
            }
            else
            {
                costToSeedOpp = GetCostToSeed(false);

                canSeedOpp = oppSun >= costToSeedOpp && costToSeedOpp < 1;//only seed when cost is 0
                canCutOpp = day > 11 && oppSun >= treeCompleteCost && (day > 20 || GetNumberOfTrees(false, (int)TreeSize.Large) > size3TreeToCut);
            }

            foreach (Tree tree in TreeEnumeration)
            {
                if (tree.isDormant)
                {
                    continue;
                }
                int treeSize = tree.size;
                int treeCellIndex = tree.cellIndex;

                if (tree.isMine && !meWaiting)
                {
                    //Complete Actions
                    if (canCutMe && treeSize == maxTreeSize)
                    {
                        myPossibleMoves.Add(Move.CreateMove(Actions.COMPLETE, treeCellIndex));
                    }

                    //Grow Actions
                    if (treeSize != maxTreeSize && mySun >= GetCostToGrow(tree))
                    {
                        myPossibleMoves.Add(Move.CreateMove(Actions.GROW, treeCellIndex));
                    }


                    if (canSeedMe && treeSize > 1)//do not seed with size 1 trees
                    {
                        for(int i = 2; i<=treeSize; i++)//Skip seeding 1 space away
                        {
                            foreach (int cellIndex in seedMap.GetSeedMap(treeCellIndex, i))
                            {
                                bool canSeed = true;
                                foreach(int treeCheckIndex in seedMap.GetSeedMap(cellIndex, 1))
                                {
                                    Tree treeCheck = GetTree(treeCheckIndex);
                                    if (treeCheck != null && treeCheck.isMine)
                                    {
                                        canSeed = false;
                                    }
                                }
                                if(canSeed)
                                    AddSeedAction(myPossibleMoves, cellIndex, treeCellIndex);
                            }
                        }
                    }
                }
                else if(!oppWaiting)
                {
                    //Complete Actions
                    if (canCutOpp && treeSize == maxTreeSize)
                    {
                        oppPossibleMoves.Add(Move.CreateMove(Actions.COMPLETE, treeCellIndex));
                    }

                    //Grow Actions
                    if (treeSize != maxTreeSize && oppSun >= GetCostToGrow(tree))
                    {
                        oppPossibleMoves.Add(Move.CreateMove(Actions.GROW, treeCellIndex));
                    }

                    //Seed Actions
                    if (canSeedOpp && treeSize > 1)//do not seed with size 1 trees
                    {
                        for (int i = 2; i <= treeSize; i++)//Skip seeding 1 space away
                        {
                            foreach (int cellIndex in seedMap.GetSeedMap(treeCellIndex, i))
                            {
                                bool canSeed = true;
                                foreach (int treeCheckIndex in seedMap.GetSeedMap(cellIndex, 1))
                                {
                                    Tree treeCheck = GetTree(treeCheckIndex);
                                    if (treeCheck != null && !treeCheck.isMine)
                                    {
                                        canSeed = false;
                                    }
                                }
                                if (canSeed)
                                    AddSeedAction(oppPossibleMoves, cellIndex, treeCellIndex);
                            }
                        }
                    } 
                }
            }

            if ((myPossibleMoves.Count == 0 || mySun < 3) && !meWaiting)
            {
                myPossibleMoves.Add(Move.CreateMove(Actions.WAIT));
            }

            if ((oppPossibleMoves.Count == 0 || oppSun < 3) && !oppWaiting)
            {
                oppPossibleMoves.Add(Move.CreateMove(Actions.WAIT));
            }
        }

        private void AddSeedAction(IList<long> moveList, int targetCellIndex, int sourceCellIndex)
        {
            Tree tree = GetTree(targetCellIndex);
            if (tree == null)
            {               
                moveList.Add(Move.CreateMove(Actions.SEED, sourceCellIndex, targetCellIndex));
            }
        }

        /// <summary>
        /// Calculates the spookiness of each tree
        /// </summary>
        private void CalculateShadows()
        {
            //Calculate the shadow size of each cell
            foreach (Tree tree in TreeEnumeration)
            {
                int distance = 1;
                foreach (int cellIndex in shadowMap.GetShadowMap(tree.cellIndex, sunDirection))
                {
                    Tree shadowTree = GetTree(cellIndex);
                    if(shadowTree != null && shadowTree.size >= distance && shadowTree.size >= tree.size)
                    {
                        tree.SetSpookyShadow();
                        break;
                    }
                    distance++;
                }
            }
        }

        private void CalculateSunGeneration(bool apply)
        {
            mySunPowerGenerationToday = 0;
            opponentSunPowerGenerationToday = 0;
            foreach(Tree tree in TreeEnumeration)
            {
                if (!tree.isSpookyShadow)
                {
                    if (tree.isMine)
                    {
                        mySunPowerGenerationToday += tree.size;
                    }
                    else
                    {
                        opponentSunPowerGenerationToday += tree.size;
                    }
                }
            }

            if (apply)
            {
                me.sun += mySunPowerGenerationToday;
                opponent.sun += opponentSunPowerGenerationToday;
                //Console.Error.WriteLine($"day: {day}, my sun gen: {mySunPowerGenerationToday}, opp sun gen: {opponentSunPowerGenerationToday}");
            }
        }

        /// <summary>
        /// Applies moves simultaneously
        /// Notes: 
        ///     simultaneous seeds cancel out
        ///     simultaneous completes are shared; nutrients are decreased at the end by the number of trees cut
        /// </summary>
        /// <param name="myMove">The move I am making</param>
        /// <param name="opponentMove">The move my opponent is making</param>
        public void ApplyMoves(long myMove, long opponentMove)
        {
            switch (Move.GetType(myMove))
            {
                case Actions.SEED:
                    if(Move.GetType(opponentMove) == Actions.SEED && Move.GetTargetIndex(myMove) == Move.GetTargetIndex(opponentMove))
                    {
                        Tree sourceTree = GetTree(Move.GetSourceIndex(myMove));
                        sourceTree.SetDormant(true);
                        sourceTree = GetTree(Move.GetSourceIndex(opponentMove));
                        sourceTree.SetDormant(true);
                    }
                    else
                    {
                        ApplyMove(myMove, me, false, false);
                        ApplyMove(opponentMove, opponent, false, false);
                    }
                    break;
                default:
                    ApplyMove(myMove, me, false, false);
                    ApplyMove(opponentMove, opponent, false, false);
                    break;
            }

            int countComplete = 0;
            if(Move.GetType(myMove) == Actions.COMPLETE)
            {
                countComplete++;
            }
            if(Move.GetType(opponentMove) == Actions.COMPLETE)
            {
                countComplete++;
            }
            nutrients -= countComplete;

            if (me.isWaiting && opponent.isWaiting)
            {
                AdvanceDay();
            }
            else
            {
                UpdateGameState();
            }

            //Reset moves for the turn
            me.ResetMoves();
            opponent.ResetMoves();
        }

        /// <summary>
        /// Apply a move for a single player
        /// </summary>
        /// <param name="move">The move to play</param>
        /// <param name="player">The player who made the move</param>
        public void ApplyMove(long move, Player player, bool updateState = true, bool updateNutrients = true)
        {
            int targetIndex = Move.GetTargetIndex(move);
            int sourceIndex = Move.GetSourceIndex(move);
            Tree targetTree = null;
            Tree sourceTree = null;

            if(targetIndex != -1)
            {
                targetTree = GetTree(targetIndex);
            }
            if(sourceIndex != -1)
            {
                sourceTree = GetTree(sourceIndex);
            }
            switch (Move.GetType(move))
            {
                case Actions.COMPLETE:
                    if(player.sun < treeCompleteCost)
                    {
                        throw new Exception("Not enough sun!");
                    }
                    if (targetTree.isDormant)
                    {
                        throw new Exception("Tree is dormant!");
                    }
                    player.sun -= treeCompleteCost;
                    player.score += GetTreeCutScore(board[targetIndex]);
                    RemoveTree(targetTree);
                    if(updateNutrients)
                        nutrients--;
                    if(updateState)
                        UpdateGameState();
                    break;
                case Actions.GROW:
                    int growCost = GetCostToGrow(targetTree);
                    if (player.sun < growCost)
                    {
                        throw new Exception("Not enough sun!");
                    }
                    if (targetTree.isDormant)
                    {
                        throw new Exception("Tree is dormant!");
                    }
                    player.sun -= growCost;
                    GrowTree(targetTree);
                    if (updateState)
                        UpdateGameState();
                    break;
                case Actions.SEED:
                    int seedCost = GetCostToSeed(player.isMe);
                    if (player.sun < seedCost)
                    {
                        throw new Exception("Not enough sun");
                    }
                    if (sourceTree.isDormant)
                    {
                        throw new Exception("Tree is dormant!");
                    }
                    player.sun -= seedCost;
                    sourceTree.SetDormant(true);
                    AddTree(new Tree(targetIndex, (int)TreeSize.Seed, player.isMe, true));
                    if (updateState)
                        UpdateGameState();
                    break;
                case Actions.WAIT:
                    player.isWaiting = true;
                    if(me.isWaiting && opponent.isWaiting)
                    {
                        if(updateState)
                            AdvanceDay();
                    }
                    break;
                default:
                    break;
            }
        }

        public void AdvanceDay()
        {
            day++;
            ResetPlayers();
            ResetTrees();
            UpdateGameState(true);
        }

        

        public int GetTreeCutScore(Cell cell)
        {
            return nutrients + (cell.richness * 2 - 2);
        }

        
        private List<int> GetCacheTreeSize(bool isMe)
        {
            if (isMe)
            {
                return myTreeSizeKeyToCount;
            }
            return opponentTreeSizeKeyToCount;
        }

        public int GetCostToSeed(bool isMe = true)
        {
            return GetCacheTreeSize(isMe)[(int)TreeSize.Seed];
        }

        public int GetNumberOfTrees(bool isMe, int size)
        {
            return GetCacheTreeSize(isMe)[size];
        }

        public int GetCostToGrow(Tree tree)
        {
            int treeSize = tree.size + 1;
            return GetCacheTreeSize(tree.isMine)[treeSize] + treeSizeToCost[treeSize];
        }

        public void ResetPlayers()
        {
            me.Reset();
            opponent.Reset();
        }

        public void ResetTrees()
        {
            foreach(Tree tree in TreeEnumeration)
            {
                tree.Reset();
            }
        }

        public void RemoveTrees()
        {
            treeState = new Tree[board.Count];
            myTreeSizeKeyToCount = new List<int>(4) { 0, 0, 0, 0 };
            opponentTreeSizeKeyToCount = new List<int>(4) { 0, 0, 0, 0 };
            treeCache = new List<Tree>(board.Count);
        }

        public IList GetPossibleMoves(bool isMax)
        {
            Player player = isMax ? me : opponent;
            return player.possibleMoves;
        }

        public void ApplyMove(object move, bool isMax)
        {
            if (isMax && opponent.movePlayedForCurrentTurn != 0)
            {
                throw new Exception("Expected opponent's move to be empty");
            }

            Player player = isMax ? me : opponent;
            long movePlayer = (long)move;
            player.movePlayedForCurrentTurn = movePlayer;

            if(me.movePlayedForCurrentTurn != 0 && opponent.movePlayedForCurrentTurn != 0)
            {
                ApplyMoves(me.movePlayedForCurrentTurn, opponent.movePlayedForCurrentTurn);
            }
        }

        public object GetMove(bool isMax)
        {
            Player player = isMax ? me : opponent;
            if (player.movePlayedForCurrentTurn != 0)
                return player.movePlayedForCurrentTurn;
            else return player.movePlayedLastTurn;
        }

        public IGameState Clone()
        {
            return new GameState(this);
        }

        public double? GetWinner()
        {
            if (day == maxTurns)
            {
                int myScore = me.GetScore();
                int opponentScore = opponent.GetScore();
                if (myScore > opponentScore)
                {
                    return (myScore - opponentScore);
                }
                else if (myScore < opponentScore)
                {
                    return (myScore - opponentScore);
                }
                else if (myScore == opponentScore)
                {
                    int countMyTrees = TreeEnumeration.Count(t => t.isMine);
                    int countOppTrees = TreeEnumeration.Count(t => !t.isMine);

                    if (countMyTrees > countOppTrees)
                    {
                        return (countMyTrees - countOppTrees) * 0.0001;
                    }
                    else if (countMyTrees < countOppTrees)
                    {
                        return (countMyTrees - countOppTrees) * 0.0001;
                    }
                    else return 0;
                }
            }
            else if (day > maxTurns)
                throw new Exception("day advanced too far");
            return null;
        }

        public bool Equals(IGameState state)
        {
            GameState gameState = state as GameState;

            if(day == gameState.day && nutrients == gameState.nutrients && me.Equals(gameState.me) && opponent.Equals(gameState.opponent))
            {
                for(int i = 0; i<treeState.Length; i++)
                {
                    Tree thisTree = treeState[i];
                    Tree otherTree = gameState.treeState[i];
                    if(thisTree == null && otherTree != null)
                    {
                        return false;
                    }
                    else if(thisTree != null && !thisTree.Equals(otherTree))
                    {
                        return false;
                    }
                }
                return true;
            }
            return false;
        }

        public double Evaluate(bool isMax)
        {
            SunPower power = GameHelper.CalculateSunPowerForGame(this);
            double denominator = power.mySunPower + power.oppSunPower;
            double difference = power.GetDifference();
            if (denominator != 0)
            {
                difference = difference / denominator;
                return isMax ? difference : -1 * difference;
            }
            return 0;
        }

        public override string ToString()
        {
            return "d: " + day + " n: " + nutrients + "\n" + string.Join("\n", TreeEnumeration.Select(c => c.ToString())) + "\n" + me.ToString() + "\n" + opponent.ToString();
        }
    }
}
