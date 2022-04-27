using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameSolution.Entities
{
    public class Monster : BoardPiece
    {
        public static int Speed = 400;
        public static int Range = 300;

        public int health { get; set; }
        public bool? threatForMax { get; set; }
        public Monster(int id, int x, int y, bool? isMax, int health, int shieldLife, bool isControlled, int vx, int vy, bool isNearBase, bool? threatForMax) : base(id, x, y, isMax, shieldLife, isControlled, vx, vy, isNearBase)
        {
            this.health = health;
            this.threatForMax = threatForMax;
        }

        public Monster(Monster piece) : base(piece)
        {
            this.health = piece.health;
            this.threatForMax = piece.threatForMax;
        }

        public override BoardPiece Clone()
        {
            return new Monster(this);
        }
    }
}
