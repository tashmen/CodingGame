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

        public bool isCopy = false;
        
        //Calculated from the trees on the board
        public List<Tree> trees
        { get
            {
                if(treeCache == null)
                {
                    treeCache = new List<Tree>();
                    foreach (Cell cell in board)
                    {
                        cell.Reset();
                        if (cell.HasTree())
                        {
                            trees.Add(cell.tree);
                        }
                    }
                }
                return treeCache;
            }
        }
        private List<Tree> treeCache;

        //Calcualted from the day
        public int sunDirection;
        public int shadowDirection;

        public int mySunPowerGenerationToday;
        public int opponentSunPowerGenerationToday;

        //lazy loaded cache
        private Dictionary<string, int> treeSizeKeyToCount;

        public GameState()
        {
            board = new List<Cell>();
            me = new Player(true);
            opponent = new Player(false);
        }

        public GameState(GameState state)
        {
            isCopy = true;
            day = state.day;
            nutrients = state.nutrients;
            board = new List<Cell>(state.board.Select(c => new Cell(c)));
            me = new Player(state.me);
            opponent = new Player(state.opponent);
            treeSizeKeyToCount = null;

            if (me.movePlayed != null && opponent.movePlayed != null)
            {
                me.movePlayed = null;
                opponent.movePlayed = null;
            }

            BuildCellNeighbors();
            UpdateGameState();
        }

        public void UpdateGameState(bool updateMyMoves = true, bool applySun = false)
        {
            treeSizeKeyToCount = null;
            treeCache = null;

            sunDirection = day % sunReset;
            shadowDirection = sunDirection + sunReset/2 % sunReset;

            //Console.Error.WriteLine($"sundirection: {sunDirection} day: {day}");

            CalculateShadows();
            CalculateSunGeneration(applySun);

            if(updateMyMoves)
            {
                CalculatePossibleMoves(true);
            }
            
            CalculatePossibleMoves(false);
            
        }

        private void CalculatePossibleMoves(bool isMe)
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

            //Seed Actions
            if(player.sun >= GetCostToSeed(isMe))
            {
                foreach(Cell cell in board.Where(c => c.HasTree() && c.tree.size > 0 && c.tree.isMine == isMe && !c.tree.isDormant))
                {
                    for (int i = 0; i < sunReset; i++)
                    {
                        Cell current = cell;
                        for (int tSize = 0; tSize < cell.tree.size; tSize++)
                        {
                            current = current.GetCellNeighbor(i);
                            if (current == null)
                            {
                                break;
                            }
                            AddSeedAction(player, current, cell);
                            

                            if(cell.tree.size > 1)
                            {
                                Cell tempCurrent = current;
                                for(int tempTSize = tSize+1; tempTSize < cell.tree.size; tempTSize++)
                                {
                                    tempCurrent = tempCurrent.GetCellNeighbor((i+1) % sunReset);
                                    if (tempCurrent == null)
                                    {
                                        break;
                                    }
                                    AddSeedAction(player, tempCurrent, cell);
                                }
                            }
                        }
                    }
                }
            }

            //Complete Actions
            if(player.sun >= treeCompleteCost)
            {
                foreach(Tree tree in trees.Where(t => t.isMine == isMe && t.size == maxTreeSize && !t.isDormant))
                {
                    player.possibleMoves.Add(new Move(Actions.COMPLETE, tree.cellIndex));   
                }
            }

            //Grow Actions
            foreach(Tree tree in trees.Where(t => t.isMine == isMe && t.size != maxTreeSize && !t.isDormant))
            {
                //Console.Error.WriteLine($"{tree.ToString()} cost: {GetCostToGrow(tree)} sun: {player.sun}");
                if (player.sun >= GetCostToGrow(tree))
                {
                    player.possibleMoves.Add(new Move(Actions.GROW, tree.cellIndex));
                }
            }
        }

        private void AddSeedAction(Player player, Cell currentTargetCell, Cell sourceCell)
        {
            Cell current = currentTargetCell;
            if (current != null && !current.HasTree() && current.richness != (int)Richness.Unusable)
            {
                /*
                if (!isCopy)
                {
                    //Console.Error.WriteLine($"seed index: {current.index} player: {sourceCell.tree.isMine}");
                }
                */

                
                if (!sourceCell.HasTree())
                {
                    Console.Error.WriteLine($"Source Cell with no tree! {sourceCell}");
                    Console.Error.WriteLine($"CurrentTarget {currentTargetCell}");
                    Console.Error.WriteLine($"player: {player}");
                }
                
                player.possibleMoves.Add(new Move(Actions.SEED, sourceCell.index, current.index));
            }
        }

        /// <summary>
        /// Calculates the shadows on each cell and spookiness of each tree
        /// </summary>
        private void CalculateShadows()
        {
            //Calculate the shadow size of each cell
            foreach (Cell cell in board)
            {
                if (cell.HasTree())
                {
                    Cell current = cell;
                    for (int i = 0; i < cell.tree.size; i++)
                    {
                        current = current.GetCellNeighbor(sunDirection);
                        if(current == null)
                        {
                            break;
                        }
                        current.SetShadowSize(cell.tree.size);
                    }
                }
            }

            //Calculate the spookiness of each tree
            foreach (Cell cell in board)
            {
                if (cell.HasTree())
                {
                    if (cell.tree.size <= cell.shadowSize)
                    {
                        cell.tree.isSpookyShadow = true;
                    }
                    else
                    {
                        cell.tree.isSpookyShadow = false;
                    }
                }
                //Console.Error.WriteLine(cell);
            }
        }

        public void BuildCellNeighbors()
        {
            foreach (Cell cell in board)
            {
                Dictionary<int, Cell> sunDirectionToCell = new Dictionary<int, Cell>();
                for (int i = 0; i < cell.neighbours.Count; i++)
                {
                    Cell c = board.FirstOrDefault(c => c.index == cell.neighbours[i]);
                    sunDirectionToCell[i] = c;
                }
                cell.SetCellNeighbors(sunDirectionToCell);
            }
        }

        private void CalculateSunGeneration(bool apply)
        {
            mySunPowerGenerationToday = 0;
            opponentSunPowerGenerationToday = 0;
            foreach(Tree tree in trees)
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
            }

            //Console.Error.WriteLine($"my sun gen: {mySunPowerGenerationToday}, opp sun gen: {opponentSunPowerGenerationToday}");
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
                        Cell sourceCell = board.First(c => c.index == myMove.sourceCellIdx);
                        sourceCell.tree.isDormant = true;
                        sourceCell = board.First(c => c.index == opponentMove.sourceCellIdx);
                        sourceCell.tree.isDormant = true;
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
            me.movePlayed = null;
            opponent.movePlayed = null;
        }

        /// <summary>
        /// Apply a move for a single player
        /// </summary>
        /// <param name="move">The move to play</param>
        /// <param name="player">The player who made the move</param>
        public void ApplyMove(Move move, Player player, bool updateState = true, bool updateNutrients = true)
        {
            Cell targetCell = board.First(c => c.index == move.targetCellIdx);
            Cell sourceCell = board.First(c => c.index == move.sourceCellIdx);
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
                    targetCell.RemoveTree();
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
                    sourceCell.tree.isDormant = true;
                    targetCell.tree = new Tree(targetCell.index, (int)TreeSize.Seed, player.isMe, true);
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
            UpdateGameState(true, true);
        }

        

        public int GetTreeCutScore(Cell cell)
        {
            return nutrients + (cell.richness * 2 - 2);
        }

        
        private Dictionary<string, int> GetCacheTreeSize()
        {
            if (treeSizeKeyToCount == null)
            {
                treeSizeKeyToCount = new Dictionary<string, int>();
                for(int i = 0; i<=(int)TreeSize.Large; i++)
                {
                    treeSizeKeyToCount[GetCacheTreeSizeKey(i, true)] = trees.Where(t => t.size == i && t.isMine == true).Count();
                    treeSizeKeyToCount[GetCacheTreeSizeKey(i, false)] = trees.Where(t => t.size == i && t.isMine == false).Count();
                }
            }

            return treeSizeKeyToCount;
        }

        private string GetCacheTreeSizeKey(int size, bool isMe)
        {
            return $"{size}|{isMe}";
        }
        public int GetCostToSeed(bool isMe = true)
        {
            string key = GetCacheTreeSizeKey((int)TreeSize.Seed, isMe);
            return GetCacheTreeSize()[key];
        }

        private Dictionary<int, int> treeSizeToCost = new Dictionary<int, int>()
        {
            {1,1 },
            {2,3 },
            {3,7 }
        };
        public int GetCostToGrow(Tree tree)
        {
            string key = GetCacheTreeSizeKey(tree.size + 1, tree.isMine);
            return GetCacheTreeSize()[key] + treeSizeToCost[tree.size + 1];
        }

        public void ResetPlayers()
        {
            me.Reset();
            opponent.Reset();
        }

        public void ResetTrees()
        {
            foreach(Tree tree in trees)
            {
                tree.Reset();
            }
        }

        public void RemoveTrees()
        {
            treeCache = null;
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
            if (isMax && opponent.movePlayed != null)
            {
                throw new Exception("Expected opponent's move to be empty");
            }

            Player player = isMax ? me : opponent;
            Move movePlayer = move as Move;
            player.movePlayed = movePlayer;

            if(me.movePlayed != null && opponent.movePlayed != null)
            {
                ApplyMoves(me.movePlayed, opponent.movePlayed);
            }
        }

        public IMove GetMove(bool isMax)
        {
            Player player = isMax ? me : opponent;
            return player.movePlayed;
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
                    int countMyTrees = trees.Where(t => t.isMine).Count();
                    int countOppTrees = trees.Where(t => !t.isMine).Count();

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

            if(day == gameState.day && nutrients == gameState.nutrients && board.TrueForAll(c => c.Equals(gameState.board.First(cell => cell.index == c.index))) && me.Equals(gameState.me) && opponent.Equals(gameState.opponent))
            {
                return true;
            }
            return false;
        }

        public double Evaluate(bool isMax)
        {
            throw new NotImplementedException();
        }

        public override string ToString()
        {
            return "n: " + nutrients + "\n" + board.ToString() + "\n" + me.ToString() + "\n" + opponent.ToString();
        }
    }
}
