using System;
using System.Linq;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using Algorithms.Graph;

/**
 * Auto-generated code below aims at helping you parse
 * the standard input according to the problem statement.
 **/
class Solution
{
    public class TrainStop
    {
        public string Id;
        public string Name;
        public double Latitude;
        public double Longitude;

        public TrainStop(string fullString)
        {
            string[] attributes = fullString.Split(',');
            Id = attributes[0];
            Name = attributes[1].Replace('"', ' ').Trim();
            Latitude = double.Parse(attributes[3]);
            Longitude = double.Parse(attributes[4]);
        }

        public double CalculateDistance(TrainStop stop)
        {
            double distance = 0;
            double x = (stop.Longitude - Longitude) * Math.Cos((Latitude + stop.Latitude) / 2);
            double y = stop.Latitude - Latitude;
            distance = Math.Sqrt(x * x + y * y) * 6371;
            return distance;
        }
    }
    static void Main(string[] args)
    {
        GraphLinks graphLinks = new GraphLinks(false);

        string startPoint = Console.ReadLine();
        string endPoint = Console.ReadLine();
        int N = int.Parse(Console.ReadLine());
        Dictionary<string, TrainStop> trainStops = new Dictionary<string, TrainStop>();
        for (int i = 0; i < N; i++)
        {
            string stopName = Console.ReadLine();
            TrainStop stop = new TrainStop(stopName);
            trainStops[stop.Id] = stop;
        }
        int M = int.Parse(Console.ReadLine());
        for (int i = 0; i < M; i++)
        {
            string route = Console.ReadLine();
            if (!string.IsNullOrEmpty(route))
            {
                string[] stopIds = route.Split(' ');
                TrainStop stop1 = trainStops[stopIds[0]];
                TrainStop stop2 = trainStops[stopIds[1]];
                graphLinks.AddLink(GenerateId(stopIds[0]), GenerateId(stopIds[1]), stop1.CalculateDistance(stop2));
            }
        }

        graphLinks.CalculateShortestPathsFromStartNode(GenerateId(startPoint), 999999999);

        try
        {
            List<Node> path = graphLinks.GetShortestPathAll(GenerateId(startPoint), GenerateId(endPoint));
            Console.WriteLine(trainStops[startPoint].Name);
            foreach (Node n in path)
            {
                TrainStop stop = trainStops[LinkIdToTrainId[n.Id]];
                Console.WriteLine(stop.Name);
            }
        }
        catch(Exception ex)
        {
            if (startPoint == endPoint)
            {
                Console.WriteLine(trainStops[startPoint].Name);
            }
            else
            {
                Console.WriteLine("IMPOSSIBLE");
            }
        }     
    }

    public static Dictionary<int, string> LinkIdToTrainId = new Dictionary<int, string>();
    static int GenerateId(string trainId)
    {
        var code = trainId.GetHashCode();
        LinkIdToTrainId[code] = trainId;
        return code;
    }
}