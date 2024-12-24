using Algorithms.Graph;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GameSolution.Entities
{
    public class Board
    {
        public int Width { get; private set; }
        public int Height { get; private set; }
        private Entity[] Entities { get; set; }
        public int GlobalOrganId { get; set; } = -1;

        public Graph Graph { get; set; }

        public static OrganDirection[] PossibleDirections = new OrganDirection[] { OrganDirection.North, OrganDirection.South, OrganDirection.East, OrganDirection.West };

        public Board(int width, int height)
        {
            Width = width;
            Height = height;
            Entities = new Entity[Width * Height];
            Graph = new Graph();
            InitializeBoard();
            UpdateBoard();
        }

        public Board(Board board)
        {
            Width = board.Width;
            Height = board.Height;
            Entities = board.Entities.Select(e => e == null ? null : e.Clone()).ToArray();
            GlobalOrganId = board.GlobalOrganId;
            Graph = board.Graph;
            _entityCache = board._entityCache;
            _moveActionCache = board._moveActionCache;
            _myEntityCount = board._myEntityCount;
            _oppEntityCount = board._oppEntityCount;
            _locationCache = board._locationCache;
            _locationIndexCache = board._locationIndexCache;
            UpdateBoard();
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
            var tentacles = GetTentacleEntities();

            List<Entity> deadEntities = new List<Entity>();
            foreach (Entity tentacle in tentacles)
            {
                if (GetEntityWithDirection(tentacle.Location, tentacle.OrganDirection, out Entity entity) && entity.IsMine.HasValue && entity.IsMine != tentacle.IsMine)
                {
                    deadEntities.Add(entity);
                }
            }
            foreach (Entity deadEntity in deadEntities)
            {
                if (deadEntity.Type == EntityType.ROOT)
                {
                    var deathToRoot = GetEntitiesList().Where(e => e.OrganRootId == deadEntity.OrganRootId);
                    foreach (Entity kill in deathToRoot)
                    {
                        Entities[GetNodeIndex(kill.Location)] = null;
                    }
                }
                else
                {
                    var deathToChildren = GetEntitiesList().Where(e => e.OrganParentId == deadEntity.OrganId);
                    foreach (Entity kill in deathToChildren)
                    {
                        Entities[GetNodeIndex(kill.Location)] = null;
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
                    var collisionActions = oppMove.Actions.Where(a => (a.Type == MoveType.GROW || a.Type == MoveType.SPORE) && a.Location.Equals(action.Location));
                    if (collisionActions.Count() > 0)
                    {
                        Entities[GetNodeIndex(action.Location)] = new Entity(action.Location, EntityType.WALL, null, 0, 0, 0, OrganDirection.None);
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
                    Entity growEntity = Entities[GetNodeIndex(action.Location)];
                    if (growEntity == null || growEntity.IsOpenSpace())
                    {
                        Entities[GetNodeIndex(action.Location)] = new Entity(action.Location, action.EntityType, isMine, GlobalOrganId++, action.OrganId, action.OrganRootId, action.OrganDirection);
                    }
                    break;
                case MoveType.SPORE:
                    Entity sporeEntity = Entities[GetNodeIndex(action.Location)];
                    if (sporeEntity == null || sporeEntity.IsOpenSpace())
                    {
                        var organId = GlobalOrganId++;
                        Entities[GetNodeIndex(action.Location)] = new Entity(action.Location, action.EntityType, isMine, organId, organId, organId, action.OrganDirection);
                    }
                    break;
            }
        }

        public List<Move> GetMoves(int[] proteins, bool isMine)
        {
            var moves = new List<Move>();

            var rootEntities = GetRootEntities(isMine);
            var organismCount = rootEntities.Count();
            MoveAction[][] organismToMoveActions = new MoveAction[organismCount][];
            List<bool> hasProteins = proteins.Select(p => p > 0).ToList();
            bool hasHarvestProteins = hasProteins[2] && hasProteins[3];
            bool hasBasicProteins = hasProteins[0];
            bool hasTentacleProteins = hasProteins[1] && hasProteins[2];
            bool hasSporerProteins = hasProteins[1] && hasProteins[3];
            bool hasRootProteins = hasProteins.All(m => m);
            List<bool> hasManyProteins = proteins.Select(p => p > 10).ToList();
            bool hasManyTentacleProteins = hasManyProteins[1] && hasManyProteins[2];
            bool hasManySporerProteins = hasManyProteins[1] && hasManyProteins[3];
            bool hasAtLeastTwoMany = hasManyProteins.Count(value => value) > 1;

            int i = 0;
            int maxOrgans = 3;//Put a limit on the number of organs we calculate moves for as this is exploding the action space
            var limitedRootEntities = rootEntities;
            if (organismCount > maxOrgans)
            {
                rootEntities.Sort((e1, e2) => e2.OrganId.CompareTo(e1.OrganId));
                limitedRootEntities = rootEntities.Take(maxOrgans).ToList();
            }
            foreach (Entity root in limitedRootEntities)
            {
                var moveActions = new List<MoveAction>
                {
                    MoveAction.CreateWait()
                };

                bool canHarvest = false;
                if (hasHarvestProteins)
                {
                    var harvestActions = GetHarvestMoveActions(root.OrganRootId, isMine);
                    moveActions.AddRange(harvestActions);
                    canHarvest = harvestActions.Count > 0;
                }

                if (!canHarvest)
                {
                    if (hasBasicProteins && !hasManyTentacleProteins)
                    {
                        moveActions.AddRange(GetGrowBasicMoveActions(root.OrganRootId, isMine));
                    }
                }

                if (hasTentacleProteins)
                {
                    moveActions.AddRange(GetTentacleMoveActions(root.OrganRootId, isMine, moveActions.Count > 1));
                }

                if ((!canHarvest || hasManySporerProteins) && hasSporerProteins)
                {
                    moveActions.AddRange(GetSporerMoveActions(root.OrganRootId, isMine));
                }

                if (hasRootProteins)
                {
                    moveActions.AddRange(GetSporeMoveActions(root.OrganRootId, isMine));
                }


                organismToMoveActions[i] = moveActions.ToArray();
                i++;
            }
            for (; i < organismCount; i++)
            {
                organismToMoveActions[i] = new MoveAction[]
                {
                    MoveAction.CreateWait()
                };
            }


            if (organismCount > 1)
            {
                bool hasSufficientProteins = (proteins[0] > maxOrgans || proteins[0] == 0) && (proteins[1] > maxOrgans || proteins[1] == 0) && (proteins[2] > maxOrgans || proteins[2] == 0) && (proteins[3] > maxOrgans || proteins[3] == 0);
                var theMoves = PrunedCartesianProduct(organismToMoveActions, hasSufficientProteins, proteins);
                moves = theMoves.ToList();
            }
            else
            {
                foreach (MoveAction action in organismToMoveActions[0])
                {
                    var move = new Move();
                    move.SetActions(new MoveAction[] { action });
                    moves.Add(move);
                }
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

        //slower
        public static IEnumerable<MoveAction[]> CartesianProductRecursive(MoveAction[][] arrays)
        {
            IEnumerable<MoveAction[]> Helper(int depth)
            {
                if (depth == arrays.Length)
                {
                    yield return new MoveAction[0];
                    yield break;
                }

                foreach (var item in arrays[depth])
                {
                    foreach (var product in Helper(depth + 1))
                    {
                        yield return new[] { item }.Concat(product).ToArray();
                    }
                }
            }

            return Helper(0);
        }

        //slower
        public static IEnumerable<MoveAction[]> CartesianProductLinq(MoveAction[][] arrays)
        {
            return arrays.Aggregate(
              new List<MoveAction[]> { new MoveAction[0] }, // Seed
              (acc, array) => acc.SelectMany(
                 product => array.Select(item => product.Concat(new[] { item }).ToArray())
              ).ToList()
            );
        }

        //should be faster with pruning
        public IEnumerable<Move> PrunedCartesianProduct(MoveAction[][] sequences, bool hasSufficientProteins, int[] proteins)
        {
            if (sequences == null || sequences.Length == 0)
                yield break;

            var sequenceArrays = sequences.Select(s => s.ToArray()).ToArray();
            var dimensions = sequenceArrays.Length;

            // Indexes to keep track of positions in each sequence
            var indices = new int[dimensions];
            var partialCosts = new int[dimensions][];

            // Initialize partialCosts for each dimension and protein
            for (int i = 0; i < dimensions; i++)
            {
                partialCosts[i] = new int[proteins.Length];
            }

            while (true)
            {
                // Build the current combination
                var currentCombination = new MoveAction[dimensions];
                for (int i = 0; i < dimensions; i++)
                {
                    currentCombination[i] = sequenceArrays[i][indices[i]];
                }

                // Update partial costs for each protein based on current combination
                for (int i = 0; i < dimensions; i++)
                {
                    var cost = currentCombination[i].GetCost();
                    for (int j = 0; j < proteins.Length; j++)
                    {
                        partialCosts[i][j] += cost[j];
                    }
                }

                // Validate the combination
                if (ValidateCollision(currentCombination) &&
                    (hasSufficientProteins || ValidatePartialCost(proteins, partialCosts)))
                {
                    var move = new Move();
                    move.SetActions(currentCombination);
                    yield return move;
                }

                // Increment indices iteratively
                int position = dimensions - 1;
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

        private bool ValidatePartialCost(int[] proteins, int[][] partialCosts)
        {
            // Iterate over each dimension's costs
            for (int i = 0; i < partialCosts.Length; i++)
            {
                for (int j = 0; j < proteins.Length; j++)
                {
                    if (partialCosts[i][j] > proteins[j]) // Compare individual protein costs across all dimensions
                    {
                        return false; // Prune if any protein's cost exceeds its limit
                    }
                }
            }
            return true;
        }


        public IEnumerable<Move> CartesianProduct(MoveAction[][] sequences, bool hasSufficientProteins, int[] proteins)
        {
            // Convert sequences to arrays for efficient access
            var arrays = sequences.Select(s => s.ToArray()).ToArray();

            // Early exit if no input sequences
            if (arrays.Length == 0)
                yield break;

            // Stack to hold indices
            var indices = new int[arrays.Length];

            while (true)
            {
                // Yield the current combination
                MoveAction[] result = indices.Select((index, i) => arrays[i][index]).ToArray();
                var move = new Move();
                move.SetActions(result);
                if (ValidateCollision(result) && (hasSufficientProteins || ValidateCost(proteins, move)))
                {
                    yield return move;
                }

                // Increment the indices from the rightmost sequence
                int position = arrays.Length - 1;
                while (position >= 0)
                {
                    indices[position]++;
                    if (indices[position] < arrays[position].Length)
                        break;

                    // Reset this position and carry over to the next
                    indices[position] = 0;
                    position--;
                }

                // Break if we've exhausted all combinations
                if (position < 0)
                    yield break;
            }
        }

        public bool ValidateCollision(MoveAction[] moves)
        {
            HashSet<int> locations = new HashSet<int>();
            foreach (MoveAction action in moves)
            {
                if (ValidateLocation(action.Location))
                {
                    int location = GetNodeIndex(action.Location);
                    if (locations.Contains(location))
                    {
                        return false;
                    }
                    else
                    {
                        locations.Add(location);
                    }
                }
            }
            return true;
        }

        public List<MoveAction> GetSporeMoveActions(int organRootId, bool isMine)
        {
            List<MoveAction> moveActions = new List<MoveAction>();

            IEnumerable<Entity> sporers = GetSporerEntities(organRootId, isMine);

            foreach (Entity sporer in sporers)
            {
                var location = sporer.Location;
                int distance = 0;
                while (true)
                {
                    distance++;
                    if (IsOpenSpace(location, sporer.OrganDirection, isMine))
                    {
                        location = GetNextLocation(location, sporer.OrganDirection);
                        if (distance <= 3)
                        {
                            continue;
                        }
                        if (!IsHarvesting(location, isMine) && IsHarvestExactly2paces(location))
                        {
                            moveActions.Add(MoveAction.CreateSpore(sporer.OrganId, location));
                        }
                    }
                    else
                    {
                        break;
                    }
                }
            }

            return moveActions;
        }

        public List<MoveAction> GetSporerMoveActions(int organRootId, bool isMine)
        {
            List<MoveAction> moveActions = new List<MoveAction>();

            List<MoveAction> growMoveActions = GetGrowBasicMoveActions(organRootId, isMine);

            foreach (MoveAction growAction in growMoveActions)
            {
                foreach (OrganDirection sporerDirection in PossibleDirections)
                {
                    var location = growAction.Location;
                    bool isOpen = true;
                    for (int i = 0; i < 4; i++)
                    {
                        if (!IsOpenSpace(location, sporerDirection, isMine))
                        {
                            isOpen = false;
                            break;
                        }
                        location = GetNextLocation(location, sporerDirection);
                    }
                    if (isOpen)
                    {
                        moveActions.Add(MoveAction.CreateGrow(growAction.OrganId, growAction.Location, EntityType.SPORER, growAction.OrganRootId, sporerDirection));
                    }
                }
            }

            return moveActions;
        }

        public List<MoveAction> GetTentacleMoveActions(int organRootId, bool isMine, bool hasActions)
        {
            List<MoveAction> moveActions = new List<MoveAction>();

            List<MoveAction> growMoveActions = GetGrowBasicMoveActions(organRootId, isMine);

            foreach (MoveAction growAction in growMoveActions)
            {
                foreach (OrganDirection tentacleDirection in PossibleDirections)
                {
                    if (IsOpponentOrEmptySpace(growAction.Location, tentacleDirection, isMine))
                    {
                        if (!hasActions || IsOpponentWithin3Spaces(growAction.Location, isMine))
                        {
                            moveActions.Add(MoveAction.CreateGrow(growAction.OrganId, growAction.Location, EntityType.TENTACLE, growAction.OrganRootId, tentacleDirection));
                        }
                    }
                }
            }

            return moveActions;
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

        private Dictionary<string, List<MoveAction>> _moveActionCache = new Dictionary<string, List<MoveAction>>();
        public List<MoveAction> GetGrowBasicMoveActions(int organRootId, bool isMine)
        {
            var key = organRootId.ToString() + isMine.ToString();
            if (!_moveActionCache.TryGetValue(key, out List<MoveAction> moveActions))
            {
                moveActions = new List<MoveAction>();
                var potentialMoveActions = new List<MoveAction>();
                var entities = GetEntitiesByRoot(organRootId, isMine);
                foreach (Entity entity in entities)
                {
                    foreach (OrganDirection direction in PossibleDirections)
                    {
                        if (IsOpenSpace(entity.Location, direction, isMine))
                        {
                            var moveAction = MoveAction.CreateGrow(entity.OrganId, GetNextLocation(entity.Location, direction), EntityType.BASIC, entity.OrganRootId);
                            if (!IsHarvesting(GetNextLocation(entity.Location, direction), isMine))
                            {
                                moveActions.Add(moveAction);
                            }
                            else
                            {
                                potentialMoveActions.Add(moveAction);
                            }
                        }
                    }
                }
                if (moveActions.Count == 0)
                {
                    moveActions = potentialMoveActions;
                }
                _moveActionCache[key] = moveActions;
            }

            int maxMoves = 5;
            if (moveActions.Count > maxMoves)
            {
                var oppRootEntities = GetEntitiesByRoot(organRootId, !isMine);
                foreach (Entity oppRoot in oppRootEntities)
                {
                    moveActions.Sort((m1, m2) => Graph.GetShortestPathDistance(GetNodeIndex(oppRoot.Location), GetNodeIndex(m1.Location)).CompareTo(Graph.GetShortestPathDistance(GetNodeIndex(oppRoot.Location), GetNodeIndex(m2.Location))));
                    moveActions = moveActions.Take(maxMoves).ToList();
                }
            }

            return moveActions;
        }

        public List<MoveAction> GetHarvestMoveActions(int organRootId, bool isMine)
        {
            List<MoveAction> moveActions = new List<MoveAction>();
            List<MoveAction> growMoveActions = GetGrowBasicMoveActions(organRootId, isMine);

            foreach (MoveAction growAction in growMoveActions)
            {
                foreach (OrganDirection direction in PossibleDirections)
                {
                    if (IsHarvestSpace(growAction.Location, direction) && !IsHarvesting(GetNextLocation(growAction.Location, direction), isMine))
                    {
                        var harvestAction = MoveAction.CreateGrow(growAction.OrganId, growAction.Location, EntityType.HARVESTER, growAction.OrganRootId, direction);
                        //Prioritize A harvests
                        if (GetEntityWithDirection(growAction.Location, direction, out Entity entity) && entity.Type == EntityType.A)
                        {
                            return new List<MoveAction>()
                            {
                                harvestAction
                            };
                        }
                        moveActions.Add(harvestAction);
                    }
                }
            }

            return moveActions;
        }

        public bool IsHarvesting(Point2d location, bool isMine)
        {
            if (ValidateLocation(location) && GetEntity(location, out Entity harvestSpace) && harvestSpace.IsOpenSpace())
            {
                foreach (OrganDirection direction in PossibleDirections)
                {
                    var locationCheck = GetNextLocation(location, direction);
                    if (ValidateLocation(locationCheck) && GetEntity(locationCheck, out Entity entity) && entity.Type == EntityType.HARVESTER && entity.IsMine.HasValue && entity.IsMine == isMine && entity.OrganDirection == GetOpposingDirection(direction))
                    {
                        return true;
                    }
                }
            }
            return false;
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

        public bool IsHarvestExactly2paces(Point2d location)
        {
            var harvestEntities = GetHarvestableEntities();
            foreach (var entity in harvestEntities)
            {
                if (location.Equals(entity.Location))
                    return true;
                double distance = Graph.GetShortestPathDistance(GetNodeIndex(location), GetNodeIndex(entity.Location));
                if (distance == 2)
                {
                    return true;
                }
            }
            return false;
        }

        public bool IsOpponentWithin3Spaces(Point2d location, bool isMine)
        {
            var oppEntities = GetEntities(!isMine);
            foreach (var oppEntity in oppEntities)
            {
                double distance = Graph.GetShortestPathDistance(GetNodeIndex(location), GetNodeIndex(oppEntity.Location));
                if (distance <= 3)
                    return true;
            }
            return false;
        }

        public bool IsOpponentOrEmptySpace(Point2d location, OrganDirection direction, bool isMine)
        {
            return ValidateLocation(location, direction) && (!GetEntityWithDirection(location, direction, out Entity entity) || (entity.IsMine.HasValue && entity.IsMine != isMine));
        }
        public bool IsOpponentSpace(Point2d location, OrganDirection direction, bool isMine)
        {
            return GetEntityWithDirection(location, direction, out Entity entity) && entity.IsMine != isMine;
        }
        public bool IsHarvestSpace(Point2d location, OrganDirection nextDirection)
        {
            return GetEntityWithDirection(location, nextDirection, out Entity entity) && entity.IsOpenSpace();
        }

        public bool IsOpenSpace(Point2d location, OrganDirection nextDirection, bool isMine)
        {
            var locationToCheck = GetNextLocation(location, nextDirection);
            if (ValidateLocation(locationToCheck))
            {
                if (!GetEntityByLocation(locationToCheck, out Entity entity) || entity.IsOpenSpace())
                {
                    var tentacles = GetTentacleEntities(!isMine);
                    if (tentacles.Count > 0)
                    {
                        foreach (OrganDirection direction in PossibleDirections)
                        {
                            if (GetEntityWithDirection(locationToCheck, direction, out Entity checkForTentacleEntity))
                            {
                                if (checkForTentacleEntity.Type == EntityType.TENTACLE && GetOpposingDirection(checkForTentacleEntity.OrganDirection) == direction && checkForTentacleEntity.IsMine != isMine)
                                {
                                    return false;
                                }
                            }
                        }
                    }
                    return true;
                }
            }

            return false;
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
            var location = GetNextLocation(currentLocation, nextDirection);
            entity = null;
            return ValidateLocation(location) && GetEntity(location, out entity);
        }

        public Point2d GetNextLocation(Point2d currentLocation, OrganDirection nextDirection)
        {
            return _locationCache[(int)currentLocation.x + 1][(int)currentLocation.y + 1][(int)nextDirection];
        }

        private Point2d GetNextLocationInternl(Point2d currentLocation, OrganDirection nextDirection)
        {
            switch (nextDirection)
            {
                case OrganDirection.North:
                    return new Point2d(currentLocation.x, currentLocation.y - 1);
                case OrganDirection.South:
                    return new Point2d(currentLocation.x, currentLocation.y + 1);
                case OrganDirection.East:
                    return new Point2d(currentLocation.x + 1, currentLocation.y);
                case OrganDirection.West:
                    return new Point2d(currentLocation.x - 1, currentLocation.y);
                default:
                    return currentLocation;
            }
        }

        public void Harvest(bool isMine, int[] proteins)
        {
            var harvesters = GetEntities(isMine).Where(e => e.Type == EntityType.HARVESTER);
            List<int> harvestedLocations = new List<int>();
            foreach (Entity harvester in harvesters)
            {
                if (GetEntityWithDirection(harvester.Location, harvester.OrganDirection, out Entity entity) && entity.IsOpenSpace() && !harvestedLocations.Contains(GetNodeIndex(entity.Location)))
                {
                    proteins[entity.Type - EntityType.A]++;
                    harvestedLocations.Add(GetNodeIndex(entity.Location));
                }
            }
        }

        public bool GetEntity(int entityIndex, out Entity entity)
        {
            entity = Entities[entityIndex];
            if (entity == null)
                return false;
            return true;
        }

        public bool GetEntity(Point2d location, out Entity entity)
        {
            return GetEntity(GetNodeIndex(location), out entity);
        }

        public List<Entity> GetHarvestableEntities()
        {
            return GetEntitiesList().Where(e => e.IsOpenSpace()).ToList();
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

        public List<Entity> GetRootEntities(bool isMine)
        {
            var key = "root" + isMine.ToString();
            if (!_entityCache.TryGetValue(key, out var entities))
            {
                entities = GetEntities(isMine).Where(e => e.Type == EntityType.ROOT).ToList();
                _entityCache[key] = entities;
            }
            return entities;
        }

        public List<Entity> GetSporerEntities(int organRootId, bool isMine)
        {
            var key = "sporer" + isMine.ToString();
            if (!_entityCache.TryGetValue(key, out var entities))
            {
                entities = GetEntitiesByRoot(organRootId, isMine).Where(e => e.Type == EntityType.SPORER).ToList();
                _entityCache[key] = entities;
            }
            return entities;
        }

        public List<Entity> GetTentacleEntities(bool isMine)
        {
            var key = "tentacles" + isMine.ToString();
            if (!_entityCache.TryGetValue(key, out var entities))
            {
                entities = GetTentacleEntities().Where(e => e.IsMine == isMine).ToList();
                _entityCache[key] = entities;
            }
            return entities;
        }

        public List<Entity> GetTentacleEntities()
        {
            var key = "tentacles";
            if (!_entityCache.TryGetValue(key, out var entities))
            {
                entities = GetEntitiesList().Where(e => e.Type == EntityType.TENTACLE).ToList();
                _entityCache[key] = entities;
            }
            return entities;
        }

        public List<Entity> GetEntitiesList()
        {
            var key = "all";
            if (!_entityCache.TryGetValue(key, out List<Entity> entities))
            {
                entities = Entities.Where(e => e != null).ToList();
                _entityCache[key] = entities;
            }
            return entities;
        }


        private Dictionary<string, List<Entity>> _entityCache = new Dictionary<string, List<Entity>>();
        public List<Entity> GetEntities(bool isMine)
        {
            var key = isMine.ToString();
            if (!_entityCache.TryGetValue(key, out List<Entity> entities))
            {
                entities = GetEntitiesList().Where(e => e.IsMine.HasValue && e.IsMine == isMine).ToList();
                _entityCache[key] = entities;
            }
            return entities;
        }

        public List<Entity> GetEntitiesByRoot(int organRootId, bool isMine)
        {
            var key = organRootId.ToString() + isMine.ToString();
            if (!_entityCache.TryGetValue(key, out List<Entity> entities))
            {
                entities = GetEntities(isMine).Where(e => e.OrganRootId == organRootId).ToList();
                _entityCache[key] = entities;
            }
            return entities;
        }

        public Entity[] GetEntities()
        {
            return Entities;
        }

        private int _myEntityCount = -1;
        public int GetMyEntityCount()
        {
            if (_myEntityCount < 0)
            {
                _myEntityCount = GetEntities(true).Count;
            }

            return _myEntityCount;
        }

        private int _oppEntityCount = -1;
        public int GetOppEntityCount()
        {
            if (_oppEntityCount < 0)
            {
                _oppEntityCount = GetEntities(false).Count;
            }
            return _oppEntityCount;
        }

        public void SetEntities(IList<Entity> entities)
        {
            UpdateBoard();
            Array.Clear(Entities);
            foreach (Entity entity in entities)
            {
                Entities[GetNodeIndex(entity.Location)] = entity;
                if (entity.OrganId > 0 && GlobalOrganId <= entity.OrganId)
                {
                    GlobalOrganId = entity.OrganId + 1;
                }
            }
        }

        public Board Clone()
        {
            return new Board(this);
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
                        stringBuilder.Append(" ");
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
            _moveActionCache = new Dictionary<string, List<MoveAction>>();
            _entityCache = new Dictionary<string, List<Entity>>();
        }

        public Point2d[][][] _locationCache;
        public void InitializeBoard()
        {
            _locationCache = new Point2d[Width + 2][][];

            for (int x = 0; x < Width + 2; x++)
            {
                _locationCache[x] = new Point2d[Height + 2][];
                for (int y = 0; y < Height + 2; y++)
                {
                    _locationCache[x][y] = new Point2d[4];
                    for (int z = 0; z < 4; z++)
                    {
                        var nextLocation = GetNextLocationInternl(new Point2d(x - 1, y - 1), PossibleDirections[z]);
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

            _locationIndexCache = new int[Width][];
            for (int x = 0; x < Width; x++)
            {
                _locationIndexCache[x] = new int[Height];
                for (int y = 0; y < Height; y++)
                {
                    _locationIndexCache[x][y] = x * Height + y;
                }
            }

            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
                {
                    var node = new Node(GetNodeIndex(new Point2d(x, y)));
                    Graph.AddNode(node);

                    for (int z = 0; z < 4; z++)
                    {
                        var location = GetNextLocation(new Point2d(x, y), PossibleDirections[z]);
                        if (location != null)
                        {
                            node.AddLink(new Link(node, new Node(GetNodeIndex(location)), 1));
                        }
                    }
                }
            }
            Graph.CalculateShortestPaths();
        }

        private int[][] _locationIndexCache = null;

        public int GetNodeIndex(int x, int y)
        {
            return _locationIndexCache[x][y];
        }

        public int GetNodeIndex(Point2d location)
        {
            return GetNodeIndex(location.x, location.y);
        }
    }
}
