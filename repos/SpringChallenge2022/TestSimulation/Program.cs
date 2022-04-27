using Algorithms.Trees;
using GameSolution.Entities;
using GameSolution.Game;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace TestSimulation
{
    internal class Program
    {
        static void Main(string[] args)
        {
            GameState game = new GameState();
            List<BoardPiece> boardPieces = new List<BoardPiece>();
            boardPieces.Add(new Base(-1, 0, 0, true, 3, 0));
            boardPieces.Add(new Base(-2, 17630, 9000, false, 3, 0));
            boardPieces.Add(new Hero(0, 0, 0, true, 0, false, 0, 0, true));
            boardPieces.Add(new Hero(1, 0, 0, true, 0, false, 0, 0, true));
            boardPieces.Add(new Hero(2, 0, 0, true, 0, false, 0, 0, true));
            Board board = new Board(boardPieces);
            game.SetNextTurn(board);
            Minimax search = new Minimax();
            Stopwatch watch = new Stopwatch();

            do
            {
                watch.Reset();
                watch.Start();
                search.SetState(game, true, false);
                Move move = (Move)search.GetNextMove(watch, 48000, 4);
                game.ApplyMove(move, true);
                watch.Stop();


                watch.Reset();
                watch.Start();
                search.SetState(game, false, false);
                move = (Move)search.GetNextMove(watch, 48000, 4);
                game.ApplyMove(move, false);
                watch.Stop();
            }
            while (!game.GetWinner().HasValue);
        }
    }
}
