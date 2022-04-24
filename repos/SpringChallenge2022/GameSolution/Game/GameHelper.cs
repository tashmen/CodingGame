using GameSolution.Algorithms;
using GameSolution.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace GameSolution.Game
{
    public class GameHelper
    {
        public enum Strategy
        {
            Defense = 0,
            Scout = 1,
            Attack =  2
        }

        public Board board { get; set; }
        public List<Tuple<double, Monster>> distToAllMonsters { get; set; }
        public List<Tuple<double, Hero>> distToOpponentHeroes { get; set; }
        public List<Tuple<double, Hero>> distToMyHeroes { get; set; }


        public Move GetBestMove(GameState state)
        {
            board = state.board;

            CalculateHelper();

            var strategy = DetermineStrategy();


            Move move = null;

            switch (strategy)
            {
                case Strategy.Defense:
                    move = DefenseStrategy();
                    break;
                case Strategy.Attack:
                    break;
                case Strategy.Scout:
                    break;
            }


            return move;
        }

        public void CalculateHelper()
        {
            distToAllMonsters = new List<Tuple<double, Monster>>();
            foreach (Monster m in board.monsters)
            {
                if (m.threatForMax.HasValue && !m.threatForMax.Value)
                    continue;

                double dist = board.myBase.GetDistance(m);
                distToAllMonsters.Add(new Tuple<double, Monster>(dist, m));
            }

            distToAllMonsters = distToAllMonsters.OrderBy(t => t.Item1).ToList();

            foreach (var distToMonster in distToAllMonsters)
            {
                Console.Error.WriteLine("Found monster at distance: " + distToMonster.Item1);
            }

            distToOpponentHeroes = new List<Tuple<double, Hero>>();
            foreach (Hero h in board.opponentHeroes)
            {
                double dist = board.myBase.GetDistance(h);
                distToOpponentHeroes.Add(new Tuple<double, Hero>(dist, h));
            }
            distToOpponentHeroes = distToOpponentHeroes.OrderBy(t => t.Item1).ToList();

            distToMyHeroes = new List<Tuple<double, Hero>>();
            foreach (Hero h in board.myHeroes)
            {
                double dist = board.myBase.GetDistance(h);
                distToMyHeroes.Add(new Tuple<double, Hero>(dist, h));
            }
            distToMyHeroes = distToMyHeroes.OrderBy(t => t.Item1).ToList();
        }

        public Strategy DetermineStrategy()
        {
            return Strategy.Defense;
        }

        public Move DefenseStrategy()
        {
            Move move = new Move();

            if (distToAllMonsters.Count == 0)
            {
                if (board.myHeroes[0].isNearBase)
                {
                    move.AddHeroMove(board.opponentBase.x, board.opponentBase.y);
                }
                else
                {
                    var target = GetOutterMidPointOfBase(board.myBase);
                    move.AddHeroMove(target.Item1, target.Item2);
                }

                move.AddHeroMove(board.opponentBase.x, board.myBase.y);
                move.AddHeroMove(board.myBase.x, board.opponentBase.y);
            }
            else
            {
                if (distToAllMonsters.Count == 1)
                {
                    var m = distToAllMonsters[0].Item2;
                    if (CanCastWindOnNearBaseMonster(distToAllMonsters[0].Item2, board.myHeroes[0]))
                    {
                        move.AddSpellMove(board.opponentBase.x, board.opponentBase.y, SpellType.WIND, -99);
                    }
                    else
                    {
                        if (m.GetDistance(board.myBase) < 7000)
                        {
                            move.AddHeroMove(m.x, m.y);
                        }
                        else
                        {
                            var target = GetOutterMidPointOfBase(board.myBase);
                            move.AddHeroMove(target.Item1, target.Item2);
                        }
                            
                    }

                    move.AddHeroMove(m.x, m.y);
                    move.AddHeroMove(m.x, m.y);
                }
                else
                {
                    var m = distToAllMonsters[0].Item2;
                    if(CanCastControlOnNearBaseMonster(m, board.myHeroes[0]))
                    {
                        var target = GetControlTargetingPointForMonster(m);
                        move.AddSpellMove(target.Item1, target.Item2, SpellType.CONTROL, m.id);
                    }
                    else if (CanCastWindOnNearBaseMonster(m, board.myHeroes[0]))
                    {
                        move.AddSpellMove(board.opponentBase.x, board.opponentBase.y, SpellType.WIND, -99);
                    }
                    else
                    {
                        if (m.GetDistance(board.myBase) < 6000)
                        {
                            move.AddHeroMove(m.x, m.y);
                        }
                        else
                        {
                            var target = GetOutterMidPointOfBase(board.myBase);
                            move.AddHeroMove(target.Item1, target.Item2);
                        }
                    }

                    if(distToOpponentHeroes.Count > 0 && CanCastControlOnNearBaseHero(distToOpponentHeroes[0].Item2, board.myHeroes[1]))
                    {
                        move.AddSpellMove(board.opponentBase.x, board.opponentBase.y, SpellType.CONTROL, distToOpponentHeroes[0].Item2.id);
                    }
                    else move.AddHeroMove(m.x, m.y);

                    m = distToAllMonsters[1].Item2;
                    move.AddHeroMove(m.x, m.y);
                }
            }

            return move;
        }


        public bool CanCastControlOnNearBaseMonster(Monster monster, Hero hero)
        {
            return CanCastControlOrShield(monster, hero) && board.myBase.mana >= 30 && board.myBase.GetDistance(monster) > 5200 && monster.health > 15 && (!monster.threatForMax.HasValue || monster.threatForMax.Value);
        }

        public bool CanCastControlOnNearBaseHero(BoardPiece piece, Hero hero)
        {
            var countCloseMonsters = 0;
            foreach(var distToMonster in distToAllMonsters)
            {
                if(distToMonster.Item1 < 5000)
                {
                    countCloseMonsters++;
                }
            }

            return hero.isNearBase && board.myBase.mana >= 20 && countCloseMonsters > 1 && CanCastControlOrShield(piece, hero);
        }

        public bool CanCastWindOnNearBaseMonster(BoardPiece piece, Hero hero)
        {
            return piece.isNearBase && CanCastWind(piece, hero);
        }


        

        public Tuple<int, int>? MaximizeTargetsOnAllMonstersInRange(Hero hero)
        {
            var monstersInRange = GetMonstersInRange(hero);
            if (!monstersInRange.Any())
                return null;

            if(monstersInRange.Count == 1)
            {
                return new Tuple<int, int>(monstersInRange[0].x, monstersInRange[0].y);
            }

            


        }

        public List<Monster> GetMonstersInRange(Hero hero)
        {
            List<Monster> monsters = new List<Monster>();
            foreach(var monster in board.monsters)
            {
                if(IsMonsterInRangeOfHero(monster, hero))
                {
                    monsters.Add(monster);
                }
            }
            return monsters;
        }

        public bool IsMonsterInRangeOfHero(Monster monster, Hero hero)
        {
            var dist = hero.GetDistance(monster);
            if (dist < hero.speed + hero.range)
                return true;
            return false;
        }

        public Tuple<int, int> GetOutterMidPointOfBase(Base b)
        {
            int direction = GetBaseDirectionality(b);

            int distance = 3535;

            int x = b.x + direction * distance;
            int y = b.y + direction * distance;

            return new Tuple<int, int>(x, y);
        }

        public int GetBaseDirectionality(Base b)
        {
            int direction = 1;

            if (b.x != 0)
            {
                direction = -1;
            }
            return direction;
        }

        public Tuple<int, int> GetControlTargetingPointForMonster(Monster monster)
        {
            int x, x1, x2;
            int y, y1, y2;
            Base b = board.opponentBase;

            int targetingDifference = 4700;
            int direction = GetBaseDirectionality(b);

            x1 = b.x + direction * targetingDifference;
            y1 = b.y;

            x2 = b.x;
            y2 = b.y + direction * targetingDifference;

            double dist1, dist2;

            dist1 = BoardPiece.GetDistance(x1, y1, monster.x, monster.y);
            dist2 = BoardPiece.GetDistance(x2, y2, monster.x, monster.y);

            if (dist1 < dist2) 
            {
                x = x1;
                y = y1;
            }
            else
            {
                x = x2;
                y = y2;
            }

            return new Tuple<int, int>(x, y);
        }

        public bool CanCastControlOrShield(BoardPiece piece, Hero hero)
        {
            return hero.GetDistance(piece) <= 2200 && piece.shieldLife == 0 && board.myBase.mana >= 10;
        }

        public bool CanCastWind(BoardPiece piece, Hero hero)
        {
            return hero.GetDistance(piece) <= 1280 && piece.shieldLife == 0 && board.myBase.mana >= 10;
        }
    }
}
