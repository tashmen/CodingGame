﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        public int speed { get; set; }
        public int range { get; set; }
        public int sightRange { get; set; }
        public int id { get; set; }
        public int x {get; set;}
        public int y { get; set; }
        public bool? isMax { get; set; }
        public int shieldLife { get; set; }
        public bool isControlled { get; set; }
        public int vx { get; set; }
        public int vy { get; set; }
        public bool isNearBase { get; set; }

        public BoardPiece(int id, int x, int y, bool? isMax, int speed, int range, int sightRange, int shieldLife, bool isControlled, int vx, int vy, bool isNearBase)
        {
            this.id = id;
            this.x = x;
            this.y = y;
            this.isMax = isMax;
            this.speed = speed;
            this.range = range;
            this.sightRange = sightRange;
            this.shieldLife = shieldLife;
            this.isControlled = isControlled;
            this.vx = vx;
            this.vy = vy;
            this.isNearBase = isNearBase;
            distanceHash = new Dictionary<int, double>();
        }

        public BoardPiece(BoardPiece piece)
        {
            this.id = piece.id;
            this.x = piece.x;
            this.y = piece.y;
            this.isMax = piece.isMax;
            this.speed = piece.speed;
            this.range = piece.range;
            this.sightRange = piece.sightRange;
            this.shieldLife = piece.shieldLife;
            this.isControlled = piece.isControlled;
            this.vx = piece.vx;
            this.vy = piece.vy;
            this.isNearBase = piece.isNearBase;
            distanceHash = new Dictionary<int, double>();
        }

        public BoardPiece Clone()
        {
            return new BoardPiece(this);
        }

        public Dictionary<int, double> distanceHash;
        public double GetDistance(BoardPiece piece)
        {
            if (!distanceHash.ContainsKey(piece.id))
            {
                distanceHash[piece.id] = GetDistance(piece.x, x, piece.y, y);
            }
            return distanceHash[piece.id];
        } 

        public static double GetDistance(int x1, int x2, int y1, int y2)
        {
            return Math.Sqrt(Math.Pow(x1 - x2, 2) + Math.Pow(y1 - y2, 2));
        }

        public string ToString()
        {
            return $"{id}, {x}, {y}, {isMax}, {GetType()}";
        }
    }
}