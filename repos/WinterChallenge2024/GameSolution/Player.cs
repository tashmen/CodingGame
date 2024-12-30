using Algorithms.Trees;
using GameSolution.Entities;
using GameSolution.Game;
using System;
using System.Collections.Generic;
using System.Diagnostics;

/**
 * Auto-generated code below aims at helping you parse
 * the standard input according to the problem statement.
 **/
class Player
{
    static void Main(string[] args)
    {
        bool submit = false;
        MonteCarloTreeSearch search = new MonteCarloTreeSearch(!submit);
        GameState gameState = new GameState();

        string[] inputs;
        inputs = Console.ReadLine().Split(' ');
        int width = int.Parse(inputs[0]); // columns in the game grid
        int height = int.Parse(inputs[1]); // rows in the game grid

        Stopwatch watch = new Stopwatch();
        watch.Start();
        Board board = new Board(width, height);
        Console.Error.WriteLine($"ms: {watch.ElapsedMilliseconds}");


        // game loop
        while (true)
        {
            int entityCount = int.Parse(Console.ReadLine());
            List<Entity> entities = new List<Entity>();
            for (int i = 0; i < entityCount; i++)
            {
                inputs = Console.ReadLine().Split(' ');
                int x = int.Parse(inputs[0]);
                int y = int.Parse(inputs[1]);
                string type = inputs[2];
                int owner = int.Parse(inputs[3]);
                int organId = int.Parse(inputs[4]);
                string organDir = inputs[5];
                int organParentId = int.Parse(inputs[6]);
                int organRootId = int.Parse(inputs[7]);
                Entity entity = new Entity(x, y, board.GetNodeIndex(x, y), type, owner, organId, organDir, organParentId, organRootId);
                entities.Add(entity);
            }
            inputs = Console.ReadLine().Split(' ');
            int myA = int.Parse(inputs[0]);
            int myB = int.Parse(inputs[1]);
            int myC = int.Parse(inputs[2]);
            int myD = int.Parse(inputs[3]);
            inputs = Console.ReadLine().Split(' ');
            int oppA = int.Parse(inputs[0]);
            int oppB = int.Parse(inputs[1]);
            int oppC = int.Parse(inputs[2]);
            int oppD = int.Parse(inputs[3]);
            int requiredActionsCount = int.Parse(Console.ReadLine());

            int[] myProtein = new int[] { myA, myB, myC, myD };
            int[] oppProtein = new int[] { oppA, oppB, oppC, oppD };

            watch.Start();
            board.SetEntities(entities, gameState.Turn == 0);
            gameState.SetNextTurn(board, myProtein, oppProtein);
            search.SetState(gameState, true, false);
            Console.Error.WriteLine($"ms: {watch.ElapsedMilliseconds}");
            if (!submit)
                board.GetMoves(gameState.MyProtein, true, true);

            GC.Collect();
            Move move = (Move)search.GetNextMove(watch, gameState.Turn > 1 ? 45 : 970, 14, 1);
            Console.Error.WriteLine($"ms: {watch.ElapsedMilliseconds}");
            if (!submit)
            {
                if (watch.ElapsedMilliseconds < 48)
                {
                    gameState.Print();
                    Console.Error.WriteLine($"ms: {watch.ElapsedMilliseconds}");
                }
            }
            watch.Stop();
            watch.Reset();

            move.Print();
        }
    }
}