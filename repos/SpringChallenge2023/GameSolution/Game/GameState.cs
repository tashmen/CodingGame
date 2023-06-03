using Algorithms.GameComponent;
using GameSolution.Entities;
using System;
using System.Collections;
using System.Collections.Generic;

namespace GameSolution.Game
{
    public class GameState : IGameState
    {
        public bool enableLogging = false;
        public Board Board { get; set; }

        public int Turn { get; set; }
        public Dictionary<int, Cell> MyBaseDictionary { get; set; }
        public Dictionary<int, Cell> OppBaseDictionary { get; set; }

        public IList<Cell> EggCells { get; set; }
        public IList<Cell> CrystalCells { get; set; }

        public int TotalMyAnts { get; set; } = 0;
        public int TotalOppAnts { get; set; } = 0;
        public int TotalCrystals { get; set; } = 0;
        public int MyScore { get; set; } = 0;
        public int OppScore { get; set; } = 0;
        

        public Move? maxMove { get; set; }
        public Move? minMove { get; set; }

        public GameState()
        {
            Turn = 0;
            maxMove = null;
            minMove = null;
        }

        public GameState(GameState state)
        {
            Board = state.Board.Clone();
            Turn = state.Turn;
            maxMove = state.maxMove;
            minMove = state.minMove;
        }

        public void SetNextTurn(Board board, int myScore, int oppScore)
        {
            Turn++;
            this.Board = board;
            MyScore = myScore;
            OppScore= oppScore;
            UpdateGameState();
        }

        public void UpdateGameState()
        {
            MyBaseDictionary = new Dictionary<int, Cell>();
            OppBaseDictionary = new Dictionary<int, Cell>();
            EggCells = new List<Cell>();
            CrystalCells = new List<Cell>();

            TotalMyAnts = 0;
            TotalOppAnts = 0;
            TotalCrystals = 0;

            foreach (Cell cell in Board.Cells.Values)
            {
                if (cell.BaseType == BaseType.MyBase)
                {
                    MyBaseDictionary[cell.Index] = cell;
                }
                else if (cell.BaseType == BaseType.OppBase)
                {
                    OppBaseDictionary[cell.Index] = cell;
                }
                if (cell.ResourceType == ResourceType.Egg)
                {
                    EggCells.Add(cell);
                }
                else if (cell.ResourceType == ResourceType.Crystal)
                {
                    CrystalCells.Add(cell);
                    TotalCrystals += cell.ResourceAmount;
                }
                TotalMyAnts += cell.MyAnts;
                TotalOppAnts += cell.OppAnts;
            }
        }

        public void ApplyMove(object move, bool isMax)
        {
            Move m = (Move)move;
            if (isMax)
            {
                maxMove = m;
                minMove = null;
            }
            else
            {
                if (maxMove == null)
                    throw new Exception("Expected max to play first.");
                minMove = m;
            }

            if(maxMove != null && minMove != null)
            {
                //board.ApplyMove(maxMove, true);
                //board.ApplyMove(minMove, false);
                //SetNextTurn(board);
            }
        }

        public IGameState Clone()
        {
            return new GameState(this);
        }

        public bool Equals(IGameState state)
        {
            throw new NotImplementedException();
        }

        public double Evaluate(bool isMax)
        {
            double value = 0;

            return value;
        }

        public object GetMove(bool isMax)
        {
            return isMax ? maxMove : minMove;
        }

        public IList GetPossibleMoves(bool isMax)
        {
            return new List<Move>();
        }

        public double? GetWinner()
        {
            double? winner = null;
            if(TotalCrystals == 0)
            {
                if(MyScore > OppScore)
                {
                    return 1;
                }
                else if(OppScore > MyScore)
                {
                    return -1;
                }
                else if(OppScore == MyScore)
                {
                    if (TotalMyAnts > TotalOppAnts)
                    {
                        return 1;
                    }
                    else if (TotalOppAnts > TotalMyAnts)
                    {
                        return -1;
                    }
                    else return 0;
                }
            }
            if (Turn == 100 & !winner.HasValue)
            {
                return 0;
            }
            return winner;
        }
    }
}
