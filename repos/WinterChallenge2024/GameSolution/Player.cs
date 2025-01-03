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
        bool showMove = false;
        MonteCarloTreeSearch search = new MonteCarloTreeSearch(!submit, MonteCarloTreeSearch.SearchStrategy.Sequential, mathLogCacheSize: 5000);
        GameState gameState = new GameState();

        string[] inputs;
        inputs = Console.ReadLine().Split(' ');
        int width = int.Parse(inputs[0]); // columns in the game grid
        int height = int.Parse(inputs[1]); // rows in the game grid

        Stopwatch watch = new Stopwatch();
        watch.Start();
        Board board = new Board(width, height);
        Console.Error.WriteLine($"after board ms: {watch.ElapsedMilliseconds}");


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
                Entity entity = Entity.GetEntity(new Point2d(x, y, board.GetNodeIndex(x, y)), Entity.GetType(type), Entity.GetOwner(owner), organId, organParentId, organRootId, Entity.GetOrganDirection(organDir));
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
            Console.Error.WriteLine($"after entities ms: {watch.ElapsedMilliseconds}");
            gameState.SetNextTurn(board, myProtein, oppProtein);
            Console.Error.WriteLine($"after turn ms: {watch.ElapsedMilliseconds}");
            search.SetState(gameState, true, false);
            Console.Error.WriteLine($"after state ms: {watch.ElapsedMilliseconds}");
            if (showMove)
            {
                board.GetMoves(gameState.MyProtein, true, true);
                Console.Error.WriteLine($"after moves ms: {watch.ElapsedMilliseconds}");
            }

            Move move = (Move)search.GetNextMove(watch, gameState.Turn > 1 ? 25 : 970, 4, 4);
            Console.Error.WriteLine($"after move ms: {watch.ElapsedMilliseconds}");

            if (!submit)
            {
                if (watch.ElapsedMilliseconds < 48)
                {
                    gameState.Print();
                    Console.Error.WriteLine($"after print ms: {watch.ElapsedMilliseconds}");
                }
            }
            move.Print();
            Console.Error.WriteLine($"after print move ms: {watch.ElapsedMilliseconds}");
            watch.Stop();
            watch.Reset();
            move.Output();
        }
    }
}