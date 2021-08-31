﻿using Algorithms;
using GameSolution.Entities;
using GameSolution.Moves;
using System;
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
        private List<int> treeSizeKeyToCount;

        public GameState(int boardSize = 37)
        {
            board = new List<Cell>(boardSize);
            me = new Player(true);
            opponent = new Player(false);
            treeState = new Tree[boardSize];
            treeCache = new List<Tree>(boardSize);
            treeSizeKeyToCount = new List<int>(8) { 0, 0, 0, 0, 0, 0, 0, 0 };
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

            treeSizeKeyToCount = new List<int>(state.treeSizeKeyToCount);
            seedMap = state.seedMap;
            shadowMap = state.shadowMap;
        }

        public void AddTree(Tree tree)
        {
            treeSizeKeyToCount[GetCacheTreeSizeKey(tree.size, tree.isMine)]++;
            treeState[tree.cellIndex] = tree;
            treeCache.Add(tree);
        }

        public void GrowTree(Tree tree)
        {
            treeSizeKeyToCount[GetCacheTreeSizeKey(tree.size, tree.isMine)]--;
            tree.Grow();
            treeSizeKeyToCount[GetCacheTreeSizeKey(tree.size, tree.isMine)]++;
        }

        public void RemoveTree(Tree tree)
        {
            treeSizeKeyToCount[GetCacheTreeSizeKey(tree.size, tree.isMine)]--;
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
            List<Move> myPossibleMoves = me.possibleMoves;
            List<Move> oppPossibleMoves = opponent.possibleMoves;
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

            if (meWaiting)
            {
                myPossibleMoves.Add(new Move(Actions.WAIT));
            }
            else
            {
                costToSeedMe = GetCostToSeed(true);
                
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
                

                canCutMe = mySun >= treeCompleteCost && (day > 20 || GetNumberOfTrees(true, (int)TreeSize.Large) > size3TreeToCut || me.score < opponent.score);
                canSeedMe = mySun >= costToSeedMe && costToSeedMe < 1;//only seed when cost is 0
            }

            oppPossibleMoves.Add(new Move(Actions.WAIT));

            int costToSeedOpp;
            bool canSeedOpp = false;
            bool canCutOpp = false;
            if (!oppWaiting)
            {
                costToSeedOpp = GetCostToSeed(false);

                canSeedOpp = oppSun >= costToSeedOpp && costToSeedOpp < 1;//only seed when cost is 0
                canCutOpp = oppSun >= treeCompleteCost;
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
                    if (canSeedMe && treeSize > 1)//do not seed with size 1 trees
                    {
                        for(int i = 2; i<=treeSize; i++)//Skip seeding 1 space away
                        {
                            foreach (int cellIndex in seedMap.GetSeedMap(treeCellIndex, i))
                            {
                                Cell tempCurrent = board[cellIndex];
                                AddSeedAction(me, cellIndex, treeCellIndex);
                            }
                        }
                    }

                    //Complete Actions
                    if (canCutMe && treeSize == maxTreeSize)
                    {
                        myPossibleMoves.Add(new Move(Actions.COMPLETE, treeCellIndex));
                    }

                    //Grow Actions
                    if (treeSize != maxTreeSize && mySun >= GetCostToGrow(tree))
                    {
                        myPossibleMoves.Add(new Move(Actions.GROW, treeCellIndex));
                    }
                }
                else if(!oppWaiting)
                {
                    //Seed Actions
                    if (canSeedOpp && treeSize > 0)
                    {
                        for (int i = 1; i <= treeSize; i++)
                        {
                            foreach (int cellIndex in seedMap.GetSeedMap(treeCellIndex, i))
                            {
                                Cell tempCurrent = board[cellIndex];
                                AddSeedAction(opponent, cellIndex, treeCellIndex);
                            }
                        }
                    }

                    //Complete Actions
                    if (canCutOpp && treeSize == maxTreeSize)
                    {
                        oppPossibleMoves.Add(new Move(Actions.COMPLETE, treeCellIndex));
                    }

                    //Grow Actions
                    if (treeSize != maxTreeSize && oppSun >= GetCostToGrow(tree))
                    {
                        oppPossibleMoves.Add(new Move(Actions.GROW, treeCellIndex));
                    }
                }
            }

            if ((myPossibleMoves.Count == 0 || mySun < 8) && !meWaiting)
            {
                myPossibleMoves.Add(new Move(Actions.WAIT));
            }
        }

        private void AddSeedAction(Player player, int targetCellIndex, int sourceCellIndex)
        {
            Tree tree = GetTree(targetCellIndex);
            if (tree == null)
            {               
                player.possibleMoves.Add(new Move(Actions.SEED, sourceCellIndex, targetCellIndex));
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
        public void ApplyMoves(Move myMove, Move opponentMove)
        {
            switch (myMove.type)
            {
                case Actions.SEED:
                    if(opponentMove.type == Actions.SEED && myMove.targetCellIdx == opponentMove.targetCellIdx)
                    {
                        Tree sourceTree = GetTree(myMove.sourceCellIdx);
                        sourceTree.SetDormant(true);
                        sourceTree = GetTree(opponentMove.sourceCellIdx);
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
            if(myMove.type == Actions.COMPLETE)
            {
                countComplete++;
            }
            if(opponentMove.type == Actions.COMPLETE)
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
        public void ApplyMove(Move move, Player player, bool updateState = true, bool updateNutrients = true)
        {
            int targetIndex = move.targetCellIdx;
            int sourceIndex = move.sourceCellIdx;
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
            switch (move.type)
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

        
        private List<int> GetCacheTreeSize()
        {
            return treeSizeKeyToCount;
        }

        private int GetCacheTreeSizeKey(int size, bool isMe)
        {
            if (isMe)
                return size*2;
            else return size* 2 + 1;
        }
        public int GetCostToSeed(bool isMe = true)
        {
            int key = GetCacheTreeSizeKey((int)TreeSize.Seed, isMe);
            return GetCacheTreeSize()[key];
        }

        public int GetNumberOfTrees(bool isMe, int size)
        {
            int key = GetCacheTreeSizeKey(size, isMe);
            return GetCacheTreeSize()[key];
        }

        public int GetCostToGrow(Tree tree)
        {
            int treeSize = tree.size + 1;
            int key = GetCacheTreeSizeKey(treeSize, tree.isMine);
            return GetCacheTreeSize()[key] + treeSizeToCost[treeSize];
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
            treeSizeKeyToCount = new List<int>(8) { 0, 0, 0, 0, 0, 0, 0, 0 };
            treeCache = new List<Tree>(board.Count);
        }

        public IList<IMove> GetPossibleMoves(bool isMax)
        {
            Player player = isMax ? me : opponent;
            return new List<IMove>(player.possibleMoves);
        }

        public void ApplyMove(IMove move, bool isMax)
        {
            if (isMax && opponent.movePlayedForCurrentTurn != null)
            {
                throw new Exception("Expected opponent's move to be empty");
            }

            Player player = isMax ? me : opponent;
            Move movePlayer = move as Move;
            player.movePlayedForCurrentTurn = movePlayer;

            if(me.movePlayedForCurrentTurn != null && opponent.movePlayedForCurrentTurn != null)
            {
                ApplyMoves(me.movePlayedForCurrentTurn, opponent.movePlayedForCurrentTurn);
            }
        }

        public IMove GetMove(bool isMax)
        {
            Player player = isMax ? me : opponent;
            if (player.movePlayedForCurrentTurn != null)
                return player.movePlayedForCurrentTurn;
            else return player.movePlayedLastTurn;
        }

        public IGameState Clone()
        {
            return new GameState(this);
        }

        public int? GetWinner()
        {
            if (day == maxTurns)
            {
                int myScore = me.GetScore();
                int opponentScore = opponent.GetScore();
                if (myScore > opponentScore)
                {
                    return 1;
                }
                else if (myScore < opponentScore)
                {
                    return -1;
                }
                else if (myScore == opponentScore)
                {
                    int countMyTrees = TreeEnumeration.Count(t => t.isMine);
                    int countOppTrees = TreeEnumeration.Count(t => !t.isMine);

                    if (countMyTrees > countOppTrees)
                    {
                        return 1;
                    }
                    else if (countMyTrees < countOppTrees)
                    {
                        return -1;
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
            return "n: " + nutrients + "\n" + string.Join("\n", board.Select(c => c.ToString())) + "\n" + me.ToString() + "\n" + opponent.ToString();
        }
    }
}
