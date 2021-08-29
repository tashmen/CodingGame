using Algorithms;
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
        
        //Calculated from the trees on the board
        public IEnumerable<Tree> TreeEnumeration
        { 
            get
            {
                return treeCache;
            }
        }

        private IList<Tree> treeCache;

        //Calcualted from the day
        public int sunDirection;
        public int shadowDirection;

        public int mySunPowerGenerationToday;
        public int opponentSunPowerGenerationToday;

        //lazy loaded cache
        private List<int> treeSizeKeyToCount;
        public int BoardSize { get; private set; }

        public GameState(int boardSize = 37)
        {
            BoardSize = boardSize;
            board = new List<Cell>(boardSize);
            me = new Player(true);
            opponent = new Player(false);
            treeCache = new List<Tree>(boardSize);
        }

        public GameState(GameState state)
        {
            day = state.day;
            nutrients = state.nutrients;
            board = new List<Cell>(state.BoardSize);
            treeCache = new List<Tree>(state.BoardSize);
            foreach (Cell cell in state.board)
            {
                Cell newCell = new Cell(cell);
                board.Add(newCell);
                if (cell.HasTree)
                {
                    AddTree(newCell.tree, false);
                }
            }
            
            me = new Player(state.me);
            opponent = new Player(state.opponent);

            treeSizeKeyToCount = state.treeSizeKeyToCount;
        }

        public void AddTree(Tree tree, bool addToCell = true)
        {
            if (addToCell)
            {
                Cell cell = board[tree.cellIndex];
                cell.AddTree(tree);
            }
            
            treeCache.Add(tree);
        }

        public void RemoveTree(Cell cell)
        {
            treeCache.Remove(cell.tree);
            cell.RemoveTree();
        }

        public Tree GetTree(int cellIndex)
        {
            return board[cellIndex].tree;
        }

        public void UpdateGameState(bool applySun = false)
        {
            treeSizeKeyToCount = null;

            sunDirection = day % sunReset;
            shadowDirection = sunDirection + halfSunReset % sunReset;

            //Console.Error.WriteLine($"sundirection: {sunDirection} day: {day}");

            GetCacheTreeSize();

            if (applySun)
            {
                CalculateShadows();
                CalculateSunGeneration(true);
            }

            CalculateMoves();

            //CalculatePossibleMovesSlim(true);
            //CalculateAllLegalMoves(false);
        }

        private void CalculateMoves()
        {
            me.possibleMoves.Clear();
            opponent.possibleMoves.Clear();

            if (day == 24)
            {
                return;
            }

            if (me.isWaiting)
            {
                me.possibleMoves.Add(new Move(Actions.WAIT));
            }

            opponent.possibleMoves.Add(new Move(Actions.WAIT));
            

            int costToSeedMe = GetCostToSeed(true);
            int size3TreeToCut = 4;
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

            bool canCutMe = me.sun >= treeCompleteCost && (day > 20 || GetNumberOfTrees(true, (int)TreeSize.Large) > size3TreeToCut || me.score < opponent.score);
            bool canSeedMe = me.sun >= costToSeedMe && costToSeedMe < 1;//only seed when cost is 0

            int costToSeedOpp = GetCostToSeed(false);

            bool canSeed = opponent.sun >= costToSeedOpp && costToSeedOpp < 1;//only seed when cost is 0
            bool canCut = opponent.sun >= treeCompleteCost;

            foreach (Tree tree in TreeEnumeration)
            {

                if (tree.isDormant)
                {
                    continue;
                }

                if (tree.isMine && !me.isWaiting)
                {
                    if (canSeedMe && tree.size > 1)//do not seed with size 1 trees
                    {
                        Cell cell = board[tree.cellIndex];
                        for (int i = 0; i < sunReset; i++)
                        {
                            Cell current = cell;

                            for (int tSize = 0; tSize < tree.size; tSize++)
                            {
                                int index = current.GetCellNeighbor(i);
                                if (index == -1)
                                {
                                    break;
                                }
                                current = board[index];

                                //Remove all seeds that are in straight lines
                                //AddSeedAction(player, current, cell);


                                if (tree.size > 1)
                                {
                                    Cell tempCurrent = current;

                                    for (int tempTSize = tSize + 1; tempTSize < tree.size; tempTSize++)
                                    {
                                        int cellIndex = tempCurrent.GetCellNeighbor((i + 1) % sunReset);
                                        if (cellIndex == -1)
                                        {
                                            break;
                                        }
                                        tempCurrent = board[cellIndex];
                                        AddSeedAction(me, tempCurrent, cell);
                                    }
                                }
                            }
                        }
                    }

                    //Complete Actions
                    if (canCutMe && tree.size == maxTreeSize)
                    {
                        me.possibleMoves.Add(new Move(Actions.COMPLETE, tree.cellIndex));
                    }

                    //Grow Actions
                    if (tree.size != maxTreeSize && me.sun >= GetCostToGrow(tree))
                    {
                        me.possibleMoves.Add(new Move(Actions.GROW, tree.cellIndex));
                    }
                }
                else if(!opponent.isWaiting)
                {
                    //Seed Actions
                    if (canSeed && tree.size > 0)
                    {
                        Cell cell = board[tree.cellIndex];
                        for (int i = 0; i < sunReset; i++)
                        {
                            Cell current = cell;

                            for (int tSize = 0; tSize < tree.size; tSize++)
                            {
                                int index = current.GetCellNeighbor(i);
                                if (index == -1)
                                {
                                    break;
                                }
                                current = board[index];

                                AddSeedAction(opponent, current, cell);

                                Cell tempCurrent = current;
                                for (int tempTSize = tSize + 1; tempTSize < tree.size; tempTSize++)
                                {
                                    int cellIndex = tempCurrent.GetCellNeighbor((i + 1) % sunReset);
                                    if (cellIndex == -1)
                                    {
                                        break;
                                    }
                                    tempCurrent = board[cellIndex];
                                    AddSeedAction(opponent, tempCurrent, cell);
                                }
                            }
                        }
                    }

                    //Complete Actions
                    if (canCut && tree.size == maxTreeSize)
                    {
                        opponent.possibleMoves.Add(new Move(Actions.COMPLETE, tree.cellIndex));
                    }

                    //Grow Actions
                    if (tree.size != maxTreeSize && opponent.sun >= GetCostToGrow(tree))
                    {
                        opponent.possibleMoves.Add(new Move(Actions.GROW, tree.cellIndex));
                    }
                }
            }

            if ((me.possibleMoves.Count == 0 || me.sun < 8) && !me.isWaiting)
            {
                me.possibleMoves.Add(new Move(Actions.WAIT));
            }
        }

        private void CalculateAllLegalMoves(bool isMe)
        {
            Player player = isMe ? me : opponent;
            player.possibleMoves.Clear();

            if (day == 24)
            {
                return;
            }

            player.possibleMoves.Add(new Move(Actions.WAIT));

            if (player.isWaiting)
            {
                return;
            }

            int costToSeed = GetCostToSeed(isMe);

            bool canSeed = player.sun >= costToSeed && costToSeed < 1;//only seed when cost is 0
            bool canCut = player.sun >= treeCompleteCost;

            //IEnumerable<Tree> activeTrees = isMe ? myActiveTrees : opponentActiveTrees;

            foreach (Tree tree in TreeEnumeration)
            {
                if (tree.isMine != isMe || tree.isDormant)
                    continue;
                //Seed Actions
                if(canSeed && tree.size > 0)
                {
                    Cell cell = board[tree.cellIndex];
                    for (int i = 0; i < sunReset; i++)
                    {
                        Cell current = cell;

                        for (int tSize = 0; tSize < tree.size; tSize++)
                        {
                            int index = current.GetCellNeighbor(i);
                            if (index == -1)
                            {
                                break;
                            }
                            current = board[index];

                            AddSeedAction(player, current, cell);

                            Cell tempCurrent = current;
                            for (int tempTSize = tSize + 1; tempTSize < tree.size; tempTSize++)
                            {
                                int cellIndex = tempCurrent.GetCellNeighbor((i + 1) % sunReset);
                                if (cellIndex == -1)
                                {
                                    break;
                                }
                                tempCurrent = board[cellIndex];
                                AddSeedAction(player, tempCurrent, cell);
                            }
                        }
                    }
                }
                
                //Complete Actions
                if (canCut && tree.size == maxTreeSize)
                {
                    player.possibleMoves.Add(new Move(Actions.COMPLETE, tree.cellIndex));
                }

                //Grow Actions
                if (tree.size != maxTreeSize && player.sun >= GetCostToGrow(tree))
                {
                    player.possibleMoves.Add(new Move(Actions.GROW, tree.cellIndex));
                }
            }
        }

        private void CalculatePossibleMovesSlim(bool isMe)
        {
            Player player = isMe ? me : opponent;
            Player playerOpp = isMe ? opponent : me;
            player.possibleMoves.Clear();

            if (day == 24)
            {
                return;
            }

            if (player.isWaiting)
            {
                player.possibleMoves.Add(new Move(Actions.WAIT));
                return;
            }

            int costToSeed = GetCostToSeed(isMe);
            int size3TreeToCut = 4;
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

            bool canCut = player.sun >= treeCompleteCost && (day > 20 || GetNumberOfTrees(isMe, (int)TreeSize.Large) > size3TreeToCut || player.score < playerOpp.score);
            bool canSeed = player.sun >= costToSeed && costToSeed < 1;//only seed when cost is 0

            foreach (Tree tree in TreeEnumeration)
            {
                if (tree.isMine != isMe || tree.isDormant)
                {
                    continue;
                }
                if (canSeed && tree.size > 1)//do not seed with size 1 trees
                {
                    Cell cell = board[tree.cellIndex];
                    for (int i = 0; i < sunReset; i++)
                    {
                        Cell current = cell;

                        for (int tSize = 0; tSize < tree.size; tSize++)
                        {
                            int index = current.GetCellNeighbor(i);
                            if (index == -1)
                            {
                                break;
                            }
                            current = board[index];

                            //Remove all seeds that are in straight lines
                            //AddSeedAction(player, current, cell);


                            if (tree.size > 1)
                            {
                                Cell tempCurrent = current;

                                for (int tempTSize = tSize + 1; tempTSize < tree.size; tempTSize++)
                                {
                                    int cellIndex = tempCurrent.GetCellNeighbor((i + 1) % sunReset);
                                    if (cellIndex == -1)
                                    {
                                        break;
                                    }
                                    tempCurrent = board[cellIndex];
                                    AddSeedAction(player, tempCurrent, cell);
                                }
                            }
                        }
                    }
                }

                //Complete Actions
                if (canCut && tree.size == maxTreeSize)
                {
                    player.possibleMoves.Add(new Move(Actions.COMPLETE, tree.cellIndex));
                }

                //Grow Actions
                if (tree.size != maxTreeSize && player.sun >= GetCostToGrow(tree))
                {
                    player.possibleMoves.Add(new Move(Actions.GROW, tree.cellIndex));
                }
            }

            if (player.possibleMoves.Count == 0 || player.sun < 8)
            {
                player.possibleMoves.Add(new Move(Actions.WAIT));
            }
        }

        private void AddSeedAction(Player player, Cell currentTargetCell, Cell sourceCell)
        {
            if (!currentTargetCell.HasTree && currentTargetCell.richness != (int)Richness.Unusable)
            {               
                player.possibleMoves.Add(new Move(Actions.SEED, sourceCell.index, currentTargetCell.index));
            }
        }

        /// <summary>
        /// Calculates the shadows on each cell and spookiness of each tree
        /// </summary>
        private void CalculateShadows()
        {
            //Calculate the shadow size of each cell
            foreach (Tree tree in TreeEnumeration)
            {
                Cell cell = board[tree.cellIndex];
                
                Cell current = cell;
                for (int i = 0; i < cell.tree.size; i++)
                {
                    int index = current.GetCellNeighbor(sunDirection);
                    if (index == -1)
                    {
                        break;
                    }
                    current = board[index];
                    
                    current.SetShadowSize(cell.tree.size);
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
                        Cell sourceCell = board[myMove.sourceCellIdx];
                        sourceCell.tree.SetDormant(true);
                        sourceCell = board[opponentMove.sourceCellIdx];
                        sourceCell.tree.SetDormant(true);
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
            Cell targetCell = board[move.targetCellIdx];
            Cell sourceCell = board[move.sourceCellIdx];
            switch (move.type)
            {
                case Actions.COMPLETE:
                    if(player.sun < treeCompleteCost)
                    {
                        throw new Exception("Not enough sun!");
                    }
                    if (targetCell.tree.isDormant)
                    {
                        throw new Exception("Tree is dormant!");
                    }
                    player.sun -= treeCompleteCost;
                    player.score += GetTreeCutScore(targetCell);
                    RemoveTree(targetCell);
                    if(updateNutrients)
                        nutrients--;
                    if(updateState)
                        UpdateGameState();
                    break;
                case Actions.GROW:
                    int growCost = GetCostToGrow(targetCell.tree);
                    if (player.sun < growCost)
                    {
                        throw new Exception("Not enough sun!");
                    }
                    if (targetCell.tree.isDormant)
                    {
                        throw new Exception("Tree is dormant!");
                    }
                    player.sun -= growCost;
                    targetCell.tree.Grow();
                    if (updateState)
                        UpdateGameState();
                    break;
                case Actions.SEED:
                    int seedCost = GetCostToSeed(player.isMe);
                    if (player.sun < seedCost)
                    {
                        throw new Exception("Not enough sun");
                    }
                    if (sourceCell.tree.isDormant)
                    {
                        throw new Exception("Tree is dormant!");
                    }
                    player.sun -= seedCost;
                    sourceCell.tree.SetDormant(true);
                    AddTree(new Tree(targetCell.index, (int)TreeSize.Seed, player.isMe, true));
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
            if (treeSizeKeyToCount == null)
            {
                List<int> myTreeCount = new List<int>(4) { 0, 0, 0, 0 };
                List<int> oppTreeCount = new List<int>(4) { 0, 0, 0, 0 };
                foreach (Tree tree in TreeEnumeration)
                {
                    for (int i = 0; i<=(int)TreeSize.Large; i++)
                    {
                        if(tree.size == i)
                        {
                            if (tree.isMine)
                            {
                                myTreeCount[i]++;
                            }
                            else
                            {
                                oppTreeCount[i]++;
                            }
                        }
                    }
                }
                treeSizeKeyToCount = new List<int>(8);
                for (int i = 0; i <= (int)TreeSize.Large; i++)
                {
                    treeSizeKeyToCount.Add(myTreeCount[i]);
                    treeSizeKeyToCount.Add(oppTreeCount[i]);
                }

            }

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
            int key = GetCacheTreeSizeKey(tree.size + 1, tree.isMine);
            return GetCacheTreeSize()[key] + treeSizeToCost[tree.size + 1];
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
            treeCache = new List<Tree>(BoardSize);
            foreach (Cell cell in board)
            {
                cell.RemoveTree();
            }
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

            if(day == gameState.day && nutrients == gameState.nutrients && board.ToList().TrueForAll(c => c.Equals(gameState.board[c.index])) && me.Equals(gameState.me) && opponent.Equals(gameState.opponent))
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
            return "n: " + nutrients + "\n" + string.Join("\n", board.Select(c => c.ToString())) + "\n" + me.ToString() + "\n" + opponent.ToString();
        }
    }
}
