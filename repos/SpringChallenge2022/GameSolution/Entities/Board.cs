using Algorithms.Space;
using GameSolution.Game;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GameSolution.Entities
{
    public class Board
    {
        public static int MaxX = 17630;
        public static int MaxY = 9000;
        public static int MinX = 0;
        public static int MinY = 0;
        public static int NearBaseDistance = 5000;
        public static int WindCastDistance = 1280;
        public static int WindPushDistance = 2200;
        public static int ControlCastDistance = 2200;
        public static int ShieldCastDistance = 2200;
        public static Point2d Origin = new Point2d(MaxX / 2, MaxY / 2);
        public IList<BoardPiece> boardPieces { get; set; }
        public Base myBase { get; set; }
        public Base opponentBase { get; set; }
        public IList<Monster> monsters { get; set; }
        public IList<Hero> myHeroes { get; set; }
        public IList<Hero> opponentHeroes { get; set; }

        public Dictionary<int, long> spellsInEffect { get; set; }


        public Board(IList<BoardPiece> boardPieces )
        {
            this.boardPieces = boardPieces;
            spellsInEffect = new Dictionary<int, long>();
            SetupBoard();
        }

        public Board(Board board)
        {
            boardPieces = board.boardPieces.Select(bp => bp.Clone()).ToList();
            spellsInEffect = board.spellsInEffect;
            SetupBoard();
        }

        public void ResetSpells()
        {
            spellsInEffect = new Dictionary<int, long>();
        }

        public void SetupBoard()
        {
            monsters = new List<Monster>();
            myHeroes = new List<Hero>();
            opponentHeroes = new List<Hero>();
            foreach(BoardPiece piece in boardPieces)
            {
                if(piece is Base)
                {
                    Base b = piece as Base;
                    if (b.isMax.Value)
                    {
                        myBase = b;
                    }
                    else
                    {
                        opponentBase = b;
                    }
                }
                if(piece is Monster)
                {
                    monsters.Add(piece as Monster);
                }
                if(piece is Hero)
                {
                    Hero h = piece as Hero;
                    if (h.isMax.Value)
                    {
                        myHeroes.Add(h);
                    }
                    else
                    {
                        opponentHeroes.Add(h);
                    }
                }
            }
        }

        public void ApplyMove(Move move, bool isMax)
        {
            var heroes = isMax ? myHeroes : opponentHeroes;
            var b = isMax ? myBase : opponentBase;
            
            foreach(Hero hero in heroes)
            {
                long heroMove = move.GetMove(hero.id);
                switch (HeroMove.GetMoveType(heroMove))
                {
                    case MoveType.MOVE:
                        var point = new Point2d(HeroMove.GetX(heroMove), HeroMove.GetY(heroMove));
                        var newPoint = Space2d.TranslatePoint(hero.point, point, Hero.Speed);
                        newPoint.SymmetricTruncate(Origin);

                        hero.point = new Point2d((int)newPoint.x, (int)newPoint.y);

                        if (hero.GetDistance(myBase) <= NearBaseDistance || hero.GetDistance(opponentBase) <= NearBaseDistance)
                            hero.isNearBase = true;
                        else hero.isNearBase = false;
                        break;
                    case MoveType.SPELL:
                        BoardPiece piece;
                        if(b.mana >= 10)
                        {
                            switch (HeroMove.GetSpellType(heroMove))
                            {
                                case SpellType.SHIELD:
                                    piece = boardPieces.First(p => p.id == HeroMove.GetTargetId(heroMove));
                                    if (hero.GetDistance(piece) <= ShieldCastDistance)
                                    {
                                        if (piece.shieldLife == 0)
                                            spellsInEffect[hero.id] = heroMove;
                                        
                                        b.mana -= 10;
                                    }
                                    break;
                                case SpellType.CONTROL:
                                    piece = boardPieces.First(p => p.id == HeroMove.GetTargetId(heroMove));
                                    if (hero.GetDistance(piece) <= ControlCastDistance)
                                    {
                                        if(piece.shieldLife == 0)
                                            spellsInEffect[hero.id] = heroMove;

                                        b.mana -= 10;
                                    }
                                    break;
                                case SpellType.WIND:
                                    spellsInEffect[hero.id] = heroMove;
                                    b.mana -= 10;
                                    break;
                            }
                        }
                        break;
                }
            }
        }

        public void ApplyControlAndShieldSpells()
        {

        }

        public void ApplyWindSpells()
        {
            Dictionary<int, Point2d> windApplied = new Dictionary<int, Point2d>();
            foreach(var item in spellsInEffect)
            {
                var spellType = HeroMove.GetSpellType(item.Value);
                var spellPoint = new Point2d(HeroMove.GetX(item.Value), HeroMove.GetY(item.Value));
                if (spellType == SpellType.WIND)
                {
                    ApplyWindSpell(myHeroes, opponentHeroes, item.Key, spellPoint, windApplied);
                    ApplyWindSpell(opponentHeroes, myHeroes, item.Key, spellPoint, windApplied);
                }
            }

            if(windApplied.Count > 0)
            {
                foreach(var piece in boardPieces)
                {
                    Point2d vector;
                    if(windApplied.TryGetValue(piece.id, out vector))
                    {
                        vector.Multiply(WindPushDistance);
                        vector.SymmetricTruncate(Origin);
                        piece.point.Add(vector);
                        piece.isWinded = true;
                    }
                }
            }
        }

        private void ApplyWindSpell(IList<Hero> castingHeroes, IList<Hero> targetedHeroes, int spellCastBy, Point2d spellPoint, Dictionary<int, Point2d> windApplied)
        {
            foreach (Hero hero in castingHeroes)
            {
                if (hero.id == spellCastBy)
                {
                    Point2d directionVector = Space2d.CreateVector(hero.point, spellPoint).Normalize();
                    foreach (Monster monster in monsters)
                    {
                        SetWindCastVector(hero, monster, windApplied, directionVector);
                    }

                    foreach (Hero targetHero in targetedHeroes)
                    {
                        SetWindCastVector(hero, targetHero, windApplied, directionVector);
                    }
                }
            }
        }

        private void SetWindCastVector(Hero hero, BoardPiece targetPiece, Dictionary<int, Point2d> windApplied, Point2d directionVector)
        {
            if (hero.GetDistance(targetPiece) <= WindCastDistance && targetPiece.shieldLife == 0)
            {
                var targetId = targetPiece.id;
                Point2d vector;
                if (windApplied.TryGetValue(targetId, out vector))
                {
                    vector.Add(directionVector);
                }
                else
                {
                    vector = directionVector.Clone();
                }
                windApplied[targetId] = vector;
            }
        }


        public Board Clone()
        {
            return new Board(this);
        }

        public double? GetWinner()
        {
            if(myBase.health == 0)
            {
                return -1;
            }
            else if (opponentBase.health == 0)
            {
                return 1;
            }

            return null;
        }
    }
}
