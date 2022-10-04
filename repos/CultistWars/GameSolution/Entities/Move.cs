using Algorithms.Space;
using System;

namespace GameSolution.Entities
{
    public enum MoveType
    {
        Wait = 0,
        Move = 1,
        Shoot = 2,
        Convert = 3
    }
    public static class Move
    {
        public static long UnitIdMask = (long)Math.Pow(2, 4) - 1;
        public static long MoveTypeMask = (long)Math.Pow(2, 6) - 1 - UnitIdMask;
        public static long PointXMask = (long)Math.Pow(2, 10) - 1 - MoveTypeMask;
        public static long PointYMask = (long)Math.Pow(2, 14) - 1 - PointXMask;
        public static long TargetUnitIdMask = (long)Math.Pow(2, 18) - 1 - PointYMask;

        /*
        public int UnitId;//4 bits
        public MoveType Type;//2 bits
        public Point2d Point;//2 (4 bits) = 8 bits
        public int TargetUnitId;//4 bits
        */

        public static int GetUnitId(long move)
        {
            return (int)(move & UnitIdMask);
        }

        public static MoveType GetMoveType(long move)
        {
            return (MoveType)((move & MoveTypeMask) >> 4);
        }

        public static int GetX(long move)
        {
            return (int)((move & PointXMask) >> 6);
        }

        public static int GetY(long move)
        {
            return (int)((move & PointYMask) >> 10);
        }

        public static int GetTargetUnitId(long move)
        {
            return (int)((move & TargetUnitIdMask) >> 14);
        }

        public static long CreateMove(int unitId, MoveType moveType, int targetX, int targetY, int targetUnitId)
        {
            return unitId | (int)moveType << 4 | targetX << 6 | targetY << 10 | targetUnitId << 14;
        }

        public static long Wait()
        {
            return CreateMove(0, MoveType.Wait, 0, 0, 0);
        }

        public static long MoveUnit(int unitId, int targetX, int targetY)
        {
            return CreateMove(unitId, MoveType.Move, targetX, targetY, 0);
        }

        public static long Shoot(int unitId, int targetUnitId)
        {
            return CreateMove(unitId, MoveType.Shoot, 0, 0, targetUnitId);
        }

        public static long Convert(int unitId, int targetUnitId)
        {
            return CreateMove(unitId, MoveType.Convert, 0, 0, targetUnitId);
        }

        public static bool IsWait(long move)
        {
            return GetMoveType(move) == MoveType.Wait;
        }

        public static string ToString(long move)
        {
            switch(GetMoveType(move))
            {
                case MoveType.Move:
                    return $"{GetUnitId(move)} MOVE {GetX(move)} {GetY(move)}";
                case MoveType.Wait:
                    return $"WAIT";
                case MoveType.Shoot:
                    return $"{GetUnitId(move)} SHOOT {GetTargetUnitId(move)}";
                case MoveType.Convert:
                    return $"{GetUnitId(move)} CONVERT {GetTargetUnitId(move)}";
            }
            return "";
        }
    }
}
