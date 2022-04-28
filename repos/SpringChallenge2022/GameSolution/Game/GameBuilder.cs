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
            new GameState();
            List<BoardPiece> boardPieces = new List<BoardPiece>();
            boardPieces.Add(new Base(BoardPiece.MaxEntityId - 1, 0, 0, true, 3, 0));
            boardPieces.Add(new Base(BoardPiece.MaxEntityId - 2, 17630, 9000, false, 3, 0));
            boardPieces.Add(new Hero(0, 0, 0, true, 0, false, 0, 0, true));
            boardPieces.Add(new Hero(1, 0, 0, true, 0, false, 0, 0, true));
            boardPieces.Add(new Hero(2, 0, 0, true, 0, false, 0, 0, true));
            Board board = new Board(boardPieces);

            if (setNextTurn)
                game.SetNextTurn(board);
            else game.board = board;

            return game;
        }

        public static GameState BuildGameWithSingleMonster()
        {
            GameState game = BuildEmptyGame(false);
            game.board.boardPieces.Add(new Monster(20, 0, 5000, null, 10, 0, false, 0, -300, true, true));
            game.board.SetupBoard();

            game.SetNextTurn(game.board);

            return game;
        }

        public static GameState BuildGameWithSingleMonsterHeadingTowardMax()
        {
            GameState game = BuildEmptyGame(false);
            var nextPoint = Space2d.TranslatePoint(new Point2d(3535, 3535), new Point2d(0, 0), Monster.Speed);
            game.board.boardPieces.Add(new Monster(20, 3535, 3535, null, 10, 0, false, nextPoint.GetTruncatedX() - 3535, nextPoint.GetTruncatedY() - 3535, true, true));
            game.board.SetupBoard();

            game.SetNextTurn(game.board);

            return game;
        }
    }
}
