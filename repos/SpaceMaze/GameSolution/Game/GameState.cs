using Algorithms.GameComponent;
using Algorithms.Space;
using GameSolution.Entities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GameSolution
{
    public class GameState : IGameState
    {
        public Board Board;
        public List<Entity> Entities;
        public int Turn = 0;

        public Move LastMove;

        public GameState(Board board)
        {
            Board = board;
        }

        public GameState(GameState state)
        {
            Board = state.Board;
            Entities = state.Entities.Select(e => e.Clone()).ToList();
            Turn = state.Turn;
            LastMove = state.LastMove;
        }

        public void SetState(List<Entity> entities)
        {
            Entities = entities;
        }

        public void ApplyMove(object move, bool isMax)
        {
            if (isMax)
            {
                Move m = (Move)move;
                Entity entity = Entities.Where(e => e.Id == m.Id).First();
                entity.ApplyDirection(m.Direction);
                Turn++;
                LastMove = m;
            }
        }

        public IGameState Clone()
        {
            return new GameState(this);
        }

        public bool Equals(IGameState state)
        {
            GameState game = (GameState)state;
            return false;
        }

        public double Evaluate(bool isMax)
        {
            return 0;
        }

        public object GetMove(bool isMax)
        {
            if(isMax)
                return LastMove;
            return null;
        }

        public IList GetPossibleMoves(bool isMax)
        {
            IList possibleMoves = new List<Move>();

            if (isMax)
            {
                foreach (Entity e in Entities)
                {
                    foreach (char direction in Board.GetLegalDirections(e))
                    {
                        possibleMoves.Add(new Move(e.Id, direction));
                    }
                }
            }
            else
            {
                possibleMoves.Add(null);
            }

            return possibleMoves;
        }

        public Entity GetCar()
        {
            return Entities.Where(e => e.IsCar).FirstOrDefault();
        }

        public double? GetWinner()
        {
            if(Turn == 200)
            {
                return -1;
            }
            var car = GetCar();
            if (car.Point.Equals(Board.Target))
                return 1;

            return null;
        }
    }
}
