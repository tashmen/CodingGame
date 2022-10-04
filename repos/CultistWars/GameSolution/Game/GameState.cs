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
        int[][] boardMap = new int[Board.MaxHeight][];
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
            numberOfUnitsMine = numberOfUnitsOpponent = hpOfUnitsMine = hpOfUnitsOpponent = 0;
            boardMap = new int[Board.MaxHeight][];
            for (int y = 0; y < Board.MaxHeight; y++)
            {
                boardMap[y] = new int[Board.MaxWidth];
                for (int x = 0; x < Board.MaxWidth; x++)
                {
                    boardMap[y][x] = (int)Board.GetLocation(x, y);
                }
            }
            foreach(Entity? entity in Entities)
            {
                if (entity == null)
                    continue;
                if(!entity.IsDead())
                    boardMap[entity.Point.y][entity.Point.x] = entity.Id;
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
                    moveEntity.Move(new Point2d(Move.GetX(m), Move.GetY(m)));
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
                        if (isMax)
                        {
                            numberOfUnitsMine++;
                        }
                        else numberOfUnitsOpponent++;
                    }
                    else
                    {
                        var modifier = isMax ? 1 : -1;
                        numberOfUnitsMine += modifier;
                        numberOfUnitsOpponent -= modifier;
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
                foreach (Entity entity in Entities)
                {
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

        public int GetLocation(int x, int y)
        {
            return boardMap[y][x];
        }

        public void SetBoardMap(Entity entity)
        {
            boardMap[entity.Point.y][entity.Point.x] = entity.Id;
        }

        public void ClearBoardMap(Entity entity)
        {
            boardMap[entity.Point.y][entity.Point.x] = (int)LocationType.Empty;
        }

        public bool IsSpaceEmpty(int x, int y)
        {
            return GetLocation(x, y) == (int)LocationType.Empty;
        }

        public bool IsUnit(int locationType)
        {
            return locationType >= 0;
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

        private static int[] X_MODIFIER = new int[] { 0, 1, 0, -1 };
        private static int[] Y_MODIFIER = new int[] { -1, 0, 1, 0 };
        public void GetMovesForEntity(ref IList possibleMoves, Entity entity)
        {
            int pointY = entity.Point.y;
            int pointX = entity.Point.x;

            for(int i = 0; i<4; i++)
            {
                int x = pointX + X_MODIFIER[i];
                int y = pointY + Y_MODIFIER[i];

                if (Board.IsInBounds(x, y) && IsSpaceEmpty(x, y))
                {
                    possibleMoves.Add(Move.MoveUnit(entity.Id, x, y));
                }
            }
        }

        public IList GetPossibleMoves(bool isMax)
        {
            IList possibleMoves = new List<long>();
            if (Turn == 150)
                return possibleMoves;

            foreach(Entity? entity in Entities)
            {
                if (entity == null)
                    continue;

                if (entity.IsOwned(isMax) && !entity.IsDead())
                {
                    int pointY = entity.Point.y;
                    int pointX = entity.Point.x;
                    GetMovesForEntity(ref possibleMoves, entity);

                    if (entity.Type == EntityType.CultLeader)
                    {
                        for (int x = pointX - 1; x <= pointX + 1; x += 2)
                        {
                            if (Board.IsInBounds(x, pointY) && HasCultist(x, pointY, isMax))
                            {
                                possibleMoves.Add(Move.Convert(entity.Id, boardMap[pointY][x]));
                            }
                        }

                        for (int y = pointY - 1; y <= pointY + 1; y += 2)
                        {
                            if (Board.IsInBounds(pointX, y) && HasCultist(pointX, y, isMax))
                            {
                                possibleMoves.Add(Move.Convert(entity.Id, boardMap[y][pointX]));
                            }
                        }
                    }
                    else
                    {
                        foreach (Entity? targetEntity in Entities)
                        {
                            if (targetEntity == null)
                                continue;

                            if(entity.Id != targetEntity.Id && !targetEntity.IsDead() && !targetEntity.IsOwned(isMax) && entity.Point.GetManhattenDistance(targetEntity.Point) <= 6)
                            {
                                var endPoint = CheckBulletPath(entity.Point, targetEntity.Point);
                                if (endPoint.Equals(targetEntity.Point))
                                {
                                    possibleMoves.Add(Move.Shoot(entity.Id, targetEntity.Id));
                                }
                            }
                        }
                    }
                }
            }

            //if (possibleMoves.Count == 0)
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
        public Point2d CheckBulletPath(Point2d startTile, Point2d targetTile)
        {
            var points = Board.GetBresenhamPoints(startTile, targetTile);
            for (int i = 0; i < points.Length; i++)
            {
                var point = points[i];
                var location = GetLocation(point.x, point.y);
                if (IsUnit(location) && !Entities[location].IsDead())
                {
                    return point;
                }
            }

            return points[points.Length - 1];
        }

        private void MoveNeutralUnit()
        {
            Entity[] neutralUnits = new Entity[12];
            bool hasNeutral = false;
            int neutralCount = 0;
            foreach (Entity? entity in Entities)
            {
                if (entity == null)
                    continue;

                if (entity.Owner == OwnerType.Neutral)
                {
                    neutralUnits[neutralCount++] = entity;
                    hasNeutral = true;
                }

            }

            
            if (hasNeutral)
            {
                int index = InternalRandom.rand(ref Seed, 12);
                if (index < neutralCount)
                {
                    Entity neutralUnit = neutralUnits[index];
                    IList moves = new List<long>();
                    GetMovesForEntity(ref moves, neutralUnit);
                    if (moves.Count > 0)
                    {
                        long action = (long)moves[InternalRandom.rand(ref Seed, moves.Count)];
                        NeutralLastMove = action;
                        ClearBoardMap(neutralUnit);
                        neutralUnit.Move(new Point2d(Move.GetX(action), Move.GetY(action)));
                        SetBoardMap(neutralUnit);
                    }
                }
            }
        }
    }
}
