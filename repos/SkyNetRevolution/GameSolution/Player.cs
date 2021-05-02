using GameSolution.Utility;
using System;
using System.Collections.Generic;

/**
 * Auto-generated code below aims at helping you parse
 * the standard input according to the problem statement.
 **/
class Player
{
    static void Main(string[] args)
    {
        GraphLinks links = new GraphLinks();
        string[] inputs;
        inputs = Console.ReadLine().Split(' ');
        int N = int.Parse(inputs[0]); // the total number of nodes in the level, including the gateways
        int L = int.Parse(inputs[1]); // the number of links
        int E = int.Parse(inputs[2]); // the number of exit gateways
        for (int i = 0; i < L; i++)
        {
            inputs = Console.ReadLine().Split(' ');
            int N1 = int.Parse(inputs[0]); // N1 and N2 defines a link between these nodes
            int N2 = int.Parse(inputs[1]);
            links.AddLink(N1, N2, 1);
        }
        IList<int> gateways = new List<int>();
        for (int i = 0; i < E; i++)
        {
            int EI = int.Parse(Console.ReadLine()); // the index of a gateway node
            gateways.Add(EI);
        }

        links.CalculateShortestPaths();

        GraphLinks graph2 = new GraphLinks();
        foreach (int link in links.Links.Keys)
        {
            foreach (Node n in links.Links[link])
            {
                int dist = 1;
                foreach (int gateway in gateways)
                {
                    if (gateway == link || gateway == n.Id)
                    {
                        dist = 9999999;
                    }
                    else
                    {
                        int i1 = links.GetShortestPathDistance(gateway, link);
                        int i2 = links.GetShortestPathDistance(gateway, n.Id);

                        if (i2 == 1)
                        {
                            dist = 0;
                        }
                    }
                }
                graph2.AddLinkInternal(link, n.Id, dist);
            }
        }
        graph2.CalculateShortestPaths();

        // game loop
        while (true)
        {
            int SI = int.Parse(Console.ReadLine()); // The index of the node on which the Skynet agent is positioned this turn
            links.CalculateShortestPaths();
            int minDist = 99999;

            int minStart = -1;
            int minEnd = -1;
            Dictionary<int, int> gatewayLinkToCount = new Dictionary<int, int>();
            foreach (int gateway in gateways)
            {
                Console.Error.WriteLine($"Checking: {gateway}");
                int start = gateway;
                int end = links.GetShortestPath(gateway, SI);
                if (gatewayLinkToCount.ContainsKey(end))
                {
                    gatewayLinkToCount[end] += 1;
                }
                else
                {
                    gatewayLinkToCount[end] = 0;
                }

            }

            foreach(int gateway in gateways)
            {
                int start = gateway;
                int end = links.GetShortestPath(gateway, SI);
                int dist = links.GetShortestPathDistance(gateway, SI);
                if (dist < minDist || (minEnd != -1 && dist == minDist && gatewayLinkToCount[minEnd] < gatewayLinkToCount[end]))
                {
                    minEnd = end;
                    minDist = dist;
                    minStart = start;
                    Console.Error.WriteLine($"min path: {minStart}, {minEnd}, {minDist}");
                }
            }

            if(minDist > 1)
            {
                int minDist2 = 99999;
                int minStart2 = -1;
                int minEnd2 = -1;
                foreach(int gateway in gateways)
                {
                    foreach(int gateway2 in gateways)
                    {
                        if (gateway == gateway2)
                            continue;

                        int start = gateway;
                        int end = links.GetShortestPath(gateway, gateway2);
                        int dist = links.GetShortestPathDistance(gateway, gateway2);
                        if(dist < minDist2)
                        {
                            minEnd2 = end;
                            minStart2 = start;
                            minDist2 = dist;
                        }
                        else if(dist == minDist2)
                        {
                            int i1 = graph2.GetShortestPathDistance(SI, end);
                            int i2 = graph2.GetShortestPathDistance(SI, minEnd2);
                            Console.Error.WriteLine($"SI: {SI}, end: {end}, minEnd2: {minEnd2}");
                            Console.Error.WriteLine($"i1: {i1}, i2: {i2}");
                            if(i1 < i2)
                            {
                                minEnd2 = end;
                                minStart2 = start;
                                minDist2 = dist;
                            }
                            
                        }
                    }
                }
                if(minEnd2 != -1)
                {
                    minEnd = minEnd2;
                    minStart = minStart2;
                    minDist = minDist2;
                }
            }


            links.RemoveLink(minStart, minEnd);
            // Example: 0 1 are the indices of the nodes you wish to sever the link between
            Console.WriteLine(minStart + " " + minEnd);
        }
    }
}