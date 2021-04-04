using GameSolution.Entities;
using GameSolution.Moves;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static GameSolution.Constants;

namespace GameSolution.Utility
{
    public class GameState
    {
        public int EnemyBombCount { get; private set; } = 2;
        public int MyBombCount { get; private set; } = 2;
        public int GameCounter { get; private set; } = 0;
        public int EnemyTroopsCount { get; private set; } = 0;
        public int MyTroopsCount { get; private set; } = 0;
        public int EnemyIncome { get; private set; } = 0;
        public int MyIncome { get; private set; } = 0;

        public FactoryLinks Links { get; private set; }
        public List<Entity> Entities { get; private set; }

        public List<FactoryEntity> Factories { get; private set; }
        public List<FactoryEntity> MyFactories { get; private set; }
        public List<FactoryEntity> EnemyFactories { get; private set; }
        public List<FactoryEntity> NeutralFactories { get; private set; }

        public List<BombEntity> Bombs { get; private set; }
        public List<BombEntity> MyBombs { get; private set; }
        public List<BombEntity> EnemyBombs { get; private set; }

        public List<TroopEntity> Troops { get; private set; }
        public List<TroopEntity> MyTroops { get; private set; }
        public List<TroopEntity> EnemyTroops { get; private set; }

        public GameState(FactoryLinks links)
        {
            Links = links;
        }

        public GameState(GameState state)
        {
            GameCounter = state.GameCounter;
            Links = state.Links;//Shouldn't be modified
            Entities = state.Entities.Select(e => EntityFactory.CreateEntity(e)).ToList();//Clone the entities as we want to update this
            UpdateGameState();
            CalculateStats();
        }

        public void PlayFriendlyMove(Move move)
        {
            //Should we check if it's legal??
            string strMove = move.GetMove();
            List<string> moveArgs = strMove.Split(' ').ToList();
            int source, target;
            FactoryEntity sourceFactory;
            switch (moveArgs[0])
            {
                case MoveType.Bomb:
                    source = Convert.ToInt32(moveArgs[1]);
                    target = Convert.ToInt32(moveArgs[2]);
                    Entities.Add(EntityFactory.CreateEntity(EntityTypes.Bomb, -1, (int)Owner.Me, source, target, Links.GetDistance(source, target), 0));
                    MyBombCount--;
                    break;
                case MoveType.Move:
                    source = Convert.ToInt32(moveArgs[1]);
                    target = Convert.ToInt32(moveArgs[2]);
                    int cyborgCount = Convert.ToInt32(moveArgs[3]);
                    sourceFactory = MyFactories.First(e => e.Id == source);
                    sourceFactory.Move(cyborgCount);
                    Entities.Add(EntityFactory.CreateEntity(EntityTypes.Troop, -1, (int)Owner.Me, source, target, cyborgCount, Links.GetDistance(source, target)));
                    break;
                case MoveType.Upgrade:
                    source = Convert.ToInt32(moveArgs[1]);
                    sourceFactory = MyFactories.First(e => e.Id == source);
                    sourceFactory.Upgrade();
                    MyTroopsCount -= 10;
                    break;
            }
        }

        public void SetEntities(List<Entity> entites)
        {
            Entities = entites;
            UpdateGameState();
            CalculateStats();
            GameCounter++;
        }

        private void UpdateGameState()
        {
            Factories = Entities.Where(e => e is FactoryEntity).Select(e => e as FactoryEntity).ToList();
            MyFactories = Factories.Where(e => e.IsFriendly()).ToList();
            EnemyFactories = Factories.Where(e => e.IsEnemy()).ToList();
            NeutralFactories = Factories.Where(e => e.IsNeutral()).ToList();
            Bombs = Entities.Where(e => e is BombEntity).Select(e => e as BombEntity).ToList();
            MyBombs = Bombs.Where(e => e.IsFriendly()).ToList();
            EnemyBombs = Bombs.Where(e => e.IsEnemy()).ToList();
            Troops = Entities.Where(e => e is TroopEntity).Select(e => e as TroopEntity).ToList();
            MyTroops = Troops.Where(e => e.IsFriendly()).ToList();
            EnemyTroops = Troops.Where(e => e.IsEnemy()).ToList();
        }

        private void CalculateStats()
        {
            EnemyIncome = 0;
            MyIncome = 0;
            MyTroopsCount = 0;
            EnemyTroopsCount = 0;
            foreach (FactoryEntity factory in Factories)
            {
                if (factory.IsProducing())
                {
                    if (factory.IsFriendly())
                    {
                        MyIncome += factory.ProductionCount;
                    }
                    else if (factory.IsEnemy())
                    {
                        EnemyIncome += factory.ProductionCount;
                    }
                }
                if (factory.IsFriendly())
                {
                    MyTroopsCount += factory.NumberOfCyborgs;
                }
                else if (factory.IsEnemy())
                {
                    EnemyTroopsCount += factory.NumberOfCyborgs;
                }
            }
            foreach (TroopEntity troop in Troops)
            {
                if (troop.IsFriendly())
                {
                    MyTroopsCount += troop.NumberOfCyborgs;
                }
                else if (troop.IsEnemy())
                {
                    EnemyTroopsCount += troop.NumberOfCyborgs;
                }
            }
            foreach (BombEntity bomb in Bombs)
            {
                if (bomb.IsFirstTurn(Links))
                {
                    Console.Error.WriteLine($"Bomb was used {bomb.Id} on target {bomb.TargetFactoryId}.");
                    if (bomb.IsFriendly())
                    {
                        MyBombCount--;
                    }
                    else if (bomb.IsEnemy())
                    {
                        EnemyBombCount--;
                    }
                }
            }
        }

        public void ShowStats()
        {
            Console.Error.WriteLine("Diff: " + (MyTroopsCount - EnemyTroopsCount) + " My Troops: " + MyTroopsCount + " Enemy Troops: " + EnemyTroopsCount);
            Console.Error.WriteLine("Diff: " + (MyIncome - EnemyIncome) + " My Income: " + MyIncome + " Enemy Income: " + EnemyIncome);
            Console.Error.WriteLine($"My Bombs: {MyBombCount}, Enemy Bombs: {EnemyBombCount}");
        }
    }
}
