using System;

namespace GameSolution.Entities
{
    public class FactoryEntity : Entity
    {
        public int NumberOfCyborgs
        {
            get { return Arg2; }
            private set 
            { 
                Arg2 = value;
                if (NumberOfCyborgs < 0)
                {
                    throw new InvalidOperationException("Not enough troops!");
                }
            }
        }
        public int ProductionCount
        {
            get { return Arg3; }
            private set 
            { 
                Arg3 = value;
                if (ProductionCount > 3)
                {
                    throw new InvalidOperationException("At maximum production!");
                }
            }
        }

        public int TurnsTillProduction
        {
            get { return Arg4; }
        }

        public FactoryEntity(int id, int arg1, int arg2, int arg3, int arg4, int arg5)
        : base(id, arg1, arg2, arg3, arg4, arg5)
        {

        }

        public FactoryEntity(FactoryEntity entity) : base(entity)
        {
            
        }

        public void Move(int cyborgCount)
        {
            NumberOfCyborgs = NumberOfCyborgs - cyborgCount;
        }

        public void Upgrade()
        {
            ProductionCount++;
            NumberOfCyborgs -= 10;
        }

        public bool IsProducing()
        {
            return TurnsTillProduction == 0;
        }
    }
}
