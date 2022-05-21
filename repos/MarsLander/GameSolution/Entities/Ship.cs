using Algorithms.Space;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace GameSolution.Entities
{
    public class Ship
    {
        public Point2d Location;
        public Point2d VelocityVector;
        public int Fuel;
        public int RotationAngle;// limited +-15
        public int Power;//limited +-1

        public static Point2d GravityVector = new Point2d(0, -3.711);

        public Ship(int x, int y, int xSpeed, int ySpeed, int fuel, int rotate, int power)
        {
            Location = new Point2d(x, y);
            VelocityVector = new Point2d(xSpeed, ySpeed);
            Fuel = fuel;
            RotationAngle = rotate;
            Power = power;
        }

        public Ship(Ship ship)
        {
            Location = ship.Location.Clone();
            VelocityVector = ship.VelocityVector.Clone();
            Fuel = ship.Fuel;
            RotationAngle = ship.RotationAngle;
            Power = ship.Power;
        }

        private static Point2d directionVector = new Point2d(0, 0);
        private static Point2d speedVector = new Point2d(0, 0);
        public void AdvanceTurn()
        {
            if (Fuel < Power)
            {
                Power = Fuel;
            }

            double radians = RotationAngle * Math.PI / 180;
            directionVector.x = Math.Sin(-radians);
            directionVector.y = Math.Cos(-radians);
            directionVector.Multiply(Power).Add(GravityVector).Add(VelocityVector);

            speedVector.Fill(directionVector);
            speedVector.Add(VelocityVector).Multiply(0.5);

            VelocityVector.Fill(directionVector);
            Fuel -= Power;
            Location = Location.Add(speedVector);
        }

        public void SetPower(int newPower)
        {
            var difference = newPower - Power;
            if (difference < 0)
                Power--;
            else if(difference > 0)
                Power++;

            Power = Math.Max(Math.Min(Power, 4), 0);
        }

        public void SetRotation(int rotate)
        {
            var difference = rotate - RotationAngle;
            if(difference > 15)
            {
                difference = 15;
            }
            else if(difference < -15)
            {
                difference = -15;
            }
            RotationAngle = Math.Max(Math.Min(RotationAngle + difference, 90), -90);
        }

        public bool Equals(Ship ship)
        {
            return ship.Location.GetRoundedAwayFromZeroPoint().Equals(Location.GetRoundedAwayFromZeroPoint()) && ship.VelocityVector.GetRoundedAwayFromZeroPoint().Equals(VelocityVector.GetRoundedAwayFromZeroPoint()) && ship.Fuel == Fuel && ship.RotationAngle == RotationAngle;
        }

        public Ship Clone()
        {
            return new Ship(this);
        }

        public void Fill(Ship ship)
        {
            Location = ship.Location;
            VelocityVector = ship.VelocityVector;
            Fuel = ship.Fuel;
            RotationAngle = ship.RotationAngle;
            Power = ship.Power;
        }

        public override string ToString()
        {
            return $"l:{Location}, v:{VelocityVector}, f:{Fuel}, r:{RotationAngle}, p:{Power}";
        }
    }
}
