using GameSolution.Entities;
using GameSolution.Moves;
using System;
using System.Collections.Generic;
using System.Linq;
using static GameSolution.Constants;

namespace GameSolution.Utility
{
    public class GameHelper
    {
        private GameState _pristineState;
        private GameState _internalState;

        public GameHelper(GameState state)
        {
            _pristineState = state;
            _internalState = new GameState(state);
        }
        

        public MoveList PickMoves()
        {
            MoveList moves = new MoveList();
            List<FactoryEntity> otherFactories = _internalState.Factories;
            List<FactoryEntity> friendlyFactories = _internalState.MyFactories;

            int globalCyborgsAvailableToSend = CalculateTotalCyborgsAvailableToSend();
            

            Dictionary<int, int> factoryIdToCyborgsToTakeover = new Dictionary<int, int>();
            List<MoveOption> multiFactoryTakeoverMoves = new List<MoveOption>();
            foreach (FactoryEntity sourceFactory in friendlyFactories)
            {
                //Console.Error.WriteLine(sourceFactory.ToString());
                int cyborgsAvailableToSend = CalculateCyborgsAvailableToSend(sourceFactory);
                bool hasTarget = true;//Assume a target will be found
                globalCyborgsAvailableToSend -= (sourceFactory.NumberOfCyborgs - cyborgsAvailableToSend);

                //Condition for holding onto troops in factory to get to an upgrade
                if (!IsFrontLineFactory(sourceFactory) && sourceFactory.ProductionCount < 3 && (_internalState.MyIncome < _internalState.EnemyIncome || _internalState.MyTroopsCount > _internalState.EnemyTroopsCount))
                {
                    //Don't send troops out of this factory we want to use it to upgrade
                    if(sourceFactory.ProductionCount < 2)
                    {
                        globalCyborgsAvailableToSend -= cyborgsAvailableToSend;
                        cyborgsAvailableToSend = 0;
                    }
                    
                    if (sourceFactory.NumberOfCyborgs >= 10)
                    {
                        //At the final upgrade stage so send the rest onward to wherever
                        if(sourceFactory.ProductionCount == 2)
                        {
                            globalCyborgsAvailableToSend -= 10;
                            cyborgsAvailableToSend -= 10;
                        }

                        Move move = new Move(sourceFactory.Id);
                        moves.AddMove(move);
                        _internalState.PlayMove(move, Owner.Me);
                    }
                }

                Console.Error.WriteLine("Available : " + cyborgsAvailableToSend);
                //As long as there are cyborgs to send let's see if there are any targets
                while (cyborgsAvailableToSend > 0 && hasTarget)
                {
                    FactoryEntity bestTarget = null;
                    int bestValue = -99999;
                    int cyborgsToSend = 0;
                    int cyborgsLeftToTakeover = 0;
                    bool isCompleteTakeover = false;
                    foreach (FactoryEntity targetFactory in otherFactories)
                    {
                        int val = 0;
                        if (targetFactory.Id == sourceFactory.Id)
                        {
                            continue;//Can't send to self
                        }

                        factoryIdToCyborgsToTakeover.TryGetValue(targetFactory.Id, out int cyborgsToTakeover);
                        int cyborgsToTakeoverComplete = 0;
                        if (cyborgsToTakeover > 0)
                        {
                            val += 25;//we already said this was a good target so commit to it.
                            Console.Error.WriteLine("Target: " + targetFactory.Id + " Takeover left: " + cyborgsToTakeover);
                            cyborgsToTakeoverComplete = CalculateCyborgsRequiredToTakeover(sourceFactory, targetFactory);
                        }
                        else
                        {
                            cyborgsToTakeover = CalculateCyborgsRequiredToTakeover(sourceFactory, targetFactory);
                            if (cyborgsToTakeover > 0)
                            {
                                Console.Error.WriteLine("Target: " + targetFactory.Id + " Takeover: " + cyborgsToTakeover);
                            }
                        }
                        if (cyborgsToTakeover <= 0)
                        {
                            continue;//skip places where we are already on track to take over
                        }

                        int distance = _internalState.Links.GetShortestPathDistance(sourceFactory.Id, targetFactory.Id);
                        if (distance >= 9)
                        {
                            continue;
                        }

                        if (targetFactory.IsNeutral())
                        {
                            var enemyFactories = otherFactories.Where(f => f.IsEnemy()).ToList();
                            int minDist = 9999;
                            foreach (FactoryEntity enemy in enemyFactories)
                            {
                                int dist = _internalState.Links.GetDistance(targetFactory.Id, enemy.Id);
                                if (distance < minDist)
                                {
                                    minDist = distance;
                                }
                            }
                            val += minDist * 5;
                        }

                        val += cyborgsToTakeover * -1;//factories that take a lot of borgs to take over aren't as good of a choice
                        val += targetFactory.IsProducing() ? targetFactory.ProductionCount * 10 : 0;//lots of bonus for high yield factories
                        val += targetFactory.Owner == Owner.Opponent ? targetFactory.ProductionCount * 5 : 0;
                        val += distance * -10;

                        if (val > bestValue)
                        {
                            //If we don't have enough troops for a takeover then it's not the best target
                            //Need to adjust the multi-source takeover to include synchronization on time or put it into a holding that will only execute if fulfilled
                            if (cyborgsToTakeover <= globalCyborgsAvailableToSend)
                            {
                                cyborgsLeftToTakeover = 0;
                                isCompleteTakeover = false;
                                if (cyborgsToTakeover > cyborgsAvailableToSend)
                                {
                                    cyborgsLeftToTakeover = cyborgsToTakeover - sourceFactory.NumberOfCyborgs;
                                    Console.Error.WriteLine("Not enough cyborgs at this factory!");
                                }
                                else if(cyborgsToTakeoverComplete > 0 && cyborgsToTakeoverComplete < cyborgsAvailableToSend)
                                {
                                    cyborgsToTakeover = cyborgsToTakeoverComplete;
                                    isCompleteTakeover = true;
                                    Console.Error.WriteLine($"Factory: {sourceFactory.Id} can take over {targetFactory.Id} by itself.");
                                }
                                bestValue = val;
                                bestTarget = targetFactory;
                                cyborgsToSend = Math.Min(Math.Min(cyborgsToTakeover, sourceFactory.NumberOfCyborgs), cyborgsAvailableToSend);
                                //Console.Error.WriteLine("Value: " + val + " Best Target: " + targetFactory.Id + " to send " + cyborgsToSend);
                            }
                        }
                    }
                    if (bestTarget != null)
                    {
                        int bestTargetId = _internalState.Links.GetShortestPath(sourceFactory.Id, bestTarget.Id);
                        //If the best target is being sent to a non-friendly factory that has troops then take the long way
                        if (otherFactories.Where(e => e.Id == bestTargetId && !e.IsFriendly() && ((FactoryEntity)e).NumberOfCyborgs != 0).Any())
                        {
                            bestTargetId = bestTarget.Id;
                        }
                        cyborgsAvailableToSend -= cyborgsToSend;
                        globalCyborgsAvailableToSend -= cyborgsToSend;

                        if (!isCompleteTakeover && (cyborgsLeftToTakeover > 0 || multiFactoryTakeoverMoves.Where(m => m.TargetFactory.Id == bestTarget.Id || m.SourceFactory.Id == sourceFactory.Id).Any()))
                        {
                            factoryIdToCyborgsToTakeover[bestTarget.Id] = cyborgsLeftToTakeover;
                            Console.Error.WriteLine($"Leftover to send: {cyborgsLeftToTakeover} at {bestTarget.Id}");
                            MoveOption move = new MoveOption(sourceFactory, bestTarget, cyborgsToSend, bestTargetId);
                            multiFactoryTakeoverMoves.Add(move);
                        }
                        else
                        {
                            Console.Error.WriteLine("Best Target Acquired: " + bestTarget.Id + " to send " + cyborgsToSend);
                            factoryIdToCyborgsToTakeover[bestTarget.Id] = 0;
                            Move move = new Move(sourceFactory.Id, bestTargetId, cyborgsToSend);
                            moves.AddMove(move);
                            _internalState.PlayMove(move, Owner.Me);
                        }
                    }
                    else
                    {
                        hasTarget = false;//No targets so abort

                        if ((sourceFactory.ProductionCount < 3 && cyborgsAvailableToSend >= 10) && (_internalState.MyTroopsCount - _internalState.EnemyTroopsCount) > -10 && _internalState.GameCounter > 5)
                        {
                            Move move = new Move(sourceFactory.Id);
                            moves.AddMove(move);
                            cyborgsAvailableToSend -= 10;
                            globalCyborgsAvailableToSend -= 10;
                            _internalState.PlayMove(move, Owner.Me);
                            continue;
                        }

                        //If there are no targets then spew out borgs to facilities that need to grow
                        bool isFrontLineFactory = IsFrontLineFactory(sourceFactory);
                        if (sourceFactory.ProductionCount == 3 && !isFrontLineFactory)
                        {
                            Console.Error.WriteLine("Evacuating 3 production facility");
                            FactoryEntity friendlySourceTarget = null;
                            int bestFriendlyValue = -9999;
                            List<FactoryEntity> enemyFactories = _internalState.EnemyFactories;
                            foreach (FactoryEntity friendlySource in friendlyFactories)
                            {
                                if (sourceFactory.Id == friendlySource.Id)
                                    continue;

                                int friendlyVal = 0;
                                //score points for closeness to the soure
                                friendlyVal += _internalState.Links.GetShortestPathDistance(sourceFactory.Id, friendlySource.Id) * -5;
                                foreach(FactoryEntity enemyFactory in _internalState.EnemyFactories)
                                {
                                    //score more points the closer the factory is to the enemy
                                    friendlyVal += _internalState.Links.GetShortestPathDistance(friendlySource.Id, enemyFactory.Id) * -10;
                                }
                                if (friendlySource.NumberOfCyborgs > 20)
                                {
                                    friendlyVal += -100;
                                }
                                if (friendlySource.ProductionCount == 3)
                                {
                                    friendlyVal += -200;
                                }
                                //less points for being close to the enemy
                                foreach (FactoryEntity enemyFactory in enemyFactories)
                                {
                                    friendlyVal += _internalState.Links.GetShortestPathDistance(friendlySource.Id, enemyFactory.Id) * -1;
                                }

                                if (friendlyVal > bestFriendlyValue)
                                {
                                    friendlySourceTarget = friendlySource;
                                    bestFriendlyValue = friendlyVal;
                                }
                            }
                            if (friendlySourceTarget != null)
                            {
                                Console.Error.WriteLine("Evacuation Target: " + friendlySourceTarget.Id);
                                int shortestId = _internalState.Links.GetShortestPath(sourceFactory.Id, friendlySourceTarget.Id);
                                moves.AddMove(new Move(sourceFactory.Id, shortestId, cyborgsAvailableToSend));
                                cyborgsAvailableToSend -= cyborgsAvailableToSend;
                                globalCyborgsAvailableToSend -= cyborgsAvailableToSend;
                            }
                            else
                            {
                                Console.Error.WriteLine("Could not find any targets!");
                            }
                        }
                        else
                        {
                            //Check if any bombs in play and evacuate
                            BombEvacuation(moves, sourceFactory);
                        }
                    }
                }
            }

            if (multiFactoryTakeoverMoves.Any())
            {
                PlayMultiFactoryTakeoverMoves(moves, multiFactoryTakeoverMoves);
            }

            if (_internalState.MyBombCount > 0)
            {
                UseBomb(moves);
            }

            if (!moves.Moves.Any())
            {
                Console.Error.WriteLine("No moves to make!");
                moves.AddMove(new Move());
            }

            return moves;
        }

        /// <summary>
        /// Calculates the total number of cyborgs that can be sent
        /// </summary>
        /// <returns>The total number of cyborgs available</returns>
        public int CalculateTotalCyborgsAvailableToSend()
        {
            int cyborgsAvailableToSend = 0;
            foreach (FactoryEntity sourceFactory in _internalState.MyFactories)
            {
                cyborgsAvailableToSend += sourceFactory.NumberOfCyborgs;
            }
            return cyborgsAvailableToSend;
        }

        /// <summary>
        /// Calculate the number of cyborgs that can be used to send elsewhere taking defense into consideration
        /// </summary>
        /// <param name="sourceFactory">The source factory to check</param>
        /// <returns>The number of cyborgs available</returns>
        public int CalculateCyborgsAvailableToSend(FactoryEntity sourceFactory)
        {
            int cyborgsToDefend = GetCyborgDefense(sourceFactory);
            if (cyborgsToDefend > sourceFactory.NumberOfCyborgs)
                return 0;
            else 
                return sourceFactory.NumberOfCyborgs - cyborgsToDefend;
        }

        //Checks for bombs in play and evacuates the source factory
        public void BombEvacuation(MoveList moves, FactoryEntity sourceFactory)
        {
            if (_internalState.EnemyBombs.Any())
            {
                List<FactoryEntity> friendlyFactories = _internalState.MyFactories;
                int minDist = 9999;
                int minDistNon3 = 9999;
                FactoryEntity bestNon3ProductionFactory = null;
                FactoryEntity bestFactory = null;
                foreach (Node n in _internalState.Links.GetLinks(sourceFactory.Id))
                {
                    FactoryEntity factory = friendlyFactories.FirstOrDefault(f => f.Id == n.FactoryId);
                    if (factory != null)
                    {
                        if (n.Distance < minDist)
                        {
                            minDist = n.Distance;
                            bestFactory = factory;
                        }
                        if (n.Distance < minDistNon3 && factory.ProductionCount < 3)
                        {
                            bestNon3ProductionFactory = factory;
                            minDistNon3 = n.Distance;
                        }
                    }
                }
                if (bestNon3ProductionFactory != null)
                {
                    Console.Error.WriteLine("Evacuating " + sourceFactory.Id + " to " + bestNon3ProductionFactory.Id);
                    Move move = new Move(sourceFactory.Id, bestNon3ProductionFactory.Id, CalculateCyborgsAvailableToSend(sourceFactory));
                    moves.AddMove(move);
                    _internalState.PlayMove(move, Owner.Me);
                }
                else if (bestFactory != null)
                {
                    Console.Error.WriteLine("Evacuating " + sourceFactory.Id + " to " + bestFactory.Id);
                    moves.AddMove(new Move(sourceFactory.Id, bestFactory.Id, CalculateCyborgsAvailableToSend(sourceFactory)));
                }
                else
                {
                    Console.Error.WriteLine("No factory found to evacuate to...");
                }
            }
        }

        public void PlayMultiFactoryTakeoverMoves(MoveList moves, List<MoveOption> moveOptions)
        {
            //Might need to handle 2 different multi-factory takeovers against more than 1 target; currently assumes that if all are on target then we fulfilled the amount to take over
            bool didPlayMove = false;
            foreach (MoveOption move in moveOptions)
            {
                //Play every move that isn't at the target;
                if (move.TargetFactory.Id != move.BestTargetId)
                {
                    Console.Error.WriteLine("Move not on target: " + move.TargetFactory.Id + " adapted: " + move.BestTargetId);
                    moves.AddMove(move.GenerateMove());
                    didPlayMove = true;
                }
            }

            //If all moves go to target then play all of them.
            if (!didPlayMove)
            {
                Console.Error.WriteLine("Playing all multifactory target moves.");
                foreach (MoveOption move in moveOptions)
                {
                    moves.AddMove(move.GenerateMove());
                }
            }
        }

        public bool IsFrontLineFactory(FactoryEntity factory)
        {
            List<Node> adjacentFactories = _internalState.Links.GetLinks(factory.Id);
            foreach (Node n in adjacentFactories)
            {
                if (n.Distance < 5 && _internalState.EnemyFactories.Where(e => e.Id == n.FactoryId).Any())
                {
                    Console.Error.WriteLine("Factory: " + factory.Id + " is frontline.  Distance: " + n.Distance + " to " + n.FactoryId);
                    return true;
                }
            }
            Console.Error.WriteLine("Factory: " + factory.Id + " is not frontline.");
            return false;
        }

        /// <summary>
        /// Gets the total number of cyborgs required to keep control of a factory
        /// </summary>
        /// <param name="sourceFactory">The factory we currently have under our control</param>
        /// <returns>The number of cyborgs required to stay in control</returns>
        public int GetCyborgDefense(FactoryEntity sourceFactory)
        {
            int cyborgsToDefend = 0;
            List<TroopEntity> enemyTroops = _internalState.EnemyTroops;
            if (sourceFactory.ProductionCount == 0)
            {
                return 0;//do not defend 0 production sites..
            }

            
            int minArrival = 9999;
            foreach (TroopEntity enemyTroop in enemyTroops)
            {
                if (enemyTroop.TargetFactory == sourceFactory.Id)
                {
                    cyborgsToDefend += enemyTroop.NumberOfCyborgs;
                    if (minArrival > enemyTroop.TurnsToArrive)
                    {
                        minArrival = enemyTroop.TurnsToArrive;
                    }
                }
            }
            List<FactoryEntity> enemyFactories = _internalState.EnemyFactories;
            foreach (FactoryEntity enemyFactory in enemyFactories)
            {
                int dist = _internalState.Links.GetShortestPathDistance(sourceFactory.Id, enemyFactory.Id);
                if (dist <= 2 && dist <= minArrival)
                {
                    cyborgsToDefend += enemyFactory.NumberOfCyborgs;
                    minArrival = dist;
                }
            }
            if (cyborgsToDefend != 0)
            {
                Console.Error.WriteLine("Source: " + sourceFactory.Id + " Incoming: " + cyborgsToDefend + " arrival: " + minArrival);
            }

            cyborgsToDefend -= minArrival * sourceFactory.ProductionCount;
            cyborgsToDefend = cyborgsToDefend < 0 ? 0 : cyborgsToDefend;

            if(cyborgsToDefend > 0)
            {
                Console.Error.WriteLine("***Source: " + sourceFactory.Id + " Defense required: " + cyborgsToDefend);
            }
            
            return cyborgsToDefend;
        }

        //Decides whether or not to use bomb after all movements have been declared
        public void UseBomb(MoveList moves)
        {
            List<BombEntity> bombs = _internalState.MyBombs;
            List<TroopEntity> friendlyTroops = _internalState.MyTroops;

            List<FactoryEntity> enemyFactories = _internalState.EnemyFactories;

            FactoryEntity bestTargetFactory = null;
            int bestVal = -9999;
            foreach (FactoryEntity targetFactory in enemyFactories)
            {
                bool targetHasTroops = false;
                int val = 0;
                int troopCount = 0;
                foreach (TroopEntity troop in friendlyTroops)
                {
                    if (troop.TargetFactory == targetFactory.Id)
                    {
                        troopCount += troop.NumberOfCyborgs;
                    }
                }
                if (troopCount > 0)
                {
                    targetHasTroops = true;
                }

                bool targetHasBomb = bombs.Any() && bombs.First().TargetFactoryId == targetFactory.Id;
                bool targetLevel2 = targetFactory.ProductionCount == 2 && targetFactory.NumberOfCyborgs > 5 && targetFactory.IsProducing();
                bool targetLevel3 = targetFactory.ProductionCount == 3 && targetFactory.IsProducing();
                if ((targetLevel3 || targetLevel2) && !targetHasBomb && !targetHasTroops)
                {
                    val += targetFactory.NumberOfCyborgs;
                    val += targetFactory.ProductionCount * 5;
                    if (val > bestVal)
                    {
                        bestTargetFactory = targetFactory;
                    }
                }
            }

            if (bestTargetFactory != null)
            {
                Console.Error.WriteLine("Bombing: " + bestTargetFactory.Id + " with production: " + bestTargetFactory.ProductionCount);
                List<FactoryEntity> friendlyFactories = _internalState.MyFactories;
                FactoryEntity bestSource = null;
                int minDist = 99999;
                foreach (FactoryEntity sourceFactory in friendlyFactories)
                {
                    int currentDistance = _internalState.Links.GetDistance(sourceFactory.Id, bestTargetFactory.Id);
                    if (currentDistance < minDist && currentDistance < 10)
                    {
                        minDist = currentDistance;
                        bestSource = sourceFactory;
                    }
                }

                if (bestSource != null)
                {
                    Move move = new Move(bestSource.Id, bestTargetFactory.Id);
                    moves.AddMove(move);
                    _internalState.PlayMove(move, Owner.Me);
                }
            }

        }

        //Calculates the number of cyborgs required to takeover a factory
        public int CalculateCyborgsRequiredToTakeover(FactoryEntity sourceFactory, FactoryEntity targetFactory)
        {
            int cyborgs = 1;//minimum to takeover is 1
            List<TroopEntity> troops = _internalState.Troops;

            Dictionary<int, int> timeToEnemyTroops = new Dictionary<int, int>();
            Dictionary<int, int> timeToFriendlyTroops = new Dictionary<int, int>();

            //Console.Error.WriteLine(" Target: " + targetFactory.Id);

            //Check the incoming troop count
            foreach (TroopEntity troop in troops)
            {
                if (troop.TargetFactory == targetFactory.Id)
                {
                    if (troop.IsFriendly())
                    {
                        if (timeToFriendlyTroops.ContainsKey(troop.TurnsToArrive))
                        {
                            timeToFriendlyTroops[troop.TurnsToArrive] += troop.NumberOfCyborgs;
                        }
                        else
                        {
                            timeToFriendlyTroops[troop.TurnsToArrive] = troop.NumberOfCyborgs;
                        }
                    }
                    else if (troop.IsEnemy())
                    {
                        Console.Error.WriteLine("Enemy troop count: " + troop.NumberOfCyborgs + " arrives: " + troop.TurnsToArrive);
                        if (timeToEnemyTroops.ContainsKey(troop.TurnsToArrive))
                        {
                            timeToEnemyTroops[troop.TurnsToArrive] += troop.NumberOfCyborgs;
                        }
                        else
                        {
                            timeToEnemyTroops[troop.TurnsToArrive] = troop.NumberOfCyborgs;
                        }
                    }
                }
            }
            //Add check for cyborgs sitting in factories that could be sent and assume they will be sent...
            List<FactoryEntity> enemyFactories = _internalState.EnemyFactories.Where(e => e.Id != targetFactory.Id).ToList();
            foreach (FactoryEntity enemyFactory in enemyFactories)
            {
                int time = _internalState.Links.GetShortestPathDistance(targetFactory.Id, enemyFactory.Id);

                if (targetFactory.IsEnemy())
                {
                    if (timeToEnemyTroops.ContainsKey(time))
                    {
                        timeToEnemyTroops[time] += enemyFactory.NumberOfCyborgs;
                    }
                    else
                    {
                        timeToEnemyTroops[time] = enemyFactory.NumberOfCyborgs;
                    }
                }
            }

            int distance = _internalState.Links.GetShortestPathDistance(sourceFactory.Id, targetFactory.Id);
            Owner previousOwnership = targetFactory.Owner;
            List<BombEntity> friendlyBombs = _internalState.MyBombs;
            BombEntity friendlyBomb = null;
            if (friendlyBombs.Any())
            {
                friendlyBomb = friendlyBombs.First() as BombEntity;
                if (friendlyBomb.TurnsToArrive >= distance && friendlyBomb.TargetFactoryId == targetFactory.Id)
                {
                    return 999999;//Don't send troops where a bomb is going to explode after we send troops
                }
                //Console.Error.WriteLine("Bomb: " + friendlyBomb.Id + " Target: " + friendlyBomb.TargetFactory + " Time: " + friendlyBomb.TurnsToArrive);
            }

            //Calculate the number of cyborgs in the factory up to the distance of the source factory
            int cyborgsInFactory = targetFactory.NumberOfCyborgs;
            int bombCount = 0;
            for (int i = 1; i <= distance; i++)
            {
                timeToFriendlyTroops.TryGetValue(i, out int friendlyCount);
                timeToEnemyTroops.TryGetValue(i, out int enemyCount);
                bool isBomb = false;


                if (targetFactory.Id == 2)
                {
                    //Console.Error.WriteLine("Own: " + previousOwnership + " Borg: " + cyborgsInFactory + " Dist: " + distance);
                    //Console.Error.WriteLine("Friend: " + friendlyCount + " Enemy: " + enemyCount + " Time: " + i);
                }


                if (friendlyBomb != null && friendlyBomb.TargetFactoryId == targetFactory.Id && friendlyBomb.TurnsToArrive == i)
                {
                    bombCount = 5;//bombs disrupt production for 5 turns starting with the turn it goes off
                    isBomb = true;
                }

                switch (previousOwnership)
                {
                    case Owner.Me:
                        if ((targetFactory.IsProducing() || targetFactory.TurnsTillProduction - i < 1) && bombCount <= 0)
                        {
                            cyborgsInFactory += targetFactory.ProductionCount;
                        }
                        cyborgsInFactory += friendlyCount - enemyCount;
                        if (isBomb)
                        {
                            int cyborgsLost = (int)Math.Floor(cyborgsInFactory / 2.0);
                            cyborgsLost = cyborgsLost < 10 ? Math.Min(10, cyborgsInFactory) : cyborgsLost;
                            cyborgsInFactory -= cyborgsLost;
                        }
                        if (cyborgsInFactory < 0)
                        {
                            previousOwnership = Owner.Opponent;
                            cyborgsInFactory *= -1;
                            //Console.Error.WriteLine("We own the factory by " + i);
                        }
                        break;
                    case Owner.Neutral:
                        cyborgsInFactory -= Math.Abs(friendlyCount - enemyCount);
                        if (cyborgsInFactory < 0)
                        {
                            previousOwnership = friendlyCount > enemyCount ? Owner.Me : Owner.Opponent;
                            cyborgsInFactory *= -1;
                        }
                        break;
                    case Owner.Opponent:
                        if ((targetFactory.IsProducing() || targetFactory.TurnsTillProduction - i < 1) && bombCount <= 0)
                        {
                            cyborgsInFactory += targetFactory.ProductionCount;
                        }
                        cyborgsInFactory += enemyCount - friendlyCount;
                        if (isBomb)
                        {
                            int cyborgsLost = (int)Math.Floor(cyborgsInFactory / 2.0);
                            cyborgsLost = cyborgsLost < 10 ? Math.Min(10, cyborgsInFactory) : cyborgsLost;
                            cyborgsInFactory -= cyborgsLost;
                            //Console.Error.WriteLine("Target: " + targetFactory.Id + " Current: " + cyborgsInFactory + " Bomb: " + cyborgsLost);
                        }
                        if (cyborgsInFactory < 0)
                        {
                            previousOwnership = Owner.Me;
                            cyborgsInFactory *= -1;
                            //Console.Error.WriteLine("Enemy owns the factory by " + i);
                        }
                        break;
                }
                bombCount--;
            }

            if (previousOwnership == Owner.Me)
            {
                return 0;//I already own it; no more borgs required to takeover
            }

            cyborgs += cyborgsInFactory;

            return cyborgs;
        }
    }
}
