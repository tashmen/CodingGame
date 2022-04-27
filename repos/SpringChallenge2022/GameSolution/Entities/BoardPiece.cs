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

        public double[] distanceHash;

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
            distanceHash = new double[2000];
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
            distanceHash = new double[100];
        }

        public virtual BoardPiece Clone()
        {
            return new BoardPiece(this);
        }

        
        public double GetDistance(BoardPiece piece)
        {
            if (distanceHash[piece.id] == 0)
            {
                distanceHash[piece.id] = GetDistance(piece.x, piece.y, x, y);
            }
            return distanceHash[piece.id];
        } 

        public Tuple<int, int> GetMidPoint(BoardPiece piece)
        {
            return GetMidPoint(piece.x, piece.y, x, y);
        }

        public static double GetDistance(int x1, int y1, int x2, int y2)
        {
            return Math.Sqrt(Math.Pow(x1 - x2, 2) + Math.Pow(y1 - y2, 2));
        }

        public static Tuple<int, int> GetMidPoint(int x1, int y1, int x2, int y2)
        {
            return new Tuple<int, int>(Math.Abs(x1 - x2) / 2, Math.Abs(y1 - y2) / 2);
        }

        public override string ToString()
        {
            return $"{id}, {x}, {y}, {isMax}, {GetType()}";
        }
    }
}
