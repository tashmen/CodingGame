using GameSolution.Arrivals;
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
        public int MyTotalCyborgsAvailableToSend { get; private set; } = 0;

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

        //Track bombs that have been sent along with the number of turns the bomb has been on the board
        private Dictionary<int, int> _bombsSent = new Dictionary<int, int>();

        public GameState(FactoryLinks links)
        {
            Links = links;
        }

        public GameState(GameState state)
        {
            //Because we are already tracking bombs that have been sent we have to copy over the bomb counts
            MyBombCount = state.MyBombCount;
            EnemyBombCount = state.EnemyBombCount;
            GameCounter = state.GameCounter;
            Links = state.Links;//Shouldn't be modified
            Entities = state.Entities.Select(e => EntityFactory.CreateEntity(e)).ToList();//Clone the entities as we want to update this
            _bombsSent = new Dictionary<int, int>(state._bombsSent);
            UpdateGameState();
            CalculateStats();
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
                    break;
                case MoveType.Move:
                    source = Convert.ToInt32(moveArgs[1]);
                    target = Convert.ToInt32(moveArgs[2]);
                    int cyborgCount = Convert.ToInt32(moveArgs[3]);
                    sourceFactory = Factories.First(e => e.Id == source);
                    sourceFactory.Move(cyborgCount);
                    Entities.Add(EntityFactory.CreateEntity(EntityTypes.Troop, -1, (int)owner, source, target, cyborgCount, Links.GetDistance(source, target)));
                    break;
                case MoveType.Upgrade:
                    source = Convert.ToInt32(moveArgs[1]);
                    sourceFactory = Factories.First(e => e.Id == source);
                    sourceFactory.Upgrade();
                    break;
            }
            UpdateGameState();
            CalculateStats();
        }

        /// <summary>
        /// Setup entities for this turn
        /// </summary>
        /// <param name="entites">The entities to fill the game state</param>
        public void SetEntities(List<Entity> entites)
        {
            Entities = entites;
            UpdateGameState();
            CalculateStats(true);
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
        private void CalculateStats(bool advanceBombs = false)
        {
            EnemyIncome = 0;
            MyIncome = 0;
            MyTroopsCount = 0;
            EnemyTroopsCount = 0;
            MyTotalCyborgsAvailableToSend = 0;

            foreach (BombEntity bomb in Bombs)
            {
                if (!_bombsSent.ContainsKey(bomb.Id))
                {
                    _bombsSent[bomb.Id] = 1;//By the time bombs are seen they are moving
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
                else if (advanceBombs)//skip incrementing unless we are doing a new turn.
                {
                    _bombsSent[bomb.Id]++;
                }
            }

            foreach (FactoryEntity factory in Factories)
            {
                factory.BuildArrivals(Troops, Bombs, _bombsSent, Links);
                if (factory.IsFriendly())
                {
                    if (factory.IsProducing())
                    {
                        MyIncome += factory.ProductionCount;
                    }
                    MyTotalCyborgsAvailableToSend += factory.NumberOfCyborgs;
                    MyTroopsCount += factory.NumberOfCyborgs;
                }
                else if (factory.IsEnemy())
                {
                    if (factory.IsProducing())
                    {
                        EnemyIncome += factory.ProductionCount;
                    }
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
