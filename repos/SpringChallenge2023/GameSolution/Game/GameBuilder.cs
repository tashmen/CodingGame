using Algorithms.Space;
using GameSolution.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace GameSolution.Game
{
    public static class GameBuilder
    {
        public static GameState BuildEmptyGame(bool setNextTurn = true)
        {
            GameState game = new GameState();


            return game;
        }

        public static GameState BuildBasicGame()
        {
            GameState game = new GameState();

            List<Cell> cells = new List<Cell>()
            {
                new Cell(0, 0, 0, new List<int>(){-1,2,-1,-1,-1,11}),
                new Cell(1, 0, 0, new List<int>(){-1,-1,10,-1,3,-1}),
                new Cell(2, 0, 0, new List<int>(){14,16,4,-1,0,-1}),
                new Cell(3, 0, 0, new List<int>(){-1,1,-1,15,17,5}),
                new Cell(4, 0, 0, new List<int>(){16,-1,-1,6,-1,2}),
                new Cell(5, 0, 0, new List<int>(){7,-1,3,17,-1,-1}),
                new Cell(6, 0, 0, new List<int>(){4,-1,-1,8,-1,-1}),
                new Cell(7, 0, 0, new List<int>(){9,-1,-1,5,-1,-1}),
                new Cell(8, (ResourceType)2, 19, new List<int>(){6,-1,-1,18,10,-1}),
                new Cell(9, (ResourceType)2, 19, new List<int>(){19,11,-1,7,-1,-1}),
                new Cell(10, (ResourceType)2, 58, new List<int>(){-1,8,18,20,-1,1}),
                new Cell(11, (ResourceType)2, 58, new List<int>(){21,-1,0,-1,9,19}),
                new Cell(12, 0, 0, new List<int>(){22,24,14,-1,21,29}),
                new Cell(13, 0, 0, new List<int>(){-1,20,28,23,25,15}),
                new Cell(14, 0, 0, new List<int>(){24,-1,16,2,-1,12}),
                new Cell(15, 0, 0, new List<int>(){3,-1,13,25,-1,17}),
                new Cell(16, 0, 0, new List<int>(){-1,-1,-1,4,2,14}),
                new Cell(17, 0, 0, new List<int>(){5,3,15,-1,-1,-1}),
                new Cell(18, 0, 0, new List<int>(){8,-1,-1,26,20,10}),
                new Cell(19, 0, 0, new List<int>(){27,21,11,9,-1,-1}),
                new Cell(20, (ResourceType)2, 41, new List<int>(){10,18,26,28,13,-1}),
                new Cell(21, (ResourceType)2, 41, new List<int>(){29,12,-1,11,19,27}),
                new Cell(22, 0, 0, new List<int>(){-1,-1,24,12,29,-1}),
                new Cell(23, 0, 0, new List<int>(){13,28,-1,-1,-1,25}),
                new Cell(24, (ResourceType)1, 16, new List<int>(){-1,-1,-1,14,12,22}),
                new Cell(25, (ResourceType)1, 16, new List<int>(){15,13,23,-1,-1,-1}),
                new Cell(26, (ResourceType)1, 39, new List<int>(){18,-1,-1,-1,28,20}),
                new Cell(27, (ResourceType)1, 39, new List<int>(){-1,29,21,19,-1,-1}),
                new Cell(28, 0, 0, new List<int>(){20,26,-1,-1,23,13}),
                new Cell(29, 0, 0, new List<int>(){-1,22,12,21,27,-1})
            };
            Board board = new Board();
            board.SetCells(cells);
            game.SetNextTurn(board);

            return game;
        }
    }
}
