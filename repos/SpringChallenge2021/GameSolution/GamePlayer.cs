using System;
using System.Linq;
using System.Collections.Generic;
using GameSolution.Utility;
using GameSolution.Entities;
using GameSolution;
using GameSolution.Moves;
using static GameSolution.Constants;
using System.Diagnostics;
using GameSolution.Algorithm;

public class GamePlayer
{
    static void Main(string[] args)
    {
        string[] inputs;

        MonteCarloTreeSearch search = new MonteCarloTreeSearch();

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
            List<int> neighs = new List<int>(){ neigh0, neigh1, neigh2, neigh3, neigh4, neigh5 };
            Cell cell = new Cell(index, richness, neighs);
            game.board.Add(cell);
        }

        game.BuildCellNeighbors();

        // game loop
        while (true)
        {
            Stopwatch watch = new Stopwatch();
            watch.Start();

            game.ResetTrees();
            game.ResetPlayers();

            game.day = int.Parse(Console.ReadLine()); // the game lasts 24 days: 0-23
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
                Cell cell = game.board.First(c => c.index == tree.cellIndex);
                cell.AddTree(tree);
            }

            List<Move> possibleActions = new List<Move>();
            int numberOfPossibleMoves = int.Parse(Console.ReadLine());
            for (int i = 0; i < numberOfPossibleMoves; i++)
            {
                string possibleMove = Console.ReadLine();
                possibleActions.Add(Move.Parse(possibleMove));
            }
            
            game.me.possibleMoves = possibleActions;

            game.UpdateGameState();

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

            GameHelper gameHelper = new GameHelper(game, possibleActions);
            Move move = gameHelper.GetNextMove();

            int limit = game.day == 0 ? 1000 : 50;
            
            if(limit - watch.ElapsedMilliseconds > 20)
            {
                search.SetState(game);
                IMove moveToPlay = search.GetNextMove(watch, limit);
                MoveSimultaneous moveSimultaneous = moveToPlay as MoveSimultaneous;
                Console.Error.WriteLine(moveSimultaneous.myMove.ToString());
                Console.Error.WriteLine(moveSimultaneous.opponentMove.ToString());
            }

            watch.Stop();
            Console.Error.WriteLine($"ms: {watch.ElapsedMilliseconds} / 100");

            Console.WriteLine(move);
        }
    }

    private static List<T> List<T>()
    {
        throw new NotImplementedException();
    }
}