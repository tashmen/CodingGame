using System;
using System.Linq;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using GameSolution;
using Algorithms.Graph;
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
        inputs = Console.ReadLine().Split(' ');
        int R = int.Parse(inputs[0]); // number of rows.
        int C = int.Parse(inputs[1]); // number of columns.
        int A = int.Parse(inputs[2]); // number of rounds between the time the alarm countdown is activated and the time the alarm goes off.

        

        // game loop
        while (true)
        {
            inputs = Console.ReadLine().Split(' ');
            int KR = int.Parse(inputs[0]); // row where Kirk is located.
            int KC = int.Parse(inputs[1]); // column where Kirk is located.
            string[] strMap = new string[R];
            for (int i = 0; i<R; i++)
            {
                strMap[i] = Console.ReadLine(); // C of the characters in '#.TC?' (i.e. one line of the ASCII maze).
            }

            Map2d map = new Map2d(strMap, new Tuple<int, int>(KC, KR));
            map.Print();

            GraphLinks links = new GraphLinks();
            int kirkLocationId = GenerateId(KC, KR);

            Stopwatch watch = new Stopwatch();
            
            AddLinks(map, links, KC, KR, kirkLocationId);
            
            links.CalculateShortestPathsFromStartNode(kirkLocationId, 999999);

            int nextDirection = 0;
            if (map.isControlRoomFound())
            {
                Tuple<int, int> controlRoomLocation = map.GetControlRoomLocation();
                Tuple<int, int> startLocation = map.GetStartLocation();
                if (controlRoomLocation.Item1 == KC && controlRoomLocation.Item2 == KR)
                {
                    strategy = "escape";
                }
                
                if(strategy == "explore")
                {
                    try
                    {
                        nextDirection = links.GetShortestPath(kirkLocationId, GenerateId(controlRoomLocation.Item1, controlRoomLocation.Item2));
                    }
                    catch(Exception ex)
                    {
                        //do nothing; need to keep exploring
                    }
                }
                else
                {
                    nextDirection = links.GetShortestPath(kirkLocationId, GenerateId(startLocation.Item1, startLocation.Item2));
                }
            }
            
            if(nextDirection == 0)
            {
                Dictionary<int, List<Node>> endPoints = links.GetPaths(kirkLocationId);
                int minDist = 999999;
                foreach(int endLocation in endPoints.Keys)
                {
                    var paths = endPoints[endLocation];

                    var point = GetPointFromId(endLocation);
                    var location = map.GetLocation(point.Item1, point.Item2);
                    if(location == Location.unknown)
                    {
                        var dist = links.GetShortestPathDistance(kirkLocationId, endLocation);
                        if (minDist > dist)
                        {
                            minDist = dist;
                            nextDirection = links.GetShortestPath(kirkLocationId, endLocation);
                        }
                    }
                }
            }

            // Write an action using Console.WriteLine()
            // To debug: Console.Error.WriteLine("Debug messages...");
            Console.Error.WriteLine(nextDirection);

            Console.WriteLine(GetDirectionForLocation(kirkLocationId, nextDirection)); // Kirk's next move (UP DOWN LEFT or RIGHT).
        }
    }

    static string strategy = "explore";

    public static string GetDirectionForLocation(int kirkLocationId, int nextDirectionId)
    {
        var kirkLocation = GetPointFromId(kirkLocationId);
        var nextLocation = GetPointFromId(nextDirectionId);
        if(kirkLocation.Item1 + 1 == nextLocation.Item1)
        {
            return "RIGHT";
        }
        else if (kirkLocation.Item1 - 1 == nextLocation.Item1)
        {
            return "LEFT";
        }
        else if (kirkLocation.Item2 - 1 == nextLocation.Item2)
        {
            return "UP";
        }
        else if (kirkLocation.Item2 + 1 == nextLocation.Item2)
        {
            return "DOWN";
        }

        return null;
    }

    public static void AddLinks(Map2d map, GraphLinks links, int startX, int startY, int startId)
    {
        if (map.GetLocation(startX, startY) == Location.unknown)
        {
            return;
        }

        int dx = startX;
        int dy = startY - 1;
        int id = GenerateId(dx, dy);
        Location up = map.GetLocation(dx, dy);
        if (map.isPassable(up) && !links.ContainsLink(startId, id))
        {
            links.AddLink(startId, id, 1);
            AddLinks(map, links, dx, dy, id);
        }

        dx = startX;
        dy = startY + 1;
        id = GenerateId(dx, dy);
        Location down = map.GetLocation(dx, dy);
        if (map.isPassable(down) && !links.ContainsLink(startId, id))
        {
            links.AddLink(startId, id, 1);
            AddLinks(map, links, dx, dy, id);
        }

        dx = startX - 1;
        dy = startY;
        id = GenerateId(dx, dy);
        Location left = map.GetLocation(dx, dy);
        if (map.isPassable(left) && !links.ContainsLink(startId, id))
        {
            links.AddLink(startId, id, 1);
            AddLinks(map, links, dx, dy, id);
        }

        dx = startX + 1;
        dy = startY;
        id = GenerateId(dx, dy);
        Location right = map.GetLocation(dx, dy);
        if (map.isPassable(right) && !links.ContainsLink(startId, id))
        {
            links.AddLink(startId, id, 1);
            AddLinks(map, links, dx, dy, id);
        }
    }
    public static int GenerateId(int x, int y)
    {
        return x * 10000 + y;
    }

    public static Tuple<int, int> GetPointFromId(int id)
    {
        return new Tuple<int, int>(id / 10000, id % 10000);
    }
}