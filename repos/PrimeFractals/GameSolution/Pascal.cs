using System;
using System.Collections.Generic;
using System.Numerics;

namespace GameSolution
{
    public class Pascal
    {
        public List<BigInteger[]> Triangle;
        public Pascal()
        {
            Triangle = new List<BigInteger[]>();
        }

        public long CountTriangleOptimized(long rows, long maxColumn, long divisor)
        {
            Console.Error.WriteLine("Optimized:");
            long counter = 0;
            long multiplier = 1;
            List<long> counters = new List<long>();
            for(int i =0; i < rows; i++)
            {
                if((rows / (long)Math.Pow(divisor, i)) > 0)
                {
                    counters.Add(1);
                }
            }
            
            for (int r = 0; r < rows; r++)
            {
                multiplier = 1;
                
                for (int c = 0; c<counters.Count;c++)
                {
                    multiplier *= Math.Min(counters[c], maxColumn);
                    if (c == 0)
                    {
                        counters[c]++;
                    }
                    if(c >= 1 && counters[c-1] == divisor + 1)
                    {
                        counters[c-1] = 1;
                        counters[c]++;
                    }
                }
                Console.Error.WriteLine(" " + multiplier);
                counter += multiplier;
            }

            return counter;
        }


        public long CountTriangle(long rows, long maxColumn, long divisor)
        {
            long counter = 0;
            for(int r = 0; r<rows; r++)
            {
                long mc = Math.Min(maxColumn, r + 1);
                long rowCount = 0;
                for (int c = 0; c< mc; c++)
                {
                    if(Triangle[r][c] % (ulong)divisor != 0)
                    {
                        rowCount++;
                        counter++;
                        counter%=1000000007;
                    }
                }
                Console.Error.WriteLine(" " + rowCount);
            }

            return counter;
        }

        public void BuildTriangle(long rows)
        {
            if (rows == 1)
            {
                Triangle.Add(new BigInteger[1]);
                Triangle[0][0] = 1;
            }
            else
            {
                for(int i = 0; i<rows; i++)
                {
                    Triangle.Add(new BigInteger[i+1]);
                    for(int j = 0; j<i+1; j++)
                    {
                        if (j == 0 || j == i)
                            Triangle[i][j] = 1;
                        else Triangle[i][j] = Triangle[i - 1][j - 1] + Triangle[i - 1][j];
                    }
                }
            }
        }
    }
}
