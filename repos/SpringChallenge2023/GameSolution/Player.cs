using System;
using System.Linq;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using GameSolution.Entities;
using GameSolution.Game;
using System.Diagnostics;

/**
 * Auto-generated code below aims at helping you parse
 * the standard input according to the problem statement.
 **/
class Player
{
    static void Main(string[] args)
    {
        Board board = new Board();
        GameState gameState = new GameState();


        string[] inputs;
        int numberOfCells = int.Parse(Console.ReadLine()); // amount of hexagonal cells in this map
        List<Cell> cells = new List<Cell>(numberOfCells);
        for (int i = 0; i < numberOfCells; i++)
        {
            inputs = Console.ReadLine().Split(' ');
            int type = int.Parse(inputs[0]); // 0 for empty, 1 for eggs, 2 for crystal
            int initialResources = int.Parse(inputs[1]); // the initial amount of eggs/crystals on this cell
            int neigh0 = int.Parse(inputs[2]); // the index of the neighbouring cell for each direction
            int neigh1 = int.Parse(inputs[3]);
            int neigh2 = int.Parse(inputs[4]);
            int neigh3 = int.Parse(inputs[5]);
            int neigh4 = int.Parse(inputs[6]);
            int neigh5 = int.Parse(inputs[7]);
            cells.Add(new Cell(i, (ResourceType)type, initialResources, new List<int>() { neigh0, neigh1, neigh2, neigh3, neigh4, neigh5 }));
        }
        board.SetCells(cells);

        int numberOfBases = int.Parse(Console.ReadLine());
        inputs = Console.ReadLine().Split(' ');
        for (int i = 0; i < numberOfBases; i++)
        {
            int myBaseIndex = int.Parse(inputs[i]);
            board.GetCell(myBaseIndex).SetBaseType(BaseType.MyBase);
        }
        inputs = Console.ReadLine().Split(' ');
        for (int i = 0; i < numberOfBases; i++)
        {
            int oppBaseIndex = int.Parse(inputs[i]);
            board.GetCell(oppBaseIndex).SetBaseType(BaseType.OppBase);
        }

        // game loop
        while (true)
        {
            inputs = Console.ReadLine().Split(' ');
            int myScore = int.Parse(inputs[0]);
            int oppScore = int.Parse(inputs[1]);
            for (int i = 0; i < numberOfCells; i++)
            {
                inputs = Console.ReadLine().Split(' ');
                int resources = int.Parse(inputs[0]); // the current amount of eggs/crystals on this cell
                int myAnts = int.Parse(inputs[1]); // the amount of your ants on this cell
                int oppAnts = int.Parse(inputs[2]); // the amount of opponent ants on this cell
                var cell = board.GetCell(i);
                cell.SetAnts(myAnts, oppAnts);
                cell.SetResource(resources);
                
                Console.Error.WriteLine(cell.ToString());
            }

            // Write an action using Console.WriteLine()
            // To debug: Console.Error.WriteLine("Debug messages...");

            Stopwatch watch = new Stopwatch();
            watch.Start();

            gameState.SetNextTurn(board, myScore, oppScore);

            GameHelper gameHelper = new GameHelper(gameState);
            Move move = gameHelper.GetMove();

            Console.Error.WriteLine($"ms: {watch.ElapsedMilliseconds}");

            // WAIT | LINE <sourceIdx> <targetIdx> <strength> | BEACON <cellIdx> <strength> | MESSAGE <text>
            Console.WriteLine(move);
        }
    }
}