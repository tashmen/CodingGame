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

        GameState gameState = new GameState();


        string[] inputs;
        inputs = Console.ReadLine().Split(' ');
        int width = int.Parse(inputs[0]); // columns in the game grid
        int height = int.Parse(inputs[1]); // rows in the game grid

        Board board = new Board(width, height);

        // game loop
        while (true)
        {
            int entityCount = int.Parse(Console.ReadLine());
            List<Entity> entities = new List<Entity>();
            for (int i = 0; i < entityCount; i++)
            {
                inputs = Console.ReadLine().Split(' ');
                int x = int.Parse(inputs[0]);
                int y = int.Parse(inputs[1]); // grid coordinate
                string type = inputs[2]; // WALL, ROOT, BASIC, TENTACLE, HARVESTER, SPORER, A, B, C, D
                int owner = int.Parse(inputs[3]); // 1 if your organ, 0 if enemy organ, -1 if neither
                int organId = int.Parse(inputs[4]); // id of this entity if it's an organ, 0 otherwise
                string organDir = inputs[5]; // N,E,S,W or X if not an organ
                int organParentId = int.Parse(inputs[6]);
                int organRootId = int.Parse(inputs[7]);
                Entity entity = new Entity(x, y, type, owner, organId, organDir, organParentId, organRootId);
                entities.Add(entity);
            }
            board.SetEntities(entities);
            inputs = Console.ReadLine().Split(' ');
            int myA = int.Parse(inputs[0]);
            int myB = int.Parse(inputs[1]);
            int myC = int.Parse(inputs[2]);
            int myD = int.Parse(inputs[3]); // your protein stock
            inputs = Console.ReadLine().Split(' ');
            int oppA = int.Parse(inputs[0]);
            int oppB = int.Parse(inputs[1]);
            int oppC = int.Parse(inputs[2]);
            int oppD = int.Parse(inputs[3]); // opponent's protein stock
            int requiredActionsCount = int.Parse(Console.ReadLine()); // your number of organisms, output an action for each one in any order

            Dictionary<EntityType, int> myProtein = new Dictionary<EntityType, int>();
            myProtein[EntityType.A] = myA;
            myProtein[EntityType.B] = myB;
            myProtein[EntityType.C] = myC;
            myProtein[EntityType.D] = myD;

            Dictionary<EntityType, int> oppProtein = new Dictionary<EntityType, int>();
            oppProtein[EntityType.A] = oppA;
            oppProtein[EntityType.B] = oppB;
            oppProtein[EntityType.C] = oppC;
            oppProtein[EntityType.D] = oppD;

            Stopwatch watch = new Stopwatch();
            watch.Start();

            gameState.SetNextTurn(board, myProtein, oppProtein);

            /*
            GameHelper gameHelper = new GameHelper(gameState);
            Move move = gameHelper.GetMove();
            */
            MonteCarloTreeSearch search = new MonteCarloTreeSearch();
            search.SetState(gameState, true, false);
            Move move = (Move)search.GetNextMove(watch, 48, 80, 2);

            Console.Error.WriteLine($"ms: {watch.ElapsedMilliseconds}");

            board.Print();

            move.Print();
        }
    }
}