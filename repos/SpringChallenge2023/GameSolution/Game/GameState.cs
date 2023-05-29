using Algorithms.GameComponent;
using Algorithms.Graph;
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

        public int turn { get; set; }
        public Dictionary<int, Cell> MyBaseDictionary { get; set; }
        public Dictionary<int, Cell> OppBaseDictionary { get; set; }

        public IList<Cell> EggCells { get; set; }
        public IList<Cell> CrystalCells { get; set; }

        public int TotalMyAnts { get; set; } = 0;
        public int TotalOppAnts { get; set; } = 0;

        public Graph Graph { get; set; }

        public Move? maxMove { get; set; }
        public Move? minMove { get; set; }

        public GameState()
        {
            turn = 0;
            maxMove = null;
            minMove = null;
        }

        public GameState(GameState state)
        {
            Board = state.Board.Clone();
            turn = state.turn;
            maxMove = state.maxMove;
            minMove = state.minMove;
        }

        public void SetNextTurn(Board board)
        {
            turn++;
            this.Board = board;
            UpdateGameState();
        }

        public void UpdateGameState()
        {
            Graph = new Graph();
            MyBaseDictionary = new Dictionary<int, Cell>();
            OppBaseDictionary = new Dictionary<int, Cell>();
            EggCells = new List<Cell>();
            CrystalCells = new List<Cell>();

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
                }
                TotalMyAnts += cell.MyAnts;
                TotalOppAnts += cell.OppAnts;
                var node = new Node(cell.Index);
                Graph.AddNode(node);
                foreach (int index in cell.Neighbors)
                {
                    if (index != -1)
                    {
                        var neighborCell = Board.GetCell(index);
                        var distance = neighborCell.ResourceAmount > 0 ? 1 : 1.001;
                        node.AddLink(new Link(node, new Node(index), distance));
                        //Console.Error.WriteLine($"adding line {cell.Index}, {index}, {distance}");
                    }
                }
            }

            Graph.CalculateShortestPaths();
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
            return true;
        }

        public IList GetPossibleMoves(bool isMax)
        {
            return new List<Move>();
        }

        public double? GetWinner()
        {
            double? winner = Board.GetWinner();
            if (this.turn == 220 & !winner.HasValue)
            {
                return 0;
            }
            return winner;
        }
    }
}
