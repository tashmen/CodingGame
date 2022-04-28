using Algorithms.Space;
using System;

namespace GameSolution.Entities
{
    public class Monster : BoardPiece
    {
        public static int Speed = 400;
        public static int Range = 300;
        public static int TargetingRange = 5000;

        public int health { get; set; }
        public bool? threatForMax { get; set; }

        public Monster() : base()
        {

        }
        public Monster(int id, int x, int y, bool? isMax, int health, int shieldLife, bool isControlled, int vx, int vy, bool isNearBase, bool? threatForMax) : base(id, x, y, isMax, shieldLife, isControlled, vx, vy, isNearBase)
        {
            this.health = health;
            this.threatForMax = threatForMax;
        }

        public void Fill(Monster piece)
        {
            base.Fill(piece);
            this.health = piece.health;
            this.threatForMax = piece.threatForMax;
        }

        public override BoardPiece Clone()
        {
            Monster m = MemoryAllocator.GetMonster();
            m.Fill(this);
            return m;
        }

        public void Move()
        {
            point = new Point2d(x + vx, y + vy);
        }
    }
}
