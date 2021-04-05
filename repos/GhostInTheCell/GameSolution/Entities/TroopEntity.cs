namespace GameSolution.Entities
{
    public class TroopEntity : Entity
    {
        public int SourceFactory
        {
            get { return Arg2; }
        }

        public int TargetFactory
        {
            get { return Arg3; }
        }

        public int NumberOfCyborgs
        {
            get { return Arg4; }
        }

        public int TurnsToArrive
        {
            get { return Arg5; }
        }

        /// <summary>
        /// Creates a new Troop Entity
        /// </summary>
        /// <param name="id">Unique Identifier</param>
        /// <param name="arg1">Owner</param>
        /// <param name="arg2">Source factory Id</param>
        /// <param name="arg3">Target factory Id</param>
        /// <param name="arg4">Number of cyborgs in the troop</param>
        /// <param name="arg5">Number of turns until arrival</param>
        public TroopEntity(int id, int arg1, int arg2, int arg3, int arg4, int arg5)
        : base(id, arg1, arg2, arg3, arg4, arg5)
        {

        }

        public TroopEntity(TroopEntity entity) : base(entity)
        {

        }
    }
}
