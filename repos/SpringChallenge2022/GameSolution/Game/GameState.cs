using Algorithms.GameComponent;
using GameSolution.Entities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace GameSolution.Game
{
    public class GameState : IGameState
    {
        public Board board { get; set; }

        public int turn { get; set; }

        public Move? maxMove { get; set; }
        public Move? minMove { get; set; }

        public IList<Move> possibleMaxMoves { get; set; }
        public IList<Move> possibleMinMoves { get; set; }

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

            possibleMaxMoves = CalculateMoves(true);
            possibleMinMoves = CalculateMoves(false);
        }

        public GameState(GameState state)
        {
            board = state.board.Clone();
            turn = state.turn;
            maxMove = state.maxMove != null ? state.maxMove.Clone() : null;
            minMove= state.minMove != null ? state.minMove.Clone() : null;

            possibleMaxMoves = state.possibleMaxMoves;
            possibleMinMoves = state.possibleMinMoves;
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

        private IList<Move> CalculateMoves(bool isMax)
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

            if (isMax)
            {
                finalPossibleMoves.Add(gameHelper.GetBestMove(this));
            }


            /*
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
            */

            //Build movement moves
            
            foreach (BoardPiece piece in board.boardPieces)
            {
                //Do not move towards enemy heroes
                if (piece is Hero && piece.isMax.Value != isMax)
                    continue;
                //Do not move towards monsters that are targeting the enemy
                if (piece is Monster && ((Monster)piece).threatForMax.HasValue && ((Monster)piece).threatForMax.Value != isMax)
                    continue;

                for (int i = 0; i < myHeroes.Count; i++)
                {
                    if (!myHeroes[i].isControlled)
                    {
                        if (heroMoves[i].Count > 1)
                            continue;
                        heroMove = HeroMove.CreateHeroMove(piece.x, piece.y);
                        heroMoves[i].Add(heroMove);
                    }
                }
            }
            


            //Build spell moves
            /*
            foreach (BoardPiece piece in board.boardPieces)
            {
                if (piece is Base)
                {
                    //do nothing
                }
                else
                {
                    if (myBase.mana >= 10 && piece.shieldLife == 0)
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
                                    heroMove = HeroMove.CreateWindSpellMove(myBase.x, myBase.y);
                                    heroMoves[i].Add(heroMove);
                                    heroMove = HeroMove.CreateWindSpellMove(opponentBase.x, opponentBase.y);
                                    heroMoves[i].Add(heroMove);
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
                                    heroMove = HeroMove.CreateShieldSpellMove(piece.id);
                                    heroMoves[i].Add(heroMove);
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
                                    heroMove = HeroMove.CreateControlSpellMove(myBase.x, myBase.y, piece.id);
                                    heroMoves[i].Add(heroMove);

                                    heroMove = HeroMove.CreateControlSpellMove(opponentBase.x, opponentBase.y, piece.id);
                                    heroMoves[i].Add(heroMove);
                                }
                            }
                        }
                    }
                }
            }
            */


            //Take each single hero move and combine them into a set of 3 hero moves using all permutations
            foreach (HeroMove heroMove1 in heroMoves[0])
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

            //Console.Error.WriteLine("Total Move Count: " + finalPossibleMoves.Count);
            return finalPossibleMoves;
        }

        public IList GetPossibleMoves(bool isMax)
        {
            return isMax ? (IList)possibleMaxMoves : (IList)possibleMinMoves;
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
