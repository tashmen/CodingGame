﻿using GameSolution.Entities;

namespace GameSolution.Game
{
     public class GameHelper
     {
        GameState State { get; set; }
        public GameHelper(GameState state)
        {
          State = state;
        }

        public Move GetMove()
        {
          Move move = new Move();
          move.AddAction(MoveAction.CreateWait());
          return move;
        }
     }
}

