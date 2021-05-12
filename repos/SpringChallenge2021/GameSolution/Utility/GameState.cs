using GameSolution.Entities;
using GameSolution.Moves;
using System;
using System.Collections.Generic;
using System.Linq;
using static GameSolution.Constants;

namespace GameSolution.Utility
{
    public class GameState
    {
        public int day;
        public int nutrients;
        public int mySun, opponentSun;
        public int myScore, opponentScore;
        public bool opponentIsWaiting;
        public List<Cell> board;
        
        
        //Calculated from the trees on the board
        public List<Tree> trees;

        //Calcualted from the day
        public int sunDirection;
        public int shadowDirection;

        public int mySunPowerGenerationToday;
        public int opponentSunPowerGenerationToday;

        public GameState()
        {
            board = new List<Cell>();
        }

        public GameState(GameState state)
        {
            day = state.day;
            nutrients = state.nutrients;
            board = new List<Cell>(state.board.Select(c => new Cell(c)));
            mySun = state.mySun;
            opponentSun = state.opponentSun;
            myScore = state.myScore;
            opponentScore = state.opponentScore;
            opponentIsWaiting = state.opponentIsWaiting;

            BuildCellNeighbors();
            UpdateGameState();
        }

        public void UpdateGameState()
        {
            trees = new List<Tree>();
            //Resets cell data that is recalulated and builds tree list
            foreach(Cell cell in board)
            {
                cell.Reset();
                if (cell.HasTree())
                {
                    trees.Add(cell.tree);
                }
            }

            sunDirection = day % sunReset;
            shadowDirection = sunDirection + sunReset/2 % sunReset;

            Console.Error.WriteLine($"sundirection: {sunDirection} day: {day}");

            CalculateShadows();
            CalculateSunGeneration();
        }

        /// <summary>
        /// Calcualtes the shadows on each cell and spookiness of each tree
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

        private void CalculateSunGeneration()
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

            Console.Error.WriteLine($"my sun gen: {mySunPowerGenerationToday}, opp sun gen: {opponentSunPowerGenerationToday}");
        }

        public void ApplyMove(Move move)
        {
            Cell targetCell = board.First(c => c.index == move.targetCellIdx);
            Cell sourceCell = board.First(c => c.index == move.sourceCellIdx);
            switch (move.type)
            {
                case Actions.COMPLETE:
                    mySun -= 4;
                    myScore += GetTreeCutScore(targetCell);
                    targetCell.RemoveTree();
                    nutrients--;
                    UpdateGameState();
                    break;
                case Actions.GROW:
                    mySun -= GetCostToGrow(targetCell);
                    targetCell.tree.Grow();
                    break;
                case Actions.SEED:
                    mySun -= GetCostToSeed();
                    targetCell.tree = new Tree(targetCell.index, 0, true, true);
                    UpdateGameState();
                    break;
                case Actions.WAIT:
                    //Should we advance day on wait if opponent is waiting?
                default:
                    break;
            }
        }

        public void AdvanceDay()
        {
            day++;
            UpdateGameState();
            //In theory we should also add sun power to mySun and opponentSun
        }

        public int GetCostToSeed()
        {
            return trees.Where(t => t.size == 0).Count();
        }

        public int GetTreeCutScore(Cell cell)
        {
            return nutrients + (cell.richness * 2 - 2);
        }

        public int GetCostToGrow(Cell cell)
        {
            return trees.Where(t => t.size == cell.tree.size + 1).Count() + (int)Math.Pow(2, cell.tree.size + 1) - 1;
        }

        public void ResetTrees()
        {
            foreach (Cell cell in board)
            {
                cell.RemoveTree();
            }
        }
    }
}
