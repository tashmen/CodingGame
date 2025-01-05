namespace GameSolution.Entities
{

    public class Entity3 //: PooledObject<Entity>
    {
        public bool? IsMine;
        public Point2d Location;
        public EntityType Type;
        public int OrganId;
        public OrganDirection OrganDirection;
        public int OrganParentId;
        public int OrganRootId;

        private bool _IsOpenSpace;

        /*
        static Entity()
        {
            SetInitialCapacity(500000);
        }
        */

        public Entity3()
        {

        }
    }
}
