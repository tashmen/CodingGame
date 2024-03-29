﻿using Algorithms.GameComponent;
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
        public Board Board;
        public Ship Ship;

        public Move LastMove;

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
                if (move is StaticMove)
                {
                    Ship.SetRotation(m.Rotation + Ship.RotationAngle);
                    Ship.SetPower(m.Power + Ship.Power);
                }
                else
                {
                    Ship.SetRotation(m.Rotation);
                    Ship.SetPower(m.Power);
                }
                
                LastMove = m;
                Ship.AdvanceTurn();
            }
        }

        public IGameState Clone()
        {
            return new GameState(this);
        }

        public void Fill(GameState state)
        {
            Board = state.Board;
            Ship.Fill(state.Ship);
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
            var distance = Math.Max(Board.CalculatePathDistance(Ship), Ship.Location.GetDistance(midPoint));
            var vx = Math.Abs(Ship.VelocityVector.x);
            var vy = Math.Abs(Ship.VelocityVector.y);
            //var xDiff = Math.Abs(Ship.Location.x - midPoint.x);
            //var xDiff2 = midPoint.x - Ship.Location.x;
            double value = 0;
            //value += (1 - (xDiff * xDiff / 49000000.0)) * 0.5;
            //value += (1 - (yDiff * yDiff / 9000000.0)) * 0.0001;
            
            value += (1 - (distance / 7615.0)) * 0.6;

            if (landingSpot.Item1.x <= Ship.Location.x && Ship.Location.x <= landingSpot.Item2.x)
            {
                value += (1 - (Math.Abs(Ship.RotationAngle) / 90.0)) * 0.1;
                if (vx <= 20)
                    value += 0.15;
                else if (vx <= 100)
                    value += (1 - ((vx - 20) / 80.0)) * 0.15;
                if (vy <= 40)
                    value += 0.15;
                else if (vy <= 100)
                    value += (1 - ((vy - 40) / 60.0)) * 0.15;
            }
            else
            {
                if (vx <= 100)
                    value += (1 - ((vx) / 100)) * 0.1;
                if (vy <= 100)
                    value += (1 - ((vy) / 100)) * 0.1;
            }
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
            //If the ship goes outside of bounds then this should be considered a collision, but checking this first so that the MaxY functionality is always in bounds.
            if (!Board.IsInBounds(Ship))
                return Evaluate(true);

            //if the ship is higher than the maximum spot around the ship then it couldn't have possibly crashed or landed.
            var maxY = Math.Max(Board.MaxYAtX[Ship.Location.GetTruncatedX()], Board.MaxYAtX[Ship.LastLocation.GetTruncatedX()]);
            if (Ship.Location.y > (maxY + 100))
                return null;

            var shipCollision = Board.ShipCollision(Ship);
            if (shipCollision.HasValue)
            {
                if (!shipCollision.Value)
                {
                    return Ship.Fuel + 1;
                }
                else
                {
                    return Evaluate(true);
                }
            }

            return null;
        }
    }
}
