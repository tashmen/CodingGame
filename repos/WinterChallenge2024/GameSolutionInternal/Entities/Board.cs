using Algorithms.Graph;
using System.Diagnostics;
using System.Text;
using static Algorithms.Graph.Graph;

namespace GameSolution.Entities
{
    public class Board
    {
        public int Width;
        public int Height;
        private Entity[] Entities;
        public int GlobalOrganId = -1;

        public Graph Graph;

        private int _myEntityCount = -1;
        private int _oppEntityCount = -1;
        private Point2d[][][] _locationCache;
        private int[][] _locationIndexCache = null;
        private LocationNeighbor[][] _locationNeighbors = null;
        private string[,,] _keyCache;
        //First is by the number of organisms and second is by current organism number; general goal was to limit to a maximum expansion of 24 moves and then we take 10
        const int _maxMovesTotal = 20;
        private static readonly int[,] _maxActionsPerOrganism = { { _maxMovesTotal, 0, 0, 0, 0 }, { 6, 4, 0, 0, 0 }, { 6, 2, 2, 0, 0 }, { 3, 2, 2, 2, 0 }, { 3, 2, 2, 2, 1 } };
        private static Stopwatch _watch = new Stopwatch();

        private int _initialOpenSpacesCount = 0;
        private static Entity[] _tempHolder = new Entity[500];
        private Dictionary<string, Entity[]> _entityCache = new Dictionary<string, Entity[]>();
        private bool[] _isOpenSpaceInitial;
        private Dictionary<string, bool> _locationCheckCache = new Dictionary<string, bool>();
        private Dictionary<string, bool?> _harvestCache = new Dictionary<string, bool?>();
        private Dictionary<string, List<MoveAction>> _moveActionCache = new Dictionary<string, List<MoveAction>>();
        private const int _maxGrowMoves = 7;

        public static OrganDirection[] PossibleDirections = new OrganDirection[] { OrganDirection.North, OrganDirection.South, OrganDirection.East, OrganDirection.West };

        public Board()
        {

        }

        public Board(int width, int height)
        {
            Width = width;
            Height = height;
            Entities = new Entity[Width * Height];
            Graph = new Graph();
            InitializeBoard();
        }

        public Board CopyFrom(Board board)
        {
            Width = board.Width;
            Height = board.Height;
            if (Entities != null)
            {
                Array.Copy(board.Entities, Entities, board.Entities.Length);
            }
            else
            {
                Entities = board.Entities.ToArray();
            }
            GlobalOrganId = board.GlobalOrganId;
            Graph = board.Graph;
            foreach (string key in board._entityCache.Keys)
            {
                _entityCache[key] = board._entityCache[key];
            }
            foreach (string key in board._moveActionCache.Keys)
            {
                _moveActionCache[key] = board._moveActionCache[key];
            }
            foreach (string key in board._locationCheckCache.Keys)
            {
                _locationCheckCache[key] = board._locationCheckCache[key];
            }
            foreach (string key in board._harvestCache.Keys)
            {
                _harvestCache[key] = board._harvestCache[key];
            }
            _myEntityCount = board._myEntityCount;
            _oppEntityCount = board._oppEntityCount;
            _locationCache = board._locationCache;
            _locationIndexCache = board._locationIndexCache;
            _locationNeighbors = board._locationNeighbors;
            _keyCache = board._keyCache;
            _isOpenSpaceInitial = board._isOpenSpaceInitial;
            _initialOpenSpacesCount = board._initialOpenSpacesCount;


            return this;
        }

        public bool Equals(Board board)
        {
            if (Width != board.Width || Height != board.Height)
                return false;

            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
                {
                    board.GetEntity(GetNodeIndex(x, y), out Entity entity);
                    GetEntity(GetNodeIndex(x, y), out Entity currentEntity);

                    if (entity == null && currentEntity == null)
                        continue;
                    if (entity == null && currentEntity != null)
                        return false;
                    if (entity != null && currentEntity == null)
                        return false;
                    else if (!entity.Equals(currentEntity))
                        return false;
                }
            }
            return true;
        }

        public void Attack()
        {
            Entity[] tentacles = GetTentacleEntities();

            Entity[] deadEntities = new Entity[tentacles.Length];
            for (int i = 0, j = 0; j < tentacles.Length; j++)
            {
                Entity tentacle = tentacles[i];
                if (GetEntityWithDirection(tentacle.Location, tentacle.OrganDirection, out Entity entity) && entity.IsMine.HasValue && entity.IsMine != tentacle.IsMine)
                {
                    deadEntities[i] = entity;
                    i++;
                }
            }
            foreach (Entity deadEntity in deadEntities)
            {
                if (deadEntity == null)
                {
                    break;
                }
                if (deadEntity.Type == EntityType.ROOT)
                {
                    IEnumerable<Entity> deathToRoot = GetEntitiesList().Where(e => e.OrganRootId == deadEntity.OrganId);
                    foreach (Entity kill in deathToRoot)
                    {
                        //Entities[kill.Location.index]?.Dispose();
                        Entities[kill.Location.index] = null;
                    }
                }
                else
                {
                    IEnumerable<Entity> deathToChildren = GetEntitiesList().Where(e => e.OrganParentId == deadEntity.OrganId);
                    foreach (Entity kill in deathToChildren)
                    {
                        //Entities[kill.Location.index]?.Dispose();
                        Entities[kill.Location.index] = null;
                    }
                }
            }
        }

        public void ApplyMove(Move myMove, Move oppMove)
        {
            foreach (MoveAction action in myMove.Actions)
            {
                if (action.Type != MoveType.WAIT)
                {
                    IEnumerable<MoveAction> collisionActions = oppMove.Actions.Where(a => (a.Type == MoveType.GROW || a.Type == MoveType.SPORE) && a.Location.Equals(action.Location));
                    if (collisionActions.Count() > 0)
                    {
                        //Entities[action.Location.index]?.Dispose();
                        Entities[action.Location.index] = Entity.GetEntity(action.Location, EntityType.WALL, null, 0, 0, 0, OrganDirection.None);
                    }
                    else
                    {
                        ApplyAction(action, true);
                    }
                }
            }
            foreach (MoveAction action in oppMove.Actions)
            {
                ApplyAction(action, false);
            }
        }

        public void ApplyAction(MoveAction action, bool isMine)
        {
            switch (action.Type)
            {
                case MoveType.GROW:
                    Entity growEntity = GetEntityByLocation(action.Location);
                    if (growEntity == null || growEntity.IsOpenSpace())
                    {
                        //Entities[action.Location.index]?.Dispose();
                        Entities[action.Location.index] = Entity.GetEntity(action.Location, action.EntityType, isMine, GlobalOrganId++, action.OrganId, action.OrganRootId, action.OrganDirection);
                    }
                    break;
                case MoveType.SPORE:
                    Entity sporeEntity = GetEntityByLocation(action.Location);
                    if (sporeEntity == null || sporeEntity.IsOpenSpace())
                    {
                        int organId = GlobalOrganId++;
                        //Entities[action.Location.index]?.Dispose();
                        Entities[action.Location.index] = Entity.GetEntity(action.Location, action.EntityType, isMine, organId, organId, organId, action.OrganDirection);
                    }
                    break;
            }
        }

        public List<Move> GetMoves(int[] proteins, bool isMine, bool debug = false)
        {
            _watch.Reset();
            _watch.Start();
            List<Move> moves = new List<Move>();

            Entity[] rootEntities = GetRootEntities(isMine);
            int organismCount = rootEntities.Length;
            bool[] organismHasMoves = new bool[organismCount];
            MoveAction[][] organismToMoveActions = new MoveAction[organismCount][];

            ProteinInfo proteinInfo = new ProteinInfo(proteins, this, isMine);

            int i = 0;
            int maxOrgans = 5;//Put a limit on the number of organs we calculate moves for as this is exploding the action space

            Entity[] limitedRootEntities = rootEntities;
            if (organismCount > maxOrgans)
            {
                limitedRootEntities = rootEntities.Take(maxOrgans).ToArray();
            }
            int selectedOrgans = limitedRootEntities.Length;

            bool hasSufficientProteins = (proteins[0] >= selectedOrgans || proteins[0] == 0) && (proteins[1] >= selectedOrgans || proteins[1] == 0) && (proteins[2] >= selectedOrgans || proteins[2] == 0) && (proteins[3] >= selectedOrgans || proteins[3] == 0);//Checks for 0 because if we don't have any of that protein then we won't have actions of that type.

            HashSet<int> locationsTaken = new HashSet<int>();

            foreach (Entity root in limitedRootEntities)
            {
                List<MoveAction> moveActions = new List<MoveAction>();

                if (proteinInfo.HasRootProteins)
                {
                    AddSporeMoveActions(moveActions, root.OrganRootId, isMine, locationsTaken, proteinInfo);
                    if (debug)
                        Console.Error.WriteLine($"Spore: {root.OrganRootId}: " + string.Join('\n', moveActions));
                    if (moveActions.Count > 0)
                    {
                        moveActions = moveActions.OrderBy(m => m.Score).ToList();
                        if (moveActions[0].Score < 0)
                        {
                            moveActions = moveActions.Where(m => m.Score < 0).ToList();
                            organismHasMoves[i] = true;
                        }
                    }
                }


                AddGrowMoveActions(moveActions, root.OrganRootId, isMine, proteinInfo, locationsTaken, debug);

                if (moveActions.Count > 0)
                {
                    moveActions = moveActions.OrderBy(m => m.Score).Take(_maxActionsPerOrganism[limitedRootEntities.Length - 1, i]).ToList();
                    /*if (moveActions[0].Score < -100000)
                    {
                        moveActions = moveActions.Where(m => m.Score < -100000).ToList();
                        organismHasMoves[i] = true;
                    }
                    else*/
                    if (moveActions[0].Score < 0)
                    {
                        moveActions = moveActions.Where(m => m.Score < 0).ToList();
                        organismHasMoves[i] = true;
                    }
                }




                locationsTaken.UnionWith(moveActions.Select(m => m.Location.index));

                if (organismCount == 1 && moveActions.Count > 0)
                {

                }
                else
                {
                    if (moveActions.Count > 0 && hasSufficientProteins)
                    {

                    }
                    else
                        moveActions.Add(MoveAction.CreateWait());
                }


                organismToMoveActions[i] = moveActions.ToArray();
                if (debug)
                    Console.Error.WriteLine($"{root.OrganId}: " + string.Join('\n', moveActions));
                i++;

                if (_watch.ElapsedMilliseconds > 5)
                {
                    Console.Error.WriteLine($"Move generation took too long organism #: {i}, {_watch.ElapsedMilliseconds} ms");
                }
            }
            _watch.Stop();
            for (; i < organismCount; i++)
            {
                organismToMoveActions[i] = new MoveAction[]
                {
                    MoveAction.CreateWait()
                };
            }


            if (organismCount > 1)
            {
                //When some organism has good moves to play then prune the left over moves that aren't as good.
                bool hasAtLeastOneMove = organismHasMoves.Any(m => m);
                if (hasAtLeastOneMove)
                {
                    for (int a = 0; a < organismCount; a++)
                    {
                        if (!organismHasMoves[a])
                        {
                            if (debug)
                                Console.Error.WriteLine($"Pruning moves for {a}");
                            organismToMoveActions[a] = new MoveAction[]
                            {
                                MoveAction.CreateWait()
                            };
                        }
                    }
                }

                if (debug)
                {
                    foreach (MoveAction[] organMoves in organismToMoveActions)
                    {
                        Console.Error.WriteLine($"{organMoves[0].OrganRootId}: " + string.Join('\n', organMoves.AsEnumerable()));
                    }
                    Console.Error.WriteLine($"Proteins: " + string.Join(",", proteins));
                }
                IEnumerable<Move> theMoves = PrunedCartesianProduct(organismToMoveActions, hasSufficientProteins, proteins);
                moves = theMoves.OrderBy(m => m.Score).Take(_maxMovesTotal).ToList();
                if (debug)
                    Console.Error.WriteLine($"Final set: {moves.Count}");
            }
            else if (organismCount == 1)
            {
                foreach (MoveAction action in organismToMoveActions[0])
                {
                    Move move = new Move();
                    move.SetActions(new MoveAction[] { action });
                    moves.Add(move);
                }
            }
            else
            {
                //Since we don't check game end conditions until after a move occurs, when there is an attack that kills the last root entity then we can end up in this situation so just give a wait action and do nothing.  The game is over.
                Move move = new Move();
                move.SetActions(new MoveAction[]
                {
                    MoveAction.CreateWait()
                });
                moves.Add(move);
            }

            if (moves.Count == 0)
            {
                if (!debug)
                    GetMoves(proteins, isMine, true);
            }

            return moves;
        }

        public bool ValidateCost(int[] proteins, Move move)
        {
            int[] theCosts = move.GetCost();

            for (int i = 0; i < 4; i++)
            {
                if (proteins[i] < theCosts[i])
                    return false;
            }
            return true;
        }

        //should be faster with pruning
        public IEnumerable<Move> PrunedCartesianProduct(MoveAction[][] sequences, bool hasSufficientProteins, int[] proteins)
        {
            if (sequences == null || sequences.Length == 0)
                yield break;

            MoveAction[][] sequenceArrays = sequences;
            int dimensions = sequenceArrays.Length;

            // Indexes to keep track of positions in each sequence
            int[] indices = new int[dimensions];

            // Pre-allocate arrays for partial costs and collisions
            int[][] partialCosts = new int[dimensions][];
            HashSet<int>[] partialCollision = new HashSet<int>[dimensions];
            for (int i = 0; i < dimensions; i++)
            {
                partialCosts[i] = new int[4]; // Assuming a constant size for protein cost array
                partialCollision[i] = new HashSet<int>();
            }
            int[] initialCost = new int[4] { 0, 0, 0, 0 };
            HashSet<int> initialCollision = new HashSet<int>();

            MoveAction[] currentCombination = new MoveAction[dimensions];
            int position = 0;

            while (true)
            {
                bool hasCollision = false;
                bool hasProteins = true;

                // Build the current combination
                int i;
                for (i = position; i < dimensions; i++)
                {
                    // Reuse partial costs and collisions arrays instead of re-allocating them
                    if (i > 0)
                    {
                        Array.Copy(partialCosts[i - 1], partialCosts[i], partialCosts[i - 1].Length);
                        partialCollision[i] = new HashSet<int>(partialCollision[i - 1]);
                    }
                    else
                    {
                        Array.Copy(initialCost, partialCosts[i], initialCost.Length);
                        partialCollision[i] = new HashSet<int>(initialCollision);
                    }
                    currentCombination[i] = sequenceArrays[i][indices[i]];

                    // Update partial costs for each protein based on current combination
                    if (!hasSufficientProteins)
                    {
                        int[] cost = currentCombination[i].GetCost();
                        for (int j = 0; j < proteins.Length; j++)
                        {
                            partialCosts[i][j] += cost[j];
                            if (partialCosts[i][j] > proteins[j])
                            {
                                hasProteins = false;
                                break;
                            }
                        }

                        if (!hasProteins)
                        {
                            break;
                        }
                    }

                    // Update partial collisions
                    if (ValidateLocation(currentCombination[i].Location))
                    {
                        if (!partialCollision[i].Add(currentCombination[i].Location.index))
                        {
                            hasCollision = true;
                            break;
                        }
                    }
                }

                // Validate the combination
                if (!hasCollision && (hasSufficientProteins || hasProteins))
                {
                    Move move = new Move();
                    move.SetActions(currentCombination.ToArray());
                    yield return move;
                    position = dimensions - 1;
                }
                else
                {
                    if (position < i)
                        position = i;
                }

                // Increment indices iteratively
                while (position >= 0)
                {
                    indices[position]++;
                    if (indices[position] < sequenceArrays[position].Length)
                        break;

                    // Reset this position and move to the next
                    indices[position] = 0;
                    position--;
                }

                // Terminate if we've exhausted all combinations
                if (position < 0)
                    break;
            }
        }


        public List<MoveAction> AddSporeMoveActions(List<MoveAction> moveActions, int organRootId, bool isMine, HashSet<int> locationsTaken, ProteinInfo proteinInfo)
        {
            IEnumerable<Entity> sporers = GetSporerEntities(organRootId, isMine);

            foreach (Entity sporer in sporers)
            {
                Point2d location = sporer.Location;
                //int distance = 0;
                while (true)
                {
                    //distance++;
                    location = GetNextLocation(location, sporer.OrganDirection);
                    if (ValidateLocation(location) && IsOpenSpace(location.index, isMine))
                    {
                        //we don't care about distance anymore; a spore location is still valid even if it's one space away.
                        if (/*distance <= 3 || */locationsTaken.Contains(location.index))
                        {
                            continue;
                        }
                        bool? isHarvesting = IsHarvesting(location, isMine);
                        MoveAction sporeMove = MoveAction.CreateSpore(sporer.OrganId, location);
                        moveActions.Add(sporeMove);

                        if (!(isHarvesting.HasValue && isHarvesting.Value) && proteinInfo.HasHarvestable && IsHarvestExactly2spaces(location, proteinInfo.ToHarvestEntities))
                        {
                            sporeMove.Score = -1000;
                        }
                        else
                        {
                            if (proteinInfo.HasHarvestable)
                            {
                                double minValue = 99999;
                                for (int i = 0; i < proteinInfo.ToHarvestEntities.Length; i++)
                                {
                                    double pathDistance = Graph.GetShortestPathDistance(proteinInfo.ToHarvestEntities[i].Location.index, sporeMove.Location.index);
                                    if (minValue > pathDistance)
                                        minValue = pathDistance;
                                }
                                sporeMove.Score = minValue - 29;//30 is a basic move action so make this override a basic action if it's closer.

                                /*
                                if (proteinInfo.IsHarvestingProteins[0])
                                    sporeMove.Score -= 100;//another -100 for if we have A so that it takes priority over basic even with the A bonus.
                                */
                            }
                            else
                                sporeMove.Score = 1000;
                        }

                        if (proteinInfo.HasManyRootProteins || proteinInfo.IsHarvestingRootProteins)
                        {
                            sporeMove.Score -= 500;//move spores if we have the resources...
                        }
                    }
                    else
                    {
                        break;
                    }

                    /* Spores are weird actions; always consider all of them.
                    if (moveActions.Count > 2)
                    {
                        break;
                    }
                    */
                }
            }

            return moveActions;
        }

        public List<MoveAction> AddGrowMoveActions(List<MoveAction> moveActions, int organRootId, bool isMine, ProteinInfo proteinInfo, HashSet<int> locationsTaken, bool debug = false)
        {


            List<MoveAction> growMoveActions = GetGrowMoveActions(organRootId, isMine, locationsTaken, proteinInfo);

            if (debug)
            {

                Console.Error.WriteLine($"All: {organRootId}: " + string.Join('\n', growMoveActions));
            }

            bool canHarvest = false;
            List<MoveAction> harvestActions = new List<MoveAction>();
            if (proteinInfo.HasHarvestProteins)
            {
                harvestActions = GetHarvestMoveActions(organRootId, isMine, locationsTaken, proteinInfo);
                moveActions.AddRange(harvestActions);
                canHarvest = harvestActions.Count > 0;
                if (debug)
                    Console.Error.WriteLine($"Harvest: {organRootId}: " + string.Join('\n', harvestActions));
            }

            if (moveActions.Count > 0 && _watch.ElapsedMilliseconds > 5)
            {
                Console.Error.WriteLine($"Took too long to find Harvest Actions: {_watch.ElapsedMilliseconds}");
            }

            List<MoveAction> tentacleActions = new List<MoveAction>();
            if (proteinInfo.HasTentacleProteins)
            {
                tentacleActions = GetTentacleMoveActions(organRootId, isMine, locationsTaken, proteinInfo);
                moveActions.AddRange(tentacleActions);
                if (debug)
                    Console.Error.WriteLine($"Tentacle: {organRootId}: " + string.Join('\n', tentacleActions));
            }

            if (moveActions.Count > 0 && _watch.ElapsedMilliseconds > 5)
            {
                Console.Error.WriteLine($"Took too long to find Tentacle Actions: {_watch.ElapsedMilliseconds}");
            }

            bool hasHarvestMove = harvestActions.Any(m => m.Score < 0);
            bool hasTentacleMove = tentacleActions.Any(m => m.Score < 0);

            if (proteinInfo.HasBasicProteins)
            {
                List<MoveAction> basicActions = GetGrowBasicMoveActions(organRootId, isMine, locationsTaken, proteinInfo);

                foreach (MoveAction basicAction in basicActions)
                {

                    if (hasHarvestMove)
                        basicAction.Score += 100;//Prefer harvest actions over basics

                    if (hasTentacleMove)
                        basicAction.Score += 200;//Prefer tentacle actions over basics

                    if (proteinInfo.HasManyTentacleProteins)
                    {
                        foreach (MoveAction tentacleAction in tentacleActions)
                        {
                            if (tentacleAction.Location.Equals(basicAction.Location))
                            {
                                basicAction.Score += 100;//Prefer tentacle actions over basics.
                            }
                        }
                    }
                }
                moveActions.AddRange(basicActions);
                if (debug)
                    Console.Error.WriteLine($"Basic: {organRootId}: " + string.Join('\n', basicActions));
            }

            if (moveActions.Count > 0 && _watch.ElapsedMilliseconds > 5)
            {
                Console.Error.WriteLine($"Took too long to find Basic Actions: {_watch.ElapsedMilliseconds}");
            }

            if (proteinInfo.HasSporerProteins)
            {
                List<MoveAction> sporerActions = GetSporerMoveActions(organRootId, isMine, locationsTaken, proteinInfo);

                foreach (MoveAction sporerAction in sporerActions)
                {
                    if (!proteinInfo.HasManyRootProteins || !proteinInfo.IsHarvestingSporerProteins)
                    {
                        if (hasHarvestMove)
                            sporerAction.Score += 100;//Prefer harvest actions over sporers
                        if (!proteinInfo.HasManyTentacleProteins && !proteinInfo.HasTentacleProteins && hasTentacleMove)
                            sporerAction.Score += 100;//Prefer tentacle actions over sporers, but only if they weren't granted through having proteins
                    }
                    if (!proteinInfo.HasRootProteins)
                        sporerAction.Score += 100;
                }


                moveActions.AddRange(sporerActions);
                if (debug)
                    Console.Error.WriteLine($"Sporer: {organRootId}: " + string.Join('\n', sporerActions));
            }

            return moveActions;
        }

        public List<MoveAction> GetSporerMoveActions(int organRootId, bool isMine, HashSet<int> locationsTaken, ProteinInfo proteinInfo)
        {
            List<MoveAction> moveActions = new List<MoveAction>();

            List<MoveAction> growMoveActions = GetGrowMoveActions(organRootId, isMine, locationsTaken, proteinInfo);

            foreach (MoveAction growAction in growMoveActions)
            {
                List<MoveAction> sporerMoves = new List<MoveAction>();
                foreach (LocationNeighbor locationNeighbor in GetLocationNeighbors(growAction.Location))
                {
                    //Console.Error.WriteLine($"checking direction: {locationNeighbor.direction}");
                    MoveAction sporerAction = MoveAction.CreateGrow(growAction.OrganId, growAction.Location, EntityType.SPORER, growAction.OrganRootId, locationNeighbor.direction);
                    sporerAction.Score = growAction.Score;

                    if (proteinInfo.IsHarvestingSporerProteins)
                    {
                        sporerAction.Score -= 100;
                    }

                    if (proteinInfo.Proteins[3] + proteinInfo.HarvestingProteins[3] < 2)
                    {
                        sporerAction.Score += 300;//Can't build a sporer if there won't be enough resources to spore on the next turn
                    }


                    Point2d location = locationNeighbor.point;
                    bool isOpen = true;
                    for (int i = 0; i < 1; i++)//As long as there is one valid space we are good to go.
                    {
                        if (!ValidateLocation(location) || !IsOpenSpace(location.index, isMine))
                        {
                            /*
                            if (ValidateLocation(location))
                                Console.Error.WriteLine($"Found something in the way on: {location.x}, {location.y}, grow location {growAction.Location.x}, {growAction.Location.y}, direction: {locationNeighbor.direction}");
                            */
                            isOpen = false;
                            break;
                        }
                        //location = GetNextLocation(location, locationNeighbor.direction);
                    }
                    if (isOpen)
                    {
                        sporerAction.Score -= 100;
                        sporerMoves.Add(sporerAction);
                    }
                }
                if (sporerMoves.Count > 0)
                {
                    moveActions.AddRange(sporerMoves);
                }
                else
                {
                    MoveAction moveAction = MoveAction.CreateGrow(growAction.OrganId, growAction.Location, EntityType.SPORER, growAction.OrganRootId);
                    moveAction.Score = growAction.Score;
                    moveAction.Score += 1000;
                    moveActions.Add(moveAction);
                }
            }

            return moveActions;
        }

        public List<MoveAction> GetTentacleMoveActions(int organRootId, bool isMine, HashSet<int> locationsTaken, ProteinInfo proteinInfo)
        {
            List<MoveAction> moveActions = new List<MoveAction>();
            List<MoveAction> growMoveActions = GetGrowMoveActions(organRootId, isMine, locationsTaken, proteinInfo);

            foreach (MoveAction growAction in growMoveActions)
            {
                List<MoveAction> tentacleMoveActions = new List<MoveAction>();
                //Could suggest the direction based on the shortest path.
                bool isOppIn3Spaces = IsOpponentClose(growAction.Location, isMine);
                foreach (LocationNeighbor locationNeighbor in GetLocationNeighbors(growAction.Location))
                {
                    MoveAction tentacleAction = MoveAction.CreateGrow(growAction.OrganId, growAction.Location, EntityType.TENTACLE, growAction.OrganRootId, locationNeighbor.direction);
                    tentacleAction.Score = growAction.Score;

                    if (IsOpponentOrOpenSpace(locationNeighbor.point.index, isMine))
                    {
                        tentacleMoveActions.Add(tentacleAction);
                        if (proteinInfo.HasManyTentacleProteins)
                            tentacleAction.Score -= 50;
                        if (isOppIn3Spaces)
                        {


                            tentacleAction.Score -= 50;
                            //Check to see if any are facing the enemy and use those as priority
                            if (GetEntity(locationNeighbor.point.index, out Entity entity) && entity.IsMine.HasValue && entity.IsMine != isMine)
                            {
                                tentacleAction.Score -= 50000;
                                if (entity.Type == EntityType.ROOT && GetEntitiesByRoot(entity.OrganId, !isMine).Length > 0)
                                {
                                    tentacleAction.Score -= 50000;//hit the root!
                                    return new List<MoveAction> { tentacleAction };//Destroying roots should always be the priority
                                }
                            }
                            else
                            {
                                //Since opponent is within 3 spaces; they must be 3 away or less so go look for them and provide it as the priority
                                Point2d nextLocation = GetNextLocation(locationNeighbor.point, locationNeighbor.direction);
                                if (ValidateLocation(nextLocation))
                                {
                                    foreach (LocationNeighbor locationNeighbor2 in GetLocationNeighbors(nextLocation))
                                    {
                                        if (GetEntity(locationNeighbor2.point.index, out Entity entity2) && entity2.IsMine.HasValue && entity2.IsMine != isMine)
                                        {
                                            tentacleAction.Score -= 30000;
                                        }
                                    }
                                }
                            }
                        }
                        if (proteinInfo.IsHarvestingTentacleProteins)
                            tentacleAction.Score -= 100;
                    }
                }

                if (tentacleMoveActions.Count > 0)
                {
                    double best = tentacleMoveActions.Min(m => m.Score);
                    moveActions.AddRange(tentacleMoveActions.Where(m => m.Score == best));
                }
                else
                {
                    MoveAction moveAction = MoveAction.CreateGrow(growAction.OrganId, growAction.Location, EntityType.TENTACLE, growAction.OrganRootId, OrganDirection.North);
                    moveAction.Score = growAction.Score;
                    moveAction.Score += 10000;
                    moveActions.Add(moveAction);
                }
            }

            return moveActions;
        }

        public List<MoveAction> GetGrowMoveActions(int organRootId, bool isMine, HashSet<int> locationsTaken, ProteinInfo proteinInfo)
        {
            string key = GetKey(6, isMine, organRootId);
            if (!_moveActionCache.TryGetValue(key, out List<MoveAction> moveActions))
            {
                Entity[] oppRootEntities = GetRootEntities(!isMine);

                bool hasOppRoot = oppRootEntities.Length > 0;



                moveActions = new List<MoveAction>();
                Entity[] entities = GetEntitiesByRoot(organRootId, isMine);
                HashSet<int> locationsChecked = new HashSet<int>(locationsTaken);
                foreach (Entity entity in entities)
                {
                    foreach (LocationNeighbor locationNeighbor in GetLocationNeighbors(entity.Location))
                    {
                        if (locationsChecked.Add(locationNeighbor.point.index))
                        {
                            if (IsOpenSpace(locationNeighbor.point.index, isMine))
                            {
                                MoveAction moveAction = MoveAction.CreateGrow(entity.OrganId, locationNeighbor.point, EntityType.NONE, entity.OrganRootId);
                                moveAction.Score = 0;
                                if (proteinInfo.HasHarvestable)
                                {
                                    double minValue = 99999;
                                    for (int i = 0; i < proteinInfo.ToHarvestEntities.Length; i++)
                                    {
                                        double distance = Graph.GetShortestPathDistance(proteinInfo.ToHarvestEntities[i].Location.index, moveAction.Location.index);
                                        if (minValue > distance)
                                            minValue = distance;
                                    }
                                    moveAction.Score += minValue;
                                }
                                else if (hasOppRoot)
                                {
                                    moveAction.Score += oppRootEntities.Min(r => Graph.GetShortestPathDistance(r.Location.index, moveAction.Location.index));
                                }

                                bool? isHarvesting = IsHarvesting(locationNeighbor.point.index, isMine);
                                if (isHarvesting.HasValue)
                                {
                                    if (GetEntity(locationNeighbor.point.index, out Entity harvestEntity) && !proteinInfo.HasHarvestProteins && ((harvestEntity.Type == EntityType.C && !proteinInfo.HasProteins[2]) || (harvestEntity.Type == EntityType.D && !proteinInfo.HasProteins[3])))
                                    {
                                        moveAction.Score -= 1000;//if we have no C/D then eat it.  We need C/D to build harvesters
                                    }
                                    bool isOppIn3Spaces = IsOpponentClose(moveAction.Location, isMine);
                                    if (!isOppIn3Spaces)
                                    {
                                        if (proteinInfo.HarvestingProteins[harvestEntity.Type - EntityType.A] <= 1)
                                            moveAction.Score += 1000;//Avoid eating proteins we aren't harvesting yet
                                    }

                                    if (isOppIn3Spaces)
                                    {
                                        moveAction.Score -= 20;//Give higher priority to spaces where the opponent is near
                                    }
                                }

                                moveActions.Add(moveAction);
                            }
                        }

                    }
                }


                if (moveActions.Count > _maxGrowMoves)
                {
                    moveActions = moveActions.OrderBy(m => m.Score).Take(_maxGrowMoves).ToList();
                }
                _moveActionCache[key] = moveActions;
            }
            return moveActions;
        }

        public List<MoveAction> GetGrowBasicMoveActions(int organRootId, bool isMine, HashSet<int> locationsTaken, ProteinInfo proteinInfo)
        {
            List<MoveAction> moveActions = new List<MoveAction>();
            List<MoveAction> growMoveActions = GetGrowMoveActions(organRootId, isMine, locationsTaken, proteinInfo);

            //List<MoveAction> growBasicActions = new List<MoveAction>();
            foreach (MoveAction growAction in growMoveActions)
            {
                MoveAction moveAction = MoveAction.CreateGrow(growAction.OrganId, growAction.Location, EntityType.BASIC, growAction.OrganRootId);
                moveAction.Score = growAction.Score;
                if (GetEntity(growAction.Location, out Entity entity))
                {
                    if (proteinInfo.Proteins[entity.Type - EntityType.A] == 0 && !proteinInfo.IsHarvestingProteins[entity.Type - EntityType.A])
                    {
                        moveAction.Score -= 100;//we have none of this type and we aren't harvesting it yet so we need it?
                    }
                }
                else
                    moveAction.Score -= 30;

                /*
                if (proteinInfo.IsHarvestingBasicProteins)
                {
                    moveAction.Score -= 100;
                }
                */
                moveActions.Add(moveAction);
            }

            /* Not sure why this was here; these don't depend on facing so let MCTS decide
            if (growBasicActions.Count > 0)
            {
                double best = growBasicActions.Min(m => m.Score);
                moveActions.AddRange(growBasicActions.Where(m => m.Score == best));
            }
            */

            return moveActions;
        }

        public List<MoveAction> GetHarvestMoveActions(int organRootId, bool isMine, HashSet<int> locationsTaken, ProteinInfo proteinInfo)
        {
            List<MoveAction> moveActions = new List<MoveAction>();
            List<MoveAction> growMoveActions = GetGrowMoveActions(organRootId, isMine, locationsTaken, proteinInfo);

            foreach (MoveAction growAction in growMoveActions)
            {
                bool isOppIn3Spaces = IsOpponentClose(growAction.Location, isMine);
                List<MoveAction> harvestActions = new List<MoveAction>();
                foreach (LocationNeighbor locationNeighbor in GetLocationNeighbors(growAction.Location))
                {
                    bool? isHarvesting = IsHarvesting(locationNeighbor.point.index, isMine);

                    MoveAction harvestAction = MoveAction.CreateGrow(growAction.OrganId, growAction.Location, EntityType.HARVESTER, growAction.OrganRootId, locationNeighbor.direction);
                    harvestAction.Score = growAction.Score;

                    if (isOppIn3Spaces)
                    {
                        harvestAction.Score += 1000;
                    }

                    if (IsHarvestSpace(locationNeighbor.point.index, out Entity harvest) && !proteinInfo.IsHarvestingProteins[harvest.Type - EntityType.A])
                    {
                        harvestAction.Score -= 500;

                        if (IsHarvestSpace(growAction.Location.index, out Entity currentHarvestSpace) && proteinInfo.Proteins[currentHarvestSpace.Type - EntityType.A] > proteinInfo.Proteins[harvest.Type - EntityType.A])
                        {
                            harvestAction.Score -= 500;
                        }
                    }

                    if (isHarvesting.HasValue && !isHarvesting.Value)
                    {
                        harvestAction.Score -= 500;
                    }



                    /* Just because we have harvester resources doesn't mean we should build them.  Tentacles or sporers would be better if we have too many resources.
                    if (proteinInfo.IsHarvestingHarvesterProteins)
                    {
                        harvestAction.Score -= 100;
                    }
                    */

                    harvestActions.Add(harvestAction);
                }

                if (harvestActions.Count > 0)
                {
                    double best = harvestActions.Min(m => m.Score);
                    moveActions.AddRange(harvestActions.Where(m => m.Score == best));
                }
                else
                {
                    MoveAction moveAction = MoveAction.CreateGrow(growAction.OrganId, growAction.Location, EntityType.HARVESTER, growAction.OrganRootId);
                    moveAction.Score = growAction.Score;
                    moveAction.Score += 1000;
                    moveActions.Add(moveAction);
                }
            }

            return moveActions;
        }

        public bool? IsHarvesting(int location, bool isMine)
        {
            string key = GetKey(0, isMine, location);
            if (!_harvestCache.TryGetValue(key, out bool? result))
            {
                result = null;
                if (IsHarvestSpace(location, out Entity harvestEntity))
                {
                    result = false;
                    foreach (LocationNeighbor locationNeighbor in GetLocationNeighbors(location))
                    {
                        if (GetEntity(locationNeighbor.point.index, out Entity entity) && entity.Type == EntityType.HARVESTER && entity.IsMine.HasValue && entity.IsMine == isMine && entity.OrganDirection == GetOpposingDirection(locationNeighbor.direction))
                        {
                            result = true;
                            break;
                        }
                    }
                }
                _harvestCache[key] = result;
            }
            return result;
        }

        public bool? IsHarvesting(Point2d location, bool isMine)
        {
            if (ValidateLocation(location))
            {
                return IsHarvesting(location.index, isMine);
            }
            return null;
        }

        public bool IsFull()
        {
            foreach (Entity entity in Entities)
            {
                if (entity == null)
                {
                    return false;
                }
            }
            return true;
        }

        public int[] GetHarvestProteins(bool isMine)
        {
            Entity[] harvesters = GetHarvesterEntities(isMine);
            int[] harvestedProteins = new int[4] { 0, 0, 0, 0 };
            foreach (Entity harvester in harvesters)
            {
                if (GetEntityWithDirection(harvester.Location, harvester.OrganDirection, out Entity entity) && entity.IsOpenSpace())
                {
                    harvestedProteins[entity.Type - EntityType.A]++;
                }
            }
            return harvestedProteins;
        }

        public OrganDirection GetOpposingDirection(OrganDirection direction)
        {
            switch (direction)
            {
                case OrganDirection.North:
                    return OrganDirection.South;
                case OrganDirection.South:
                    return OrganDirection.North;
                case OrganDirection.East:
                    return OrganDirection.West;
                case OrganDirection.West:
                    return OrganDirection.East;
            }
            throw new Exception("No direction given");
        }

        public bool IsHarvestExactly2spaces(Point2d location, Entity[] toHarvestEntities)
        {
            Entity[] harvestEntities = toHarvestEntities;//GetHarvestableEntities();
            foreach (Entity entity in harvestEntities)
            {
                if (location.Equals(entity.Location))
                    continue;
                double distance = Graph.GetShortestPathDistance(location.index, entity.Location.index);
                if (distance == 2)
                {
                    return true;
                }
            }
            return false;
        }

        public bool IsOpponentClose(Point2d location, bool isMine)
        {
            string key = GetKey(1, isMine, location.index);
            if (!_locationCheckCache.TryGetValue(key, out bool result))
            {
                Entity[] oppEntities = GetEntities(!isMine);
                foreach (Entity oppEntity in oppEntities)
                {
                    bool shouldContinue = false;
                    Graph.GetShortest(location.index, oppEntity.Location.index, out DistancePath distancePath);
                    double distance = distancePath.Distance;
                    if (distance <= 4)
                    {
                        //Follow the path and check if it's really open or if we are in the way.
                        foreach (ILink link in distancePath.Paths)
                        {
                            if (!IsOpponentOrOpenSpace(link.EndNodeId, isMine))
                            {
                                shouldContinue = true;
                                break;
                            }
                        }
                        if (shouldContinue)
                            continue;
                        result = true;
                        _locationCheckCache[key] = result;
                        return result;
                    }
                }
                result = false;
                _locationCheckCache[key] = result;
            }

            return result;
        }

        public bool IsOpponentOrOpenSpace(int index, bool isMine)
        {
            return !GetEntity(index, out Entity entity) || entity.IsOpenSpace() || (entity.IsMine.HasValue && entity.IsMine != isMine);
        }
        public bool IsOpponentSpace(Point2d location, OrganDirection direction, bool isMine)
        {
            return GetEntityWithDirection(location, direction, out Entity entity) && entity.IsMine != isMine;
        }

        public bool IsHarvestSpace(int location, out Entity entity)
        {
            return GetEntity(location, out entity) && entity.IsOpenSpace();
        }

        public bool IsHarvestSpace(Point2d location, OrganDirection nextDirection)
        {
            return GetEntityWithDirection(location, nextDirection, out Entity entity) && entity.IsOpenSpace();
        }


        private bool IsOpenSpace(int location)
        {
            return !GetEntity(location, out Entity entity) || entity.IsOpenSpace();
        }

        public bool IsOpenSpace(int location, bool isMine)
        {
            if (!_isOpenSpaceInitial[location] || !IsOpenSpace(location))
            {
                return false;
            }

            return !HasOpposingTentacle(location, isMine);
        }

        // Helper method to check for opposing tentacles
        private bool HasOpposingTentacle(int location, bool isMine)
        {
            string key = GetKey(2, isMine, location);
            if (!_locationCheckCache.TryGetValue(key, out bool result))
            {
                result = false;
                foreach (LocationNeighbor neighbor in GetLocationNeighbors(location))
                {
                    if (GetEntity(neighbor.point.index, out Entity entity) &&
                        entity.Type == EntityType.TENTACLE &&
                        entity.IsMine != isMine &&
                        GetOpposingDirection(entity.OrganDirection) == neighbor.direction)
                    {
                        result = true;
                        break;
                    }
                }
                _locationCheckCache[key] = result;
            }

            return result;
        }

        public bool ValidateLocation(Point2d location, OrganDirection nextDirection)
        {
            return ValidateLocation(GetNextLocation(location, nextDirection));
        }

        public bool ValidateLocation(Point2d location)
        {
            return location != null;
        }

        public bool GetEntityWithDirection(Point2d currentLocation, OrganDirection nextDirection, out Entity entity)
        {
            Point2d location = GetNextLocation(currentLocation, nextDirection);
            entity = null;
            return ValidateLocation(location) && GetEntity(location, out entity);
        }

        public Point2d GetNextLocation(Point2d currentLocation, OrganDirection nextDirection)
        {
            return _locationCache[(int)currentLocation.x + 1][(int)currentLocation.y + 1][(int)nextDirection];
        }

        private Point2d GetNextLocationInternl(int x, int y, OrganDirection nextDirection)
        {
            switch (nextDirection)
            {
                case OrganDirection.North:
                    return new Point2d(x, y - 1, GetNodeIndexInternal(x, y - 1));
                case OrganDirection.South:
                    return new Point2d(x, y + 1, GetNodeIndexInternal(x, y + 1));
                case OrganDirection.East:
                    return new Point2d(x + 1, y, GetNodeIndexInternal(x + 1, y));
                case OrganDirection.West:
                    return new Point2d(x - 1, y, GetNodeIndexInternal(x - 1, y));
                default:
                    return new Point2d(x, y, GetNodeIndexInternal(x, y));
            }
        }

        public void Harvest(bool isMine, int[] proteins)
        {
            foreach (Entity entity in GetHarvestedEntities(isMine))
            {
                proteins[entity.Type - EntityType.A]++;
            }
        }

        public Entity[] GetHarvestedEntities(bool isMine)
        {
            string key = GetKey(3, isMine, 0);
            if (!_entityCache.TryGetValue(key, out Entity[] entities))
            {
                List<Entity> entitiesList = new List<Entity>();
                Entity[] harvesters = GetHarvesterEntities(isMine);
                HashSet<int> harvestedLocations = new HashSet<int>();
                foreach (Entity harvester in harvesters)
                {
                    if (GetEntityWithDirection(harvester.Location, harvester.OrganDirection, out Entity entity) && entity.IsOpenSpace() && harvestedLocations.Add(entity.Location.index))
                    {
                        entitiesList.Add(entity);
                    }
                }
                entities = entitiesList.ToArray();
                _entityCache[key] = entities;
            }
            return entities;
        }

        public bool GetEntity(int entityIndex, out Entity entity)
        {
            entity = Entities[entityIndex];
            return entity != null;
        }

        public bool GetEntity(Point2d location, out Entity entity)
        {
            return GetEntity(GetNodeIndex(location), out entity);
        }

        public Entity[] GetHarvestableEntities()
        {
            string key = "harvestable";
            if (!_entityCache.TryGetValue(key, out Entity[]? entities))
            {
                entities = GetEntitiesList().Where(e => e.IsOpenSpace()).ToArray();
                _entityCache[key] = entities;
            }
            return entities;
        }

        public bool GetEntityByLocation(Point2d location, out Entity entity)
        {
            return GetEntity(location, out entity);
        }

        public Entity GetEntityByLocation(Point2d location)
        {
            Entity entity = null;
            GetEntity(location, out entity);
            return entity;
        }

        public Entity[] GetHarvesterEntities(bool isMine)
        {
            string key = isMine ? "harvest1" : "harvest0";
            if (!_entityCache.TryGetValue(key, out Entity[]? entities))
            {
                entities = GetEntities(isMine).Where(e => e.Type == EntityType.HARVESTER).ToArray();
                _entityCache[key] = entities;
            }
            return entities;
        }

        public Entity[] GetRootEntities(bool isMine)
        {
            string key = isMine ? "root1" : "root0";
            if (!_entityCache.TryGetValue(key, out Entity[]? entities))
            {
                Entity[] baseEntities = GetEntities(isMine);
                int j = 0;
                for (int i = 0; i < baseEntities.Length; i++)
                {
                    Entity baseEntity = baseEntities[i];
                    if (baseEntity.Type == EntityType.ROOT)
                    {
                        _tempHolder[j++] = baseEntity;
                    }
                }
                entities = new Entity[j];
                for (int i = 0; i < j; i++)
                {
                    entities[i] = _tempHolder[i];
                }
                Array.Sort(entities, (m1, m2) => m2.OrganId.CompareTo(m1.OrganId));

                //entities = GetEntities(isMine).Where(e => e.Type == EntityType.ROOT).OrderByDescending(e => e.OrganId).ToArray();
                _entityCache[key] = entities;
            }
            return entities;
        }

        public Entity[] GetSporerEntities(int organRootId, bool isMine)
        {
            string key = GetKey(4, isMine, organRootId);
            if (!_entityCache.TryGetValue(key, out Entity[]? entities))
            {
                entities = GetEntitiesByRoot(organRootId, isMine).Where(e => e.Type == EntityType.SPORER).ToArray();
                _entityCache[key] = entities;
            }
            return entities;
        }

        public Entity[] GetTentacleEntities(bool isMine)
        {
            string key = isMine ? "tentacles1" : "tentacles0";
            if (!_entityCache.TryGetValue(key, out Entity[]? entities))
            {
                entities = GetTentacleEntities().Where(e => e.IsMine == isMine).ToArray();
                _entityCache[key] = entities;
            }
            return entities;
        }

        public Entity[] GetTentacleEntities()
        {
            string key = "tentacles";
            if (!_entityCache.TryGetValue(key, out Entity[]? entities))
            {
                entities = GetEntitiesList().Where(e => e.Type == EntityType.TENTACLE).ToArray();
                _entityCache[key] = entities;
            }
            return entities;
        }

        public Entity[] GetEntitiesList()
        {
            string key = "all";
            if (!_entityCache.TryGetValue(key, out Entity[] entities))
            {
                Entity[] baseEntities = Entities;
                int j = 0;
                for (int i = 0; i < baseEntities.Length; i++)
                {
                    Entity baseEntity = baseEntities[i];
                    if (baseEntity != null)
                    {
                        _tempHolder[j++] = baseEntity;
                    }
                }
                entities = new Entity[j];
                for (int i = 0; i < j; i++)
                {
                    entities[i] = _tempHolder[i];
                }

                //entities = Entities.Where(e => e != null).Select(e => e).ToArray();
                _entityCache[key] = entities;
            }
            return entities;
        }

        public Entity[] GetEntities(bool isMine)
        {
            string key = isMine ? "e1" : "e0";
            if (!_entityCache.TryGetValue(key, out Entity[] entities))
            {
                Entity[] baseEntities = GetEntitiesList();
                int j = 0;
                for (int i = 0; i < baseEntities.Length; i++)
                {
                    Entity baseEntity = baseEntities[i];
                    if (baseEntity.IsMine.HasValue && baseEntity.IsMine == isMine)
                    {
                        _tempHolder[j++] = baseEntity;
                    }
                }
                entities = new Entity[j];
                for (int i = 0; i < j; i++)
                {
                    entities[i] = _tempHolder[i];
                }

                //entities = GetEntitiesList().Where(e => e.IsMine.HasValue && e.IsMine == isMine).ToArray();
                _entityCache[key] = entities;
            }
            return entities;
        }

        public Entity[] GetEntitiesByRoot(int organRootId, bool isMine)
        {
            string key = GetKey(5, isMine, organRootId);
            if (!_entityCache.TryGetValue(key, out Entity[] entities))
            {
                Entity[] baseEntities = GetEntities(isMine);
                int j = 0;
                for (int i = 0; i < baseEntities.Length; i++)
                {
                    Entity baseEntity = baseEntities[i];
                    if (baseEntity.OrganRootId == organRootId)
                    {
                        _tempHolder[j++] = baseEntity;
                    }
                }
                entities = new Entity[j];
                for (int i = 0; i < j; i++)
                {
                    entities[i] = _tempHolder[i];
                }
                //entities = GetEntities(isMine).Where(e => e.OrganRootId == organRootId).ToArray();
                _entityCache[key] = entities;
            }
            return entities;
        }

        public Entity[] GetEntities()
        {
            return Entities;
        }

        public int GetMyEntityCount()
        {
            if (_myEntityCount < 0)
            {
                _myEntityCount = GetEntities(true).Length;
            }

            return _myEntityCount;
        }

        public int GetOppEntityCount()
        {
            if (_oppEntityCount < 0)
            {
                _oppEntityCount = GetEntities(false).Length;
            }
            return _oppEntityCount;
        }


        public int GetInitialOpenSpacesCount()
        {
            return _initialOpenSpacesCount;
        }

        public int GetEntityLifeCount(bool isMine)
        {
            HashSet<int> locationsChecked = new HashSet<int>();
            Queue<int> locationsToCheckForInfiniteGrowth = new Queue<int>();

            foreach (Entity entity in GetEntities(!isMine))
            {
                if (locationsChecked.Add(entity.Location.index))
                {
                    //Find all locations where there is an open space.  Once it's open we can add it to the list that needs to be checked for infinite growth.
                    foreach (LocationNeighbor locationNeighbor in GetLocationNeighbors(entity.Location.index))
                    {
                        if (IsOpenSpace(locationNeighbor.point.index))
                            locationsToCheckForInfiniteGrowth.Enqueue(entity.Location.index);//Space is open so begin the infinite growth check
                    }
                }
            }

            //Infinite growth check as long as there is some space available.
            while (locationsToCheckForInfiniteGrowth.Count > 0)
            {
                foreach (LocationNeighbor locationNeighbor in GetLocationNeighbors(locationsToCheckForInfiniteGrowth.Dequeue()))
                {
                    if (locationsChecked.Add(locationNeighbor.point.index))
                    {
                        //Here we check if it's an opponent space or open since we can build a tentacle into their space, but not if there is an oppossing tentacle that would block us from expanding.
                        if (IsOpponentOrOpenSpace(locationNeighbor.point.index, !isMine) && !HasOpposingTentacle(locationNeighbor.point.index, !isMine))
                        {
                            locationsToCheckForInfiniteGrowth.Enqueue(locationNeighbor.point.index);
                        }
                    }
                }
            }

            return GetInitialOpenSpacesCount() - locationsChecked.Count;
        }

        public void SetEntities(Entity[] entities, bool isFirstTurn = false)
        {
            Array.Clear(Entities);
            foreach (Entity entity in entities)
            {
                //Entities[entity.Location.index]?.Dispose();
                Entities[entity.Location.index] = entity;
                if (entity.OrganId > 0 && GlobalOrganId <= entity.OrganId)
                {
                    GlobalOrganId = entity.OrganId + 1;
                }
            }

            UpdateBoard();

            if (isFirstTurn)
            {
                CalculatePathsAndNeighbors();
            }
        }

        public Board Clone()
        {
            Board cleanState = new Board();
            cleanState.CopyFrom(this);
            return cleanState;
        }

        public double? GetWinner()
        {
            return null;
        }

        public void Print()
        {
            for (int y = 0; y < Height; y++)
            {
                StringBuilder stringBuilder = new StringBuilder();
                for (int x = 0; x < Width; x++)
                {
                    if (!GetEntity(GetNodeIndex(x, y), out Entity entity))
                    {
                        stringBuilder.Append(' ');
                    }
                    else
                    {
                        stringBuilder.Append(GetCharacter(entity.Type, entity.IsMine));
                    }
                }
                Console.Error.WriteLine(stringBuilder.ToString());
            }
        }
        public char GetCharacter(EntityType type, bool? isMine)
        {
            bool isMineInt = isMine.HasValue && isMine.Value;
            switch (type)
            {
                case EntityType.WALL:
                    return 'X';
                case EntityType.ROOT:
                    return isMineInt ? 'R' : 'r';
                case EntityType.BASIC:
                    return isMineInt ? 'B' : 'b';
                case EntityType.TENTACLE:
                    return isMineInt ? 'T' : 't';
                case EntityType.HARVESTER:
                    return isMineInt ? 'H' : 'h';
                case EntityType.SPORER:
                    return isMineInt ? 'S' : 's';
                case EntityType.A:
                    return 'A';
                case EntityType.B:
                    return 'B';
                case EntityType.C:
                    return 'C';
                case EntityType.D:
                    return 'D';
            }
            throw new ArgumentException($"Type: {type} not supported");
        }

        public void UpdateBoard()
        {
            _myEntityCount = -1;
            _oppEntityCount = -1;
            _moveActionCache.Clear();
            _entityCache.Clear();
            _locationCheckCache.Clear();
            _harvestCache.Clear();
        }

        public readonly struct LocationNeighbor
        {
            public readonly Point2d point;
            public readonly OrganDirection direction;

            public LocationNeighbor(int x, int y, int index, OrganDirection direction)
            {
                this.point = new Point2d(x, y, index);
                this.direction = direction;
            }
        }

        private string GetKey(int i, bool isMine, int location)
        {
            return _keyCache[i, isMine ? 1 : 0, location];
        }

        public void InitializeBoard()
        {
            _keyCache = new string[20, 2, Width * Height];
            _locationIndexCache = new int[Width][];
            for (int x = 0; x < Width; x++)
            {
                _locationIndexCache[x] = new int[Height];
                for (int y = 0; y < Height; y++)
                {
                    int index = GetNodeIndexInternal(x, y);
                    _locationIndexCache[x][y] = index;
                    for (int i = 0; i < 20; i++)
                    {
                        _keyCache[i, 0, index] = $"{i}_0_{index}";
                        _keyCache[i, 1, index] = $"{i}_1_{index}";
                    }
                }
            }


            _locationCache = new Point2d[Width + 2][][];

            for (int x = 0; x < Width + 2; x++)
            {
                _locationCache[x] = new Point2d[Height + 2][];
                for (int y = 0; y < Height + 2; y++)
                {
                    _locationCache[x][y] = new Point2d[4];
                    for (int z = 0; z < 4; z++)
                    {
                        Point2d nextLocation = GetNextLocationInternl(x - 1, y - 1, PossibleDirections[z]);
                        if (nextLocation.x > -1 && nextLocation.y > -1 && nextLocation.x < Width && nextLocation.y < Height)
                        {
                            _locationCache[x][y][z] = nextLocation;
                        }
                        else
                        {
                            _locationCache[x][y][z] = null;
                        }
                    }
                }
            }
        }

        private void CalculatePathsAndNeighbors()
        {
            _isOpenSpaceInitial = new bool[Width * Height];
            Array.Fill(_isOpenSpaceInitial, true);
            _initialOpenSpacesCount = Width * Height;
            _locationNeighbors = new LocationNeighbor[Width * Height][];
            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
                {
                    int index = GetNodeIndex(x, y);
                    Node node = new Node(index);
                    Graph.AddNode(node);

                    List<LocationNeighbor> tempNeighbors = new List<LocationNeighbor>(4);
                    for (int z = 0; z < 4; z++)
                    {
                        Point2d location = GetNextLocation(new Point2d(x, y, GetNodeIndex(x, y)), PossibleDirections[z]);
                        if (ValidateLocation(location))
                        {
                            if (IsOpenSpace(location.index) || (GetEntity(location.index, out Entity entity) && entity.Type != EntityType.WALL))//The start of the map always includes 2 root entities, but we only want to handle walls
                            {
                                int nextIndex = GetNodeIndex(location);
                                node.AddLink(new Link(node, new Node(nextIndex), 1));
                                tempNeighbors.Add(new LocationNeighbor(location.x, location.y, nextIndex, PossibleDirections[z]));
                            }
                            else
                            {
                                if (_isOpenSpaceInitial[location.index])
                                    _initialOpenSpacesCount--;
                                _isOpenSpaceInitial[location.index] = false;

                            }
                        }
                    }
                    _locationNeighbors[index] = tempNeighbors.ToArray();
                }
            }
            Graph.CalculateShortestPaths();
        }

        private int GetNodeIndexInternal(int x, int y)
        {
            return x * Height + y;
        }


        public LocationNeighbor[] GetLocationNeighbors(int nodeIndex)
        {
            return _locationNeighbors[nodeIndex];
        }

        public LocationNeighbor[] GetLocationNeighbors(Point2d location)
        {
            return _locationNeighbors[location.index];
        }

        public IEnumerable<LocationNeighbor> GetOpenSpaceLocationNeighbor(Point2d location, bool isMine)
        {
            for (int i = 0; i < _locationNeighbors[location.index].Length; i++)
            {
                LocationNeighbor locationNeighbor = _locationNeighbors[location.index][i];
                if (IsOpenSpace(locationNeighbor.point.index, isMine))
                {
                    yield return locationNeighbor;
                }
            }
        }

        public int GetNodeIndex(int x, int y)
        {
            return _locationIndexCache[x][y];
        }

        public int GetNodeIndex(Point2d location)
        {
            return location.index;
        }
    }
}
