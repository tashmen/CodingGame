using Algorithms.Space;
using System;

namespace GameSolution.Entities
{
    public class Monster : BoardPiece
    {
        public static int Speed = 400;
        public static int Range = 300;
        public static int TargetingRange = 5000;

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
        public bool? threatForMax
        {
            get
            {
                var val = (bitboard & threatForMaxMask) >> 52;
                return val == 1 ? true : val == 0 ? false : (bool?)null;
            }
            set
            {
                bitboard &= ~threatForMaxMask;
                bitboard += (value.HasValue ? value.Value ? 1 : 0 : 2) << 52;
            }
        }

        /*
         * health       2 ^ 6
         * threatForMax 2 ^ 2
         * */

        protected static long healthMask = (long)Math.Pow(2, 52) - 1 - vyMask - vxMask - isControlledMask - isNearBaseMask - shieldLifeMask - isMaxMask - idMask;
        protected static long threatForMaxMask = (long)Math.Pow(2, 56) - 1 - healthMask - vyMask - vxMask - isControlledMask - isNearBaseMask - shieldLifeMask - isMaxMask - idMask;

        public Monster(int id, int x, int y, bool? isMax, int health, int shieldLife, bool isControlled, int vx, int vy, bool isNearBase, bool? threatForMax) : base(id, x, y, isMax, shieldLife, isControlled, vx, vy, isNearBase)
        {
            bitboard += (((long)health) << 46) + ((threatForMax.HasValue ? threatForMax.Value ? (long)1 : 0 : 2) << 52);
        }

        public Monster(Monster piece) : base(piece)
        {
            //Values copied as part of base
        }

        public override BoardPiece Clone()
        {
            return new Monster(this);
        }

        public void Move()
        {
            point = new Point2d(x + vx, y + vy);
        }

        public override string ToString()
        {
            return base.ToString() + $", h: {health}, t? {threatForMax}";
        }
    }
}
