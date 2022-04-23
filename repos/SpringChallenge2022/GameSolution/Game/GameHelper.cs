using GameSolution.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
                else move.AddWaitMove();

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
                        if (m.GetDistance(board.myBase) < 6000)
                        {
                            move.AddHeroMove(m.x, m.y);
                        }
                        else move.AddWaitMove();
                    }

                    move.AddHeroMove(m.x, m.y);
                    move.AddHeroMove(m.x, m.y);
                }
                else
                {
                    var m = distToAllMonsters[0].Item2;
                    if(CanCastControlOnNearBaseMonster(m, board.myHeroes[0]))
                    {
                        move.AddSpellMove(board.opponentBase.x, board.opponentBase.y, SpellType.CONTROL, m.id);
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
                        else move.AddWaitMove();
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
            return hero.GetDistance(monster) <= 2200 && monster.shieldLife == 0 && board.myBase.mana >= 30 && board.myBase.GetDistance(monster) > 5200 && monster.health > 10 && (!monster.threatForMax.HasValue || monster.threatForMax.Value);
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

            return hero.isNearBase && hero.GetDistance(piece) <= 2200 && piece.shieldLife == 0 && board.myBase.mana >= 20 && countCloseMonsters > 1;
        }

        public bool CanCastWindOnNearBaseMonster(BoardPiece piece, Hero hero)
        {
            return piece.isNearBase && hero.GetDistance(piece) <= 1280 && piece.shieldLife == 0 && board.myBase.mana >= 10;
        }
    }
}
