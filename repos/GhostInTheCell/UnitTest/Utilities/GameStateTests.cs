using GameSolution.Entities;
using GameSolution.Utility;
using System.Collections.Generic;
using System.Linq;
using Xunit;
using static GameSolution.Constants;

namespace UnitTest.Utilities
{
    public class GameStateTests
    {
        GameState _state;
        public GameStateTests()
        {
            FactoryLinks _links;
            _links = new FactoryLinks();
            _links.AddLink(1, 2, 1);
            _links.AddLink(1, 3, 2);
            _links.AddLink(1, 4, 4);
            _links.AddLink(2, 3, 1);
            _links.AddLink(2, 4, 2);

            _state = new GameState(_links);
        }

        [Fact]
        public void Test_SetEntities()
        {
            List<Entity> entities = new List<Entity>()
            {

            };

            _state.SetEntities(entities);
            Assert.Equal(2, _state.MyBombCount);
            Assert.Equal(2, _state.EnemyBombCount);
            Assert.Equal(0, _state.MyIncome);
            Assert.Equal(0, _state.EnemyIncome);
            Assert.Equal(0, _state.MyTroopsCount);
            Assert.Equal(0, _state.EnemyTroopsCount);
            Assert.Equal(1, _state.GameCounter);

            entities = new List<Entity>()
            {
                new FactoryEntity(1, (int)Owner.Me, 5, 1, 0, 0),
                new FactoryEntity(2, (int)Owner.Opponent, 7, 2, 0, 0),
                new FactoryEntity(10, (int)Owner.Opponent, 0, 0, 0, 0),
                new FactoryEntity(11, (int)Owner.Opponent, 0, 0, 0, 0),
                new FactoryEntity(3, (int)Owner.Neutral, 5, 3, 0, 0),
                new FactoryEntity(7, (int)Owner.Neutral, 5, 3, 0, 0),
                new TroopEntity(4, (int)Owner.Me, 1, 2, 1, 0),
                new TroopEntity(5, (int)Owner.Opponent, 2, 1, 2, 0),
                new TroopEntity(12, (int)Owner.Opponent, 2, 1, 0, 0),
                new BombEntity(6, (int)Owner.Me, 1, 2, 2, 0),
                new BombEntity(8, (int)Owner.Opponent, 2, 1, 2, 0),
                new BombEntity(9, (int)Owner.Opponent, 2, 1, 2, 0),
            };

            _state.SetEntities(entities);
            Assert.Equal(1, _state.MyBombCount);
            Assert.Equal(0, _state.EnemyBombCount);
            Assert.Equal(1, _state.MyIncome);
            Assert.Equal(2, _state.EnemyIncome);
            Assert.Equal(6, _state.MyTroopsCount);
            Assert.Equal(9, _state.EnemyTroopsCount);
            Assert.Equal(2, _state.GameCounter);

            Assert.Equal(6, _state.Factories.Count);
            Assert.Equal(3, _state.EnemyFactories.Count);
            Assert.Equal(2, _state.NeutralFactories.Count);
            Assert.Single(_state.MyFactories);

            Assert.Equal(3, _state.Bombs.Count);
            Assert.Single(_state.MyBombs);
            Assert.Equal(2, _state.EnemyBombs.Count);

            Assert.Equal(3, _state.Troops.Count);
            Assert.Single(_state.MyTroops);
            Assert.Equal(2, _state.EnemyTroops.Count);
        }

        [Fact]
        public void Test_GameStateFromState()
        {
            List<Entity> entities = new List<Entity>()
            {

            };

            _state.SetEntities(entities);

            GameState local = new GameState(_state);

            Assert.Equal(2, local.MyBombCount);
            Assert.Equal(2, local.EnemyBombCount);
            Assert.Equal(0, local.MyIncome);
            Assert.Equal(0, local.EnemyIncome);
            Assert.Equal(0, local.MyTroopsCount);
            Assert.Equal(0, local.EnemyTroopsCount);
            Assert.Equal(1, local.GameCounter);
            Assert.Equal(0, local.MyTotalCyborgsAvailableToSend);


            entities = new List<Entity>()
            {
                new FactoryEntity(1, (int)Owner.Me, 5, 1, 0, 0),
                new FactoryEntity(2, (int)Owner.Opponent, 7, 2, 0, 0),
                new FactoryEntity(10, (int)Owner.Opponent, 0, 0, 0, 0),
                new FactoryEntity(11, (int)Owner.Opponent, 0, 0, 0, 0),
                new FactoryEntity(3, (int)Owner.Neutral, 5, 3, 0, 0),
                new FactoryEntity(7, (int)Owner.Neutral, 5, 3, 0, 0),
                new TroopEntity(4, (int)Owner.Me, 1, 2, 1, 0),
                new TroopEntity(5, (int)Owner.Opponent, 2, 1, 2, 0),
                new TroopEntity(12, (int)Owner.Opponent, 2, 1, 0, 0),
                new BombEntity(6, (int)Owner.Me, 1, 2, 2, 0),
                new BombEntity(8, (int)Owner.Opponent, 2, -1, -1, 0),
                new BombEntity(9, (int)Owner.Opponent, 2, -1, -1, 0),
            };

            _state.SetEntities(entities);

            local = new GameState(_state);
            Assert.Equal(1, local.MyBombCount);
            Assert.Equal(0, local.EnemyBombCount);
            Assert.Equal(1, local.MyIncome);
            Assert.Equal(2, local.EnemyIncome);
            Assert.Equal(6, local.MyTroopsCount);
            Assert.Equal(9, local.EnemyTroopsCount);
            Assert.Equal(2, local.GameCounter);

            Assert.Equal(6, local.Factories.Count);
            Assert.Equal(3, local.EnemyFactories.Count);
            Assert.Equal(2, local.NeutralFactories.Count);
            Assert.Single(local.MyFactories);

            Assert.Equal(3, local.Bombs.Count);
            Assert.Single(local.MyBombs);
            Assert.Equal(2, local.EnemyBombs.Count);

            Assert.Equal(3, local.Troops.Count);
            Assert.Single(local.MyTroops);
            Assert.Equal(2, local.EnemyTroops.Count);
            Assert.Equal(5, local.MyTotalCyborgsAvailableToSend);

            entities = new List<Entity>()
            {
                new FactoryEntity(1, (int)Owner.Me, 5, 1, 0, 0),
                new FactoryEntity(2, (int)Owner.Opponent, 7, 2, 0, 0),
                new FactoryEntity(10, (int)Owner.Opponent, 0, 0, 0, 0),
                new FactoryEntity(11, (int)Owner.Opponent, 0, 0, 0, 0),
                new FactoryEntity(3, (int)Owner.Neutral, 5, 3, 0, 0),
                new FactoryEntity(7, (int)Owner.Neutral, 5, 3, 0, 0),
                new TroopEntity(4, (int)Owner.Me, 1, 2, 1, 0),
                new TroopEntity(5, (int)Owner.Opponent, 2, 1, 2, 0),
                new TroopEntity(12, (int)Owner.Opponent, 2, 1, 0, 0),
                new BombEntity(6, (int)Owner.Me, 1, 2, 2, 0),
                new BombEntity(8, (int)Owner.Opponent, 2, -1, -1, 0),
                new BombEntity(9, (int)Owner.Opponent, 2, -1, -1, 0),
            };

            _state.SetEntities(entities.Select(e => EntityFactory.CreateEntity(e)).ToList());

            local = new GameState(_state);
            Assert.Equal(1, local.MyBombCount);
            Assert.Equal(0, local.EnemyBombCount);
            Assert.Equal(1, local.MyIncome);
            Assert.Equal(2, local.EnemyIncome);
            Assert.Equal(6, local.MyTroopsCount);
            Assert.Equal(9, local.EnemyTroopsCount);
            Assert.Equal(3, local.GameCounter);

            Assert.Equal(6, local.Factories.Count);
            Assert.Equal(3, local.EnemyFactories.Count);
            Assert.Equal(2, local.NeutralFactories.Count);
            Assert.Single(local.MyFactories);

            Assert.Equal(3, local.Bombs.Count);
            Assert.Single(local.MyBombs);
            Assert.Equal(2, local.EnemyBombs.Count);

            Assert.Equal(3, local.Troops.Count);
            Assert.Single(local.MyTroops);
            Assert.Equal(2, local.EnemyTroops.Count);

            entities = new List<Entity>()
            {
                new FactoryEntity(1, (int)Owner.Me, 5, 1, 0, 0),
                new FactoryEntity(2, (int)Owner.Opponent, 7, 2, 0, 0),
                new FactoryEntity(10, (int)Owner.Opponent, 0, 0, 0, 0),
                new FactoryEntity(11, (int)Owner.Opponent, 0, 0, 0, 0),
                new FactoryEntity(3, (int)Owner.Neutral, 5, 3, 0, 0),
                new FactoryEntity(7, (int)Owner.Neutral, 5, 3, 0, 0),
                new TroopEntity(4, (int)Owner.Me, 1, 2, 1, 0),
                new TroopEntity(5, (int)Owner.Opponent, 2, 1, 2, 0),
                new TroopEntity(12, (int)Owner.Opponent, 2, 1, 0, 0),
            };

            _state.SetEntities(entities.Select(e => EntityFactory.CreateEntity(e)).ToList());

            local = new GameState(_state);
            Assert.Equal(1, local.MyBombCount);
            Assert.Equal(0, local.EnemyBombCount);
            Assert.Equal(1, local.MyIncome);
            Assert.Equal(2, local.EnemyIncome);
            Assert.Equal(6, local.MyTroopsCount);
            Assert.Equal(9, local.EnemyTroopsCount);
            Assert.Equal(4, local.GameCounter);

            Assert.Equal(6, local.Factories.Count);
            Assert.Equal(3, local.EnemyFactories.Count);
            Assert.Equal(2, local.NeutralFactories.Count);
            Assert.Single(local.MyFactories);

            Assert.Empty(local.Bombs);
            Assert.Empty(local.MyBombs);
            Assert.Empty(local.EnemyBombs);

            Assert.Equal(3, local.Troops.Count);
            Assert.Single(local.MyTroops);
            Assert.Equal(2, local.EnemyTroops.Count);
        }
    }
}
