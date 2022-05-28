using System;
using System.Linq;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using GameSolution;

/**
 * Auto-generated code below aims at helping you parse
 * the standard input according to the problem statement.
 **/
class Solution
{
    static void Main(string[] args)
    {
        int T = int.Parse(Console.ReadLine());
        long[] P, R, C;
        P = new long[T];
        R = new long[T];
        C = new long[T];

        long maximumRows = 0;
        for (int i = 0; i < T; i++)
        {
            string[] inputs = Console.ReadLine().Split(' ');
            P[i] = long.Parse(inputs[0]);
            R[i] = long.Parse(inputs[1]);
            C[i] = long.Parse(inputs[2]);
            maximumRows = Math.Max(maximumRows, R[i]);

            Console.Error.WriteLine(P[i] + " " + R[i] + " " + C[i]);
        }

        

        Pascal pascal = new Pascal();

        //pascal.BuildTriangle(maximumRows);


        for (int i = 0; i < T; i++)
        {   
            Console.WriteLine(pascal.CountTriangleOptimized(R[i], C[i], P[i]));
        }
    }
}