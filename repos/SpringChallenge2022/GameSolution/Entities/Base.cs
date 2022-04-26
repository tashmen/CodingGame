using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameSolution.Entities
{
    public class Base : BoardPiece
    {
        public int health { get; set; }
        public int mana { get; set; }
        public Base(int id, int x, int y, bool isMax, int health, int mana) : base(id, x, y, isMax, 0, 0, 6000, 0, false, 0, 0, false)
        {
            this.health = health;
            this.mana = mana;
        }

        public Base(Base piece) : base(piece)
        {
            this.health = piece.health;
            this.mana = piece.mana;
        }

        public override BoardPiece Clone()
        {
            return new Base(this);
        }
    }
}
