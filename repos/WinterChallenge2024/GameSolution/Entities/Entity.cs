using System;

namespace GameSolution.Entities
{
    //WALL, ROOT, BASIC, TENTACLE, HARVESTER, SPORER, A, B, C, D
    public enum EntityType
    {
        WALL = 0,
        NONE,
        ROOT,
        BASIC,
        TENTACLE,
        HARVESTER,
        SPORER,
        A,
        B,
        C,
        D,
    }

    public enum OrganDirection
    {
        North = 0,
        South,
        East,
        West,
        None
    }

    public class Entity
    {
        public bool? IsMine { get; set; }
        public Point2d Location { get; set; }
        public EntityType Type { get; set; }
        public int OrganId { get; set; }
        public OrganDirection OrganDirection { get; set; }
        public int OrganParentId { get; set; }
        public int OrganRootId { get; set; }

        private bool _IsOpenSpace { get; set; }
        public Entity(int x, int y, int index, string type, int owner, int organId, string organDir, int organParentId, int organRootId)
        {
            Location = new Point2d(x, y, index);
            IsMine = owner == 1 ? true : owner == -1 ? (bool?)null : false;
            Type = GetType(type);
            OrganDirection = GetOrganDirection(organDir);
            OrganId = organId;
            OrganParentId = organParentId;
            OrganRootId = organRootId;
            _IsOpenSpace = IsOpenSpaceInternal();

        }

        public Entity(Point2d location, EntityType type, bool? isMine, int organId, int organParentId, int organRootId, OrganDirection organDirection)
        {
            Location = location;
            IsMine = isMine;
            Type = type;
            OrganId = organId;
            OrganParentId = organParentId;
            OrganRootId = organRootId;
            OrganDirection = organDirection;
            _IsOpenSpace = IsOpenSpaceInternal();
        }

        public Entity(Entity entity)
        {
            this.IsMine = entity.IsMine;
            this.Type = entity.Type;
            this.OrganDirection = entity.OrganDirection;
            this.Location = entity.Location.Clone();
            this.OrganId = entity.OrganId;
            this.OrganParentId = entity.OrganParentId;
            this.OrganRootId = entity.OrganRootId;
            _IsOpenSpace = entity._IsOpenSpace;
        }

        public OrganDirection GetOrganDirection(string organDir)
        {
            switch (organDir)
            {
                case "N":
                    return OrganDirection.North;
                case "E":
                    return OrganDirection.East;
                case "W":
                    return OrganDirection.West;
                case "S":
                    return OrganDirection.South;
                case "X":
                    return OrganDirection.None;
            }
            throw new ArgumentException($"Invalid direction: {organDir}", nameof(organDir));
        }
        public EntityType GetType(string type)
        {
            switch (type)
            {
                case "WALL":
                    return EntityType.WALL;
                case "ROOT":
                    return EntityType.ROOT;
                case "BASIC":
                    return EntityType.BASIC;
                case "TENTACLE":
                    return EntityType.TENTACLE;
                case "HARVESTER":
                    return EntityType.HARVESTER;
                case "SPORER":
                    return EntityType.SPORER;
                case "A":
                    return EntityType.A;
                case "B":
                    return EntityType.B;
                case "C":
                    return EntityType.C;
                case "D":
                    return EntityType.D;
            }
            throw new ArgumentException($"Invalid type: {type}", nameof(type));
        }

        public bool IsOpenSpace()
        {
            return _IsOpenSpace;
        }
        private bool IsOpenSpaceInternal()
        {
            return Type == EntityType.A || Type == EntityType.B || Type == EntityType.C || Type == EntityType.D;
        }
        public Entity Clone()
        {
            return new Entity(this);
        }

        public bool Equals(Entity entity)
        {
            return entity.IsMine == IsMine && entity.OrganParentId == OrganParentId && entity.OrganRootId == OrganRootId && entity.Type == Type && entity.Location.Equals(Location) && entity.OrganId == OrganId && entity.OrganDirection == OrganDirection;
        }

        public override string ToString()
        {
            return "";
        }
    }
}
