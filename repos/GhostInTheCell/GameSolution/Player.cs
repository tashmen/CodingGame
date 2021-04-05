using System;
using System.Linq;
using System.Collections.Generic;
using GameSolution.Utility;
using GameSolution.Entities;
using GameSolution.Moves;

/**
 * Auto-generated code below aims at helping you parse
 * the standard input according to the problem statement.
 **/
public class Player
{
    static void Main(string[] args)
    {
        FactoryLinks links = new FactoryLinks();
        string[] inputs;
        int factoryCount = int.Parse(Console.ReadLine()); // the number of factories
        int linkCount = int.Parse(Console.ReadLine()); // the number of links between factories
        for (int i = 0; i < linkCount; i++)
        {
            inputs = Console.ReadLine().Split(' ');
            int factory1 = int.Parse(inputs[0]);
            int factory2 = int.Parse(inputs[1]);
            int distance = int.Parse(inputs[2]);
            links.AddLink(factory1, factory2, distance);
        }
        Console.Error.WriteLine(factoryCount);
        Console.Error.WriteLine(linkCount);
        links.CalculateShortestPaths();

        GameState gs = new GameState(links);
        
        // game loop
        while (true)
        {
            int entityCount = int.Parse(Console.ReadLine()); // the number of entities (e.g. factories and troops)
            List<Entity> entities = new List<Entity>();
            Console.Error.WriteLine(entities.Count);
            for (int i = 0; i < entityCount; i++)
            {
                inputs = Console.ReadLine().Split(' ');
                int entityId = int.Parse(inputs[0]);
                string entityType = inputs[1];
                int arg1 = int.Parse(inputs[2]);
                int arg2 = int.Parse(inputs[3]);
                int arg3 = int.Parse(inputs[4]);
                int arg4 = int.Parse(inputs[5]);
                int arg5 = int.Parse(inputs[6]);
                entities.Add(EntityFactory.CreateEntity(entityType, entityId, arg1, arg2, arg3, arg4, arg5));
            }

            Console.Error.WriteLine($"Found invalid entities: {entities.Where(e => e.Id == -1).Count()}");
            gs.SetEntities(entities);
            gs.ShowStats();

            GameHelper gh = new GameHelper(gs);
            MoveList moves = gh.PickMoves();
            moves.PlayMoves();
        }
    }
}