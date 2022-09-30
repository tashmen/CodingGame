using System;
using System.Collections.Generic;
using System.Text;

namespace GameSolution.Entities
{
    public class Move
    {
        public int Id;
        public char Direction;

        public Move(int id, char direction)
        {
            Id = id;
            Direction = direction;
        }

        public override string ToString()
        {
            return $"{Id} {Direction}";
        }

        public virtual Move Clone()
        {
            return new Move(Id, Direction);
        }
    }
}
