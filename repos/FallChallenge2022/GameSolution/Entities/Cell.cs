using Algorithms.Space;
using System;

namespace GameSolution.Entities
{
    public enum Owner
    {
        Me = 1,
        Opponent = 0,
        Neutral = -1
    }

    public class Cell
    {
        public Point2d Point { get; set; }
        public Owner Owner { get; set; }
        public int ScrapAmount { get; set; }
        public int Units { get; set; }
        public bool HasRecycler { get; set; }
        public bool CanBuild { get; set; }
        public bool CanSpawn { get; set; }
        public bool InRangeOfRecycler { get; set; }

        public bool IsGrass 
        { 
            get
            {
                return ScrapAmount <= 0;
            } 
        }

        public int x
        {
            get
            {
                return (int)Point.x;
            }
        }
        public int y
        {
            get
            {
                return (int)Point.y;
            }
        }
        

        public Cell(int x, int y, int owner, int scrapAmount, int units, int recycler, int build, int spawn, int rangeRecycler)
        {
            Point= new Point2d(x, y);
            Owner = (Owner)owner;
            ScrapAmount = scrapAmount;
            Units = units;
            HasRecycler = recycler == 1;
            CanBuild = build == 1;
            CanSpawn = spawn == 1;
            InRangeOfRecycler = rangeRecycler == 1;
        }

        public Cell(Cell cell)
        {
            Point = new Point2d(cell.x, cell.y);
            Owner = cell.Owner;
            ScrapAmount = cell.ScrapAmount;
            Units = cell.Units;
            HasRecycler = cell.HasRecycler;
            CanBuild = cell.CanBuild;
            CanSpawn = cell.CanSpawn;
            InRangeOfRecycler = cell.InRangeOfRecycler;
        }

        public Cell Clone()
        {
            return new Cell(this);
        }

        public bool Equals(Cell cell)
        {
            return false;
        }

        public override string ToString()
        {
            return $"{x}, {y}, o? {Owner}, s: {ScrapAmount}, u: {Units}, r? {HasRecycler}, b? {CanBuild}, s? {CanSpawn}, rr? {InRangeOfRecycler}";
        }
    }
}
