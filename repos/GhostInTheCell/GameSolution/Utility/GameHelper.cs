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
            List<FactoryEntity> allFactories = _internalState.Factories;
            List<FactoryEntity> friendlyFactories = _internalState.MyFactories;

            int globalCyborgsAvailableToSend =  _internalState.MyTotalCyborgsAvailableToSend;
            Console.Error.WriteLine($"Global cyborgs available:{globalCyborgsAvailableToSend}");

            Dictionary<int, int> factoryIdToCyborgsToTakeover = new Dictionary<int, int>();
            List<MoveOption> multiFactoryTakeoverMoves = new List<MoveOption>();

            Dictionary<int, int> sourceFactoryToCyborgsAvailableToSend = new Dictionary<int, int>();
            foreach (FactoryEntity sourceFactory in friendlyFactories)
            {
                //Console.Error.WriteLine(sourceFactory.ToString());
                int cyborgsAvailableToSend = CalculateCyborgsAvailableToSend(sourceFactory);
                globalCyborgsAvailableToSend -= (sourceFactory.NumberOfCyborgs - cyborgsAvailableToSend);

                //Condition for holding onto troops in factory to get to an upgrade
                if (!IsFrontLineFactory(sourceFactory) && sourceFactory.ProductionCount < 3 && (_internalState.MyIncome < _internalState.EnemyIncome || _internalState.MyTroopsCount > (_internalState.EnemyTroopsCount - 5)))
                {
                    if (_internalState.EnemyBombs.Count == 0 || sourceFactory.ProductionCount <= 1)
                    {
                        List<FactoryEntity> neutralFactories = _internalState.NeutralFactories.Where(e => e.ProductionCount != 0 && e.NumberOfCyborgs != 0).ToList();
                        int totalNeutralCyborgs = 0;
                        //Find the number of neutral cyborgs where production of that factory is greater than 0 where we are closer than the enemy
                        //(it would be better to take these over as it would cost less than 10 troops)
                        foreach (FactoryEntity neutralFactory in neutralFactories)
                        {
                            if (neutralFactory.ProductionCount == 0)
                                continue;
                            int minFriendly = 9999;
                            int minEnemy = 9999;
                            foreach (FactoryEntity myFactory in _internalState.MyFactories)
                            {
                                int dist = _internalState.Links.GetShortestPathDistance(neutralFactory.Id, myFactory.Id);
                                if (dist < minFriendly)
                                {
                                    minFriendly = dist;
                                }
                            }
                            foreach (FactoryEntity enemyFactory in _internalState.EnemyFactories)
                            {
                                int dist = _internalState.Links.GetShortestPathDistance(neutralFactory.Id, enemyFactory.Id);
                                if (dist < minEnemy)
                                {
                                    minEnemy = dist;
                                }
                            }
                            if (minFriendly < minEnemy)
                            {
                                totalNeutralCyborgs += CalculateCyborgsRequiredToTakeover(sourceFactory, neutralFactory);
                            }
                        }
                        Console.Error.WriteLine($"Neutrals left:{totalNeutralCyborgs}");

                        if (totalNeutralCyborgs < (globalCyborgsAvailableToSend - 10))
                        {
                            if (sourceFactory.NumberOfCyborgs >= 10)
                            {
                                //At the final upgrade stage so send the rest onward to wherever or we can't wait another turn for the next upgrade
                                if (sourceFactory.ProductionCount == 2 || totalNeutralCyborgs >= (globalCyborgsAvailableToSend - 20))
                                {
                                    globalCyborgsAvailableToSend -= 10;
                                    cyborgsAvailableToSend -= 10;
                                }
                                else if(sourceFactory.ProductionCount < 2)
                                {
                                    Console.Error.WriteLine($"Holding for Upgrade:{sourceFactory.Id} troops:{cyborgsAvailableToSend}");
                                    globalCyborgsAvailableToSend -= cyborgsAvailableToSend;
                                    cyborgsAvailableToSend = 0;
                                }

                                Move move = new Move(sourceFactory.Id);
                                moves.AddMove(move);
                                _internalState.PlayMove(move, Owner.Me);
                            }
                            else
                            {
                                //Don't send troops out of this factory we want to use it to upgrade
                                Console.Error.WriteLine($"Holding for Upgrade:{sourceFactory.Id} troops:{cyborgsAvailableToSend}");
                                globalCyborgsAvailableToSend -= cyborgsAvailableToSend;
                                cyborgsAvailableToSend = 0;
                            }
                        }
                    }
                }

                sourceFactoryToCyborgsAvailableToSend[sourceFactory.Id] = cyborgsAvailableToSend;
            }

            foreach (FactoryEntity sourceFactory in friendlyFactories)
            {
                int cyborgsAvailableToSend = sourceFactoryToCyborgsAvailableToSend[sourceFactory.Id];

                Console.Error.WriteLine($"***Src: {sourceFactory.Id} Dfns: {sourceFactory.CyborgsRequiredToDefend} Available: {cyborgsAvailableToSend} Glbl:{globalCyborgsAvailableToSend}");

                bool hasTarget = true;//Assume a target will be found
                //As long as there are cyborgs to send let's see if there are any targets
                while (cyborgsAvailableToSend > 0 && hasTarget)
                {
                    FactoryEntity bestTarget = null;
                    int bestValue = -99999;
                    int cyborgsToSend = 0;
                    int cyborgsLeftToTakeover = 0;
                    bool isCompleteTakeover = false;
                    bool isLeftover = false;
                    foreach (FactoryEntity targetFactory in allFactories)
                    {
                        int val = 0;
                        if (targetFactory.Id == sourceFactory.Id)
                        {
                            continue;//Can't send to self
                        }

                        int cyborgsToTakeover = -1;
                        if (factoryIdToCyborgsToTakeover.ContainsKey(targetFactory.Id))
                        {
                            cyborgsToTakeover = factoryIdToCyborgsToTakeover[targetFactory.Id];
                        }
                        
                        int cyborgsToTakeoverComplete = 0;
                        if (cyborgsToTakeover > 0)
                        {
                            val += 25;//we already said this was a good target so commit to it.
                            cyborgsToTakeoverComplete = CalculateCyborgsRequiredToTakeover(sourceFactory, targetFactory);
                            isLeftover = true;
                        }
                        else if(cyborgsToTakeover == -1)
                        {
                            cyborgsToTakeover = CalculateCyborgsRequiredToTakeover(sourceFactory, targetFactory);
                            cyborgsToTakeoverComplete = cyborgsToTakeover;
                            isLeftover = false;
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
                            var enemyFactories = allFactories.Where(f => f.IsEnemy()).ToList();
                            int minDist = 9999;
                            foreach (FactoryEntity enemy in enemyFactories)
                            {
                                int dist = _internalState.Links.GetDistance(targetFactory.Id, enemy.Id);
                                if (dist < minDist)
                                {
                                    minDist = dist;
                                }
                            }
                            //Console.Error.WriteLine($"Min Dist to Enemy:{minDist}");
                            val += minDist * 5;//neutral factories that are farther from the enemy are worth more
                        }

                        val += cyborgsToTakeover * -1;//factories that take a lot of borgs to take over aren't as good of a choice
                        val += targetFactory.IsProducing() ? targetFactory.ProductionCount * 10 : 0;//lots of bonus for high yield factories
                        val += targetFactory.Owner == Owner.Opponent ? targetFactory.ProductionCount * 5 : 0;
                        val += targetFactory.Owner == Owner.Me && targetFactory.IsProducing() ? targetFactory.ProductionCount * 20 : 0;//Defend places that are producing
                        val += distance * -20;

                        if (!isLeftover)
                        {
                            Console.Error.WriteLine($"Trgt:{targetFactory.Id} Tkvr:{cyborgsToTakeover} Vl:{val} Dst:{distance}");
                        }
                        else
                        {
                            Console.Error.WriteLine($"Trgt:{targetFactory.Id} Tkvr lft:{cyborgsToTakeover} Vl:{val} Dst:{distance}");
                        }

                        if (val > bestValue)
                        {
                            //Only target items where we have enough to take over or the best target is not the target and there are no potential incoming bombs
                            int bestTargetId = _internalState.Links.GetShortestPath(sourceFactory.Id, targetFactory.Id);
                            int bestDistance = _internalState.Links.GetShortestPathDistance(sourceFactory.Id, bestTargetId);
                            var bestTargetFactory = _internalState.Factories.First(f => f.Id == bestTargetId);
                            if(bestTargetFactory.BombArrival.TurnsUntilArrival.Contains(bestDistance))
                            {
                                Console.Error.WriteLine($"Bomb might explode arriving at {bestTargetId} from {sourceFactory.Id}");
                                continue;//Skip locations where a bomb is going to explode
                            }
                            if ((cyborgsToTakeover <= globalCyborgsAvailableToSend || bestTargetId != targetFactory.Id))
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
                                Console.Error.WriteLine("Value:" + val + " Current Best Target:" + targetFactory.Id + " Send:" + cyborgsToSend);
                            }
                            else 
                            {
                                bestValue = val;
                                bestTarget = null;
                                Console.Error.WriteLine("Best target is no target!");
                            }
                        }
                    }
                    if (bestTarget != null)
                    {
                        int bestTargetId = _internalState.Links.GetShortestPath(sourceFactory.Id, bestTarget.Id);
                        //If the best target is being sent to a non-friendly factory that has troops then take the long way
                        if (allFactories.Where(e => e.Id == bestTargetId && !e.IsFriendly() && CalculateCyborgsRequiredToTakeover(sourceFactory, e) > 1).Any())
                        {
                            Console.Error.WriteLine($"Found a non-friendly factory: {bestTargetId} with troops");
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
        /// Calculate the number of cyborgs that can be used to send elsewhere taking defense into consideration
        /// </summary>
        /// <param name="sourceFactory">The source factory to check</param>
        /// <returns>The number of cyborgs available</returns>
        public int CalculateCyborgsAvailableToSend(FactoryEntity sourceFactory)
        {
            int cyborgsToDefend = GetCyborgDefense(sourceFactory);
            int available = 0;
            if (cyborgsToDefend < sourceFactory.NumberOfCyborgs)
                available = sourceFactory.NumberOfCyborgs - cyborgsToDefend;

            sourceFactory.CyborgsRequiredToDefend = cyborgsToDefend;

            return available;
        }

        //Checks for bombs in play and evacuates the source factory
        public void BombEvacuation(MoveList moves, FactoryEntity sourceFactory)
        {
            //Check for enemy bombs that are going to arrive this turn
            if (_internalState.EnemyBombs.Any() && sourceFactory.BombArrival.TurnsUntilArrival.Contains(1))
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
            Dictionary<int, bool> didPlayMoveForTarget = new Dictionary<int, bool>();
            foreach (MoveOption move in moveOptions)
            {
                //Play every move that isn't at the target;
                if (move.TargetFactory.Id != move.BestTargetId)
                {
                    Console.Error.WriteLine("Move not on target: " + move.TargetFactory.Id + " adapted: " + move.BestTargetId);
                    moves.AddMove(move.GenerateMove());
                    didPlayMoveForTarget[move.TargetFactory.Id] = true;
                }
            }

            foreach (MoveOption move in moveOptions)
            {
                didPlayMoveForTarget.TryGetValue(move.TargetFactory.Id, out bool didPlayMove);
                if (!didPlayMove)//If all moves go to target then play all of them.
                {
                    Console.Error.WriteLine("Playing all multifactory target moves.");
                    moves.AddMove(move.GenerateMove());
                }
            }            
        }

        public bool IsFrontLineFactory(FactoryEntity factory)
        {
            FactoryEntity closestEnemy = GetClosestFactory(_internalState.EnemyFactories, factory);
            FactoryEntity closestFriendly = GetClosestFactory(_internalState.MyFactories, closestEnemy);
            
            if(closestFriendly.Id == factory.Id)
            {
                Console.Error.WriteLine("Factory: " + factory.Id + " is frontline.  ");
                return true;
            }
            //Console.Error.WriteLine("Factory: " + factory.Id + " is not frontline.");
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

            if (sourceFactory.BombArrival.HasIncomingBombNextTurn)
            {
                return 0;//No defense for potentially bombed sites
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

            int productionTime = Math.Max(minArrival - sourceFactory.TurnsTillProduction, 0);
            cyborgsToDefend -= productionTime * sourceFactory.ProductionCount;
            cyborgsToDefend = cyborgsToDefend < 0 ? 0 : cyborgsToDefend;
            
            return cyborgsToDefend;
        }

        //Decides whether or not to use bomb after all movements have been declared
        public void UseBomb(MoveList moves)
        {
            List<BombEntity> bombs = _internalState.MyBombs;
            List<TroopEntity> friendlyTroops = _internalState.MyTroops;

            List<FactoryEntity> allFactories = _internalState.Factories;

            FactoryEntity bestTargetFactory = null;
            int bestVal = -9999;
            foreach (FactoryEntity targetFactory in allFactories)
            {
                int val = 0;
                /*
                bool targetHasTroops = false;
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
                */
                bool isEnemy = targetFactory.IsEnemy();
                FactoryEntity bestSource = GetClosestFactory(_internalState.MyFactories, targetFactory);
                int numberOfCyborgs = targetFactory.NumberOfCyborgs;
                int level2CyborgCount = 5;
                if (bestSource != null)
                {
                    int dist = _internalState.Links.GetDistance(bestSource.Id, targetFactory.Id);
                    level2CyborgCount += dist * 2;
                    var ownerAndCount = targetFactory.TroopArrival.TimeTofactoryOwnershipAndCyborgCount[dist];
                    Owner owner = ownerAndCount.Item1;
                    numberOfCyborgs = ownerAndCount.Item2;
                    isEnemy = owner == Owner.Opponent;
                }
                else
                {
                    Console.Error.WriteLine("Could not find a friendly source factory!");
                }

                bool targetHasBomb = bombs.Any() && bombs.First().TargetFactoryId == targetFactory.Id;
                bool targetLevel2 = targetFactory.ProductionCount == 2 && numberOfCyborgs > level2CyborgCount && targetFactory.IsProducing();
                bool targetLevel3 = targetFactory.ProductionCount == 3 && targetFactory.IsProducing();
                if ((targetLevel3 || targetLevel2) && !targetHasBomb && isEnemy)
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
                FactoryEntity bestSource = GetClosestFactory(friendlyFactories, bestTargetFactory);

                if (bestSource != null)
                {
                    Move move = new Move(bestSource.Id, bestTargetFactory.Id);
                    moves.AddMove(move);
                    _internalState.PlayMove(move, Owner.Me);
                }
            }
        }

        public FactoryEntity GetClosestFactory(List<FactoryEntity> sourceFactories, FactoryEntity targetFactory)
        {
            FactoryEntity bestSource = null;
            int minDist = 99999;
            foreach (FactoryEntity sourceFactory in sourceFactories)
            {
                if(sourceFactory.Id == targetFactory.Id)
                {
                    continue;
                }
                int currentDistance = _internalState.Links.GetDistance(sourceFactory.Id, targetFactory.Id);
                if (currentDistance < minDist)
                {
                    minDist = currentDistance;
                    bestSource = sourceFactory;
                }
            }
            return bestSource;
        }

        //Calculates the number of cyborgs required to takeover a factory
        public int CalculateCyborgsRequiredToTakeover(FactoryEntity sourceFactory, FactoryEntity targetFactory)
        {
            //Console.Error.WriteLine(" Target: " + targetFactory.Id);

            int cyborgs = 1;//minimum to takeover is 1
            List<TroopEntity> troops = _internalState.Troops;

            //Check the incoming troop count.  This should be expanded to look at troops that are incoming along shortest paths.
            Dictionary<int, int> timeToEnemyTroops = targetFactory.TroopArrival.EnemyTroopArrival;
            Dictionary<int, int> timeToFriendlyTroops = targetFactory.TroopArrival.MyTroopArrival;
           
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
            Owner currentOwner = targetFactory.Owner;
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

                if (targetFactory.Id == 5)
                {
                    //Console.Error.WriteLine("Own: " + currentOwner + " Borg: " + cyborgsInFactory + " Dist: " + distance);
                    //Console.Error.WriteLine("Friend: " + friendlyCount + " Enemy: " + enemyCount + " Time: " + i);
                }

                currentOwner = DetermineFactoryOwnership(ref cyborgsInFactory, ref bombCount, currentOwner, targetFactory, friendlyBomb, i, friendlyCount, enemyCount);
            }

            if (currentOwner == Owner.Me)
            {
                return 0;//I already own it; no more borgs required to takeover
            }

            cyborgs += cyborgsInFactory;

            return cyborgs;
        }

        public static Owner DetermineFactoryOwnership(ref int cyborgsInFactory, ref int bombCount, Owner currentOwner, FactoryEntity targetFactory, BombEntity friendlyBomb, int time, int friendlyCount, int enemyCount)
        {
            bool isBomb = false;
            if (friendlyBomb != null && friendlyBomb.TargetFactoryId == targetFactory.Id && friendlyBomb.TurnsToArrive == time)
            {
                bombCount = 5;//bombs disrupt production for 5 turns starting with the turn it goes off
                isBomb = true;
            }
            switch (currentOwner)
            {
                case Owner.Me:
                    if ((targetFactory.IsProducing() || targetFactory.TurnsTillProduction - time < 1) && bombCount <= 0)
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
                        currentOwner = Owner.Opponent;
                        cyborgsInFactory *= -1;
                        //Console.Error.WriteLine("We own the factory by " + time);
                    }
                    break;
                case Owner.Neutral:
                    cyborgsInFactory -= Math.Abs(friendlyCount - enemyCount);
                    if (cyborgsInFactory < 0)
                    {
                        currentOwner = friendlyCount > enemyCount ? Owner.Me : Owner.Opponent;
                        cyborgsInFactory *= -1;
                    }
                    break;
                case Owner.Opponent:
                    if ((targetFactory.IsProducing() || targetFactory.TurnsTillProduction - time < 1) && bombCount <= 0)
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
                        currentOwner = Owner.Me;
                        cyborgsInFactory *= -1;
                        //Console.Error.WriteLine("Enemy owns the factory by " + i);
                    }
                    break;
            }
            bombCount--;
            return currentOwner;
        }
    }
}
