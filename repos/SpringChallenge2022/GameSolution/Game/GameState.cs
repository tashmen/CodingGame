using Algorithms.GameComponent;
using Algorithms.Space;
using GameSolution.Entities;
using System;
using System.Collections;
using System.Collections.Generic;

namespace GameSolution.Game
{
    public class GameState : IGameState
    {
        public bool enableLogging = false;
        public Board board { get; set; }

        public int turn { get; set; }

        public int myManaGained = 0;
        public int opponentManaGained = 0;

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

        public GameState(GameState state)
        {
            board = state.board.Clone();
            turn = state.turn;
            maxMove = state.maxMove;
            minMove = state.minMove;
            myManaGained = state.myManaGained;
            opponentManaGained = state.opponentManaGained;

            possibleMaxMoves = state.possibleMaxMoves;
            possibleMinMoves = state.possibleMinMoves;
        }

        public void SetNextTurn(Board board, bool simulate = false)
        {
            turn++;
            this.board = board;

            if (simulate)
            {
                bool boardPiecesChanged = false;
                boardPiecesChanged |= DamageMonsters();

                ApplyWindSpells();

                boardPiecesChanged |= MoveMonsters();

                ApplyControlAndShieldSpells();

                if (boardPiecesChanged)
                {
                    board.SetupBoard();
                }
                board.ResetSpells();
            }

            possibleMaxMoves = CalculateMoves(true);
            possibleMinMoves = CalculateMoves(false);
        }

        public void ApplyWindSpells()
        {
            board.ApplyWindSpells();
        }

        public void ApplyControlAndShieldSpells()
        {
            board.ApplyControlAndShieldSpells();
        }

        public bool DamageMonsters()
        {
            bool boardPiecesChanged = false;
            foreach(Hero hero in board.myHeroes)
            {
                foreach(Monster monster in board.monsters)
                {
                    if(hero.GetDistance(monster) <= Hero.Range)
                    {
                        if(enableLogging)
                            Console.Error.WriteLine($"Hit: {monster.id}");
                        monster.health -= 2;
                        board.myBase.mana+=2;
                        myManaGained+=2;
                    }
                }
            }

            foreach(Hero hero in board.opponentHeroes)
            {
                foreach (Monster monster in board.monsters)
                {
                    if (hero.GetDistance(monster) <= Hero.Range)
                    {
                        monster.health -= 2;
                        board.opponentBase.mana++;
                        opponentManaGained++;
                    }
                }
            }

            for(int i = 0; i<board.monsters.Count; i++)
            {
                var monster = board.monsters[i];
                if(monster.health <= 0)
                {
                    board.boardPieces.Remove(monster);
                    boardPiecesChanged = true;
                }
            }

            return boardPiecesChanged;
        }

        public bool MoveMonsters()
        {
            bool boardPiecesChanged = false;
            foreach (Monster monster in board.monsters)
            {
                if(!monster.isWinded)
                    monster.Move();

                if (!monster.isNearBase)
                {
                    if (monster.x > Board.MaxX || monster.x < Board.MinX || monster.y > Board.MaxY || monster.y < Board.MinY)
                    {
                        board.boardPieces.Remove(monster);
                    }
                }

                boardPiecesChanged = CheckBaseDistance(board.myBase, monster);
                boardPiecesChanged = CheckBaseDistance(board.opponentBase, monster);
            }

            return boardPiecesChanged;
        }

        public bool CheckBaseDistance(Base b, Monster monster)
        {
            bool boardPiecesChanged = false;

            var distance = b.GetDistance(monster);
            if (distance <= Monster.Range)
            {
                b.health--;
                board.boardPieces.Remove(monster);
                boardPiecesChanged = true;
                if (enableLogging)
                {
                    Console.Error.WriteLine($"Monster ran into base: {b.id}, {monster}");
                }
            }

            if (distance <= Monster.TargetingRange)
            {
                /*
                if (enableLogging)
                {
                    Console.Error.WriteLine($"Monster targeting base was moving: {monster}");
                }
                */
                monster.isNearBase = true;
                Point2d vector = Space2d.CreateVector(monster.point, b.point);
                vector.Normalize().Multiply(Monster.Speed).SymmetricTruncate(Board.Origin);
                monster.vx = (int)vector.x;
                monster.vy = (int)vector.y;
                /*
                if (enableLogging)
                {
                    Console.Error.WriteLine($"Monster targeting base: {b.id}, {monster}");
                }
                */
            }            

            return boardPiecesChanged;
        }

        public void ApplyMove(object move, bool isMax)
        {
            Move m = (Move)move;
            if (isMax)
            {
                maxMove = m;
                minMove = null;
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
                SetNextTurn(board, true);
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

        public static double totalSightArea = 3 * Math.PI* Hero.SightRange * Hero.SightRange;
        public double Evaluate(bool isMax)
        {
            double value = 0;

            var myBase = board.myBase;
            var opponentBase = board.opponentBase;
            var myHeroes = board.myHeroes;

            var sightBonus = (CalculateSightArea() / totalSightArea) *  0.20 ;
            var manaGainBonus = ((myManaGained - opponentManaGained) / 200) * 0.80;

            value += sightBonus + manaGainBonus;
            
            if(value < 0 || value > 1)
            {
                Console.Error.WriteLine($"sight: {sightBonus}, mana gain: {manaGainBonus}");
            }


            return value;
        }

        public double CalculateSightArea()
        {
            var sightArea = totalSightArea;
            foreach(Hero hero in board.myHeroes)
            {
                foreach(Hero hero1 in board.myHeroes)
                {
                    if(hero.id == hero1.id)
                        continue;

                    sightArea -= Space2d.CalculateOverlappingArea(new Circle2d(hero.x, hero.y, Hero.SightRange), new Circle2d(hero1.x, hero1.y, Hero.SightRange));
                }

                sightArea -= Space2d.CalculateOverlappingArea(new Circle2d(hero.x, hero.y, Hero.SightRange), new Circle2d(board.myBase.x, board.myBase.y, Base.SightRange));
            }


            return sightArea < 0 ? 0 : sightArea;
        }

        public object GetMove(bool isMax)
        {
            return isMax ? maxMove : minMove;
        }

        private IList<Move> CalculateMoves(bool isMax)
        {
            IList<Move> finalPossibleMoves = new List<Move>();
            GameHelper gameHelper = new GameHelper(this);

            var myHeroes = isMax ? board.myHeroes : board.opponentHeroes;
            var myBase = isMax ? board.myBase : board.opponentBase;
            var opponentBase = isMax ? board.opponentBase : board.myBase;



            IList<long>[] heroMoves = new List<long>[3];
            long heroMove;

            //Initialize 
            for (int i = 0; i < 3; i++)
            {
                heroMoves[i] = new List<long>();
            }

            if (isMax)
            {
                var move = gameHelper.GetBestMove();
                for (int i = 0; i < 3; i++)
                {
                    heroMoves[i].Add(move.GetMove(i));
                }
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

            for (int i = 0; i < myHeroes.Count; i++)
            {
                Hero h = myHeroes[i];

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
                                //Check range 
                                if (h.GetDistance(piece) <= Board.WindCastDistance)
                                {
                                    //heroMove = HeroMove.CreateWindSpellMove(myBase.x, myBase.y);
                                    //heroMoves[i].Add(heroMove);
                                    heroMove = HeroMove.CreateWindSpellMove(opponentBase.x, opponentBase.y);
                                    heroMoves[i].Add(heroMove);
                                    break;
                                }
                            }
                        }
                    }
                }
            }

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

            for (int i = 0; i < 3; i++)
            {
                heroMove = HeroMove.CreateWaitMove();
                if(heroMoves[i].Count == 0)
                    heroMoves[i].Add(heroMove);
            }

            //Take each single hero move and combine them into a set of 3 hero moves using all permutations
            foreach (long heroMove1 in heroMoves[0])
            {
                foreach (long heroMove2 in heroMoves[1])
                {
                    foreach (long heroMove3 in heroMoves[2])
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
