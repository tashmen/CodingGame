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
using Algorithms.Genetic;

/**
 * Auto-generated code below aims at helping you parse
 * the standard input according to the problem statement.
 **/
class Player
{
    static void Main(string[] args)
    {
        bool monte = false;
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

        Population population = new Population();
        MarsLanderSolution? winningSolution = null;


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

            if (!isFirstTurn && (true || !state.Ship.Equals(ship)))
            {
                Console.Error.WriteLine($"Matches: {state.Ship.Equals(ship)}");
                Console.Error.WriteLine($"F: {state.Ship} \nE: {ship}");
            }

            if(isFirstTurn)
                state.SetShip(ship);
            
            Move move;

            var limit = isFirstTurn ? 998 : 98;

            if (monte)
            {
                MonteCarloTreeSearch search = new MonteCarloTreeSearch(true);
                search.SetState(state, true, true);
                move = (Move)search.GetNextMove(watch, limit, -1, 30);
            }
            else
            {
                if (isFirstTurn)
                {
                    for (int i = 0; i < 100; i++)
                    {
                        population.Add(new MarsLanderSolution(state));
                    }
                }
                GeneticAlgorithm genetic = new GeneticAlgorithm(population, 0.05, 0.01, 0.2);
                move = (Move)genetic.GetNextMove(watch, limit);
                population = genetic.Population;
            }
            
            state.ApplyMove(move, true);

            if (!monte)
            {
                foreach(Individual i in population)
                {
                    var solution = (MarsLanderSolution)i;
                    solution.AdvanceTurn(state);
                }
            }

            watch.Stop();

            // rotate power. rotate is the desired rotation angle. power is the desired thrust power.
            Console.WriteLine(move.ToString());
            isFirstTurn = false;
        }
    }
}