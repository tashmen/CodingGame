using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameSolution.Entities
{
    public class Hero : BoardPiece
    {
        public static int Speed = 800;
        public static int Range = 800;
        public static int SightRange = 2200;

        public Hero(int id, int x, int y, bool? isMax, int shieldLife, bool isControlled, int vx, int vy, bool isNearBase) : base(id, x, y, isMax, shieldLife, isControlled, vx, vy, isNearBase)
        {

        }

        public Hero(Hero piece) : base(piece)
        {

        }

        public override BoardPiece Clone()
        {
            return new Hero(this);
        }
    }
}
