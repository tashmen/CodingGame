using static GameSolution.Constants;

namespace GameSolution.Entities
{
    public class EntityFactory
    {
        public EntityFactory()
        {

        }

        public Entity CreateEntity(string type, int id, int arg1, int arg2, int arg3, int arg4, int arg5)
        {
            return type switch
            {
                EntityTypes.Factory => new FactoryEntity(id, arg1, arg2, arg3, arg4, arg5),
                EntityTypes.Troop => new TroopEntity(id, arg1, arg2, arg3, arg4, arg5),
                EntityTypes.Bomb => new BombEntity(id, arg1, arg2, arg3, arg4, arg5),
                _ => null
            };
        }
    }
}
