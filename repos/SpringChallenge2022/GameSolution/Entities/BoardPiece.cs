using Algorithms.Space;
using System;

namespace GameSolution.Entities
{
    public enum BoardPieceType
    {
        Base = 0,
        Monster = 1,
        Hero = 2
    }

    public class BoardPiece
    {
        public Point2d point { get; set; }
        public int x { get
            {
                return (int)point.x;
            } 
        }
        public int y { get
            {
                return (int)point.y;
            } 
        }
        public int id { get; set; }
        public bool? isMax { get; set; }
        public int shieldLife { get; set; }
        public bool isControlled { get; set; }
        public int vx { get; set; }
        public int vy { get; set; }
        public bool isNearBase { get; set; }

        public static int MaxEntityId = 1000;

        public BoardPiece(int id, int x, int y, bool? isMax, int shieldLife, bool isControlled, int vx, int vy, bool isNearBase)
        {
            this.id = id;
            this.point = new Point2d(x, y);
            this.isMax = isMax;
            this.shieldLife = shieldLife;
            this.isControlled = isControlled;
            this.vx = vx;
            this.vy = vy;
            this.isNearBase = isNearBase;
        }

        public BoardPiece(BoardPiece piece)
        {
            this.id = piece.id;
            this.point = new Point2d(piece.x, piece.y);
            this.isMax = piece.isMax;
            this.shieldLife = piece.shieldLife;
            this.isControlled = piece.isControlled;
            this.vx = piece.vx;
            this.vy = piece.vy;
            this.isNearBase = piece.isNearBase;
        }

        public virtual BoardPiece Clone()
        {
            return new BoardPiece(this);
        }

        
        public double GetDistance(BoardPiece piece)
        {
            return DistanceHash.GetDistance(x, y, piece.x, piece.y);
        } 

        public override string ToString()
        {
            return $"{id}, {x}, {y}, {isMax}, {GetType()}";
        }
    }
}
