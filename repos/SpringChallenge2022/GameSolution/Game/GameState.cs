using Algorithms;
using GameSolution.Entities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GameSolution.Game
{
    public class GameState : IGameState
    {
        public Board board { get; set; }

        public int turn { get; set; }

        public GameState()
        {
            turn = 0;
        }

        public void SetNextTurn(Board board)
        {
            turn++;
            this.board = board;
        }

        public GameState(GameState state)
        {
            board = state.board.Clone();
        }

        public void ApplyMove(object move, bool isMax)
        {

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
            throw new NotImplementedException();
        }

        public object GetMove(bool isMax)
        {
            return null;
        }

        public IList GetPossibleMoves(bool isMax)
        {
            IList<Move> finalPossibleMoves = new List<Move>();

            var myHeroes = isMax ? board.myHeroes : board.opponentHeroes;
            var myBase = isMax ? board.myBase : board.opponentBase;
            var opponentBase = isMax ? board.opponentBase : board.myBase;

            if(myHeroes.Count != 3)
            {
                throw new Exception("must have 3 heroes on the map!");
            }

            IList<Move>[] heroMoves = new List<Move>[3];
            Move move;
            //Initialize with wait move
            for (int i = 0; i < 3; i++)
            {
                move = new Move();
                move.AddWaitMove();
                heroMoves[i] = new List<Move>();
                heroMoves[i].Add(move);
            }


            //Build movement moves
            foreach (BoardPiece piece in board.boardPieces)
            {
                //Do not move towards enemy heroes
                if (piece is Hero && piece.isMax.Value != isMax)
                    continue;
                //Do not move towards monsters that are targeting the enemy
                if(piece is Monster && ((Monster)piece).threatForMax.HasValue && ((Monster)piece).threatForMax.Value != isMax)
                    continue;

                for (int i = 0; i < 3; i++)
                {
                    if (!myHeroes[i].isControlled)
                    {
                        move = new Move();
                        move.AddHeroMove(piece.x, piece.y);
                        heroMoves[i].Add(move);
                    }
                }
            }

            //Build spell moves
            foreach (BoardPiece piece in board.boardPieces)
            {
                if(piece is Base)
                {
                    //do nothing
                }
                else
                {
                    if(myBase.mana >= 10 && piece.shieldLife == 0)
                    {
                        //do not wind friendly heroes
                        if (!(piece is Hero && piece.isMax.Value == isMax))
                        {//Wind spell
                            for (int i = 0; i < 3; i++)
                            {
                                Hero h = myHeroes[i];
                                //Console.Error.WriteLine("Piece: " + piece.ToString());
                                //Check range and shield, 
                                if (h.GetDistance(piece) < 1280)
                                {
                                    move = new Move();
                                    move.AddSpellMove(myBase.x, myBase.y, SpellType.WIND, -99);
                                    heroMoves[i].Add(move);
                                    move = new Move();
                                    move.AddSpellMove(opponentBase.x, opponentBase.y, SpellType.WIND, -99);
                                    heroMoves[i].Add(move);
                                }
                            }
                        }
                        //do not shield opponent heroes
                        if (!(piece is Hero && piece.isMax.Value != isMax))
                        {//Shield spell
                            for (int i = 0; i < 3; i++)
                            {
                                Hero h = myHeroes[i];
                                //Check range and shield; 
                                if (h.GetDistance(piece) < 2200)
                                {
                                    move = new Move();
                                    move.AddSpellMove(-1, -1, SpellType.SHIELD, piece.id);
                                    heroMoves[i].Add(move);
                                }
                            }
                        }
                        //do not control friendly heroes
                        if (!(piece is Hero && piece.isMax.Value == isMax))
                        {//Control spell
                            for (int i = 0; i < 3; i++)
                            {
                                Hero h = myHeroes[i];
                                //Check range and shield; 
                                if (h.GetDistance(piece) < 2200)
                                {
                                    move = new Move();
                                    move.AddSpellMove(myBase.x, myBase.y, SpellType.CONTROL, piece.id);
                                    heroMoves[i].Add(move);

                                    move = new Move();
                                    move.AddSpellMove(opponentBase.x, opponentBase.y, SpellType.CONTROL, piece.id);
                                    heroMoves[i].Add(move);
                                }
                            }
                        }
                    }
                }
            }

            //Take each single hero move and combine them into a set of 3 hero moves using all permutations
            foreach(Move heroMove1 in heroMoves[0])
            {
                foreach (Move heroMove2 in heroMoves[1])
                {
                    foreach (Move heroMove3 in heroMoves[2])
                    {
                        move = new Move();
                        move.AddMove(heroMove1);
                        move.AddMove(heroMove2);
                        move.AddMove(heroMove3);
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
