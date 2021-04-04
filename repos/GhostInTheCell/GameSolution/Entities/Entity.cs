using static GameSolution.Constants;

namespace GameSolution.Entities
{
    public class Entity
    {
        public int Id { get; set; }
        public Owner Owner { get; set; }
        protected int Arg2 { get; set; }
        protected int Arg3 { get; set; }
        protected int Arg4 { get; set; }
        protected int Arg5 { get; set; }

        public Entity(int id, int arg1, int arg2, int arg3, int arg4, int arg5)
        {
            Id = id;
            Owner = (Owner)arg1;
            Arg2 = arg2;
            Arg3 = arg3;
            Arg4 = arg4;
            Arg5 = arg5;
        }

        public Entity(Entity entity)
        {
            Id = entity.Id;
            Owner = entity.Owner;
            Arg2 = entity.Arg2;
            Arg3 = entity.Arg3;
            Arg4 = entity.Arg4;
            Arg5 = entity.Arg5;
        }

        public bool IsFriendly()
        {
            return Owner == Owner.Me;
        }

        public bool IsEnemy()
        {
            return Owner == Owner.Opponent;
        }

        public bool IsNeutral()
        {
            return Owner == Owner.Neutral;
        }
    }
}
