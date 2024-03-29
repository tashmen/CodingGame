﻿using System;
using System.Linq;
using System.Collections.Generic;
using GameSolution.Utility;
using GameSolution.Entities;
using GameSolution.Moves;
using System.Diagnostics;
using Algorithms;
using Algorithms.Trees;

public class GamePlayer
{
    static void Main(string[] args)
    {
        string[] inputs;
        bool isFirstRound = true;

        //Minimax search = new Minimax();
        MonteCarloTreeSearch search = new MonteCarloTreeSearch(false, MonteCarloTreeSearch.SearchStrategy.Sequential);

        GameState game = new GameState();

        int numberOfCells = int.Parse(Console.ReadLine()); // 37
        for (int i = 0; i < numberOfCells; i++)
        {
            inputs = Console.ReadLine().Split(' ');
            int index = int.Parse(inputs[0]); // 0 is the center cell, the next cells spiral outwards
            int richness = int.Parse(inputs[1]); // 0 if the cell is unusable, 1-3 for usable cells
            int neigh0 = int.Parse(inputs[2]); // the index of the neighbouring cell for each direction
            int neigh1 = int.Parse(inputs[3]);
            int neigh2 = int.Parse(inputs[4]);
            int neigh3 = int.Parse(inputs[5]);
            int neigh4 = int.Parse(inputs[6]);
            int neigh5 = int.Parse(inputs[7]);
            int[] neighs = new int[]{ neigh0, neigh1, neigh2, neigh3, neigh4, neigh5 };
            Cell cell = new Cell(index, richness, neighs);
            game.board.Insert(cell.index, cell);
        }
        SeedMapMask seedMapMask = new SeedMapMask(game.board);
        game.seedMapMask = seedMapMask;

        ShadowMap shadowMap = new ShadowMap(game.board);
        game.shadowMap = shadowMap;

        //ShadowMapMask shadowMapMask = new ShadowMapMask(game.board);
        //game.shadowMapMask = shadowMapMask;

        // game loop
        while (true)
        {
            //Note: loop appears to start when we read the first item...
            game.day = int.Parse(Console.ReadLine()); // the game lasts 24 days: 0-23

            Stopwatch watch = new Stopwatch();
            watch.Start();

            game.RemoveTrees();
            game.ResetPlayers();

            
            game.nutrients = int.Parse(Console.ReadLine()); // the base score you gain from the next COMPLETE action
            inputs = Console.ReadLine().Split(' ');
            game.me.sun = int.Parse(inputs[0]); // your sun points
            game.me.score = int.Parse(inputs[1]); // your current score
            game.me.isWaiting = false;
            inputs = Console.ReadLine().Split(' ');
            game.opponent.sun = int.Parse(inputs[0]); // opponent's sun points
            game.opponent.score = int.Parse(inputs[1]); // opponent's score
            game.opponent.isWaiting = inputs[2] != "0"; // whether your opponent is asleep until the next day

            int numberOfTrees = int.Parse(Console.ReadLine()); // the current amount of trees
            for (int i = 0; i < numberOfTrees; i++)
            {
                inputs = Console.ReadLine().Split(' ');
                int cellIndex = int.Parse(inputs[0]); // location of this tree
                int size = int.Parse(inputs[1]); // size of this tree: 0-3
                bool isMine = inputs[2] != "0"; // 1 if this is your tree
                bool isDormant = inputs[3] != "0"; // 1 if this tree is dormant
                Tree tree = new Tree(cellIndex, size, isMine, isDormant);
                game.AddTree(tree);
            }

            List<long> possibleActions = new List<long>();
            int numberOfPossibleMoves = int.Parse(Console.ReadLine());
            for (int i = 0; i < numberOfPossibleMoves; i++)
            {
                string possibleMove = Console.ReadLine();
                long movePlayer = Move.Parse(possibleMove);
                possibleActions.Add(movePlayer);
            }

            //Console.Error.WriteLine($"After parsing: {watch.ElapsedMilliseconds}ms");

            //game.me.possibleMoves = possibleActions;
            game.UpdateGameState();
            //Console.Error.WriteLine($"After updating gamestate: {watch.ElapsedMilliseconds}ms");
            /*
            if(game.me.possibleMoves.Count != possibleActions.Count)
            {
                Console.Error.WriteLine($"{possibleActions.Where(m => m.type == Actions.SEED).Count()}");
                Console.Error.WriteLine($"{possibleActions.Where(m => m.type == Actions.WAIT).Count()}");
                Console.Error.WriteLine($"{possibleActions.Where(m => m.type == Actions.COMPLETE).Count()}");
                Console.Error.WriteLine($"{possibleActions.Where(m => m.type == Actions.GROW).Count()}");

                Console.Error.WriteLine("Calculated Actions:");
                Console.Error.WriteLine($"{game.me.possibleMoves.Where(m => m.type == Actions.SEED).Count()}");
                Console.Error.WriteLine($"{game.me.possibleMoves.Where(m => m.type == Actions.WAIT).Count()}");
                Console.Error.WriteLine($"{game.me.possibleMoves.Where(m => m.type == Actions.COMPLETE).Count()}");
                Console.Error.WriteLine($"{game.me.possibleMoves.Where(m => m.type == Actions.GROW).Count()}");
                throw new Exception($"Possible moves not matching! ");
            }
            */

            GC.Collect();

            long move;
            int limit = isFirstRound ? 998 : 98;
            if (true)
            {
                //Console.Error.WriteLine($"Before search: {watch.ElapsedMilliseconds}ms");
                search.SetState(game, true, true);
                object moveToPlay = search.GetNextMove(watch, limit, -1, 20, 1);
                move = (long)moveToPlay;
            }
            else
            {
                GameHelper gameHelper = new GameHelper(game, possibleActions);
                move = gameHelper.GetNextMove();
            }

            watch.Stop();
            Console.Error.WriteLine($"ms: {watch.ElapsedMilliseconds} / {limit}");


            isFirstRound = false;
            Console.WriteLine(Move.ToString(move));
        }
    }
}