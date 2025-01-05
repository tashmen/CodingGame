using Algorithms.Utility;

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

    public class Entity2 : PooledObject<Entity2>
    {
        public bool? IsMine;
        public Point2d Location;
        public EntityType Type;
        public int OrganId;
        public OrganDirection OrganDirection;
        public int OrganParentId;
        public int OrganRootId;

        private bool _IsOpenSpace;


        static Entity2()
        {
            SetInitialCapacity(500000);
        }


        public Entity2()
        {

        }

        protected override void Reset()
        {

        }

    }
}
