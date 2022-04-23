using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameSolution.Entities
{
    public class Hero : BoardPiece
    {


        public Hero(int id, int x, int y, bool? isMax, int shieldLife, bool isControlled, int vx, int vy, bool isNearBase) : base(id, x, y, isMax, 800, 800, 2200, shieldLife, isControlled, vx, vy, isNearBase)
        {

        }

        public Hero(Hero piece) : base(piece)
        {

        }

        public Hero Clone(Hero piece)
        {
            return new Hero(piece);
        }
    }
}
