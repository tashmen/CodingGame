using Algorithms.Space;
using System;

namespace GameSolution.Entities
{
    public enum BoardPieceType
    {
        Base = 0,
        Monster = 1,
        Hero = 2
    }

    public class BoardPiece
    {
        public Point2d point { get; set; }
        public bool isWinded { get; set; } = false;
        public int x
        {
            get
            {
                return (int)point.x;
            }
        }
        public int y
        {
            get
            {
                return (int)point.y;
            }
        }
        public int id
        {
            get
            {
                return (int)(bitboard & idMask);
            }
        }
        public bool? isMax
        {
            get
            {
                var val = (bitboard & isMaxMask) >> 10;
                return val == 1 ? true : val == 0 ? false : (bool?)null;
            }
        }
        public int shieldLife
        {
            get
            {
                return (int)((bitboard & shieldLifeMask) >> 12);
            }
            set
            {
                bitboard &= ~shieldLifeMask;
                bitboard += (value << 12);
            }
        }
        public bool isControlled
        {
            get
            {
                return ((bitboard & isControlledMask) >> 17) == 1 ? true : false;
            }
            set
            {
                bitboard &= ~isControlledMask;
                bitboard += ((value ? 1 : 0) << 17);
            }
        }
        public int vx
        {
            get
            {
                int val = (int)((bitboard & vxMask) >> 18);
                var isNegative = val % 2 == 1 ? true : false;
                val = val >> 1;
                return isNegative ? val * -1 : val;
            }
            set
            {
                var isNegative = value < 0;
                bitboard &= ~vxMask;
                bitboard += ((isNegative ? (long)1 : 0) << 18) + (((long)Math.Abs(value)) << 19);
            }
        }
        public int vy
        {
            get
            {
                int val = (int)((bitboard & vyMask) >> 32);
                var isNegative = val % 2 == 1 ? true : false;
                val = val >> 1;
                return isNegative ? val * -1 : val;
            }
            set
            {
                var isNegative = value < 0;
                bitboard &= ~vyMask;
                bitboard += ((isNegative ? (long)1 : 0) << 32) + (((long)Math.Abs(value)) << 33);
            }
        }
        public bool isNearBase
        {
            get
            {
                return ((bitboard & isNearBaseMask) >> 16) == 1 ? true : false;
            }
            set
            {
                bitboard &= ~isNearBaseMask;
                bitboard += (value ? 1 : 0) << 16;
            }
        }

        public static int MaxEntityId = 1000;

        /*
         * id           2^10
         * isMax        2^2
         * shieldLife   2^4
         * isControlled 2^1
         * isNearBase   2^1
         * vx           2^1 + 2^13 = 2^14
         * vy           2^1 + 2^13 = 2^14
         * 
         * x            2^15 -> needed as point
         * y            2^15 -> needed as point
         * */

        protected long bitboard = 0;

        protected static long idMask = (long)Math.Pow(2, 10) - 1;
        protected static long isMaxMask = (long)Math.Pow(2, 12) - 1 - idMask;
        protected static long shieldLifeMask = (long)Math.Pow(2, 16) - 1 - isMaxMask - idMask;
        protected static long isNearBaseMask = (long)Math.Pow(2, 17) - 1 - shieldLifeMask - isMaxMask - idMask;
        protected static long isControlledMask = (long)Math.Pow(2, 18) - 1 - isNearBaseMask - shieldLifeMask - isMaxMask - idMask;
        protected static long vxMask = (long)Math.Pow(2, 32) - 1 - isControlledMask - isNearBaseMask - shieldLifeMask - isMaxMask - idMask;
        protected static long vyMask = (long)Math.Pow(2, 46) - 1 - vxMask - isControlledMask - isNearBaseMask - shieldLifeMask - isMaxMask - idMask;


        public BoardPiece(int id, int x, int y, bool? isMax, int shieldLife, bool isControlled, int vx, int vy, bool isNearBase)
        {
            point = new Point2d(x, y);
            bitboard = id + ((isMax.HasValue ? isMax.Value ? (long)1 : 0 : 2) << 10) + ((long)shieldLife << 12) + ((isNearBase ? 1 : 0) << 16) + ((isControlled ? 1 : 0) << 17) + ((vx < 0 ? (long)1 : 0) << 18) + (((long)Math.Abs(vx)) << 19) + ((vy < 0 ? (long)1 : 0) << 32) + (((long)Math.Abs(vy)) << 33);
        }

        public BoardPiece(BoardPiece piece)
        {
            point = new Point2d(piece.x, piece.y);
            bitboard = piece.bitboard;
        }

        public virtual BoardPiece Clone()
        {
            return new BoardPiece(this);
        }

        public bool Equals(BoardPiece piece)
        {
            return bitboard == piece.bitboard && x == piece.x && y == piece.y;
        }


        public double GetDistance(BoardPiece piece)
        {
            return DistanceHash.GetDistance(x, y, piece.x, piece.y);
        }

        public override string ToString()
        {
            return $"{id}, {x}, {y}, m? {isMax}, s: {shieldLife}, b? {isNearBase}, {vx}, {vy}, c? {isControlled}, bb: {bitboard}";
        }
    }
}
