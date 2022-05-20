using Algorithms.Space;
using System;
using System.Collections.Generic;
using System.Text;

namespace GameSolution.Entities
{
    public class Board
    {
        public IList<Point2d> Points;
        public Tuple<Point2d, Point2d> LandingSpot;

        public double MaxY;

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
                if (ship.Location.y <= landingSpot.Item1.y && landingSpot.Item1.y <= ship.Location.y - ship.VelocityVector.y)
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

            Point2d shipLastLocation = new Point2d(ship.Location.x - ship.VelocityVector.x, ship.Location.y - ship.VelocityVector.y);

            Point2d lastPoint = null;
            foreach (var point in Points)
            {
                if (lastPoint == null)
                    lastPoint = point;
                else
                {
                    var currentPoint = point;
                    if (IsIntersecting(lastPoint, currentPoint, ship.Location, shipLastLocation))
                        return true;
                }
            }
            return false;
        }

        bool IsIntersecting(Point2d a, Point2d b, Point2d c, Point2d d)
        {
            if(!((a.x <= c.x && c.x <= b.x) || (a.x <= d.x && d.x <= b.x)))
               return false;
            
            double denominator = ((b.x - a.x) * (d.y - c.y)) - ((b.y - a.y) * (d.x - c.x));
            double numerator1 = ((a.y - c.y) * (d.x - c.x)) - ((a.x - c.x) * (d.y - c.y));
            double numerator2 = ((a.y - c.y) * (b.x - a.x)) - ((a.x - c.x) * (b.y - a.y));

            // Detect coincident lines (has a problem, read below)
            if (denominator == 0) return numerator1 == 0 && numerator2 == 0;

            double r = numerator1 / denominator;
            double s = numerator2 / denominator;

            return (r >= 0 && r <= 1) && (s >= 0 && s <= 1);
        }
    }
}
