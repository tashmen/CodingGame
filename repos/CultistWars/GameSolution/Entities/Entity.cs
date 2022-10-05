using Algorithms.Space;
using System;

namespace GameSolution.Entities
{
    public enum EntityType
    {
        Cultist = 0,
        CultLeader = 1
    };

    public enum OwnerType
    {
        Max = 1,
        Neutral = 0,
        Min = -1
    }
    public class Entity
    {
        public int Location;
        public int Id;
        public EntityType Type;
        public int Hp;
        public OwnerType Owner;

        public Entity(int id, int x, int y, int type, int hp, int isMine)
        {
            Id = id;
            Type = (EntityType)type;
            Hp = hp;
            Owner = (OwnerType)isMine;
            Location = Board.ConvertPointToLocation(x, y);
        }

        public Entity(Entity entity)
        {
            Id = entity.Id;
            Type = entity.Type;
            Hp = entity.Hp;
            Owner = entity.Owner;
            Location = entity.Location;
        }

        public void Move(int targetLocation)
        {
            var distance = Board.GetManhattenDistance(Location, targetLocation);
            if (distance != 1)
            {
                throw new Exception($"Point is not one space away it is: {distance}");
            }
            Location = targetLocation;
        }

        public void Shoot(Entity targetEntity)
        {
            if (Type != EntityType.Cultist)
                throw new Exception("Cult Leaders can't shoot!");

            var distance = Board.GetManhattenDistance(Location, targetEntity.Location);
            if (distance > 6)
                throw new Exception("Unit to far!");

            targetEntity.Damage(7 - distance);
        }

        public void Damage(int damage)
        {
            Hp = Math.Max(Hp - damage, 0);
        }

        public void Convert(Entity targetEntity)
        {
            if (Type != EntityType.CultLeader)
                throw new Exception("Cultists can't convert!");

            var distance = Board.GetManhattenDistance(Location, targetEntity.Location);
            if (distance != 1)
                throw new Exception("Unit is not one space away!");

            if (targetEntity.Type == EntityType.CultLeader)
                throw new Exception("Can't convert a cult leader!");

            targetEntity.Owner = Owner;
        }

        public Entity Clone()
        {
            return new Entity(this);
        }

        public bool Equals(Entity other)
        {
            return other != null && Id == other.Id && Hp == other.Hp && Type == other.Type && Owner == other.Owner && Location == other.Location;
        }

        public bool IsDead()
        {
            return Hp == 0;
        }

        public bool IsOwned(bool isMax)
        {
            if (isMax)
            {
                return Owner == OwnerType.Max;
            }
            else 
                return Owner == OwnerType.Min;
        }


        public override string ToString()
        {
            var point = Board.ConvertLocationToPoint(Location);
            return $"{Id}, {point.x}, {point.y}, {(int)Type}, {Hp}, {(int)Owner}";
            //return $"Id:{Id}, P:{Point}";
        }
    }
}
