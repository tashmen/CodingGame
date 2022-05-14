using Algorithms.Space;
using System;
using System.Collections.Generic;
using System.Text;

namespace GameSolution.Entities
{
    public class Board
    {
        public IList<Point2d> Points { get; set; }
        public Tuple<Point2d, Point2d> LandingSpot { get; set; }

        public double MaxY { get; set; }

        public Board(IList<Point2d> points)
        {
            Points = points;
            MaxY = 0;
        }

        public Tuple<Point2d, Point2d> GetLandingSpot()
        {
            if(LandingSpot == null)
            {
                Point2d lastPoint = null;
                foreach (var point in Points)
                {
                    MaxY = Math.Max(MaxY, point.y);
                    if (lastPoint == null)
                        lastPoint = point;
                    else
                    {
                        var currentPoint = point;

                        var vector = Space2d.CreateVector(lastPoint, currentPoint);
                        if (vector.y == 0)
                            LandingSpot =  new Tuple<Point2d, Point2d>(lastPoint, currentPoint);

                        lastPoint = currentPoint;
                    }
                }
            }

            return LandingSpot;
        }

        public bool ShipLanded(Ship ship)
        {
            var landingSpot = GetLandingSpot();
            if (landingSpot.Item1.x <= ship.Location.x && ship.Location.x <= landingSpot.Item2.x)
            {
                if (ship.Location.y <= landingSpot.Item1.y)
                {
                    if (Math.Abs(ship.VelocityVector.x) <= 20 && Math.Abs(ship.VelocityVector.y) <= 40 && ship.RotationAngle == 0)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public bool ShipCollision(Ship ship)
        {
            if (ship.Location.x <= 0 || ship.Location.x >= 6900)
                return true;
            if (ship.Location.y <= 0 || ship.Location.y >= 2900)
                return true;

            Point2d lastPoint = null;
            foreach (var point in Points)
            {
                if (lastPoint == null)
                    lastPoint = point;
                else
                {
                    var currentPoint = point;
                    if (lastPoint.x <= ship.Location.x && ship.Location.x <= currentPoint.x)
                    {
                        Point2d v1 = Space2d.CreateVector(lastPoint, currentPoint);
                        Point2d v2 = Space2d.CreateVector(ship.Location, currentPoint);
                        double xp = v1.x * v2.y - v1.y * v2.x;
                        if (xp > 0)
                            return true;
                        else return false;
                    }
                    lastPoint = currentPoint;
                }
            }
            return false;
        }
    }
}
