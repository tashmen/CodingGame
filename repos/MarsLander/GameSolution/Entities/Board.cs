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
        public int[] MaxYAtX;
        public double[] pointIndexToDistanceFromLandingSpot;

        public double MaxY;

        public Board(IList<Point2d> points)
        {
            Points = points;
            MaxY = 0;
            MaxYAtX = new int[7000];
            Point2d lastPoint = null;
            foreach(Point2d currentPoint in points)
            {
                if (lastPoint != null)
                {
                    var slope = (lastPoint.y - currentPoint.y) / (lastPoint.x - currentPoint.x);
                    var yIntercept = currentPoint.y - (slope * currentPoint.x);
                    for(int x = lastPoint.GetTruncatedX(); x<currentPoint.x; x++)
                    {
                        MaxYAtX[x] = (int)Math.Ceiling(slope * x + yIntercept);
                    }
                }

                lastPoint = currentPoint;
            }

            var landingSpot = GetLandingSpot();
            pointIndexToDistanceFromLandingSpot = new double[points.Count];
            double distance = 0;
            lastPoint = null;
            bool foundLandingSpot = false;
            for (int i = 0; i<points.Count; i++)
            {
                var currentPoint = points[i];
                if(lastPoint != null)
                {
                    if (!foundLandingSpot)
                    {
                        distance = lastPoint.GetDistance(currentPoint);
                        for (int j = 0; j < i; j++)
                        {
                            pointIndexToDistanceFromLandingSpot[j] += distance;
                        }
                    }
                    else
                    {
                        distance += lastPoint.GetDistance(currentPoint);
                        pointIndexToDistanceFromLandingSpot[i] = distance;
                    }
                }
                
                if (currentPoint == landingSpot.Item1 || currentPoint == landingSpot.Item2)
                {
                    foundLandingSpot = true;
                    distance = 0;
                    pointIndexToDistanceFromLandingSpot[i] = distance;
                }
                
                lastPoint = currentPoint;
            }
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
            var shipLocation = ship.Location.GetRoundedAwayFromZeroPoint();
            if ((landingSpot.Item1.x + 20) < shipLocation.x && shipLocation.x < (landingSpot.Item2.x - 20))
            {
                if (shipLocation.y < landingSpot.Item1.y && landingSpot.Item1.y <= shipLocation.y - ship.VelocityVector.y)
                {
                    return ShipWithinVelocityForLanding(ship);
                }
            }
            return false;
        }

        public bool ShipWithinVelocityForLanding(Ship ship)
        {
            if (Math.Abs(ship.VelocityVector.x) <= 20 && Math.Abs(ship.VelocityVector.y) <= 40 && ship.RotationAngle == 0)
            {
                return true;
            }
            return false;
        }

        public bool IsInBounds(Ship ship)
        {
            //Check if ship is still within the board
            if (ship.Location.x <= 0)
            {
                ship.CrashPoint = new Tuple<Point2d, Point2d>(Points[0], Points[1]);
                return false;
            }
            if(ship.Location.x >= 6950)
            {
                ship.CrashPoint = new Tuple<Point2d, Point2d>(Points[Points.Count - 2], Points[Points.Count - 1]);
                return false;
            }

            if (ship.Location.y <= 0 || ship.Location.y >= 2950)
            {
                ship.CrashPoint = null;
                return false;
            }

            return true;
        }

        public bool? ShipCollision(Ship ship, Point2d lastLocation = null)
        {
            Point2d shipLastLocation = lastLocation == null ? ship.LastLocation : lastLocation;

            Point2d lastPoint = null;
            foreach (var point in Points)
            {
                if (lastPoint != null)
                {
                    var currentPoint = point;
                    if (IsIntersecting(lastPoint, currentPoint, ship.Location, shipLastLocation))
                    {
                        ship.CrashPoint = new Tuple<Point2d, Point2d>(lastPoint, currentPoint);
                        var landingSpot = GetLandingSpot();
                        if(landingSpot.Item1 == lastPoint && landingSpot.Item2 == currentPoint)
                        {
                            return !ShipLanded(ship);
                        }
                        return true;
                    }
                }
                lastPoint = point;
            }
            return null;
        }

        public bool IsIntersecting(Point2d a, Point2d b, Point2d c, Point2d d)
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

            return (r >= 0 && r <= 1) && (s > 0 && s <= 1);
        }

        public double CalculatePathDistance(Ship ship)
        {
            if (ship.CrashPoint == null)
                return 0;

            var distanceToPoint1 = pointIndexToDistanceFromLandingSpot[Points.IndexOf(ship.CrashPoint.Item1)];
            var distanceToPoint2 = pointIndexToDistanceFromLandingSpot[Points.IndexOf(ship.CrashPoint.Item2)];
            if (distanceToPoint1 == 0 && distanceToPoint2 == 0)
                return 0;

            double minDist = Math.Min(ship.Location.GetDistance(ship.CrashPoint.Item1) + distanceToPoint1, ship.Location.GetDistance(ship.CrashPoint.Item2) + distanceToPoint2);

            return minDist;
        }
    }
}
