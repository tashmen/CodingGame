using System;

namespace GameSolution.Entities
{
    public class Base : BoardPiece
    {
        public static int SightRange = 6000;
        public int health
        {
            get
            {
                return (int)((bitboard & healthMask) >> 46);
            }
            set
            {
                bitboard &= ~healthMask;
                bitboard += (long)value << 46;
            }
        }
        public int mana
        {
            get
            {
                return (int)((bitboard & manaMask) >> 48);
            }
            set
            {
                bitboard &= ~manaMask;
                bitboard += (long)value << 48;
            }
        }
        /*
         * health   2 ^ 2
         * mana     2 ^ 12
         * **/
        protected static long healthMask = (long)Math.Pow(2, 48) - 1 - vyMask - vxMask - isControlledMask - isNearBaseMask - shieldLifeMask - isMaxMask - idMask;
        protected static long manaMask = (long)Math.Pow(2, 60) - 1 - healthMask - vyMask - vxMask - isControlledMask - isNearBaseMask - shieldLifeMask - isMaxMask - idMask;
        public Base(int id, int x, int y, bool isMax, int health, int mana) : base(id, x, y, isMax, 0, false, 0, 0, false)
        {
            bitboard += (((long)health) << 46) + (((long)mana) << 48);
        }

        public Base(Base piece) : base(piece)
        {
            //handled by base
        }

        public override BoardPiece Clone()
        {
            return new Base(this);
        }

        public override string ToString()
        {
            return base.ToString() + $", {health}, {mana}";
        }
    }
}
