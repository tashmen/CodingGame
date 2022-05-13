using System;
using System.Collections.Generic;
using System.Text;

namespace GameSolution.Entities
{
    public class Move
    {
        public int Rotation { get; set; }
        public int Power { get; set; }

        public Move(int rotate, int power)
        {
            Rotation = rotate;
            Power = power;
        }

        public override string ToString()
        {
            return $"{Rotation} {Power}";
        }
    }

    public class StaticMove : Move
    {
        public StaticMove(int rotate, int power) : base(rotate, power)
        {
        }
    }
}
