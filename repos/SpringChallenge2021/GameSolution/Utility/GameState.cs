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
        public SeedMapMask seedMapMask;
        public ShadowMap shadowMap;
        //public ShadowMapMask shadowMapMask;
        public TreeState treeList;

        //Calcualted from the day
        public int sunDirection;

        public int mySunPowerGenerationToday;
        public int opponentSunPowerGenerationToday;

        //Calculated from the trees on the board
        public IEnumerable<Tree> TreeEnumeration
        {
            get
            {
                List<Tree> trees = new List<Tree>(board.Count);
                for(int i =0; i<board.Count; i++)
                {
                    Tree tree = treeList.GetTree(i);
                    if(tree.size != -1)
                    {
                        trees.Add(tree);
                    }
                }
                return trees;
            }
        }


        public GameState(int boardSize = 37)
        {
            board = new List<Cell>(boardSize);
            me = new Player(true);
            opponent = new Player(false);
            treeList = new TreeState();
        }

        public GameState(GameState state)
        {
            day = state.day;
            nutrients = state.nutrients;
            board = state.board;
            
            me = new Player(state.me);
            opponent = new Player(state.opponent);

            seedMapMask = state.seedMapMask;
            shadowMap = state.shadowMap;
            //shadowMapMask = state.shadowMapMask;
            treeList = new TreeState(state.treeList);
        }

        public void AddTree(Tree tree)
        {
            treeList.AddTree(tree);
        }

        public void GrowTree(Tree tree)
        {
            treeList.GrowTree(tree);
        }

        public void RemoveTree(Tree tree)
        {
            treeList.RemoveTree(tree);
        }

        public void SetDormant(Tree tree)
        {
            treeList.SetDormant(tree);
            tree.SetDormant(true);
        }

        public void ChangeTreeOwnership()
        {
            treeList.ChangeTreeOwnership();
        }

        public Tree GetTree(int cellIndex)
        {
            return treeList.GetTree(cellIndex);
        }

        public void UpdateGameState(bool applySun = false)
        {
            sunDirection = day % sunReset;

            if (applySun)
            {
                CalculateShadowsNew();
                CalculateSunGeneration(true);
            }

            CalculateMoves(true);
            CalculateMoves(false);
        }

        private void CalculateMoves(bool isMe)
        {
            Player player = isMe ? me : opponent;

            List<long> possibleMoves = player.possibleMoves;

            bool isWaiting = player.isWaiting;
            int sun = player.sun;

            possibleMoves.Clear();

            if (day == 24)
            {
                return;
            }

            int costToSeed;
            int size3TreeToCut = 4;
            bool canCut;
            bool canSeed;

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

            if (isWaiting)
            {
                possibleMoves.Add(Move.CreateMove(Actions.WAIT));
                return;
            }
            else
            {
                costToSeed = treeList.GetCount(0, isMe);

                canCut = day > 11 && sun >= treeCompleteCost && (day > 22 || treeList.GetCount((int)TreeSize.Large, isMe) > size3TreeToCut);
                canSeed = sun >= costToSeed && costToSeed < 1;//only seed when cost is 0
            }

            

            if (canCut)
            {
                //Complete actions
                long completeTrees = treeList.GetCompleteActions(isMe);
                for (var cellIndex = 0; completeTrees != 0; cellIndex++)
                {
                    if ((completeTrees & 1) == 1)
                        possibleMoves.Add(Move.CreateMove(Actions.COMPLETE, cellIndex));
                    completeTrees >>= 1;
                }
            }

            //Grow actions
            {
                bool[] canGrow = new bool[3]
                {
                    sun >= GetCostToGrow(isMe, 0),
                    sun >= GetCostToGrow(isMe, 1),
                    sun >= GetCostToGrow(isMe, 2)
                };

                long growTrees = treeList.GetGrowActions(canGrow, isMe);
                for (var cellIndex = 0; growTrees != 0; cellIndex++)
                {
                    if ((growTrees & 1) == 1)
                        possibleMoves.Add(Move.CreateMove(Actions.GROW, cellIndex));
                    growTrees >>= 1;
                }
            }

            if (canSeed)
            {
                long allTrees = treeList.GetTrees(isMe);
                for (int i = 2; i <= maxTreeSize; i++)//Skip seeding for size 1 trees
                {
                    long seedTrees = treeList.GetSeedActions(i, isMe);
                    for (var cellIndex = 0; seedTrees != 0; cellIndex++)
                    {
                        if ((seedTrees & 1) == 1)
                        {
                            long seedableLocations = treeList.GetSeedableSpaces() & seedMapMask.GetSeedMap(cellIndex, i, (int)TreeSize.Medium);//Skip seeding 1 space away; retrieve seedable locations starting from size 2;
                            for(var targetCellIndex = 0; seedableLocations != 0; targetCellIndex++)
                            {
                                if ((seedableLocations & 1) == 1)
                                {
                                    if ((seedMapMask.GetSeedMap(targetCellIndex, (int)TreeSize.Small) & allTrees) == 0)
                                    {
                                        possibleMoves.Add(Move.CreateMove(Actions.SEED, cellIndex, targetCellIndex));
                                    }
                                }
                                seedableLocations >>= 1;
                            }
                        }
                        seedTrees >>= 1;
                    }
                }
            }

            if (possibleMoves.Count == 0 || sun < 3)
            {
                possibleMoves.Add(Move.CreateMove(Actions.WAIT));
            }
        }

        /// <summary>
        /// Calculates the spookiness of each tree
        /// </summary>
        private void CalculateShadowsNew()
        {
            //Calculate the shadow size of each cell
            long trees = treeList.GetTrees(1) | treeList.GetTrees(2) | treeList.GetTrees(3);
            for (var cellIndex = 0; trees != 0; cellIndex++)
            {
                if ((trees & 1) == 1)
                {
                    int distance = 1;
                    int treeSize = treeList.GetSize(cellIndex);
                    foreach (int shadowCellIndex in shadowMap.GetShadowMap(cellIndex, sunDirection))
                    {
                        int shadowSize = treeList.GetSize(shadowCellIndex);
                        if (shadowSize >= distance && shadowSize >= treeSize)
                        {
                            treeList.SetSpookyShadow(cellIndex);
                            break;
                        }
                        distance++;
                    }
                }
                trees >>= 1;
            }
        }

        private void CalculateSunGeneration(bool apply)
        {
            mySunPowerGenerationToday = 0;
            opponentSunPowerGenerationToday = 0;

            for (int i = 1; i<=maxTreeSize; i++)
            {
                mySunPowerGenerationToday += treeList.GetCountForSun(i, true) * i;
                opponentSunPowerGenerationToday += treeList.GetCountForSun(i, false) * i;
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
                        SetDormant(sourceTree);
                        sourceTree = GetTree(Move.GetSourceIndex(opponentMove));
                        SetDormant(sourceTree);
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
                    SetDormant(sourceTree);
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

        


        public int GetCostToSeed(bool isMe = true)
        {
            return treeList.GetCount((int)TreeSize.Seed, isMe);
        }

        public int GetNumberOfTrees(bool isMe, int size)
        {
            return treeList.GetCount(size, isMe);
        }

        public int GetCostToGrow(Tree tree)
        {
            return GetCostToGrow(tree.isMine, tree.size);
        }

        public int GetCostToGrow(bool isMe, int treeSize)
        {
            treeSize++;
            return treeList.GetCount(treeSize, isMe) + treeSizeToCost[treeSize];
        }

        public void ResetPlayers()
        {
            me.Reset();
            opponent.Reset();
        }

        public void ResetTrees()
        {
            treeList.ResetTrees();
        }

        public void RemoveTrees()
        {
            treeList = new TreeState();
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
                    int countMyTrees = treeList.GetCountTrees(true);
                    int countOppTrees = treeList.GetCountTrees(false);

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

            if(day == gameState.day && nutrients == gameState.nutrients && me.Equals(gameState.me) && opponent.Equals(gameState.opponent) && treeList.Equals(gameState.treeList))
            {
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
            return "d: " + day + " n: " + nutrients + "\n" + treeList.ToString() + "\n" + me.ToString() + "\n" + opponent.ToString();
        }
    }
}
