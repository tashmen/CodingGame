using Algorithms.GameComponent;
using Algorithms.Space;
using GameSolution.Entities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace GameSolution
{
    public class GameState : IGameState
    {
        public Board Board { get; set; }
        public Ship Ship { get; set; }

        public Move LastMove { get; set; }

        public GameState(Board board)
        {
            Board = board;
        }

        public GameState(GameState state)
        {
            Board = state.Board;
            Ship = state.Ship.Clone();
        }

        public void SetShip(Ship ship)
        {
            Ship = ship;
        }

        public void ApplyMove(object move, bool isMax)
        {
            if (isMax)
            {
                Move m = (Move)move;
                LastMove = m;
                Ship.SetRotation(m.Rotation);
                Ship.SetPower(m.Power);
                Ship.AdvanceTurn();
            }
        }

        public IGameState Clone()
        {
            return new GameState(this);
        }

        public bool Equals(IGameState state)
        {
            GameState game = (GameState)state;
            return Ship.Equals(game.Ship);
        }

        public double Evaluate(bool isMax)
        {
            var landingSpot = Board.GetLandingSpot();
            var midPoint = landingSpot.Item1.GetMidPoint(landingSpot.Item2);
            var distance = Ship.Location.GetDistance(midPoint);


            var vx = Ship.VelocityVector.x;
            var vy = Ship.VelocityVector.y;

            var xDiff = Math.Abs(Ship.Location.x - midPoint.x);
            var xDiff2 = midPoint.x - Ship.Location.x;

            double value = 0;
            value += (1 - (xDiff * xDiff / 49000000.0)) * 0.5;
            //value += (1 - (yDiff * yDiff / 9000000.0)) * 0.0001;
            value += (1 - (Math.Abs(Ship.RotationAngle) / 90.0)) * 0.0001;
            value += (1 - (distance / 7615.0)) * 0.005;
            value += (1 - (vx * vx) / 40000.0) * 0.05;
            value += (1 - (vy * vy) / 40000.0) * 0.05;

            //Console.Error.WriteLine(value);

            return value; 
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
                for (int r = Math.Max(Ship.RotationAngle - 15, -90); r <= Math.Min(Ship.RotationAngle + 15, 90); r+=5)
                {
                    for (int p = Math.Max(Ship.Power - 1, 0); p <= Math.Min(Ship.Power + 1, 4); p++)
                    {
                        possibleMoves.Add(new Move(r, p));
                    }
                }
            }
            else
            {
                possibleMoves.Add(new Move(0, 0));
            }

            return possibleMoves;
        }

        public double? GetWinner()
        {
            if (Board.ShipLanded(Ship))
            {
                return Ship.Fuel;
            }
            else if (Board.ShipCollision(Ship))
            {
                return -1 + Evaluate(true);
            }

            return null;
        }
    }
}
