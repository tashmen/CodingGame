using Algorithms.GameComponent;
using Algorithms.Space;
using GameSolution.Entities;
using GameSolution.Game;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace GameSolution
{
    public class GameState : IGameState
    {
        public Board Board;
        public Entity?[] Entities;
        public int Turn = 0;
        public int Seed = 42;

        public long LastMove = 0;
        public long NeutralLastMove = 0;

        //Calculated Values
        double numberOfUnitsMine = 0, numberOfUnitsOpponent = 0, hpOfUnitsMine = 0, hpOfUnitsOpponent = 0;
        int[] boardMap = new int[Board.MaxHeight * Board.MaxWidth];
        List<Entity> myEntities;
        List<Entity> oppEntities;
        List<Entity> neutralEntities;
        //End Calculated Values

        public GameState(Board board)
        {
            Board = board;
        }

        public GameState(GameState state)
        {
            Board = state.Board;
            Entities = state.Entities.Select(e => e?.Clone()).ToArray();
            Turn = state.Turn;
            LastMove = state.LastMove;
            NeutralLastMove = state.NeutralLastMove;
            Seed = state.Seed;
            Reset();
        }

        public void Reset()
        {
            myEntities = new List<Entity>(14);
            oppEntities = new List<Entity>(14);
            neutralEntities = new List<Entity>(14);

            numberOfUnitsMine = numberOfUnitsOpponent = hpOfUnitsMine = hpOfUnitsOpponent = 0;
            boardMap = new int[Board.MaxHeight * Board.MaxWidth];
            for (int y = 0; y < Board.MaxHeight; y++)
            {
                for (int x = 0; x < Board.MaxWidth; x++)
                {
                    SetLocation(x, y, (int)Board.GetLocation(x, y));
                }
            }
            for(int i = 0; i<Entities.Length; i++)
            {
                var entity = Entities[i];
                if (entity == null)
                    continue;

                if (entity.IsOwned(true))
                {
                    myEntities.Add(entity);
                }
                else if(entity.IsOwned(false))
                {
                    oppEntities.Add(entity);
                }
                else neutralEntities.Add(entity);
                
                SetBoardMap(entity);
            }
        }

        public void SetState(Entity[] entities)
        {
            Entities = entities;
            Reset();
        }

        public void ApplyMove(object move, bool isMax)
        {
            long m = (long)move;
            switch (Move.GetMoveType(m))
            {
                case MoveType.Wait:
                    break;
                case MoveType.Move:
                    Entity moveEntity = Entities[Move.GetUnitId(m)];
                    ClearBoardMap(moveEntity);
                    moveEntity.Move(Move.GetLocation(m));
                    SetBoardMap(moveEntity);
                    break;
                case MoveType.Shoot:
                    Entity shootEntity = Entities[Move.GetUnitId(m)];
                    Entity shootEntityTarget = Entities[Move.GetTargetUnitId(m)];

                    bool isMine = shootEntityTarget.IsOwned(true);
                    bool isOpp = shootEntityTarget.IsOwned(false);
                    if (isMine)
                        hpOfUnitsMine -= shootEntityTarget.Hp;
                    else if(isOpp)
                        hpOfUnitsOpponent -= shootEntityTarget.Hp;
                    
                    shootEntity.Shoot(shootEntityTarget);
                    if (shootEntityTarget.IsDead())
                    {
                        Entities[Move.GetTargetUnitId(m)] = null;
                        ClearBoardMap(shootEntityTarget);
                        if (isMine)
                            myEntities.Remove(shootEntityTarget);
                        else if(isOpp)
                            oppEntities.Remove(shootEntityTarget);
                    }
                    else 
                    {
                        if (isMine)
                            hpOfUnitsMine += shootEntityTarget.Hp;
                        else if(isOpp)
                            hpOfUnitsOpponent += shootEntityTarget.Hp;
                    }
                    break;
                case MoveType.Convert:
                    Entity convertEntity = Entities[Move.GetUnitId(m)];
                    Entity convertEntityTarget = Entities[Move.GetTargetUnitId(m)];
                    if(convertEntityTarget.Owner == OwnerType.Neutral)
                    {
                        neutralEntities.Remove(convertEntityTarget);
                        if (isMax)
                        {
                            numberOfUnitsMine++;
                            myEntities.Add(convertEntityTarget);
                        }
                        else
                        {
                            numberOfUnitsOpponent++;
                            oppEntities.Add(convertEntityTarget);
                        }
                    }
                    else
                    {
                        var modifier = isMax ? 1 : -1;
                        numberOfUnitsMine += modifier;
                        numberOfUnitsOpponent -= modifier;
                        if (isMax)
                        {
                            myEntities.Add(convertEntityTarget);
                            oppEntities.Remove(convertEntityTarget);
                        }
                        else
                        {
                            oppEntities.Add(convertEntityTarget);
                            myEntities.Remove(convertEntityTarget);
                        }
                    }
                    convertEntity.Convert(convertEntityTarget);
                    break;
            }

            /*
            for (int y = 0; y < Board.MaxHeight; y++)
            {
                for (int x = 0; x < Board.MaxWidth; x++)
                {
                    if (Board.GetLocation(x, y) == LocationType.Obstacle && GetLocation(x, y) >= 0)
                        throw new Exception("Discrepency in board map.");
                }
            }
            foreach (Entity? entity in Entities)
            {
                if (entity == null)
                    continue;
                if (!entity.IsDead())
                {
                    if (GetLocation(entity.Point.x, entity.Point.y) != entity.Id)
                        throw new Exception("Discrepency in board map.");
                }
            }
            */

            //Reset();

            MoveNeutralUnit();
            Turn++;
            LastMove = m;
        }

        public IGameState Clone()
        {
            return new GameState(this);
        }

        public bool Equals(IGameState state)
        {
            GameState game = (GameState)state;

            if(Turn != game.Turn)
                return false;

            /*
            if (Seed != game.Seed)
                return false;
            */

            //Assumes entities are always in same order...
            for(int i = 0; i<Entities.Length; i++)
            {
                if (Entities[i] == null && game.Entities[i] != null)
                    return false;
                if (!(Entities[i] == game.Entities[i] || Entities[i].Equals(game.Entities[i])))
                    return false;
            }
            return true;
        }

        
        public void CalculateUnits()
        {
            if (numberOfUnitsMine == 0 && numberOfUnitsOpponent == 0)
            {
                for(int i = 0; i<Entities.Length; i++)
                {
                    var entity = Entities[i];
                    if (entity == null)
                        continue;
                    
                    if (entity.Owner == OwnerType.Max)
                    {
                        numberOfUnitsMine++;
                        hpOfUnitsMine += entity.Hp;
                    }
                    else if(entity.Owner == OwnerType.Min)
                    {
                        numberOfUnitsOpponent++;
                        hpOfUnitsOpponent += entity.Hp;
                    }
                }
            }
        }
        public double Evaluate(bool isMax)
        {
            CalculateUnits();
            double unitValue = (numberOfUnitsMine - numberOfUnitsOpponent) / Math.Max(numberOfUnitsMine, numberOfUnitsOpponent);
            double hpValue = (hpOfUnitsMine - hpOfUnitsOpponent) / Math.Max(hpOfUnitsMine, hpOfUnitsOpponent);
            double value = unitValue * 0.75 + hpValue * 0.25;
            return value;
        }

        public object GetMove(bool isMax)
        {
            return LastMove;
        }

        public int GetLocation(int location)
        {
            return boardMap[location];
        }

        public int GetLocation(int x, int y)
        {
            return boardMap[Board.ConvertPointToLocation(x, y)];
        }

        public void SetLocation(int location, int value)
        {
            boardMap[location] = value;
        }

        public void SetLocation(int x, int y, int value)
        {
            boardMap[Board.ConvertPointToLocation(x, y)] = value;
        }

        public void SetBoardMap(Entity entity)
        {
            SetLocation(entity.Location, entity.Id);
        }

        public void ClearBoardMap(Entity entity)
        {
            SetLocation(entity.Location, (int)LocationType.Empty);
        }

        public bool IsSpaceEmpty(int location)
        {
            return GetLocation(location) == (int)LocationType.Empty;
        }

        public bool IsSpaceEmpty(int x, int y)
        {
            return GetLocation(x, y) == (int)LocationType.Empty;
        }

        public bool IsUnit(int locationType)
        {
            return locationType >= 0;
        }

        public bool HasCultist(int location, bool isMax)
        {
            var spaceId = GetLocation(location);

            if (IsUnit(spaceId))
            {
                var entity = Entities[spaceId];
                if (entity == null)
                    return false;
                return entity.Type == EntityType.Cultist && !entity.IsOwned(isMax);
            }

            return false;
        }

        public bool HasCultist(int x, int y, bool isMax)
        {
            var location = GetLocation(x, y);

            if (IsUnit(location))
            {
                var entity = Entities[location];
                if (entity == null)
                    return false;
                return entity.Type == EntityType.Cultist && !entity.IsOwned(isMax);
            }

            return false;
        }

        
        public void GetMovesForEntity(ref IList possibleMoves, Entity entity)
        {
            int[] locations = Board.GetNeighboringLocations(entity.Location);

            for (int i = 0; i<4; i++)
            {
                int location = locations[i];                

                if (location != (int)LocationType.Obstacle && IsSpaceEmpty(location))
                {
                    possibleMoves.Add(Move.MoveUnit(entity.Id, location));
                }
            }
        }

        public void GetConvertsForEntity(ref IList possibleMoves, Entity entity, bool isMax)
        {
            int[] locations = Board.GetNeighboringLocations(entity.Location);

            for (int i = 0; i < 4; i++)
            {
                int location = locations[i];

                if (location != (int)LocationType.Obstacle && HasCultist(location, isMax))
                {
                    possibleMoves.Add(Move.Convert(entity.Id, GetLocation(location)));
                }
            }
        }

        public IList GetPossibleMoves(bool isMax)
        {
            var entitiesToCheck = isMax ? myEntities : oppEntities;
            var targetEntitiesToCheck = isMax ? oppEntities : myEntities;

            IList possibleMoves = new List<long>(20);
            if (Turn == 150)
                return possibleMoves;

            for(int i = 0; i< entitiesToCheck.Count; i++)
            {
                var entity = entitiesToCheck[i];

                GetMovesForEntity(ref possibleMoves, entity);

                if (entity.Type == EntityType.CultLeader)
                {
                    GetConvertsForEntity(ref possibleMoves, entity, isMax);
                }
                else
                {
                    for (int ti = 0; ti < targetEntitiesToCheck.Count; ti++)
                    {
                        var targetEntity = targetEntitiesToCheck[ti];

                        if (Board.GetManhattenDistance(entity.Location, targetEntity.Location) <= 6)
                        {
                            var endLocation = CheckBulletPath(entity.Location, targetEntity.Location);
                            if (endLocation == targetEntity.Location)
                            {
                                possibleMoves.Add(Move.Shoot(entity.Id, targetEntity.Id));
                            }
                        }
                    }

                    for (int ti = 0; ti < neutralEntities.Count; ti++)
                    {
                        var targetEntity = neutralEntities[ti];

                        if (!targetEntity.IsDead() && Board.GetManhattenDistance(entity.Location, targetEntity.Location) <= 6)
                        {
                            var endLocation = CheckBulletPath(entity.Location, targetEntity.Location);
                            if (endLocation == targetEntity.Location)
                            {
                                possibleMoves.Add(Move.Shoot(entity.Id, targetEntity.Id));
                            }
                        }
                    }
                }
            }

            if (possibleMoves.Count == 0)
                possibleMoves.Add(Move.Wait());

            return possibleMoves;
        }


        public double? GetWinner()
        {
            CalculateUnits();
            if (Turn == 150)
            {
                var diff = numberOfUnitsMine - numberOfUnitsOpponent;
                return diff > 0 ? 1 : diff < 0 ? -1 : 0;
            }
            else
            {
                if (numberOfUnitsOpponent == 0)
                    return 1;
                else if (numberOfUnitsMine == 0)
                    return -1;
            }

            return null;
        }
        public int CheckBulletPath(int startTile, int targetTile)
        {
            var locations = Board.GetBresenhamPoints(startTile, targetTile);
            for (int i = 0; i < locations.Length; i++)
            {
                var location = locations[i];
                var spaceId = GetLocation(location);
                if (IsUnit(spaceId))
                {
                    return location;
                }
            }

            return locations[locations.Length - 1];
        }

        public void MoveNeutralUnit()
        {
            NeutralLastMove = -1;
            var neutralCount = neutralEntities.Count;
            if (neutralCount > 0)
            {
                int index = InternalRandom.rand(ref Seed, 12);
                if (index < neutralCount)
                {
                    Entity neutralUnit = neutralEntities[index];
                    if (neutralUnit.IsDead())
                        return;
                    IList moves = new List<long>();
                    GetMovesForEntity(ref moves, neutralUnit);
                    if (moves.Count > 0)
                    {
                        long action = (long)moves[InternalRandom.rand(ref Seed, moves.Count)];
                        NeutralLastMove = action;
                        ClearBoardMap(neutralUnit);
                        neutralUnit.Move(Move.GetLocation(action));
                        SetBoardMap(neutralUnit);
                    }
                    else
                    {
                        //Seed still moves even if there are no legal move actions; the unit still must 'wait'!
                        InternalRandom.rand(ref Seed, 1);
                    }
                }
            }
        }
    }
}
