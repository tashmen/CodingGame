using Algorithms.Space;
using GameSolution.Entities;
using System;
using System.Collections.Generic;
using System.Linq;

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
        public int turn;

        public GameHelper(GameState state)
        {
            turn = state.turn;
            board = state.board;

            CalculateHelper();
        }

        public Move GetBestMove(GameState state)
        {
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

                foreach(Hero hero in board.opponentHeroes)
                {
                    if(CanCastWind(m, hero))
                    {
                        dist -= 2200;
                    }
                }

                distToAllMonsters.Add(new Tuple<double, Monster>(dist, m));
            }

            distToAllMonsters = distToAllMonsters.OrderBy(t => t.Item1).ToList();

            /*
            foreach (var distToMonster in distToAllMonsters)
            {
                Console.Error.WriteLine("Found monster at distance: " + distToMonster.Item1);
            }
            */
            

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

        public static bool bottomLeftScanner = false;
        public Move DefenseStrategy()
        {
            Move move = new Move();

            var myHeroes = board.myHeroes;
            var opponentHeroes = board.opponentHeroes;

            var goalie = board.myHeroes[0];
            var topRight = board.myHeroes[1];
            var bottomLeft = board.myHeroes[2];

            var myBase = board.myBase;
            var opponentBase = board.opponentBase;

            var myBaseDirection = GetBaseDirectionality(myBase);
            var opponentBaseDirection = GetBaseDirectionality(opponentBase);

            var topRightPoint = new Point2d(myBase.x + (myBaseDirection) * (topRight.sightRange + myBase.sightRange), myBase.y + (myBaseDirection) * topRight.sightRange);
            var bottomLeftPoint = new Point2d(myBase.x + (myBaseDirection) * bottomLeft.sightRange, opponentBase.y + (opponentBaseDirection) * bottomLeft.sightRange);

            if(myBaseDirection < 0)
            {
                var swap = bottomLeftPoint;
                bottomLeftPoint = topRightPoint;
                topRightPoint = swap;
            }

            /*
            var heroes = myHeroes.Select(h => h).ToList();
            goalie = GetHeroClosestToPoint(heroes, myBase.point);
            heroes.Remove(goalie);
            topRight = GetHeroClosestToPoint(heroes, topRightPoint);
            heroes.Remove(topRight);
            bottomLeft = heroes[0];
            */

            if (bottomLeftPoint.Equals(bottomLeft.point))
                bottomLeftScanner = true;

            Console.Error.WriteLine($"goalie: {goalie.id}, topRight: {topRight.id}, bottomLeft: {bottomLeft.id}");


            //When no monsters; spread out to the corners of the map; with one player as "goalie"
            if (distToAllMonsters.Count == 0)
            {
                var target = GetOutterMidPointOfBase(board.myBase);
                move.AddHeroMove(target.Item1, target.Item2, goalie.id);
                move.AddHeroMove(topRightPoint.GetTruncatedX(), topRightPoint.GetTruncatedY(), topRight.id);
                move.AddHeroMove(bottomLeftPoint.GetTruncatedX(), bottomLeftPoint.GetTruncatedY() , bottomLeft.id);
            }
            else
            {
                if (distToAllMonsters.Count == 1)
                {
                    bool hero1CastWind = false;
                    var m = distToAllMonsters[0].Item2;
                    if (CanCastWindOnNearBaseMonster(distToAllMonsters[0].Item2, goalie))
                    {
                        hero1CastWind = true;
                        move.AddSpellMove(opponentBase.x, opponentBase.y, SpellType.WIND, -99, goalie.id);
                    }
                    else
                    {
                        if (m.GetDistance(myBase) < 7000)
                        {
                            move.AddHeroMove(m.x, m.y, goalie.id);
                        }
                        else
                        {
                            var target = GetOutterMidPointOfBase(myBase);
                            move.AddHeroMove(target.Item1, target.Item2, goalie.id);
                        }
                            
                    }

                    
                    if (!hero1CastWind && CanCastWindOnNearBaseMonster(m, topRight))
                    {
                        move.AddSpellMove(opponentBase.x, opponentBase.y, SpellType.WIND, -99, topRight.id);
                    }
                    else move.AddHeroMove(m.x, m.y, topRight.id);

                    if (!bottomLeftScanner)
                        move.AddHeroMove(bottomLeftPoint.GetTruncatedX(), bottomLeftPoint.GetTruncatedY(), bottomLeft.id);
                    else move.AddHeroMove(opponentBase.x, opponentBase.y, bottomLeft.id);
                }
                else
                {
                    bottomLeftScanner = false;
                    var monstersInBase = distToAllMonsters.Sum(m => m.Item1 < 3000 ? 1 : 0);
                    var monstersHeroCanWind = distToAllMonsters.Sum(m => m.Item1 < 3000 && CanCastWind(m.Item2, goalie) ? 1 : 0);
                    var m = distToAllMonsters[0].Item2;
                    bool goalieCastWind = false;
                    if(monstersInBase > monstersHeroCanWind && goalie.GetDistance(myBase) > distToAllMonsters[0].Item1)
                    {
                        move.AddHeroMove(myBase.x, myBase.y, goalie.id);
                    }
                    else if (CanCastWindOnNearBaseMonster(m, goalie))
                    {
                        goalieCastWind = true;
                        move.AddSpellMove(opponentBase.x, opponentBase.y, SpellType.WIND, -99, goalie.id);
                    }
                    else if (CanCastControlOnNearBaseMonster(m, goalie))
                    {
                        var target = GetControlTargetingPointForMonster(m);
                        move.AddSpellMove(target.GetTruncatedX(), target.GetTruncatedY(), SpellType.CONTROL, m.id, goalie.id);
                    } 
                    else
                    {
                        
                        if (distToOpponentHeroes.Count > 0 && distToOpponentHeroes[0].Item1 < 6500 && distToOpponentHeroes[0].Item2.GetDistance(goalie) > goalie.speed && m.GetDistance(myBase) > 5000)
                        {
                            var h = distToOpponentHeroes[0].Item2;
                            move.AddHeroMove(h.x, h.y, goalie.id);
                        }
                        else if (m.GetDistance(myBase) < 7600)
                        {
                            move.AddHeroMove(m.x, m.y, goalie.id);
                        }
                        else
                        {
                            var target = GetOutterMidPointOfBase(myBase);
                            move.AddHeroMove(target.Item1, target.Item2, goalie.id);
                        }
                    }

                    var closestMonster = distToAllMonsters[0].Item2;
                    var secondClosestMonster = distToAllMonsters[1].Item2;
                    if (distToAllMonsters.Count >= 3)
                    {
                        closestMonster = secondClosestMonster;
                        secondClosestMonster = distToAllMonsters[2].Item2;
                    }

                    var targetForTopRight = closestMonster;
                    var targetForBottomLeft = secondClosestMonster;
                    if(topRight.GetDistance(closestMonster) > bottomLeft.GetDistance(closestMonster))
                    {
                        targetForTopRight = secondClosestMonster;
                        targetForBottomLeft = closestMonster;
                    }
                    
                    

                    if ((!goalieCastWind || myBase.mana >= 20) && CanCastWindOnNearBaseMonster(m, topRight))
                    {
                        move.AddSpellMove(opponentBase.x, board.opponentBase.y, SpellType.WIND, -99, topRight.id);
                    }
                    else if (distToOpponentHeroes.Count > 0 && CanCastControlOnNearBaseHero(distToOpponentHeroes[0].Item2, topRight))
                    {
                        move.AddSpellMove(opponentBase.x, board.opponentBase.y, SpellType.CONTROL, distToOpponentHeroes[0].Item2.id, topRight.id);
                    }
                    else
                    {
                        var maximumTarget2 = MaximizeTargetsOnAllMonstersInRange(topRight);

                        if (maximumTarget2 != null && distToAllMonsters.Count < 3)
                        {
                            move.AddHeroMove(maximumTarget2.GetTruncatedX(), maximumTarget2.GetTruncatedY(), topRight.id);
                        }
                        else
                            move.AddHeroMove(targetForTopRight.x, targetForTopRight.y, topRight.id);
                    }

                    
                    var windableMonsters = GetWindableMonstersInRange(bottomLeft);
                    var controllableMonsters = GetControllableMonstersInRange(bottomLeft);
                    for(int i = 0; i<controllableMonsters.Count; i++)
                    {
                        if(controllableMonsters[i].GetDistance(myBase) < 5000)
                        {
                            controllableMonsters.RemoveAt(i);
                        }
                    }
                    if(windableMonsters.Count > 3 && myBase.mana > 60)
                    {
                        move.AddSpellMove(opponentBase.x, opponentBase.y, SpellType.WIND, -99, bottomLeft.id);
                    }
                    else if (myBase.mana > 100 && controllableMonsters.Count > 0)
                    {
                        var monster = controllableMonsters[0];
                        var target = GetControlTargetingPointForMonster(monster);
                        move.AddSpellMove(target.GetTruncatedX(), target.GetTruncatedY(), SpellType.CONTROL, monster.id, bottomLeft.id);
                    }
                    else
                    {
                        var maximumTarget3 = MaximizeTargetsOnAllMonstersInRange(bottomLeft);

                        if (maximumTarget3 != null)
                        {
                            move.AddHeroMove(maximumTarget3.GetTruncatedX(), maximumTarget3.GetTruncatedY(), bottomLeft.id);
                        }
                        else
                        {
                            move.AddHeroMove(targetForBottomLeft.x, targetForBottomLeft.y, bottomLeft.id);
                        }
                    }
                    
                    
                }
            }

            return move;
        }

        public Hero GetHeroClosestToPoint(IList<Hero> heroes, Point2d point)
        {
            double minDist = 99999;
            Hero closeHero = heroes[0];
            foreach(Hero hero in heroes)
            {
                double dist = hero.point.GetDistance(point);
                if(dist < minDist)
                {
                    minDist = dist;
                    closeHero = hero;
                }
            }

            return closeHero;
        }


        public bool CanCastControlOnNearBaseMonster(Monster monster, Hero hero)
        {
            return CanCastControlOrShield(monster, hero) && board.myBase.mana >= 30 && board.myBase.GetDistance(monster) > 6000 && monster.health > 15 && (!monster.threatForMax.HasValue || monster.threatForMax.Value);
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

        public bool CanCastWindOnNearBaseMonster(Monster monster, Hero hero)
        {
            return monster.GetDistance(board.myBase) < 6500 && CanCastWind(monster, hero) && monster.health > 4;
        }


        

        public Point2d? MaximizeTargetsOnAllMonstersInRange(Hero hero)
        {
            var monstersInRange = GetMonstersInRange(hero);
            if (!monstersInRange.Any())
                return null;

            if(monstersInRange.Count == 1)
            {
                return null;
            }

            List<Point2d> points = new List<Point2d>();
            foreach (var monster in monstersInRange)
            {
                points.Add(hero.point);
                points.Add(monster.point);
                //Console.Error.WriteLine("Found point: (" + monster.point.x + ", " + monster.point.y + ")");
            }

            //Console.Error.WriteLine("Hero point: " + hero.point);
            
            var result = Space2d.FindCircleWithMaximumPoints(points.ToArray(), hero.range-1);
            var roundedResult = result.Item2.GetRoundedPoint();
            if(hero.point.GetDistance(roundedResult) > hero.speed)
            {
                Console.Error.WriteLine("Circle too far!");
            }
            //Console.Error.WriteLine("Max circle: " + result.Item1 + result.Item2.GetRoundedPoint());

            return roundedResult;
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

        static bool alternate = true;
        public Point2d GetControlTargetingPointForMonster(Monster monster)
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

            Console.Error.WriteLine($"Targeting distances dist1, dist2: {dist1}, {dist2}");

            if (dist1 < dist2 && alternate) 
            {
                alternate = false;
                x = x1;
                y = y1;
            }
            else
            {
                alternate = true;
                x = x2;
                y = y2;
            }

            return new Point2d(x, y);
        }

        public List<Monster> GetWindableMonstersInRange(Hero hero)
        {
            List<Monster> monsters = new List<Monster>();
            foreach (var monster in board.monsters)
            {
                if (monster.health > 5 && CanCastWind(monster, hero) && hero.GetDistance(board.opponentBase) < 7000)
                {
                    monsters.Add(monster);
                }
            }
            return monsters;
        }

        public List<Monster> GetControllableMonstersInRange(Hero hero)
        {
            List<Monster> monsters = new List<Monster>();
            foreach (var monster in board.monsters)
            {
                if (monster.threatForMax.HasValue && !monster.threatForMax.Value)
                    continue;
                if (monster.health > 15 && CanCastControlOrShield(monster, hero))
                {
                    monsters.Add(monster);
                }
            }
            return monsters;
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
