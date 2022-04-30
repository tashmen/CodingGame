using System;
using System.Linq;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using Algorithms.Space;
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
        int surfaceN = int.Parse(Console.ReadLine()); // the number of points used to draw the surface of Mars.
        IList<Point2d> points = new List<Point2d>();
        for (int i = 0; i < surfaceN; i++)
        {
            inputs = Console.ReadLine().Split(' ');
            int landX = int.Parse(inputs[0]); // X coordinate of a surface point. (0 to 6999)
            int landY = int.Parse(inputs[1]); // Y coordinate of a surface point. By linking all the points together in a sequential fashion, you form the surface of Mars.
            points.Add(new Point2d(landX, landY));
        }

        Board board = new Board(points);
        Console.Error.WriteLine($"landing spot: {board.GetLandingSpot()}");

        GameState state = new GameState(board);

        bool isFirstTurn = true;
        // game loop
        while (true)
        {
            inputs = Console.ReadLine().Split(' ');
            int X = int.Parse(inputs[0]);
            int Y = int.Parse(inputs[1]);
            int hSpeed = int.Parse(inputs[2]); // the horizontal speed (in m/s), can be negative.
            int vSpeed = int.Parse(inputs[3]); // the vertical speed (in m/s), can be negative.
            int fuel = int.Parse(inputs[4]); // the quantity of remaining fuel in liters.
            int rotate = int.Parse(inputs[5]); // the rotation angle in degrees (-90 to 90).
            int power = int.Parse(inputs[6]); // the thrust power (0 to 4).


            Stopwatch watch = new Stopwatch();
            watch.Start();
            Ship ship = new Ship(X, Y, hSpeed, vSpeed, fuel, rotate, power);

            if (!isFirstTurn && !state.Ship.Equals(ship))
            {
                Console.Error.WriteLine($"F: {state.Ship} \nE: {ship}");
            }

            state.SetShip(ship);


            MonteCarloTreeSearch search = new MonteCarloTreeSearch(true);
            search.SetState(state, true, false);
            var move = search.GetNextMove(watch, 98, 1, 100);
            state.ApplyMove(move, true);
            if(state.GetWinner() < 0)
            {
                Console.Error.WriteLine("Collision detected!");
            }

            watch.Stop();

            // rotate power. rotate is the desired rotation angle. power is the desired thrust power.
            Console.WriteLine(move.ToString());
            isFirstTurn = false;
        }
    }
}