using Algorithms.Space;
using System.Collections.Generic;

namespace GameSolution.Entities
{
    public static class DistanceHash
    {
        private static Dictionary<long, double> Hash { get; set; } = new Dictionary<long, double>(); 

        private static long ConvertToLong(int x, int y, int x1, int y1)
        {
            return x + (y << 16) + (x1 << 32) + (y1 << 48);
        }

        public static double GetDistance(int x, int y, int x1, int y1)
        {
            if(x > x1 || (x == x1 && y > y1))
            {
                var swap = x;
                x = x1;
                x1 = swap;
                swap = y;
                y = y1;
                y1 = swap;
            }

            var key = ConvertToLong(x, y, x1, y1);
            if (x == x1 && y == y1)
                return 0;
            double value;
            if(!Hash.TryGetValue(key, out value))
            {
                var distance = Point2d.GetDistance(x, y, x1, y1);
                Hash[key] = distance;
                value = distance;
            }

            return value;
        }
    }
}
