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

        public int turn { get; set; }

        public int myMatter = 0;
        public int oppMatter = 0;

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
            myMatter = state.myMatter;
            oppMatter = state.oppMatter;
        }

        public void SetNextTurn(Board board, int myMatter, int oppMatter)
        {
            turn++;
            this.Board = board;
            this.myMatter = myMatter;
            this.oppMatter = oppMatter;
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
            double? winner = Board.GetWinner();
            if (this.turn == 220 & !winner.HasValue)
            {
                return 0;
            }
            return winner;
        }
    }
}
