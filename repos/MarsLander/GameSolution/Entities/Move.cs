using System;
using System.Collections.Generic;
using System.Text;

namespace GameSolution.Entities
{
    public class Move
    {
        public int Rotation;
        public int Power;

        public Move(int rotate, int power)
        {
            Rotation = rotate;
            Power = power;
        }

        public override string ToString()
        {
            return $"{Rotation} {Power}";
        }

        public virtual Move Clone()
        {
            return new Move(Rotation, Power);
        }
    }

    public class StaticMove : Move
    {
        public StaticMove(int rotate, int power) : base(rotate, power)
        {
        }

        public static Move ConvertToMove(Ship ship, StaticMove move)
        {
            return new Move(Math.Min(Math.Max(ship.RotationAngle + move.Rotation, -90), 90), Math.Max(Math.Min(ship.Power + move.Power, 4), 0));
        }

        public override Move Clone()
        {
            return new StaticMove(Rotation, Power);
        }
    }
}
