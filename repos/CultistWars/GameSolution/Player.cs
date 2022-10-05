using System;
using System.Collections.Generic;
using System.Diagnostics;
using Algorithms.Trees;
using GameSolution;
using GameSolution.Entities;

/**
 * Convert neutral units and attack enemy ones
 **/
class Player
{
    static void Main(string[] args)
    {
        bool isFirstTurn = true;
        int turn = 0;
        string[] inputs;
        int myId = int.Parse(Console.ReadLine()); // 0 - you are the first player, 1 - you are the second player
        inputs = Console.ReadLine().Split(' ');
        int width = int.Parse(inputs[0]); // Width of the board
        int height = int.Parse(inputs[1]); // Height of the board
        string[] strBoard = new string[height];
        for (int i = 0; i < height; i++)
        {
            strBoard[i] = Console.ReadLine(); // A y of the board: "." is empty, "x" is obstacle
            //Console.Error.WriteLine(strBoard[i]);
        }

        Board board = new Board(strBoard);
        MonteCarloTreeSearch monteCarlo = new MonteCarloTreeSearch();
        //Minimax miniMax = new Minimax();
        GameState state = new GameState(board);

        // game loop
        while (true)
        {
            Entity[] entities = new Entity[14];
            int numOfUnits = int.Parse(Console.ReadLine()); // The total number of units on the board
            for (int i = 0; i < numOfUnits; i++)
            {
                inputs = Console.ReadLine().Split(' ');
                int unitId = int.Parse(inputs[0]); // The unit's ID
                int unitType = int.Parse(inputs[1]); // The unit's type: 0 = Cultist, 1 = Cult Leader
                int hp = int.Parse(inputs[2]); // Health points of the unit
                int x = int.Parse(inputs[3]); // X coordinate of the unit
                int y = int.Parse(inputs[4]); // Y coordinate of the unit
                int owner = int.Parse(inputs[5]); // id of owner player
                OwnerType isMine = owner == myId ? OwnerType.Max : owner == 2 ? OwnerType.Neutral : OwnerType.Min;
                entities[unitId] = new Entity(unitId, x, y, unitType, hp, (int)isMine);
                //Console.Error.WriteLine(entities[unitId]);
            }

            state.Turn = turn;
            state.SetState(entities);
            

            // Write an action using Console.WriteLine()
            // To debug: Console.Error.WriteLine("Debug messages...");

            
            var limit = isFirstTurn ? 980 : 45;

            GC.Collect();
            
            Stopwatch watch = new Stopwatch();
            watch.Start();
            
            monteCarlo.SetState(state);
            //miniMax.SetState(state);

            long move;
            move = (long)monteCarlo.GetNextMove(watch, limit, 10, 5);
            //move = (Move)miniMax.GetNextMove(watch, limit, 5);
            watch.Stop();

            Console.Error.WriteLine($"MS: {watch.ElapsedMilliseconds}");

            /*
            var cloneState = (GameState)monteCarlo.GetRootState().Clone();
            cloneState.ApplyMove(move, true);
            Console.Error.WriteLine($"Neutral move: {cloneState.NeutralLastMove}");

            var moves = (List<Move>)cloneState.GetPossibleMoves(false);
            foreach (Move oppMove in moves)
            {
                var cs = (GameState)cloneState.Clone();
                cs.ApplyMove(oppMove, false);
                Console.Error.WriteLine($"Neutral move for opponent: {cs.NeutralLastMove} for move {oppMove}");
            }
            */
            turn += 2;
            Console.WriteLine(Move.ToString(move));
            isFirstTurn = false;
        }
    }
}