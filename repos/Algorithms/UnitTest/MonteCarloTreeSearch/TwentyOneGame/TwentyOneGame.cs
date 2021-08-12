﻿using Algorithm;
using System;
using System.Collections.Generic;

namespace UnitTest.TwentyOneGame
{
    /// <summary>
    /// In this game players are trying to reach the value 21 the count starts at 0 and each turn a player can pick a number between 1-3 to add to the total.
    /// </summary>
    public class GameState : IGameState
    {
        public int Total;
        Move LastMove;
        bool? LastMoveBy;

        public GameState()
        {
            Total = 0;
        }
        public void ApplyMove(IMove move, bool isMax)
        {
            Move m = move as Move;
            if (m.Count > 3 || m.Count <= 0)
                throw new Exception("Invalid move: " + move.ToString());
            Total += m.Count;
            LastMove = m;
            LastMoveBy = isMax;
        }

        public IGameState Clone()
        {
            GameState state = new GameState();
            state.Total = Total;
            return state;
        }

        public bool Equals(IGameState state)
        {
            GameState s = state as GameState;
            return Total == s.Total;
        }

        public IMove GetMove(bool isMax)
        {
            return LastMove;
        }

        public IList<IMove> GetPossibleMoves(bool isMax)
        {
            if (GetWinner().HasValue)
            {
                return new List<IMove>();
            }
            return new List<IMove>()
            {
                new Move(1),
                new Move(2),
                new Move(3)
            };
        }

        public int? GetWinner()
        {
            if (Total >= 21)
            {
                if (Total == 21)
                {
                    if (LastMoveBy.HasValue)
                    {
                        if (LastMoveBy.Value)
                            return 1;
                        else return -1;
                    }
                    return 0;
                }
                else if (LastMoveBy.HasValue)
                {
                    if (LastMoveBy.Value)
                        return -1;
                    else return 1;
                }

            }
            return null;
        }

        public override string ToString()
        {
            return "Total: " + Total;
        }
    }

    public class Move : IMove
    {
        public int Count;

        public Move(int count)
        {
            Count = count;
        }

        public override string ToString()
        {
            return "Move: " + Count.ToString();
        }
    }
}