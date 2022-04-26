using System;

namespace Algorithms.Space
{
    public class Point2d
    {
        public double x { get; private set; }
        public double y { get; private set; }

        public Point2d(double x, double y)
        {
            this.x = x; 
            this.y = y;
        }

        public Point2d(Point2d point)
        {
            x = point.x;
            y = point.y;
        }

        public override string ToString()
        {
            return $"({x},{y})";
        }

        public bool Equals(Point2d point)
        {
            return point.x == this.x && point.y == this.y;
        }

        public Point2d GetTruncatedPoint()
        {
            return new Point2d(Math.Truncate(this.x), Math.Truncate(this.y));
        }

        public Point2d GetRoundedPoint()
        {
            return new Point2d(Math.Round(this.x), Math.Round(this.y));
        }

        public int GetTruncatedX()
        {
            return (int)x;
        }
        public int GetTruncatedY()
        {
            return (int)y;
        }

        public double GetAngle(Point2d point)
        {
            return Math.Atan2(point.y - y, point.x - x);
        }

        public double GetDistance(Point2d point)
        {
            return GetDistance(point.x, point.y, x, y);
        }

        public Point2d GetMidPoint(Point2d point)
        {
            return GetMidPoint(point.x, point.y, x, y);
        }

        public static double GetDistance(double x1, double y1, double x2, double y2)
        {
            return Math.Sqrt(Math.Pow(x1 - x2, 2) + Math.Pow(y1 - y2, 2));
        }

        public static Point2d GetMidPoint(double x1, double y1, double x2, double y2)
        {
            return new Point2d(Math.Abs(x1 - x2) / 2, Math.Abs(y1 - y2) / 2);
        }
    }
}
