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
            _locationNeighbors = board._locationNeighbors;
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

        //15, 16, 27 actions max
        int[] _maxActionsPerOrganism = new int[3] { 10, 3, 2 };
        public List<Move> GetMoves(int[] proteins, bool isMine, bool debug = false)
        {
            var moves = new List<Move>();

            var rootEntities = GetRootEntities(isMine);
            var organismCount = rootEntities.Count();
            MoveAction[][] organismToMoveActions = new MoveAction[organismCount][];
            bool[] organismHasMoves = new bool[organismCount];
            List<bool> hasProteins = proteins.Select(p => p > 0).ToList();

            bool hasRootProteins = hasProteins.All(m => m);
            List<bool> hasManyProteins = proteins.Select(p => p > 10).ToList();
            bool hasManyRootProteins = hasManyProteins.All(m => m);

            int i = 0;
            int maxOrgans = 3;//Put a limit on the number of organs we calculate moves for as this is exploding the action space



            var limitedRootEntities = rootEntities;
            if (organismCount > maxOrgans)
            {
                rootEntities.Sort((e1, e2) => e2.OrganId.CompareTo(e1.OrganId));
                limitedRootEntities = rootEntities.Take(maxOrgans).ToList();
            }
            int selectedOrgans = limitedRootEntities.Count;
            bool hasSufficientProteins = (proteins[0] >= selectedOrgans || proteins[0] == 0) && (proteins[1] >= selectedOrgans || proteins[1] == 0) && (proteins[2] >= selectedOrgans || proteins[2] == 0) && (proteins[3] >= selectedOrgans || proteins[3] == 0);//Checks for 0 because if we don't have any of that protein then we won't have actions of that type.

            HashSet<int> locationsTaken = new HashSet<int>();

            foreach (Entity root in limitedRootEntities)
            {
                var moveActions = new List<MoveAction>();

                if (hasRootProteins && (organismCount < 2 || hasManyRootProteins))
                {
                    AddSporeMoveActions(moveActions, root.OrganRootId, isMine);
                    if (debug)
                        Console.Error.WriteLine($"Spore: {root.OrganRootId}: " + string.Join('\n', moveActions));
                    organismHasMoves[i] = true;
                }

                if (moveActions.Count <= 0)
                {
                    AddGrowMoveActions(moveActions, root.OrganRootId, isMine, hasProteins, hasManyProteins, locationsTaken, debug);

                    if (moveActions.Count > 0)
                    {
                        moveActions.Sort((m1, m2) => m1.Score.CompareTo(m2.Score));
                        moveActions = moveActions.Take(_maxActionsPerOrganism[limitedRootEntities.Count - 1]).ToList();
                        if (moveActions[0].Score < 0)
                        {
                            moveActions = moveActions.Where(m => m.Score < 0).ToList();
                            organismHasMoves[i] = true;
                        }
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
                var theMoves = PrunedCartesianProduct(organismToMoveActions, hasSufficientProteins, proteins);
                moves = theMoves.ToList();
                if (debug)
                    Console.Error.WriteLine($"Final set: {moves.Count}");
            }
            else if (organismCount == 1)
            {
                foreach (MoveAction action in organismToMoveActions[0])
                {
                    var move = new Move();
                    move.SetActions(new MoveAction[] { action });
                    moves.Add(move);
                }
            }
            else
            {
                //Since we don't check game end conditions until after a move occurs, when there is an attack that kills the last root entity then we can end up in this situation so just give a wait action and do nothing.  The game is over.
                var move = new Move();
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

            var sequenceArrays = sequences;
            var dimensions = sequenceArrays.Length;

            // Indexes to keep track of positions in each sequence
            var indices = new int[dimensions];

            // Pre-allocate arrays for partial costs and collisions
            var partialCosts = new int[dimensions][];
            var partialCollision = new HashSet<int>[dimensions];
            for (int i = 0; i < dimensions; i++)
            {
                partialCosts[i] = new int[4]; // Assuming a constant size for protein cost array
                partialCollision[i] = new HashSet<int>();
            }
            var initialCost = new int[4] { 0, 0, 0, 0 };
            var initialCollision = new HashSet<int>();

            var currentCombination = new MoveAction[dimensions];
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
                        var cost = currentCombination[i].GetCost();
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
                    var move = new Move();
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


        public List<MoveAction> AddSporeMoveActions(List<MoveAction> moveActions, int organRootId, bool isMine)
        {
            IEnumerable<Entity> sporers = GetSporerEntities(organRootId, isMine);

            foreach (Entity sporer in sporers)
            {
                var location = sporer.Location;
                int distance = 0;
                while (true)
                {
                    distance++;
                    location = GetNextLocation(location, sporer.OrganDirection);
                    if (ValidateLocation(location) && IsOpenSpace(location.index, isMine))
                    {
                        if (distance <= 3)
                        {
                            continue;
                        }
                        bool? isHarvesting = IsHarvesting(location, isMine);
                        if (!(isHarvesting.HasValue && isHarvesting.Value) && IsHarvestExactly2paces(location))
                        {
                            moveActions.Add(MoveAction.CreateSpore(sporer.OrganId, location));
                        }
                    }
                    else
                    {
                        break;
                    }

                    if (moveActions.Count > 2)
                    {
                        break;
                    }
                }
            }

            return moveActions;
        }

        public List<MoveAction> AddGrowMoveActions(List<MoveAction> moveActions, int organRootId, bool isMine, List<bool> hasProteins, List<bool> hasManyProteins, HashSet<int> locationsTaken, bool debug = false)
        {
            bool hasHarvestProteins = hasProteins[2] && hasProteins[3];
            bool hasBasicProteins = hasProteins[0];
            bool hasTentacleProteins = hasProteins[1] && hasProteins[2];
            bool hasSporerProteins = hasProteins[1] && hasProteins[3];
            bool hasManyTentacleProteins = hasManyProteins[1] && hasManyProteins[2];
            bool hasManySporerProteins = hasManyProteins[1] && hasManyProteins[3];
            bool hasAtLeastTwoMany = hasManyProteins.Count(value => value) > 1;

            List<MoveAction> growMoveActions = GetGrowMoveActions(organRootId, isMine, locationsTaken);

            if (debug)
            {

                Console.Error.WriteLine($"All: {organRootId}: " + string.Join('\n', growMoveActions));
            }

            bool canHarvest = false;
            List<MoveAction> harvestActions = new List<MoveAction>();
            if (hasHarvestProteins)
            {
                harvestActions = GetHarvestMoveActions(organRootId, isMine, locationsTaken);
                moveActions.AddRange(harvestActions);
                canHarvest = harvestActions.Count > 0;
                if (debug)
                    Console.Error.WriteLine($"Harvest: {organRootId}: " + string.Join('\n', harvestActions));
            }

            List<MoveAction> tentacleActions = new List<MoveAction>();
            if (hasTentacleProteins)
            {
                tentacleActions = GetTentacleMoveActions(organRootId, isMine, locationsTaken);
                moveActions.AddRange(tentacleActions);
                if (debug)
                    Console.Error.WriteLine($"Tentacle: {organRootId}: " + string.Join('\n', tentacleActions));
            }

            if (hasBasicProteins)
            {
                var basicActions = GetGrowBasicMoveActions(organRootId, isMine, locationsTaken);

                if (harvestActions.Any(m => m.Score < 0))
                    basicActions.ForEach(m => m.Score += 100);//Prefer harvest actions over basics

                if (hasManyTentacleProteins)
                    basicActions.Join(tentacleActions, m1 => m1.Location, m2 => m2.Location, (m1, m2) => m1).ToList().ForEach(a => a.Score += 100);//Prefer tentacle actions over basics.
                moveActions.AddRange(basicActions);
                if (debug)
                    Console.Error.WriteLine($"Basic: {organRootId}: " + string.Join('\n', basicActions));
            }

            if (hasSporerProteins)
            {
                var sporerActions = GetSporerMoveActions(organRootId, isMine, locationsTaken);
                if (harvestActions.Any(m => m.Score < 0))
                    sporerActions.ForEach(m => m.Score += 100);//Prefer harvest actions over sporers

                moveActions.AddRange(sporerActions);
                if (debug)
                    Console.Error.WriteLine($"Sporer: {organRootId}: " + string.Join('\n', sporerActions));
            }

            return moveActions;
        }

        public List<MoveAction> GetSporerMoveActions(int organRootId, bool isMine, HashSet<int> locationsTaken)
        {
            List<MoveAction> moveActions = new List<MoveAction>();

            List<MoveAction> growMoveActions = GetGrowMoveActions(organRootId, isMine, locationsTaken);

            foreach (MoveAction growAction in growMoveActions)
            {
                List<MoveAction> sporerMoves = new List<MoveAction>();
                foreach (LocationNeighbor locationNeighbor in GetLocationNeighbors(growAction.Location))
                {
                    var sporerAction = MoveAction.CreateGrow(growAction.OrganId, growAction.Location, EntityType.SPORER, growAction.OrganRootId, locationNeighbor.direction);
                    sporerAction.Score = growAction.Score;

                    var location = growAction.Location;
                    bool isOpen = true;
                    for (int i = 0; i < 4; i++)
                    {
                        location = GetNextLocation(location, locationNeighbor.direction);
                        if (!ValidateLocation(location) || !IsOpenSpace(location.index, isMine))
                        {
                            isOpen = false;
                            break;
                        }

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

        public List<MoveAction> GetTentacleMoveActions(int organRootId, bool isMine, HashSet<int> locationsTaken)
        {
            List<MoveAction> moveActions = new List<MoveAction>();
            List<MoveAction> growMoveActions = GetGrowMoveActions(organRootId, isMine, locationsTaken);

            foreach (MoveAction growAction in growMoveActions)
            {
                List<MoveAction> tentacleMoveActions = new List<MoveAction>();
                //Could suggest the direction based on the shortest path.
                bool isOppIn3Spaces = IsOpponentWithin3Spaces(growAction.Location, isMine);
                foreach (LocationNeighbor locationNeighbor in GetLocationNeighbors(growAction.Location))
                {
                    MoveAction tentacleAction = MoveAction.CreateGrow(growAction.OrganId, growAction.Location, EntityType.TENTACLE, growAction.OrganRootId, locationNeighbor.direction);
                    tentacleAction.Score = growAction.Score;

                    if (IsOpponentOrEmptySpace(locationNeighbor.point.index, isMine))
                    {
                        tentacleMoveActions.Add(tentacleAction);
                        if (isOppIn3Spaces)
                        {
                            tentacleAction.Score -= 50;
                            //Check to see if any are facing the enemy and use those as priority
                            if (GetEntity(locationNeighbor.point.index, out Entity entity) && entity.IsMine.HasValue && entity.IsMine != isMine)
                            {
                                tentacleAction.Score -= 50000;
                            }
                        }
                    }
                }

                if (tentacleMoveActions.Count > 0)
                {
                    var best = tentacleMoveActions.Min(m => m.Score);
                    moveActions.AddRange(tentacleMoveActions.Where(m => m.Score == best));
                }
                else
                {
                    var moveAction = MoveAction.CreateGrow(growAction.OrganId, growAction.Location, EntityType.TENTACLE, growAction.OrganRootId, OrganDirection.North);
                    moveAction.Score = growAction.Score;
                    moveAction.Score += 10000;
                    moveActions.Add(moveAction);
                }
            }

            return moveActions;
        }



        private Dictionary<string, List<MoveAction>> _moveActionCache = new Dictionary<string, List<MoveAction>>();

        public List<MoveAction> GetGrowMoveActions(int organRootId, bool isMine, HashSet<int> locationsTaken)
        {
            var key = (isMine ? "grow1_" : "grow0_") + organRootId.ToString();
            if (!_moveActionCache.TryGetValue(key, out List<MoveAction> moveActions))
            {
                var oppRootEntities = GetRootEntities(!isMine);
                var harvestableEntities = GetHarvestableEntities();
                var harvestingEntities = GetHarvestedEntities(isMine);
                HashSet<EntityType> hasHarvestType = new HashSet<EntityType>(harvestingEntities.Select(harvestEntity => harvestEntity.Type));
                List<Entity> toHarvestEntities;
                if (hasHarvestType.Count != 4)
                {
                    toHarvestEntities = harvestableEntities.Where(e => !hasHarvestType.Contains(e.Type)).ToList();
                }
                else
                {
                    toHarvestEntities = harvestableEntities.Except(harvestingEntities).ToList();
                }

                bool hasOppRoot = false;
                bool hasHarvestable = false;

                if (oppRootEntities.Count > 0)
                    hasOppRoot = true;
                if (toHarvestEntities.Count > 0)
                    hasHarvestable = true;

                int maxMoves = 5;


                moveActions = new List<MoveAction>();
                var entities = GetEntitiesByRoot(organRootId, isMine);
                HashSet<int> locationsChecked = new HashSet<int>(locationsTaken);
                foreach (Entity entity in entities)
                {
                    foreach (LocationNeighbor locationNeighbor in GetLocationNeighbors(entity.Location))
                    {
                        if (locationsChecked.Add(locationNeighbor.point.index) && IsOpenSpace(locationNeighbor.point.index, isMine))
                        {
                            var moveAction = MoveAction.CreateGrow(entity.OrganId, locationNeighbor.point, EntityType.NONE, entity.OrganRootId);
                            moveAction.Score = 0;
                            if (hasHarvestable)
                            {
                                moveAction.Score += toHarvestEntities.Min(r => Graph.GetShortestPathDistance(r.Location.index, moveAction.Location.index));
                            }
                            else if (hasOppRoot)
                            {
                                moveAction.Score += oppRootEntities.Min(r => Graph.GetShortestPathDistance(r.Location.index, moveAction.Location.index));
                            }

                            bool? isHarvesting = IsHarvesting(locationNeighbor.point.index, isMine);
                            if (isHarvesting.HasValue && isHarvesting.Value && !IsOpponentWithin3Spaces(moveAction.Location, isMine))
                            {
                                moveAction.Score += 1000;
                            }

                            moveActions.Add(moveAction);
                        }
                    }
                }


                if (moveActions.Count > maxMoves)
                {
                    moveActions.Sort((m1, m2) => m1.Score.CompareTo(m2.Score));
                    moveActions = moveActions.Take(maxMoves).ToList();
                }
                _moveActionCache[key] = moveActions;
            }
            return moveActions;
        }

        public List<MoveAction> GetGrowBasicMoveActions(int organRootId, bool isMine, HashSet<int> locationsTaken)
        {
            List<MoveAction> moveActions = new List<MoveAction>();
            List<MoveAction> growMoveActions = GetGrowMoveActions(organRootId, isMine, locationsTaken);

            foreach (MoveAction growAction in growMoveActions)
            {
                MoveAction moveAction = MoveAction.CreateGrow(growAction.OrganId, growAction.Location, EntityType.BASIC, growAction.OrganRootId);
                moveAction.Score = growAction.Score;
                moveAction.Score -= 30;
                moveActions.Add(moveAction);
            }

            return moveActions;
        }

        public List<MoveAction> GetHarvestMoveActions(int organRootId, bool isMine, HashSet<int> locationsTaken)
        {
            List<MoveAction> moveActions = new List<MoveAction>();
            List<MoveAction> growMoveActions = GetGrowMoveActions(organRootId, isMine, locationsTaken);

            foreach (MoveAction growAction in growMoveActions)
            {
                bool isOppIn3Spaces = IsOpponentWithin3Spaces(growAction.Location, isMine);
                List<MoveAction> harvestActions = new List<MoveAction>();
                foreach (LocationNeighbor locationNeighbor in GetLocationNeighbors(growAction.Location))
                {
                    bool? isHarvesting = IsHarvesting(locationNeighbor.point.index, isMine);

                    var harvestAction = MoveAction.CreateGrow(growAction.OrganId, growAction.Location, EntityType.HARVESTER, growAction.OrganRootId, locationNeighbor.direction);
                    harvestAction.Score = growAction.Score;

                    if (isOppIn3Spaces)
                    {
                        harvestAction.Score += 1000;
                    }

                    if (isHarvesting.HasValue && !isHarvesting.Value)
                    {
                        harvestAction.Score -= 500;

                    }

                    if (harvestAction.Score < 0)
                    {
                        harvestActions.Add(harvestAction);
                    }
                }

                if (harvestActions.Count > 0)
                {
                    moveActions.AddRange(harvestActions);
                }
                else
                {
                    var moveAction = MoveAction.CreateGrow(growAction.OrganId, growAction.Location, EntityType.HARVESTER, growAction.OrganRootId);
                    moveAction.Score = growAction.Score;
                    moveAction.Score += 1000;
                    moveActions.Add(moveAction);
                }
            }

            return moveActions;
        }

        public bool? IsHarvesting(int location, bool isMine)
        {
            if (IsHarvestSpace(location))
            {
                foreach (LocationNeighbor locationNeighbor in GetLocationNeighbors(location))
                {
                    if (GetEntity(locationNeighbor.point.index, out Entity entity) && entity.Type == EntityType.HARVESTER && entity.IsMine.HasValue && entity.IsMine == isMine && entity.OrganDirection == GetOpposingDirection(locationNeighbor.direction))
                    {
                        return true;
                    }
                }
                return false;
            }
            return null;
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
            var harvesters = GetHarvesterEntities(isMine);
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

        public bool IsHarvestExactly2paces(Point2d location)
        {
            var harvestEntities = GetHarvestableEntities();
            foreach (var entity in harvestEntities)
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


        Dictionary<string, bool> _locationCheckCache;
        public bool IsOpponentWithin3Spaces(Point2d location, bool isMine)
        {
            var key = (isMine ? "opponentclose_1" : "opponentclose_2") + location.index.ToString();
            if (!_locationCheckCache.TryGetValue(key, out bool result))
            {
                var oppEntities = GetEntities(!isMine);
                foreach (var oppEntity in oppEntities)
                {
                    double distance = Graph.GetShortestPathDistance(location.index, oppEntity.Location.index);
                    if (distance <= 3)
                    {
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

        public bool IsOpponentOrEmptySpace(int index, bool isMine)
        {
            return !GetEntity(index, out Entity entity) || entity.IsOpenSpace() || (entity.IsMine.HasValue && entity.IsMine != isMine);
        }
        public bool IsOpponentSpace(Point2d location, OrganDirection direction, bool isMine)
        {
            return GetEntityWithDirection(location, direction, out Entity entity) && entity.IsMine != isMine;
        }

        public bool IsHarvestSpace(int location)
        {
            return GetEntity(location, out Entity entity) && entity.IsOpenSpace();
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
            string key = (isMine ? "openspace_1" : "openspace_2") + location.ToString();
            if (!_locationCheckCache.TryGetValue(key, out bool result))
            {
                if (IsOpenSpace(location))
                {
                    foreach (LocationNeighbor locationNeighbor in GetLocationNeighbors(location))
                    {
                        if (GetEntity(locationNeighbor.point.index, out Entity checkForTentacleEntity))
                        {
                            if (checkForTentacleEntity.Type == EntityType.TENTACLE && GetOpposingDirection(checkForTentacleEntity.OrganDirection) == locationNeighbor.direction && checkForTentacleEntity.IsMine != isMine)
                            {
                                result = false;
                                _locationCheckCache[key] = result;
                                return result;
                            }
                        }
                    }
                    result = true;
                    _locationCheckCache[key] = result;
                    return result;
                }
                result = false;
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
            var location = GetNextLocation(currentLocation, nextDirection);
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

        public List<Entity> GetHarvestedEntities(bool isMine)
        {
            var key = isMine ? "harvested_1" : "harvested_0";
            if (!_entityCache.TryGetValue(key, out var entities))
            {
                entities = new List<Entity>();
                var harvesters = GetHarvesterEntities(isMine);
                HashSet<int> harvestedLocations = new HashSet<int>();
                foreach (Entity harvester in harvesters)
                {
                    if (GetEntityWithDirection(harvester.Location, harvester.OrganDirection, out Entity entity) && entity.IsOpenSpace() && harvestedLocations.Add(entity.Location.index))
                    {
                        entities.Add(entity);
                    }
                }

                _entityCache[key] = entities;
            }
            return entities;
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
            var key = "harvestable";
            if (!_entityCache.TryGetValue(key, out var entities))
            {
                entities = GetEntitiesList().Where(e => e.IsOpenSpace()).ToList();
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

        public List<Entity> GetHarvesterEntities(bool isMine)
        {
            var key = isMine ? "harvest1" : "harvest0";
            if (!_entityCache.TryGetValue(key, out var entities))
            {
                entities = GetEntities(isMine).Where(e => e.Type == EntityType.HARVESTER).ToList();
                _entityCache[key] = entities;
            }
            return entities;
        }

        public List<Entity> GetRootEntities(bool isMine)
        {
            var key = isMine ? "root1" : "root0";
            if (!_entityCache.TryGetValue(key, out var entities))
            {
                entities = GetEntities(isMine).Where(e => e.Type == EntityType.ROOT).ToList();
                _entityCache[key] = entities;
            }
            return entities;
        }

        public List<Entity> GetSporerEntities(int organRootId, bool isMine)
        {
            var key = isMine ? "sporer1" : "sporer0";
            if (!_entityCache.TryGetValue(key, out var entities))
            {
                entities = GetEntitiesByRoot(organRootId, isMine).Where(e => e.Type == EntityType.SPORER).ToList();
                _entityCache[key] = entities;
            }
            return entities;
        }

        public List<Entity> GetTentacleEntities(bool isMine)
        {
            var key = isMine ? "tentacles1" : "tentacles0";
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
            var key = isMine ? "e1" : "e0";
            if (!_entityCache.TryGetValue(key, out List<Entity> entities))
            {
                entities = GetEntitiesList().Where(e => e.IsMine.HasValue && e.IsMine == isMine).ToList();
                _entityCache[key] = entities;
            }
            return entities;
        }

        public List<Entity> GetEntitiesByRoot(int organRootId, bool isMine)
        {
            var key = $"{organRootId}_{isMine}";
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

        public void SetEntities(IList<Entity> entities, bool isFirstTurn = false)
        {
            UpdateBoard();
            Array.Clear(Entities);
            foreach (Entity entity in entities)
            {
                Entities[entity.Location.index] = entity;
                if (entity.OrganId > 0 && GlobalOrganId <= entity.OrganId)
                {
                    GlobalOrganId = entity.OrganId + 1;
                }
            }

            if (isFirstTurn)
            {
                CalculatePathsAndNeighbors();
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
            _locationCheckCache = new Dictionary<string, bool>();
        }

        public readonly struct LocationNeighbor
        {
            public Point2d point { get; }
            public OrganDirection direction { get; }

            public LocationNeighbor(int x, int y, int index, OrganDirection direction)
            {
                this.point = new Point2d(x, y, index);
                this.direction = direction;
            }
        }

        private Point2d[][][] _locationCache;
        private int[][] _locationIndexCache = null;
        private List<LocationNeighbor>[] _locationNeighbors = null;
        public void InitializeBoard()
        {
            _locationIndexCache = new int[Width][];
            for (int x = 0; x < Width; x++)
            {
                _locationIndexCache[x] = new int[Height];
                for (int y = 0; y < Height; y++)
                {
                    _locationIndexCache[x][y] = GetNodeIndexInternal(x, y);
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
                        var nextLocation = GetNextLocationInternl(x - 1, y - 1, PossibleDirections[z]);
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
            _locationNeighbors = new List<LocationNeighbor>[Width * Height];
            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
                {
                    var index = GetNodeIndex(x, y);
                    var node = new Node(index);
                    Graph.AddNode(node);
                    _locationNeighbors[index] = new List<LocationNeighbor>();
                    for (int z = 0; z < 4; z++)
                    {
                        var location = GetNextLocation(new Point2d(x, y, GetNodeIndex(x, y)), PossibleDirections[z]);
                        if (ValidateLocation(location) && IsOpenSpace(location.index))
                        {
                            var nextIndex = GetNodeIndex(location);
                            node.AddLink(new Link(node, new Node(nextIndex), 1));
                            _locationNeighbors[index].Add(new LocationNeighbor(location.x, location.y, nextIndex, PossibleDirections[z]));
                        }
                    }
                }
            }
            Graph.CalculateShortestPaths();
        }

        private int GetNodeIndexInternal(int x, int y)
        {
            return x * Height + y;
        }


        public List<LocationNeighbor> GetLocationNeighbors(int nodeIndex)
        {
            return _locationNeighbors[nodeIndex];
        }

        public List<LocationNeighbor> GetLocationNeighbors(Point2d location)
        {
            return _locationNeighbors[location.index];
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
