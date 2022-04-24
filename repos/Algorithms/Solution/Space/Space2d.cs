using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;

namespace Algorithms.Space
{
    public class Space2d
    {
        /// <summary>
        /// Given a list of points and a circle radius, find the circle location that covers the maximum number of points
        /// </summary>
        /// <param name="points">The list of points to consider</param>
        /// <param name="radius">The radius of the circle</param>
        /// <returns>The number of points covered by the circle that is centered at the point.</returns>
        public static Tuple<int, Point2d> FindCircleWithMaximumPoints(Point2d[] points, double radius)
        {
            Tuple<int, Point2d> maxPoint = null;
            if (points == null)
                return null;
            if (radius <= 0)
                return null;

            var numberOfPoints = points.Count();
            double[,] distance = new double[numberOfPoints, numberOfPoints];
            for (int i = 0; i < numberOfPoints - 1; i++)
            {
                for (int j = i + 1; j < numberOfPoints; j++)
                {
                    distance[i, j] = distance[j, i] = points[i].GetDistance(points[j]);
                }
            }
            
            for (int i = 0; i < numberOfPoints; i++)
            {
                var currentAnswer = GetPointsInside(distance, points, i, radius, numberOfPoints);
                var nextPoint = new Tuple<int, Point2d>(currentAnswer.Item1, new Point2d(points[i].x + (radius * Math.Round(Math.Cos(currentAnswer.Item2), 15)), points[i].y + (radius * Math.Round(Math.Sin(currentAnswer.Item2), 15))));
                if (maxPoint == null || currentAnswer.Item1 > maxPoint.Item1 || (currentAnswer.Item1 == maxPoint.Item1 && IsInteger(nextPoint.Item2.x) && IsInteger(nextPoint.Item2.y)))
                {
                    maxPoint = nextPoint;
                }

            }

            return maxPoint;
        }

        private static bool IsInteger(double d)
        {
            return Math.Abs(d % 1) <= (Double.Epsilon * 100);
        }
        

        private static Tuple<int, double> GetPointsInside(double[,] distance, Point2d[] points, int i, double radius, int numberOfPoints)
        {
            List<Tuple<double, bool>> angles = new List<Tuple<double, bool>>();
            for (int j = 0; j < numberOfPoints; j++)
            {
                if (i != j && distance[i, j] <= 2 * radius)
                {
                    double B = Math.Acos(distance[i, j] / (2 * radius));
                    Complex c1 = new Complex(points[j].x - points[i].x, points[j].y - points[i].y);
                    double A = c1.Phase;
                    double alpha = A - B;
                    double beta = A + B;
                    angles.Add(new Tuple<double, bool>(alpha, true));
                    angles.Add(new Tuple<double, bool>(beta, false));
                }
            }
            angles = angles.OrderBy(i => i.Item1).ToList();
            int count = 1, res = 1;
            double maxAngle = 0;
            foreach (var angle in angles)
            {
                if (angle.Item2)
                    count++;
                else
                    count--;
                if (count > res)
                {
                    res = count;
                    maxAngle = angle.Item1;
                }

            }
            return new Tuple<int, double>(res, maxAngle);
        }
    }
}
