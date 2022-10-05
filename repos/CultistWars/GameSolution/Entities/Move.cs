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
        public static long TargetUnitIdMask = (long)Math.Pow(2, 10) - 1 - MoveTypeMask;
        public static long LocationMask = (long)Math.Pow(2, 17) - 1 - TargetUnitIdMask;

        /*
        public int UnitId;//4 bits
        public MoveType Type;//2 bits
        public int TargetUnitId;//4 bits
        public int Location; //7 bits
        */

        public static int GetUnitId(long move)
        {
            return (int)(move & UnitIdMask);
        }

        public static MoveType GetMoveType(long move)
        {
            return (MoveType)((move & MoveTypeMask) >> 4);
        }

        public static int GetTargetUnitId(long move)
        {
            return (int)((move & TargetUnitIdMask) >> 6);
        }

        public static int GetLocation(long move)
        {
            return (int)((move & LocationMask) >> 10);
        }

        public static long CreateMove(int unitId, MoveType moveType, int targetUnitId, int location)
        {
            return unitId | (int)moveType << 4 | targetUnitId << 6 | location << 10;
        }

        public static long Wait()
        {
            return CreateMove(0, MoveType.Wait, 0, 0);
        }

        public static long MoveUnit(int unitId, int location)
        {
            return CreateMove(unitId, MoveType.Move, 0, location);
        }

        public static long Shoot(int unitId, int targetUnitId)
        {
            return CreateMove(unitId, MoveType.Shoot, targetUnitId, 0);
        }

        public static long Convert(int unitId, int targetUnitId)
        {
            return CreateMove(unitId, MoveType.Convert, targetUnitId, 0);
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
                    var point = Board.ConvertLocationToPoint(GetLocation(move));
                    return $"{GetUnitId(move)} MOVE {point.x} {point.y}";
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
