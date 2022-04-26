using Algorithms.Genetic;
using Algorithms.Space;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GameSolution.Entities
{
    public enum MoveType
    {
        WAIT = 0,
        MOVE = 1,
        SPELL = 2
    };

    public enum SpellType
    {
        NONE = -1,
        WIND = 0,
        SHIELD = 1,
        CONTROL = 2
    };

    public class HeroMove
    {
        public Point2d point { get; set; }
        public int x { get { return point.GetTruncatedX(); } }
        public int y { get { return point.GetTruncatedY(); } }
        public MoveType moveType { get; set; }
        public SpellType spellType {get; set;}

        public int targetId { get; set; }

        public HeroMove(int x, int y, MoveType moveType, SpellType spellType, int entityId)
        {
            this.point = new Point2d(x, y);
            this.moveType = moveType;
            this.spellType = spellType;
            this.targetId = entityId;
        }

        public HeroMove(HeroMove heroMove)
        {
            this.point = new Point2d(heroMove.point);
            this.moveType = heroMove.moveType;
            this.spellType = heroMove.spellType;
        }

        public bool Equals(HeroMove heroMove)
        {
            return this.x == heroMove.x && this.y == heroMove.y && this.moveType == heroMove.moveType && this.spellType == heroMove.spellType && this.targetId == heroMove.targetId;
        }

        public static HeroMove CreateWaitMove()
        {
            return new HeroMove(-1, -1, MoveType.WAIT, SpellType.NONE, -99);
        }

        public static HeroMove CreateHeroMove(int x, int y)
        {
            return new HeroMove(x, y, MoveType.MOVE, SpellType.NONE, -99);
        }

        public static HeroMove CreateWindSpellMove(int x, int y)
        {
            return CreateSpellMove(x, y, SpellType.WIND, -99);
        }

        public static HeroMove CreateControlSpellMove(int x, int y, int targetId)
        {
            return CreateSpellMove(x, y, SpellType.CONTROL, targetId);
        }

        public static HeroMove CreateShieldSpellMove(int targetId)
        {
            return CreateSpellMove(-1, -1, SpellType.SHIELD, targetId);
        }

        public static HeroMove CreateSpellMove(int x, int y, SpellType spell, int targetId)
        {
            return new HeroMove(x, y, MoveType.SPELL, spell, targetId);
        }
    }

    public class Move
    {
        public HeroMove[] heroMoves { get; set; }
        public double fitness { get; set; }

        public Move()
        {
            heroMoves = new HeroMove[3];
        }
        public Move(Move move)
        {
            heroMoves = move.heroMoves.Select(m => new HeroMove(m)).ToArray();
        }

        public int ConvertHeroIdToIndex(int heroId)
        {
            if (heroId < 3)
                return heroId;
            else return heroId - 3;
        }

        public HeroMove GetMove(int heroId)
        {
            return heroMoves[ConvertHeroIdToIndex(heroId)];
        }

        public void AddMove(HeroMove heroMove, int heroId)
        {
            heroMoves[ConvertHeroIdToIndex(heroId)] = heroMove;
        }

        public void AddHeroMove(int x, int y, int heroId)
        {
            heroMoves[ConvertHeroIdToIndex(heroId)] = HeroMove.CreateHeroMove(x, y);
        }

        public void AddWaitMove(int heroId)
        {
            heroMoves[ConvertHeroIdToIndex(heroId)] = HeroMove.CreateWaitMove();
        }

        public void AddSpellMove(int x, int y, SpellType spell, int targetId, int heroId)
        {
            heroMoves[ConvertHeroIdToIndex(heroId)] = HeroMove.CreateSpellMove(x, y, spell, targetId);
        }

        public bool Equals(Move move)
        {
            for(int i = 0; i < heroMoves.Length; i++)
            {
                if (!heroMoves.Equals(move))
                {
                    return false;
                }
            }

            return true;
        }

        public Move Clone()
        {
            return new Move(this);
        }

        public override string ToString()
        {
            StringBuilder moveStr = new StringBuilder();
            foreach(HeroMove move in heroMoves)
            {
                switch (move.moveType)
                {
                    case MoveType.MOVE:
                        moveStr.Append("MOVE " + move.x + " " + move.y + Environment.NewLine);
                        break;
                    case MoveType.WAIT:
                        moveStr.Append("WAIT " + Environment.NewLine);
                        break;
                    case MoveType.SPELL:
                        switch (move.spellType)
                        {
                            case SpellType.WIND:
                                moveStr.Append("SPELL " + move.spellType.ToString() + " " + move.x + " " + move.y + Environment.NewLine);
                                break;
                            case SpellType.SHIELD:
                                moveStr.Append("SPELL " + move.spellType.ToString() + " " + move.targetId + Environment.NewLine);
                                break;
                            case SpellType.CONTROL:
                                moveStr.Append("SPELL " + move.spellType.ToString() + " " + move.targetId + " " + move.x + " " + move.y + Environment.NewLine);
                                break;
                        }
                        
                        break;
                }
            }
            return moveStr.ToString();
        }
    }
}
