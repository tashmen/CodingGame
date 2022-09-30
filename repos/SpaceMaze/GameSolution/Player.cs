using System;
using System.Linq;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using GameSolution.Entities;
using GameSolution;
using Algorithms.Trees;
using System.Diagnostics;

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
        int width = int.Parse(inputs[0]);
        int height = int.Parse(inputs[1]);
        string[] boardInput = new string[height];
        for (int i = 0; i < height; i++)
        {
            string line = Console.ReadLine();
            boardInput[i] = line;
            Console.Error.WriteLine(line);
        }

        Board board = new Board(boardInput);
        GameState state = new GameState(board);
        bool isFirstTurn = true;

        // game loop
        while (true)
        {
            List<Entity> entities = new List<Entity>();
            int entityCount = int.Parse(Console.ReadLine());
            for (int i = 0; i < entityCount; i++)
            {
                inputs = Console.ReadLine().Split(' ');
                int id = int.Parse(inputs[0]);
                int x = int.Parse(inputs[1]);
                int y = int.Parse(inputs[2]);
                string direction = inputs[3];
                entities.Add(new Entity(id, x, y, direction));
                Console.Error.WriteLine($"{id},{x},{y},\"{direction}\"");
            }

            state.SetState(entities);

            int limit = isFirstTurn ? 995 : 45;
            Stopwatch watch = new Stopwatch();
            watch.Start();

            MonteCarloTreeSearch monteCarlo = new MonteCarloTreeSearch(true);
            Move move = (Move) monteCarlo.GetNextMove(watch, limit, -1, 20);

            watch.Stop();
            Console.Error.WriteLine("ms: " + watch.ElapsedMilliseconds);

            isFirstTurn = false;
            Console.WriteLine(move);
        }
    }
}