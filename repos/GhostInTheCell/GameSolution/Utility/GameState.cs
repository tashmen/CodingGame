using GameSolution.Entities;
using GameSolution.Moves;
using System;
using System.Collections.Generic;
using System.Linq;
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

        //Track bombs that have been sent
        private List<int> _bombIdSent = new List<int>();

        public GameState(FactoryLinks links)
        {
            Links = links;
        }

        public GameState(GameState state)
        {
            _bombIdSent = state.CopyBombsSent();
            //Because we are already tracking bombs that have been sent we have to copy over the bomb counts
            MyBombCount = state.MyBombCount;
            EnemyBombCount = state.EnemyBombCount;
            GameCounter = state.GameCounter;
            Links = state.Links;//Shouldn't be modified
            Entities = state.Entities.Select(e => EntityFactory.CreateEntity(e)).ToList();//Clone the entities as we want to update this
            UpdateGameState();
            CalculateStats();
        }

        public List<int> CopyBombsSent()
        {
            return new List<int>(_bombIdSent);
        }

        public void PlayMove(Move move, Owner owner)
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
                    Entities.Add(EntityFactory.CreateEntity(EntityTypes.Bomb, -1, (int)owner, source, target, Links.GetDistance(source, target), 0));
                    if (owner == Owner.Me)
                        MyBombCount--;
                    else EnemyBombCount--;
                    UpdateGameState();
                    break;
                case MoveType.Move:
                    source = Convert.ToInt32(moveArgs[1]);
                    target = Convert.ToInt32(moveArgs[2]);
                    int cyborgCount = Convert.ToInt32(moveArgs[3]);
                    sourceFactory = Factories.First(e => e.Id == source);
                    sourceFactory.Move(cyborgCount);
                    Entities.Add(EntityFactory.CreateEntity(EntityTypes.Troop, -1, (int)owner, source, target, cyborgCount, Links.GetDistance(source, target)));
                    UpdateGameState();
                    break;
                case MoveType.Upgrade:
                    source = Convert.ToInt32(moveArgs[1]);
                    sourceFactory = Factories.First(e => e.Id == source);
                    sourceFactory.Upgrade();
                    if (owner == Owner.Me)
                        MyTroopsCount -= 10;
                    else EnemyTroopsCount -= 10;
                    break;
            }
        }

        /// <summary>
        /// Setup entities for this turn
        /// </summary>
        /// <param name="entites">The entities to fill the game state</param>
        public void SetEntities(List<Entity> entites)
        {
            Entities = entites;
            UpdateGameState();
            CalculateStats();
            GameCounter++;
        }

        /// <summary>
        /// Updates the various shortcut lists.
        /// </summary>
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

        /// <summary>
        /// Calculates the various statistics
        /// </summary>
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
                if (!_bombIdSent.Contains(bomb.Id))
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
                    _bombIdSent.Add(bomb.Id);
                }
            }
        }

        /// <summary>
        /// Shows the stats in the error log
        /// </summary>
        public void ShowStats()
        {
            Console.Error.WriteLine("Diff: " + (MyTroopsCount - EnemyTroopsCount) + " My Troops: " + MyTroopsCount + " Enemy Troops: " + EnemyTroopsCount);
            Console.Error.WriteLine("Diff: " + (MyIncome - EnemyIncome) + " My Income: " + MyIncome + " Enemy Income: " + EnemyIncome);
            Console.Error.WriteLine($"My Bombs: {MyBombCount}, Enemy Bombs: {EnemyBombCount}");
        }
    }
}
