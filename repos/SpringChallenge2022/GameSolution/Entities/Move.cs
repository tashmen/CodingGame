using System;
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
        NONE = 0,
        WIND = 1,
        SHIELD = 2,
        CONTROL = 3
    };

    public static class HeroMove
    {
        //x => 2^16
        //y => 2^16
        //moveType => 2^2
        //SpellType => 2^2
        //targetId => 2^16

        public static long xMask = (long)Math.Pow(2, 16) - 1;
        public static long yMask = (long)Math.Pow(2, 32) - 1 - xMask;
        public static long moveTypeMask = (long)Math.Pow(2, 34) - 1 - yMask - xMask;
        public static long spellTypeMask = (long)Math.Pow(2, 36) - 1 - moveTypeMask - yMask - xMask;
        public static long targetIdMask = (long)Math.Pow(2, 52) - 1 - spellTypeMask - moveTypeMask - yMask - xMask;

        public static MoveType GetMoveType(long move)
        {
            return (MoveType)((move & moveTypeMask) >> 32);
        }

        public static int GetX(long move)
        {
            return (int)(move & xMask);
        }

        public static int GetY(long move)
        {
            return (int)((move & yMask) >> 16);
        }

        public static SpellType GetSpellType(long move)
        {
            return (SpellType)((move & spellTypeMask) >> 34);
        }

        public static int GetTargetId(long move)
        {
            return (int)((move & targetIdMask) >> 36);
        }

        public static long CreateHeroMove(int x, int y, MoveType moveType, SpellType spellType, int entityId)
        {
            long move = x + ((long)y << 16) + ((long)moveType << 32) + ((long)spellType << 34) + ((long)entityId << 36);
            return move;
        }

        public static long CreateWaitMove()
        {
            return CreateHeroMove(0, 0, MoveType.WAIT, SpellType.NONE, 0);
        }

        public static long CreateHeroMove(int x, int y)
        {
            return CreateHeroMove(x, y, MoveType.MOVE, SpellType.NONE, 0);
        }

        public static long CreateWindSpellMove(int x, int y)
        {
            return CreateSpellMove(x, y, SpellType.WIND, 0);
        }

        public static long CreateControlSpellMove(int x, int y, int targetId)
        {
            return CreateSpellMove(x, y, SpellType.CONTROL, targetId);
        }

        public static long CreateShieldSpellMove(int targetId)
        {
            return CreateSpellMove(0, 0, SpellType.SHIELD, targetId);
        }

        public static long CreateSpellMove(int x, int y, SpellType spell, int targetId)
        {
            return CreateHeroMove(x, y, MoveType.SPELL, spell, targetId);
        }
    }

    public class Move
    {
        public long[] heroMoves { get; set; }

        public Move()
        {
            heroMoves = new long[3];
        }
        public Move(Move move)
        {
            heroMoves = move.heroMoves.Select(m => m).ToArray();
        }

        public int ConvertHeroIdToIndex(int heroId)
        {
            if (heroId < 3)
                return heroId;
            else return heroId - 3;
        }

        public long GetMove(int heroId)
        {
            return heroMoves[ConvertHeroIdToIndex(heroId)];
        }

        public void AddMove(long heroMove, int heroId)
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

        public void AddWindSpellMove(int x, int y, int heroId)
        {
            heroMoves[ConvertHeroIdToIndex(heroId)] = HeroMove.CreateWindSpellMove(x, y);
        }

        public void AddControlSpellMove(int x, int y, int targetId, int heroId)
        {
            heroMoves[ConvertHeroIdToIndex(heroId)] = HeroMove.CreateControlSpellMove(x, y, targetId);
        }

        public void AddShieldSpellMove(int targetId, int heroId)
        {
            heroMoves[ConvertHeroIdToIndex(heroId)] = HeroMove.CreateShieldSpellMove(targetId);
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
            foreach(long move in heroMoves)
            {
                switch (HeroMove.GetMoveType(move))
                {
                    case MoveType.MOVE:
                        moveStr.Append("MOVE " + HeroMove.GetX(move) + " " + HeroMove.GetY(move) + Environment.NewLine);
                        break;
                    case MoveType.WAIT:
                        moveStr.Append("WAIT" + Environment.NewLine);
                        break;
                    case MoveType.SPELL:
                        var spellType = HeroMove.GetSpellType(move);
                        switch (spellType)
                        {
                            case SpellType.WIND:
                                moveStr.Append("SPELL " + spellType.ToString() + " " + HeroMove.GetX(move) + " " + HeroMove.GetY(move) + Environment.NewLine);
                                break;
                            case SpellType.SHIELD:
                                moveStr.Append("SPELL " + spellType.ToString() + " " + HeroMove.GetTargetId(move) + Environment.NewLine);
                                break;
                            case SpellType.CONTROL:
                                moveStr.Append("SPELL " + spellType.ToString() + " " + HeroMove.GetTargetId(move) + " " + HeroMove.GetX(move) + " " + HeroMove.GetY(move) + Environment.NewLine);
                                break;
                        }
                        
                        break;
                }
            }
            return moveStr.ToString();
        }
    }
}
