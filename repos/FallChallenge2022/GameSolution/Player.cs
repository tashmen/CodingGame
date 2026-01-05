using System;
using GameSolution.Entities;
using GameSolution.Game;
using System.Diagnostics;
using Algorithms.Trees;
using System.Collections.Generic;

class Player
{
    static void Main(string[] args)
    {
        GameState state = new GameState();
        int turnCounter = 0;


        string[] inputs;
        inputs = Console.ReadLine().Split(' ');
        int width = int.Parse(inputs[0]);
        int height = int.Parse(inputs[1]);

        var board = new Board(width, height);

        // game loop
        while (true)
        {
            List<Cell> cells = new List<Cell>();

            inputs = Console.ReadLine().Split(' ');
            int myMatter = int.Parse(inputs[0]);
            int oppMatter = int.Parse(inputs[1]);
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    inputs = Console.ReadLine().Split(' ');
                    int scrapAmount = int.Parse(inputs[0]);
                    int owner = int.Parse(inputs[1]); // 1 = me, 0 = foe, -1 = neutral
                    int units = int.Parse(inputs[2]);
                    int recycler = int.Parse(inputs[3]);
                    int canBuild = int.Parse(inputs[4]);
                    int canSpawn = int.Parse(inputs[5]);
                    int inRangeOfRecycler = int.Parse(inputs[6]);
                    Cell cell = new Cell(x, y, owner, scrapAmount, units, recycler, canBuild, canSpawn, inRangeOfRecycler);
                    cells.Add(cell);
                }
            }

            Stopwatch watch = new Stopwatch();
            watch.Start();

            GC.Collect();

            board.SetCells(cells);
            state.SetNextTurn(board, myMatter, oppMatter);

            int limit = turnCounter == 1 ? 995 : 45;

            Move move;

            move = new Move();
            move.UnitActions.Add(MoveAction.CreateWait());

            GameHelper gm = new GameHelper((GameState)state.Clone());
            move = gm.GetMove();

            /*
            MonteCarloTreeSearch search = new MonteCarloTreeSearch(false);
            search.SetState(state, true, false);
            Console.Error.WriteLine("state ms: " + watch.ElapsedMilliseconds);
            move = (Move)search.GetNextMove(watch, limit, 6, 20);
            */
            
            watch.Stop();
            Console.Error.WriteLine("total ms: " + watch.ElapsedMilliseconds);

            Console.WriteLine(move);
        }
    }
}