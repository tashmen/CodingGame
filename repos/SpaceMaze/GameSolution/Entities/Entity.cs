using Algorithms.Space;
using System.Collections.Generic;
using System.Linq;

namespace GameSolution.Entities
{
    public class Entity
    {
        public Point2d Point;
        public int Id;
        public string Directions;
        public bool IsCar;
        public Dictionary<char, Point2d> LegalLocations;

        public Entity(int id, int x, int y, string directions)
        {
            Id = id;
            Point = new Point2d(x, y);
            if(directions.CompareTo("CAR") == 0)
            {
                Directions = "UDLR";
                IsCar = true;
            }
            else Directions = directions;
            LegalLocations = new Dictionary<char, Point2d>();
        }

        public Entity(Entity entity)
        {
            Point = entity.Point.Clone();
            Directions = entity.Directions;
            Id = entity.Id;
            LegalLocations = entity.LegalLocations.ToDictionary(entry => entry.Key,
                                               entry => entry.Value);
            IsCar = entity.IsCar;
        }

        public Entity Clone()
        {
            return new Entity(this);
        }

        public void Fill(Entity entity)
        {
            Point = entity.Point;
            Directions = entity.Directions;
            Id = entity.Id;
        }

        public void ApplyDirection(char direction)
        {
            Point = LegalLocations[direction];
        }

        public override string ToString()
        {
            return $"Id:{Id}, D:{Directions}, P:{Point}";
        }
    }
}
