using GameSolution;
using System;

/**
 * Auto-generated code below aims at helping you parse
 * the standard input according to the problem statement.
 **/
class Player
{
    static void Main(string[] args)
    {
        string[] inputs;
        inputs = Console.ReadLine().Split(' ');
        int largestX = int.Parse(inputs[0]); // width of the building.
        int largestY = int.Parse(inputs[1]); // height of the building.
        int N = int.Parse(Console.ReadLine()); // maximum number of turns before game over.
        inputs = Console.ReadLine().Split(' ');
        int X0 = int.Parse(inputs[0]);
        int Y0 = int.Parse(inputs[1]);
       

        Game game = new Game(largestX, largestY, N, X0, Y0);

        // game loop
        while (true)
        {
            string bombDir = Console.ReadLine(); // Current distance to the bomb compared to previous distance (COLDER, WARMER, SAME or UNKNOWN)

            game.SetState(bombDir);

            var output = game.GetNextMove();

            

            Console.WriteLine($"{output.Item1} {output.Item2}");
        }
    }
}