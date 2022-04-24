using Algorithms;
using System;
using System.Collections;
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
        public void ApplyMove(object move, bool isMax)
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
            return Total == s.Total && LastMoveBy == s.LastMoveBy && LastMove.Equals(LastMove);
        }

        public double Evaluate(bool isMax)
        {
            if(Total % 4 == 1)
            {
                if (isMax)
                    return -1;
                else return 1;
            }
            else
            {
                if (isMax)
                    return 1;
                else return -1;
            }
        }

        public object GetMove(bool isMax)
        {
            return LastMove;
        }

        public IList GetPossibleMoves(bool isMax)
        {
            if (GetWinner().HasValue)
            {
                return new List<Move>();
            }
            return new List<Move>()
            {
                new Move(1),
                new Move(2),
                new Move(3)
            };
        }

        public double? GetWinner()
        {
            if (Total >= 21)
            {
                if (Total >= 24)
                    throw new Exception("over 24");
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

    public class Move : object
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

        public bool Equals(Move m)
        {
            return m.Count == Count;
        }
    }
}
