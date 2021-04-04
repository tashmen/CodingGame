using GameSolution.Utility;

namespace GameSolution.Entities
{
    public class BombEntity : Entity
    {
        public int SourceFactoryId
        {
            get { return Arg2; }
        }
        public int TargetFactoryId
        {
            get { return Arg3; }
        }
        public int TurnsToArrive
        {
            get { return Arg4; }
        }

        public bool IsFirstTurn(FactoryLinks links)
        {
            int dist = links.GetDistance(SourceFactoryId, TargetFactoryId);
            return dist == TurnsToArrive;
        }

        public BombEntity(int id, int arg1, int arg2, int arg3, int arg4, int arg5)
        : base(id, arg1, arg2, arg3, arg4, arg5)
        {

        }

        public BombEntity(BombEntity entity) : base(entity)
        {

        }
    }
}
