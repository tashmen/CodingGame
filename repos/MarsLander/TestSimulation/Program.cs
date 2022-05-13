using Algorithms.Space;
using Algorithms.Trees;
using GameSolution;
using GameSolution.Entities;
using System.Collections.Generic;
using System.Diagnostics;

namespace TestSimulation
{
    internal class Program
    {
        static void Main(string[] args)
        {
            IList<Point2d> points = new List<Point2d>();
            points.Add(new Point2d(0, 100));
            points.Add(new Point2d(7000, 100));
            Board board = new Board(points);
            GameState state = new GameState(board);
            Ship ship = new Ship(3550, 2900, 0, 0, 600, 0, 0);
            state.SetShip(ship);
            MonteCarloTreeSearch search = new MonteCarloTreeSearch(false);
            search.SetState(state);
            do
            {
                Stopwatch watch = new Stopwatch();
                watch.Start();
                var move = search.GetNextMove(watch, 98, -1, 30);
                state.ApplyMove(move, true);

                watch.Stop();
            }
            while(state.GetWinner() ==null);
        }
    }
}
