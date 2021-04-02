using System;
using System.Linq;
using System.Collections.Generic;

/**
 * Auto-generated code below aims at helping you parse
 * the standard input according to the problem statement.
 **/
public class Player
{
    static void Main(string[] args)
    {
        FactoryLinks links = new FactoryLinks();
        string[] inputs;
        int factoryCount = int.Parse(Console.ReadLine()); // the number of factories
        int linkCount = int.Parse(Console.ReadLine()); // the number of links between factories
        for (int i = 0; i < linkCount; i++)
        {
            inputs = Console.ReadLine().Split(' ');
            int factory1 = int.Parse(inputs[0]);
            //Console.Error.WriteLine(factory1);
            int factory2 = int.Parse(inputs[1]);
            //Console.Error.WriteLine(factory2);
            int distance = int.Parse(inputs[2]);
            links.AddLink(factory1, factory2, distance);
        }
        Console.Error.WriteLine(factoryCount);
        Console.Error.WriteLine(linkCount);
        links.CalculateShortestPaths();

        GameHelper gh = new GameHelper(links);
        // game loop
        while (true)
        {
            int entityCount = int.Parse(Console.ReadLine()); // the number of entities (e.g. factories and troops)
            List<Entity> entities = new List<Entity>();
            Console.Error.WriteLine(entities.Count);
            EntityFactory ef = new EntityFactory();
            for (int i = 0; i < entityCount; i++)
            {
                inputs = Console.ReadLine().Split(' ');
                int entityId = int.Parse(inputs[0]);
                string entityType = inputs[1];
                int arg1 = int.Parse(inputs[2]);
                int arg2 = int.Parse(inputs[3]);
                int arg3 = int.Parse(inputs[4]);
                int arg4 = int.Parse(inputs[5]);
                int arg5 = int.Parse(inputs[6]);
                entities.Add(ef.CreateEntity(entityType, entityId, arg1, arg2, arg3, arg4, arg5));
            }

            Console.Error.WriteLine(entities.Where(e => e.Id == -1).Count());

            // Write an action using Console.WriteLine()
            // To debug: Console.Error.WriteLine("Debug messages...");
            gh.SetEntities(entities);
            gh.ShowStats();

            MoveList moves = gh.PickMoves();
            moves.PlayMoves();
            //Console.WriteLine("WAIT");
        }
    }
}

public class FactoryLinks
{
    public class Node
    {
        public int FactoryId { get; set; }
        public int Distance { get; set; }
        public Node(int factory, int distance)
        {
            FactoryId = factory;
            Distance = distance;
        }

        public Node CreateAtDistance(int currentDist)
        {
            return new Node(FactoryId, currentDist + Distance);
        }
    }

    Dictionary<int, List<Node>> Links { get; set; }
    Dictionary<int, Dictionary<int, List<Node>>> Paths { get; set; }
    public FactoryLinks()
    {
        Links = new Dictionary<int, List<Node>>();
    }

    public void AddLink(int factory1, int factory2, int distance)
    {
        Console.Error.WriteLine(factory1 + " " + factory2 + " " + distance);
        AddLinkInternal(factory1, factory2, distance);
        AddLinkInternal(factory2, factory1, distance);
    }

    public void CalculateShortestPaths()
    {
        Paths = new Dictionary<int, Dictionary<int, List<Node>>>();

        List<int> vertices = Links.Keys.ToList();
        int vertexCount = vertices.Count;

        foreach (int vertex in vertices)
        {
            CalculateShortestPathFromStartNode(vertex, vertexCount);
        }
    }

    private void CalculateShortestPathFromStartNode(int startNode, int vertexCount)
    {
        List<Node> minimumSpanningTree = new List<Node>();
        //Console.Error.WriteLine("Starting with " + startNode);
        int currentDist = 0;
        Paths[startNode] = new Dictionary<int, List<Node>>();
        minimumSpanningTree.Add(new Node(startNode, currentDist));
        while (minimumSpanningTree.Count < vertexCount)
        {
            int minDist = 99999;
            Node bestNode = null;
            Node parentNode = null;
            foreach (Node currentNode in minimumSpanningTree)
            {
                currentDist = currentNode.Distance;
                //Console.Error.WriteLine("Inspecting: " + currentNode.FactoryId + " distance " + currentDist);
                foreach (Node adjacent in GetLinks(currentNode.FactoryId))
                {
                    if (minimumSpanningTree.Where(n => n.FactoryId == adjacent.FactoryId).Any())
                    {
                        continue;//skip factories already in minimum spanning tree
                    }
                    int distance = currentDist + adjacent.Distance;
                    if (distance < minDist)
                    {
                        //When the distances are equivalent pick the one with the longest path
                        minDist = distance;
                        bestNode = adjacent.CreateAtDistance(currentDist);
                        parentNode = currentNode;
                    }
                    else if (distance == minDist)
                    {
                        Paths[startNode].TryGetValue(currentNode.FactoryId, out List<Node> pathCurrent);
                        int lengthCurrent = pathCurrent == null ? 0 : pathCurrent.Count;
                        Paths[startNode].TryGetValue(parentNode.FactoryId, out List<Node> pathPrevious);
                        int lengthPrevious = pathPrevious == null ? 0 : pathPrevious.Count;
                        if (lengthCurrent > lengthPrevious)
                        {
                            minDist = distance;
                            bestNode = adjacent.CreateAtDistance(currentDist);
                            parentNode = currentNode;
                        }
                    }
                }
            }
            minimumSpanningTree.Add(bestNode);
            List<Node> currentPath = null;
            if (parentNode.FactoryId != startNode)
            {
                Paths[startNode].TryGetValue(parentNode.FactoryId, out currentPath);
            }
            if (currentPath == null)
            {
                currentPath = new List<Node>();
            }
            else
            {
                currentPath = new List<Node>(currentPath);
            }
            Paths[startNode].Add(bestNode.FactoryId, currentPath);
            currentPath.Add(bestNode);
            if (startNode == 0)
            {
                Console.Error.WriteLine("Parent node: " + parentNode.FactoryId + " distance: " + parentNode.Distance);
                Console.Error.WriteLine("Shortest Node: " + bestNode.FactoryId + " distance: " + bestNode.Distance);
            }
        }
    }

    public List<Node> GetLinks(int factory)
    {
        return Links[factory];
    }

    //Retrieves direct straight distance
    public int GetDistance(int startId, int endId)
    {
        return GetLinks(startId).Where(l => l.FactoryId == endId).First().Distance + 1;//All commands are issued from this turn which is always turn 1.
    }

    public int GetShortestPathDistance(int startId, int endId)
    {
        Paths.TryGetValue(startId, out Dictionary<int, List<Node>> endPoints);
        endPoints.TryGetValue(endId, out List<Node> paths);

        /*
        if(endId == 2){
            foreach(Node n in paths){
                Console.Error.WriteLine($"Path: {n.FactoryId}, dist: {n.Distance}");
            }
        }
        Console.Error.WriteLine($"From start {startId} to {endId} path length: {paths.Count}.");
        */

        return paths.Last().Distance + 1;
    }



    public int ShortestPath(int startId, int endId)
    {
        Paths.TryGetValue(startId, out Dictionary<int, List<Node>> endPoints);
        if (endPoints == null)
        {
            Console.Error.WriteLine("|||Start not found: " + startId);
            return endId;
        }
        endPoints.TryGetValue(endId, out List<Node> paths);
        if (paths == null)
        {
            Console.Error.WriteLine("|||End not found: " + endId + " start: " + startId);
            return endId;
        }

        int shortest = paths.First().FactoryId;
        Console.Error.WriteLine("|||Shortest: " + shortest + " from: " + startId + " to: " + endId);

        return shortest;
    }

    private void AddLinkInternal(int startFactory, int destinationFactory, int distance)
    {
        List<Node> factoryLinks = null;
        if (Links.ContainsKey(startFactory))
        {
            factoryLinks = Links[startFactory];
        }
        else
        {
            factoryLinks = new List<Node>();
            Links[startFactory] = factoryLinks;
        }
        factoryLinks.Add(new Node(destinationFactory, distance));

    }
}

public class GameHelper
{
    private List<Entity> _entities;
    private readonly FactoryLinks _links;
    private int _bombCount = 2;
    private int _gameCounter = 0;
    private int _myIncome = 0;
    private int _enemyIncome = 0;
    private int _myTroops = 0;
    private int _enemyTroops = 0;
    public GameHelper(FactoryLinks links)
    {
        _links = links;
    }

    public void SetEntities(List<Entity> entities)
    {
        _entities = entities;
        _gameCounter++;
    }

    public void ShowStats()
    {
        List<Entity> factories = _entities.Where(e => e is FactoryEntity).ToList();
        List<Entity> troops = _entities.Where(e => e is TroopEntity).ToList();
        _enemyIncome = 0;
        _myIncome = 0;
        _myTroops = 0;
        _enemyTroops = 0;
        foreach (FactoryEntity factory in factories)
        {
            if (factory.IsProducing())
            {
                if (factory.IsFriendly())
                {
                    _myIncome += factory.ProductionCount;
                }
                else if (factory.IsEnemy())
                {
                    _enemyIncome += factory.ProductionCount;
                }
            }
            if (factory.IsFriendly())
            {
                _myTroops += factory.CyborgCount;
            }
            else if (factory.IsEnemy())
            {
                _enemyTroops += factory.CyborgCount;
            }
        }
        foreach (TroopEntity troop in troops)
        {
            if (troop.IsFriendly())
            {
                _myTroops += troop.NumberOfCyborgs;
            }
            else if (troop.IsEnemy())
            {
                _enemyTroops += troop.NumberOfCyborgs;
            }
        }

        Console.Error.WriteLine("Diff: " + (_myTroops - _enemyTroops) + " My Troops: " + _myTroops + " Enemy Troops: " + _enemyTroops);
        Console.Error.WriteLine("Diff: " + (_myIncome - _enemyIncome) + " My Income: " + _myIncome + " Enemy Income: " + _enemyIncome);
    }

    public MoveList PickMoves()
    {
        MoveList moves = new MoveList();
        List<Entity> otherFactories = _entities.Where(e => e is FactoryEntity).ToList();
        //if(_myIncome > _enemyIncome){
        //    otherFactories = otherFactories.Where(e => !e.IsEnemy()).ToList();//Go on the Defense if we are winning income game
        //}
        List<Entity> friendlyFactories = _entities.Where(e => e is FactoryEntity && e.IsFriendly()).ToList();

        int globalCyborgsAvailableToSend = 0;
        foreach (FactoryEntity sourceFactory in friendlyFactories)
        {
            globalCyborgsAvailableToSend += sourceFactory.CyborgCount - GetCyborgDefense(sourceFactory);
        }


        Dictionary<int, int> factoryIdToCyborgsToTakeover = new Dictionary<int, int>();
        List<MoveOption> multiFactoryTakeoverMoves = new List<MoveOption>();
        foreach (FactoryEntity sourceFactory in friendlyFactories)
        {
            //Console.Error.WriteLine(sourceFactory.ToString());
            int cyborgsAvailableToSend = sourceFactory.CyborgCount;
            bool hasTarget = true;//Assume a target will be found
            int cyborgsToDefend = GetCyborgDefense(sourceFactory);

            Console.Error.WriteLine("***Source: " + sourceFactory.Id + " Defense required: " + cyborgsToDefend);

            cyborgsAvailableToSend -= cyborgsToDefend;
            globalCyborgsAvailableToSend -= cyborgsToDefend;

            if (!IsFrontLineFactory(sourceFactory) && sourceFactory.ProductionCount < 3 && (_myIncome < _enemyIncome || _myTroops > _enemyTroops))
            {
                globalCyborgsAvailableToSend -= cyborgsAvailableToSend;
                cyborgsAvailableToSend = 0;
                if (sourceFactory.CyborgCount > 10)
                {
                    moves.AddMove(new Move(sourceFactory.Id));
                    _myTroops -= 10;
                }
            }

            Console.Error.WriteLine("Available : " + cyborgsAvailableToSend);
            //As long as there are cyborgs to send let's see if there ary any targets
            while (cyborgsAvailableToSend > 0 && hasTarget)
            {
                FactoryEntity bestTarget = null;
                int bestValue = -99999;
                int cyborgsToSend = 0;
                int cyborgsLeftToTakeover = 0;
                foreach (FactoryEntity targetFactory in otherFactories)
                {
                    int val = 0;
                    if (targetFactory.Id == sourceFactory.Id)
                    {
                        continue;//Can't send to self
                    }

                    factoryIdToCyborgsToTakeover.TryGetValue(targetFactory.Id, out int cyborgsToTakeover);
                    if (cyborgsToTakeover > 0)
                    {
                        val += 25;//we already said this was a good target so commit to it.
                        Console.Error.WriteLine("Target: " + targetFactory.Id + " Takeover left: " + cyborgsToTakeover);
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

                    int distance = _links.GetShortestPathDistance(sourceFactory.Id, targetFactory.Id);
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
                            int dist = _links.GetDistance(targetFactory.Id, enemy.Id);
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
                        if (cyborgsToTakeover <= globalCyborgsAvailableToSend /*|| (targetFactory.IsNeutral() && !IsFrontLineFactory(targetFactory))*/)
                        {
                            cyborgsLeftToTakeover = 0;
                            if (cyborgsToTakeover > sourceFactory.CyborgCount)
                            {
                                cyborgsLeftToTakeover = cyborgsToTakeover - sourceFactory.CyborgCount;
                                Console.Error.WriteLine("Not enough cyborgs at this factory!");
                            }
                            bestValue = val;
                            bestTarget = targetFactory;
                            cyborgsToSend = Math.Min(Math.Min(cyborgsToTakeover, sourceFactory.CyborgCount), cyborgsAvailableToSend);
                            //Console.Error.WriteLine("Value: " + val + " Best Target: " + targetFactory.Id + " to send " + cyborgsToSend);
                        }
                    }
                }
                if (bestTarget != null)
                {
                    int bestTargetId = _links.ShortestPath(sourceFactory.Id, bestTarget.Id);
                    if (otherFactories.Where(e => e.Id == bestTargetId && e.IsNeutral() && ((FactoryEntity)e).CyborgCount != 0).Any())
                    {
                        bestTargetId = bestTarget.Id;
                    }
                    cyborgsAvailableToSend -= cyborgsToSend;
                    globalCyborgsAvailableToSend -= cyborgsToSend;

                    if (cyborgsLeftToTakeover > 0 || multiFactoryTakeoverMoves.Where(m => m.TargetFactory.Id == bestTarget.Id || m.SourceFactory.Id == sourceFactory.Id).Any())
                    {
                        factoryIdToCyborgsToTakeover[bestTarget.Id] = cyborgsLeftToTakeover;
                        Console.Error.WriteLine($"Leftover to send: {cyborgsLeftToTakeover} at {bestTarget.Id}");
                        MoveOption move = new MoveOption(sourceFactory, bestTarget, cyborgsToSend, bestTargetId, cyborgsToDefend);
                        multiFactoryTakeoverMoves.Add(move);
                    }
                    else
                    {
                        Console.Error.WriteLine("Best Target Acquired: " + bestTarget.Id + " to send " + cyborgsToSend);
                        factoryIdToCyborgsToTakeover[bestTarget.Id] = 0;
                        _entities.Add(new TroopEntity(-1, (int)Owner.Me, sourceFactory.Id, bestTarget.Id, cyborgsToSend, _links.GetDistance(sourceFactory.Id, bestTarget.Id)));
                        Move move = new Move(sourceFactory.Id, bestTargetId, cyborgsToSend);
                        moves.AddMove(move);
                    }
                }
                else
                {
                    hasTarget = false;//No targets so abort

                    if ((sourceFactory.ProductionCount < 3 && cyborgsAvailableToSend >= 10) && (_myTroops - _enemyTroops) > -10 && _gameCounter > 5)
                    {
                        moves.AddMove(new Move(sourceFactory.Id));
                        cyborgsAvailableToSend -= 10;
                        globalCyborgsAvailableToSend -= 10;
                        _myTroops -= 10;
                        continue;
                    }

                    //If there are no targets then spew out borgs to facilities that need to grow
                    bool isFrontLineFactory = IsFrontLineFactory(sourceFactory);
                    if (sourceFactory.ProductionCount == 3 && !isFrontLineFactory)
                    {
                        Console.Error.WriteLine("Evacuating 3 production facility");
                        FactoryEntity friendlySourceTarget = null;
                        int bestFriendlyValue = -9999;
                        List<Entity> enemyFactories = _entities.Where(e => e is FactoryEntity && e.IsEnemy()).ToList();
                        foreach (FactoryEntity friendlySource in friendlyFactories)
                        {
                            if (sourceFactory.Id == friendlySource.Id)
                                continue;

                            int friendlyVal = 0;
                            friendlyVal += _links.GetShortestPathDistance(sourceFactory.Id, friendlySource.Id) * -5;
                            if (friendlySource.CyborgCount > 20)
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
                                friendlyVal += _links.GetShortestPathDistance(friendlySource.Id, enemyFactory.Id) * -1;
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
                            int shortestId = _links.ShortestPath(sourceFactory.Id, friendlySourceTarget.Id);
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

        if (_bombCount > 0)
        {
            UseBomb(moves);
        }

        if (!moves.Moves.Any())
        {
            //Console.Error.WriteLine("No moves to make");
            moves.AddMove(new Move());
        }

        return moves;
    }

    //Checks for bombs in play and evacuates the source factory
    public void BombEvacuation(MoveList moves, FactoryEntity sourceFactory)
    {
        if (_entities.Where(e => e is BombEntity && e.IsEnemy()).ToList().Any())
        {
            List<Entity> friendlyFactories = _entities.Where(e => e is FactoryEntity && e.IsFriendly()).ToList();
            int minDist = 9999;
            int minDistNon3 = 9999;
            FactoryEntity bestNon3ProductionFactory = null;
            FactoryEntity bestFactory = null;
            foreach (FactoryLinks.Node n in _links.GetLinks(sourceFactory.Id))
            {
                FactoryEntity factory = (FactoryEntity)friendlyFactories.Where(f => f.Id == n.FactoryId).FirstOrDefault();
                if (factory != null && IsFrontLineFactory(factory))
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
                moves.AddMove(new Move(sourceFactory.Id, bestNon3ProductionFactory.Id, sourceFactory.CyborgCount));
            }
            else if (bestFactory != null)
            {
                Console.Error.WriteLine("Evacuating " + sourceFactory.Id + " to " + bestFactory.Id);
                moves.AddMove(new Move(sourceFactory.Id, bestFactory.Id, sourceFactory.CyborgCount));
            }
            else
            {
                Console.Error.WriteLine("No factory found to evacuate to...");
            }
        }
    }

    public void PlayMultiFactoryTakeoverMoves(MoveList moves, List<MoveOption> moveOptions)
    {
        //Might need to handle 2 different multi-factory takeovers against more than 1 target
        bool didPlayMove = false;
        //CalculateCyborgsRequiredToTakeover
        List<FactoryEntity> targets = new List<FactoryEntity>();
        //Find all the targets
        foreach (MoveOption move in moveOptions)
        {
            /*
            if(!targets.Where(t => t.Id == move.TargetFactory.Id).Any()){
                targets.Add(move.TargetFactory);
            }
            */
            //Play every move that isn't at the target;
            if (move.TargetFactory.Id != move.BestTargetId)
            {
                Console.Error.WriteLine("Move not on target: " + move.TargetFactory.Id + " adapted: " + move.BestTargetId);
                moves.AddMove(move.GenerateMove());
                didPlayMove = true;
            }
        }
        /*
        foreach(FactoryEntity targetFactory in targets){

            foreach(MoveOption move in moves.Where(m => m.TargetFactory.Id == targetFactory.Id)){

            }
        }
        */


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
        List<FactoryLinks.Node> adjacentFactories = _links.GetLinks(factory.Id);
        foreach (FactoryLinks.Node n in adjacentFactories)
        {
            if (n.Distance < 5 && _entities.Where(e => e.Id == n.FactoryId && e.IsEnemy()).Any())
            {
                Console.Error.WriteLine("Factory: " + factory.Id + " is frontline.  Distance: " + n.Distance + " to " + n.FactoryId);
                return true;
            }
        }
        Console.Error.WriteLine("Factory: " + factory.Id + " is not frontline.");
        return false;
    }

    public int GetCyborgDefense(FactoryEntity sourceFactory)
    {
        int cyborgsToDefend = 0;
        List<Entity> enemyTroops = _entities.Where(e => e is TroopEntity Entity && e.IsEnemy()).ToList();
        if (sourceFactory.ProductionCount == 0)
        {
            return 0;//do not defend 0 production sites..
        }

        //If there aren't any enemy bombs then use a more defensive strategy
        if (!_entities.Where(e => e is BombEntity && e.IsEnemy()).ToList().Any() || sourceFactory.ProductionCount > 1)
        {
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
            List<Entity> enemyFactories = _entities.Where(e => e is FactoryEntity && e.IsEnemy()).ToList();
            foreach (FactoryEntity enemyFactory in enemyFactories)
            {
                int dist = _links.GetShortestPathDistance(sourceFactory.Id, enemyFactory.Id);
                if (dist <= 2 && dist <= minArrival)
                {
                    cyborgsToDefend += enemyFactory.CyborgCount;
                    minArrival = dist;
                }
            }
            if (cyborgsToDefend != 0)
            {
                Console.Error.WriteLine("Source: " + sourceFactory.Id + " Incoming: " + cyborgsToDefend + " arrival: " + minArrival);
            }

            cyborgsToDefend -= minArrival * sourceFactory.ProductionCount;
            cyborgsToDefend = cyborgsToDefend < 0 ? 0 : cyborgsToDefend;
            cyborgsToDefend = Math.Min(cyborgsToDefend, sourceFactory.CyborgCount);
        }
        return cyborgsToDefend;
    }

    //Decides whether or not to use bomb after all movements have been declared
    public void UseBomb(MoveList moves)
    {
        List<Entity> bombs = _entities.Where(e => e is BombEntity && e.IsFriendly()).ToList();
        List<Entity> friendlyTroops = _entities.Where(e => e is TroopEntity && e.IsFriendly()).ToList();

        List<Entity> enemyFactories = _entities.Where(e => e is FactoryEntity && e.IsEnemy()).ToList();

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
            if (targetFactory.CyborgCount < troopCount)
            {
                targetHasTroops = true;
            }

            bool targetHasBomb = bombs.Any() && ((BombEntity)bombs.First()).TargetFactory == targetFactory.Id;
            bool targetLevel2 = targetFactory.ProductionCount == 2 && targetFactory.CyborgCount > 5 && targetFactory.IsProducing();
            bool targetLevel3 = targetFactory.ProductionCount == 3 && targetFactory.IsProducing();
            if ((targetLevel3 || targetLevel2) && !targetHasBomb && !targetHasTroops)
            {
                val += targetFactory.CyborgCount;
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
            List<Entity> friendlyFactories = _entities.Where(e => e is FactoryEntity && e.IsFriendly()).ToList();
            FactoryEntity bestSource = null;
            int minDist = 99999;
            foreach (FactoryEntity sourceFactory in friendlyFactories)
            {
                int currentDistance = _links.GetDistance(sourceFactory.Id, bestTargetFactory.Id);
                if (currentDistance < minDist && currentDistance < 10)
                {
                    minDist = currentDistance;
                    bestSource = sourceFactory;
                }
            }

            if (bestSource != null)
            {
                moves.AddMove(new Move(bestSource.Id, bestTargetFactory.Id));
                _entities.Add(new BombEntity(-1, (int)Owner.Me, bestSource.Id, bestTargetFactory.Id, _links.GetDistance(bestSource.Id, bestTargetFactory.Id), -1));
                _bombCount--;
            }
        }

    }

    //Calculates the number of cyborgs required to takeover a factory
    public int CalculateCyborgsRequiredToTakeover(FactoryEntity sourceFactory, FactoryEntity targetFactory)
    {
        int cyborgs = 1;//minimum to takeover is 1
        List<Entity> troops = _entities.Where(e => e is TroopEntity).ToList();

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
        List<Entity> enemyFactories = _entities.Where(e => e is FactoryEntity && e.IsEnemy() && e.Id != targetFactory.Id).ToList();
        foreach (FactoryEntity enemyFactory in enemyFactories)
        {
            int time = _links.GetShortestPathDistance(targetFactory.Id, enemyFactory.Id);

            if (targetFactory.IsEnemy())
            {
                if (timeToEnemyTroops.ContainsKey(time))
                {
                    timeToEnemyTroops[time] += enemyFactory.CyborgCount;
                }
                else
                {
                    timeToEnemyTroops[time] = enemyFactory.CyborgCount;
                }
            }
        }

        int distance = _links.GetShortestPathDistance(sourceFactory.Id, targetFactory.Id);
        Owner previousOwnership = targetFactory.Owner;
        List<Entity> friendlyBombs = _entities.Where(e => e is BombEntity && e.IsFriendly()).ToList();
        BombEntity friendlyBomb = null;
        if (friendlyBombs.Any())
        {
            friendlyBomb = friendlyBombs.First() as BombEntity;
            if (friendlyBomb.TurnsToArrive >= distance && friendlyBomb.TargetFactory == targetFactory.Id)
            {
                return 999999;//Don't send troops where a bomb is going to explode after we send troops
            }
            //Console.Error.WriteLine("Bomb: " + friendlyBomb.Id + " Target: " + friendlyBomb.TargetFactory + " Time: " + friendlyBomb.TurnsToArrive);
        }

        //Calculate the number of cyborgs in the factory up to the distance of the source factory
        int cyborgsInFactory = targetFactory.CyborgCount;
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


            if (friendlyBomb != null && friendlyBomb.TargetFactory == targetFactory.Id && friendlyBomb.TurnsToArrive == i)
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

public class MoveList
{
    public List<Move> Moves { get; set; }

    public MoveList()
    {
        Moves = new List<Move>();
    }
    public void AddMove(Move move)
    {
        Moves.Add(move);
    }

    public void PlayMoves()
    {
        bool isFirst = true;
        foreach (Move move in Moves)
        {
            if (!isFirst)
            {
                Console.Write(";");
            }
            move.PlayMove();
            isFirst = false;
        }
        Console.WriteLine();
    }
}

public class MoveType
{
    public const string Move = "MOVE";
    public const string Bomb = "BOMB";
    public const string Wait = "WAIT";
    public const string Message = "MSG";
    public const string Upgrade = "INC";
}

public class Move
{
    private readonly int source;
    private readonly int destination;
    private readonly int count;
    private readonly string moveType;
    private readonly string message;
    public Move()
    {
        moveType = MoveType.Wait;
    }

    public Move(int factoryId)
    {
        source = factoryId;
        moveType = MoveType.Upgrade;
    }

    public Move(int sourceFactory, int destinationFactory, int cyborgCount)
    {
        source = sourceFactory;
        destination = destinationFactory;
        count = cyborgCount;
        moveType = MoveType.Move;
    }
    public Move(int sourceFactory, int destinationFactory)
    {
        moveType = MoveType.Bomb;
        source = sourceFactory;
        destination = destinationFactory;
    }
    public Move(string message)
    {
        moveType = MoveType.Message;
        this.message = message;
    }


    public void PlayMove()
    {
        string move = moveType switch
        {
            MoveType.Move => MoveType.Move + " " + source + " " + destination + " " + count,
            MoveType.Wait => MoveType.Wait,
            MoveType.Bomb => MoveType.Bomb + " " + source + " " + destination,
            MoveType.Message => MoveType.Message + " " + message,
            MoveType.Upgrade => MoveType.Upgrade + " " + source,
            _ => MoveType.Wait
        };
        Console.Write(move);
    }
}


public class EntityTypes
{
    public const string Factory = "FACTORY";
    public const string Troop = "TROOP";
    public const string Bomb = "BOMB";
}

public class EntityFactory
{
    public EntityFactory()
    {

    }

    public Entity CreateEntity(string type, int id, int arg1, int arg2, int arg3, int arg4, int arg5)
    {
        return type switch
        {
            EntityTypes.Factory => new FactoryEntity(id, arg1, arg2, arg3, arg4, arg5),
            EntityTypes.Troop => new TroopEntity(id, arg1, arg2, arg3, arg4, arg5),
            EntityTypes.Bomb => new BombEntity(id, arg1, arg2, arg3, arg4, arg5),
            _ => null
        };
    }
}

public enum Owner
{
    Opponent = -1,
    Neutral = 0,
    Me = 1
}

public class BombEntity : Entity
{
    public int SourceFactory
    {
        get { return Arg2; }
    }
    public int TargetFactory
    {
        get { return Arg3; }
    }
    public int TurnsToArrive
    {
        get { return Arg4; }
    }

    public BombEntity(int id, int arg1, int arg2, int arg3, int arg4, int arg5)
    : base(id, arg1, arg2, arg3, arg4, arg5)
    {

    }
}

public class FactoryEntity : Entity
{
    public int CyborgCount
    {
        get { return Arg2; }
    }
    public int ProductionCount
    {
        get { return Arg3; }
    }

    public int TurnsTillProduction
    {
        get { return Arg4; }
    }

    public FactoryEntity(int id, int arg1, int arg2, int arg3, int arg4, int arg5)
    : base(id, arg1, arg2, arg3, arg4, arg5)
    {

    }

    public bool IsProducing()
    {
        return TurnsTillProduction == 0;
    }
}

public class TroopEntity : Entity
{
    public int SourceFactory
    {
        get { return Arg2; }
    }

    public int TargetFactory
    {
        get { return Arg3; }
    }

    public int NumberOfCyborgs
    {
        get { return Arg4; }
    }

    public int TurnsToArrive
    {
        get { return Arg5; }
    }


    public TroopEntity(int id, int arg1, int arg2, int arg3, int arg4, int arg5)
    : base(id, arg1, arg2, arg3, arg4, arg5)
    {

    }
}


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

public class MoveOption
{
    public FactoryEntity SourceFactory { get; set; }
    public FactoryEntity TargetFactory { get; set; }
    public int SendCount { get; set; }
    public int BestTargetId { get; set; }
    public int DefendCount { get; set; }
    public MoveOption(FactoryEntity sourceFactory, FactoryEntity targetFactory, int sendCount, int bestTargetId, int defendCount)
    {
        SourceFactory = sourceFactory;
        TargetFactory = targetFactory;
        SendCount = sendCount;
        BestTargetId = bestTargetId;
        DefendCount = defendCount;
    }

    public Move GenerateMove()
    {
        return new Move(SourceFactory.Id, BestTargetId, SendCount);
    }
}