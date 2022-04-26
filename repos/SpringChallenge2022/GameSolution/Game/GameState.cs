using Algorithms.GameComponent;
using GameSolution.Entities;
using System;
using System.Collections;
using System.Collections.Generic;

namespace GameSolution.Game
{
    public class GameState : IGameState
    {
        public Board board { get; set; }

        public int turn { get; set; }

        public Move? maxMove { get; set; }
        public Move? minMove { get; set; }

        public GameState()
        {
            turn = 0;
            maxMove = null;
            minMove = null;
        }

        public void SetNextTurn(Board board)
        {
            turn++;
            this.board = board;
        }

        public GameState(GameState state)
        {
            board = state.board.Clone();
            turn = state.turn;
            maxMove = state.maxMove != null ? state.maxMove.Clone() : null;
            minMove= state.minMove != null ? state.minMove.Clone() : null;
        }

        public void ApplyMove(object move, bool isMax)
        {
            Move m = (Move)move;
            if (isMax)
            {
                maxMove = m;
            }
            else
            {
                if (maxMove == null)
                    throw new Exception("Expected max to play first.");
                minMove = m;
            }

            if(maxMove != null && minMove != null)
            {
                board.ApplyMove(maxMove, true);
                board.ApplyMove(minMove, false);
                SetNextTurn(board);
            }
        }

        public IGameState Clone()
        {
            return new GameState(this);
        }

        public bool Equals(IGameState state)
        {
            throw new NotImplementedException();
        }

        public double Evaluate(bool isMax)
        {
            double value = 0;

            var myBase = isMax ? board.myBase : board.opponentBase;
            var myHeroes = isMax ? board.myHeroes : board.opponentHeroes;

            value += myBase.health * 2000;//lots of points per base health since this is how we stay alive to win the game

            value += myBase.mana * 0.1;//Small amount of bonus for mana as this give us potential to cast spells

            return value;
        }

        public object GetMove(bool isMax)
        {
            return null;
        }

        public IList GetPossibleMoves(bool isMax)
        {
            IList<Move> finalPossibleMoves = new List<Move>();
            GameHelper gameHelper = new GameHelper(this);

            var myHeroes = isMax ? board.myHeroes : board.opponentHeroes;
            var myBase = isMax ? board.myBase : board.opponentBase;
            var opponentBase = isMax ? board.opponentBase : board.myBase;

            

            IList<HeroMove>[] heroMoves = new List<HeroMove>[3];
            HeroMove heroMove;
            //Initialize with wait move
            for (int i = 0; i < 3; i++)
            {
                heroMove = HeroMove.CreateWaitMove();
                heroMoves[i] = new List<HeroMove>();
                heroMoves[i].Add(heroMove);
            }

            
            for (int i = 0; i < myHeroes.Count; i++)
            {
                var hero = myHeroes[i];
                var target = gameHelper.MaximizeTargetsOnAllMonstersInRange(hero);
                if (target != null)
                {
                    heroMove = HeroMove.CreateHeroMove(target.GetTruncatedX(), target.GetTruncatedY());
                    heroMoves[i].Add(heroMove);
                }
            }

            //Take each single hero move and combine them into a set of 3 hero moves using all permutations
            foreach(HeroMove heroMove1 in heroMoves[0])
            {
                foreach (HeroMove heroMove2 in heroMoves[1])
                {
                    foreach (HeroMove heroMove3 in heroMoves[2])
                    {
                        var move = new Move();
                        move.AddMove(heroMove1, 0);
                        move.AddMove(heroMove2, 1);
                        move.AddMove(heroMove3, 2);
                        finalPossibleMoves.Add(move);
                    }
                }
            }

            return (IList)finalPossibleMoves;
        }

        public double? GetWinner()
        {
            double? winner = board.GetWinner();
            if (this.turn == 220 & !winner.HasValue)
            {
                return 0;
            }
            return winner;
        }
    }
}
