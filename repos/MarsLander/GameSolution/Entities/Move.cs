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
}
