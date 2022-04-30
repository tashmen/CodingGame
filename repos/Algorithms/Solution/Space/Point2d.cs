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

        public Point2d GetCeilingPoint()
        {
            return new Point2d(Math.Ceiling(x), Math.Ceiling(y));
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

        public double LengthSquared()
        {
            return x * x + y * y;
        }

        public double Length()
        {
            return Math.Sqrt(LengthSquared());
        }

        public Point2d Normalize()
        {
            var length = Length();
            if (length == 0)
            {
                x = 0;
                y = 0;
            }
            else
            {
                x = x / length;
                y = y / length;
            }
            return this;
        }

        public Point2d Multiply(double scalar)
        {
            x = x * scalar;
            y = y * scalar;
            return this;
        }

        public Point2d Add(Point2d vector)
        {
            x = x + vector.x;
            y = y + vector.y;
            return this;
        }

        public Point2d Subtract(Point2d vector)
        {
            x = x - vector.x;
            y = y - vector.y;
            return this;
        }

        public Point2d Truncate()
        {
            x = GetTruncatedX();
            y = GetTruncatedY();
            return this;
        }

        public Point2d SymmetricTruncate(Point2d origin)
        {
            Subtract(origin).Truncate().Add(origin);
            return this;
        }

        public Point2d Clone()
        {
            return new Point2d(x, y);
        }

        public static double GetDistance(double x1, double y1, double x2, double y2)
        {
            return Math.Sqrt(Math.Pow(x1 - x2, 2) + Math.Pow(y1 - y2, 2));
        }

        public static Point2d GetMidPoint(double x1, double y1, double x2, double y2)
        {
            return new Point2d((x1 + x2)/2, (y1+y2)/2);
        }
    }
}
