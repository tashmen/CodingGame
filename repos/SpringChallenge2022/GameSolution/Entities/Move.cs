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
        public int x { get; set; }
        public int y { get; set; }
        public MoveType moveType { get; set; }
        public SpellType spellType {get; set;}

        public int targetId { get; set; }

        public HeroMove(int x, int y, MoveType moveType, SpellType spellType, int entityId)
        {
            this.x = x;
            this.y = y;
            this.moveType = moveType;
            this.spellType = spellType;
            this.targetId = entityId;
        }

        public HeroMove(HeroMove heroMove)
        {
            this.x = heroMove.x;
            this.y = heroMove.y;
            this.moveType = heroMove.moveType;
            this.spellType = heroMove.spellType;
        }

        public bool Equals(HeroMove heroMove)
        {
            return this.x == heroMove.x && this.y == heroMove.y && this.moveType == heroMove.moveType && this.spellType == heroMove.spellType && this.targetId == heroMove.targetId;
        }
    }

    public class Move
    {
        HeroMove[] heroMoves;

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

        public void AddMove(Move move, int heroId)
        {
            heroMoves[ConvertHeroIdToIndex(heroId)] = move.heroMoves[0];
        }

        public void AddHeroMove(int x, int y, int heroId)
        {
            heroMoves[ConvertHeroIdToIndex(heroId)] = new HeroMove(x, y, MoveType.MOVE, SpellType.NONE, -99);
        }

        public void AddWaitMove(int heroId)
        {
            heroMoves[ConvertHeroIdToIndex(heroId)] = new HeroMove(-1, -1, MoveType.WAIT, SpellType.NONE, -99);
        }

        public void AddSpellMove(int x, int y, SpellType spell, int targetId, int heroId)
        {
            heroMoves[ConvertHeroIdToIndex(heroId)] = new HeroMove(x, y, MoveType.SPELL, spell, targetId);
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
