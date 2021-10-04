using Algorithms;
using GameSolution.Entities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GameSolution.Game
{
    public class GameState : IGameState
    {
        public Board[] Boards;
        public Move LastPlayed;

        public Board FinalBoard;

        public GameState()
        {
            Boards = new Board[9];
            for(int i = 0; i<9; i++)
            {
                Boards[i] = new Board(i);
            }

            FinalBoard = new Board(-1);
        }

        public GameState(GameState state)
        {
            Boards = new Board[9];
            for (int i = 0; i < 9; i++)
            {
                Boards[i] = state.Boards[i].Clone();
            }
            LastPlayed = state.LastPlayed;
            FinalBoard = state.FinalBoard.Clone();
        }

        public void ApplyMove(object move, bool isMax)
        {
            LastPlayed = move as Move;
            Boards[LastPlayed.BoardNumber].ApplyMove(LastPlayed, isMax);
        }

        public IGameState Clone()
        {
            return new GameState(this);
        }

        public bool Equals(IGameState state)
        {
            GameState theState = state as GameState;
            for(int i = 0; i<9; i++)
            {
                if (!theState.Boards[i].Equals(Boards[i]))
                {
                    return false;
                }
            }

            return true;
        }

        public double Evaluate(bool isMax)
        {
            throw new NotImplementedException();
        }

        public object GetMove(bool isMax)
        {
            return LastPlayed;
        }

        public IList GetPossibleMoves(bool isMax)
        {
            if(LastPlayed == null)
            {
                return GetAllMoves();
            }
            else
            {
                int index = Board.ConvertRowColToIndex(LastPlayed.Row, LastPlayed.Col);
                Board board = Boards[index];
                if(board.GetWinner().HasValue)
                {
                    return GetAllMoves();
                }
                else
                {
                    return board.GetEmptySpaces();
                }
            }
        }

        public IList GetAllMoves()
        {
            IList moves = new List<Move>();
            for (int i = 0; i < 9; i++)
            {
                if (!Boards[i].GetWinner().HasValue)
                {
                    foreach (Move move in Boards[i].GetEmptySpaces())
                    {
                        moves.Add(move);
                    }
                }
            }

            return moves;
        }

        public double? GetWinner()
        {
            IList emptySpaces = FinalBoard.GetEmptySpaces();
            foreach(Move move in emptySpaces)
            {
                int index = Board.ConvertRowColToIndex(move.Row, move.Col);
                double? winner = Boards[index].GetWinner();
                if (winner.HasValue)
                {
                    if (winner == 1)
                    {
                        FinalBoard.ApplyMove(move, true);
                    }
                    else if(winner == -1)
                    {
                        FinalBoard.ApplyMove(move, false);
                    }
                    else
                    {
                        FinalBoard.ApplyDraw(move);
                    }
                }
            }

            return FinalBoard.GetWinner();// * (81 - TurnCount);
        }

        public override string ToString()
        {
            StringBuilder stringBuilder = new StringBuilder();
            List<string> holder;

            for (int r = 0; r < 3; r++)
            {
                holder = null;
                int row = r * 3;
                for (int c = 0; c < 3; c++)
                {
                    int index = row + c;
                    string[] temp = Boards[index].ToString().Split(new[] { Environment.NewLine }, StringSplitOptions.None);
                    if(holder == null)
                    {
                        holder = temp.ToList();
                    }
                    else
                    {
                        for(int i = 0; i< holder.Count; i++)
                        {
                            string line = holder[i];
                            if(i == 3)
                            {
                                holder[i] = "==============";
                            }
                            else 
                                holder[i] = line + " | " + temp[i];
                        }
                    }
                    
                }

                foreach (string line in holder)
                {
                    stringBuilder.Append(line + Environment.NewLine);
                }
            }

            stringBuilder.Append(LastPlayed == null ? "" : LastPlayed.ToString());
            return stringBuilder.ToString();
        }
    }
}
