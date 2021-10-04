using System;
using System.Linq;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using GameSolution.Game;
using Algorithms;
using GameSolution.Entities;
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
        GameState state = new GameState();
        MonteCarloTreeSearch search = new MonteCarloTreeSearch(false);
        bool isFirstRound = true;
        // game loop
        while (true)
        {
            inputs = Console.ReadLine().Split(' ');
            //Stop watch must be after the first read as this is when our time officially starts
            Stopwatch watch = new Stopwatch();
            watch.Start();
            int opponentRow = int.Parse(inputs[0]);
            int opponentCol = int.Parse(inputs[1]);
            int validActionCount = int.Parse(Console.ReadLine());
            for (int i = 0; i < validActionCount; i++)
            {
                inputs = Console.ReadLine().Split(' ');
                int row = int.Parse(inputs[0]);
                int col = int.Parse(inputs[1]);
                //Console.Error.WriteLine($"{row} {col}");
            }
            //Console.Error.WriteLine("------");

            Move move = null;
            if (opponentRow != -1 && opponentCol != -1)
            {
                int row = opponentRow % 3;
                int col = opponentCol % 3;
                int boardIndex = (opponentRow / 3) * 3 + opponentCol / 3;
                state.ApplyMove(new Move(row, col, boardIndex), false);
                search.SetState(state, true, true);
                /*
                foreach (Move m in state.GetPossibleMoves(true))
                {
                    Console.Error.WriteLine(m);
                }
                */
            }
            else if (isFirstRound)
            {
                move = new Move(1, 1, 4);
                state.ApplyMove(move, true);
                search.SetState(state, false, true);
            }
            //Console.Error.WriteLine(state.ToString());

            int limit = isFirstRound ? 998 : 98;
            Console.Error.WriteLine($"ms: {watch.ElapsedMilliseconds} / {limit}");
            object moveToPlay = search.GetNextMove(watch, limit, -1, 20, 1);

            if (move == null)
            {
                move = moveToPlay as Move;
                state.ApplyMove(move, true);
            }

            watch.Stop();
            Console.Error.WriteLine($"ms: {watch.ElapsedMilliseconds} / {limit}");
            isFirstRound = false;
            Console.WriteLine(move.ToString());
        }
    }
}
