using Algorithms.Graph;
using Algorithms.Utility;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using static Algorithms.Graph.Graph;

namespace GameSolution.Entities
{
    public class Board : PooledObject<Board>
    {
        public int Width;
        public int Height;
        private Entity[] Entities;
        public int GlobalOrganId = -1;

        public Graph Graph;

        public static OrganDirection[] PossibleDirections = new OrganDirection[] { OrganDirection.North, OrganDirection.South, OrganDirection.East, OrganDirection.West };

        static Board()
        {
            SetInitialCapacity(20000);
        }

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

        protected override void Reset()
        {
            for (int i = 0; i < Entities.Length; i++)
            {
                Entities[i]?.Dispose();
            }
            UpdateBoard();
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
            _intToStringCache = board._intToStringCache;
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
            List<Entity> tentacles = GetTentacleEntities();

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
                    IEnumerable<Entity> deathToRoot = GetEntitiesList().Where(e => e.OrganRootId == deadEntity.OrganId);
                    foreach (Entity kill in deathToRoot)
                    {
                        Entities[kill.Location.index]?.Dispose();
                        Entities[kill.Location.index] = null;
                    }
                }
                else
                {
                    IEnumerable<Entity> deathToChildren = GetEntitiesList().Where(e => e.OrganParentId == deadEntity.OrganId);
                    foreach (Entity kill in deathToChildren)
                    {
                        Entities[kill.Location.index]?.Dispose();
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
                        Entities[action.Location.index]?.Dispose();
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
                        Entities[action.Location.index]?.Dispose();
                        Entities[action.Location.index] = Entity.GetEntity(action.Location, action.EntityType, isMine, GlobalOrganId++, action.OrganId, action.OrganRootId, action.OrganDirection);
                    }
                    break;
                case MoveType.SPORE:
                    Entity sporeEntity = GetEntityByLocation(action.Location);
                    if (sporeEntity == null || sporeEntity.IsOpenSpace())
                    {
                        int organId = GlobalOrganId++;
                        Entities[action.Location.index]?.Dispose();
                        Entities[action.Location.index] = Entity.GetEntity(action.Location, action.EntityType, isMine, organId, organId, organId, action.OrganDirection);
                    }
                    break;
            }
        }

        public class ProteinInfo
        {
            public int[] Proteins;
            public bool[] HasProteins;
            public bool HasHarvestProteins;
            public bool HasBasicProteins;
            public bool HasTentacleProteins;
            public bool HasSporerProteins;
            public bool HasRootProteins;

            public bool[] HasManyProteins;
            public bool HasManyRootProteins;
            public bool HasManyTentacleProteins;
            public bool HasManySporerProteins;
            public bool HasAtLeastTwoMany;

            public bool IsHarvestingRootProteins;
            public bool IsHarvestingTentacleProteins;
            public bool IsHarvestingSporerProteins;
            public bool IsHarvestingBasicProteins;
            public bool IsHarvestingHarvesterProteins;
            public bool[] IsHarvestingProteins;
            public ProteinInfo(int[] proteins, Board board, bool isMine)
            {
                Proteins = proteins;
                HasProteins = proteins.Select(p => p > 0).ToArray();
                HasHarvestProteins = HasProteins[2] && HasProteins[3];
                HasBasicProteins = HasProteins[0];
                HasTentacleProteins = HasProteins[1] && HasProteins[2];
                HasSporerProteins = HasProteins[1] && HasProteins[3];

                HasRootProteins = HasProteins.All(m => m);
                HasManyProteins = proteins.Select(p => p > 10).ToArray();
                HasManyRootProteins = HasManyProteins.All(m => m);
                HasManyTentacleProteins = HasManyProteins[1] && HasManyProteins[2];
                HasManySporerProteins = HasManyProteins[1] && HasManyProteins[3];
                HasAtLeastTwoMany = HasManyProteins.Count(value => value) > 1;

                int[] harvestingProteins = new int[4];
                board.Harvest(isMine, harvestingProteins);
                IsHarvestingProteins = harvestingProteins.Select(p => p > 0).ToArray();
                IsHarvestingHarvesterProteins = IsHarvestingProteins[2] && IsHarvestingProteins[3];
                IsHarvestingBasicProteins = IsHarvestingProteins[0];
                IsHarvestingTentacleProteins = IsHarvestingProteins[1] && IsHarvestingProteins[2];
                IsHarvestingSporerProteins = IsHarvestingProteins[1] && IsHarvestingProteins[3];
                IsHarvestingRootProteins = IsHarvestingProteins[0] && IsHarvestingProteins[1] && IsHarvestingProteins[2] && IsHarvestingProteins[3];

            }
        }

        //First is by the number of organisms and second is by current organism number; general goal was to limit to a maximum expansion of 24 moves and then we take 10
        const int _maxMovesTotal = 10;
        readonly int[,] _maxActionsPerOrganism = { { _maxMovesTotal, 0, 0, 0, 0 }, { 6, 4, 0, 0, 0 }, { 6, 2, 2, 0, 0 }, { 3, 2, 2, 2, 0 }, { 3, 2, 2, 2, 1 } };

        Stopwatch _watch = new Stopwatch();
        public List<Move> GetMoves(int[] proteins, bool isMine, bool debug = false)
        {
            _watch.Reset();
            _watch.Start();
            List<Move> moves = new List<Move>();

            List<Entity> rootEntities = GetRootEntities(isMine);
            int organismCount = rootEntities.Count;
            bool[] organismHasMoves = new bool[organismCount];
            MoveAction[][] organismToMoveActions = new MoveAction[organismCount][];

            ProteinInfo proteinInfo = new ProteinInfo(proteins, this, isMine);

            int i = 0;
            int maxOrgans = 5;//Put a limit on the number of organs we calculate moves for as this is exploding the action space

            List<Entity> limitedRootEntities = rootEntities;
            if (organismCount > maxOrgans)
            {
                limitedRootEntities = rootEntities.Take(maxOrgans).ToList();
            }
            int selectedOrgans = limitedRootEntities.Count;

            bool hasSufficientProteins = (proteins[0] >= selectedOrgans || proteins[0] == 0) && (proteins[1] >= selectedOrgans || proteins[1] == 0) && (proteins[2] >= selectedOrgans || proteins[2] == 0) && (proteins[3] >= selectedOrgans || proteins[3] == 0);//Checks for 0 because if we don't have any of that protein then we won't have actions of that type.

            HashSet<int> locationsTaken = new HashSet<int>();

            foreach (Entity root in limitedRootEntities)
            {
                List<MoveAction> moveActions = new List<MoveAction>();

                bool sporeHasPriority = organismCount < 2 || proteinInfo.HasManyRootProteins || proteinInfo.IsHarvestingRootProteins;
                if (proteinInfo.HasRootProteins && sporeHasPriority)
                {
                    AddSporeMoveActions(moveActions, root.OrganRootId, isMine, locationsTaken);
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

                if (moveActions.Count <= 0 || !sporeHasPriority)
                {
                    AddGrowMoveActions(moveActions, root.OrganRootId, isMine, proteinInfo, locationsTaken, debug);

                    if (moveActions.Count > 0)
                    {
                        moveActions = moveActions.OrderBy(m => m.Score).Take(_maxActionsPerOrganism[limitedRootEntities.Count - 1, i]).ToList();
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

                if (_watch.ElapsedMilliseconds > 5)
                {
                    Console.Error.WriteLine("Move generation took too long");
                    break;
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


        public List<MoveAction> AddSporeMoveActions(List<MoveAction> moveActions, int organRootId, bool isMine, HashSet<int> locationsTaken)
        {
            IEnumerable<Entity> sporers = GetSporerEntities(organRootId, isMine);

            foreach (Entity sporer in sporers)
            {
                Point2d location = sporer.Location;
                int distance = 0;
                while (true)
                {
                    distance++;
                    location = GetNextLocation(location, sporer.OrganDirection);
                    if (ValidateLocation(location) && IsOpenSpace(location.index, isMine))
                    {
                        if (distance <= 3 || locationsTaken.Contains(location.index))
                        {
                            continue;
                        }
                        bool? isHarvesting = IsHarvesting(location, isMine);
                        MoveAction sporeMove = MoveAction.CreateSpore(sporer.OrganId, location);
                        moveActions.Add(sporeMove);
                        if (!(isHarvesting.HasValue && isHarvesting.Value) && IsHarvestExactly2spaces(location))
                        {
                            sporeMove.Score = -1000;
                        }
                        else
                        {
                            sporeMove.Score = 1000;
                        }
                    }
                    else
                    {
                        break;
                    }

                    if (moveActions.Count > 9)
                    {
                        break;
                    }
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
                return moveActions;
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
                return moveActions;
            }

            if (proteinInfo.HasBasicProteins)
            {
                List<MoveAction> basicActions = GetGrowBasicMoveActions(organRootId, isMine, locationsTaken, proteinInfo);

                if (harvestActions.Any(m => m.Score < 0))
                    basicActions.ForEach(m => m.Score += 100);//Prefer harvest actions over basics

                if (tentacleActions.Any(m => m.Score < 0))
                    basicActions.ForEach(m => m.Score += 200);//Prefer tentacle actions over basics

                if (proteinInfo.HasManyTentacleProteins)
                    basicActions.Join(tentacleActions, m1 => m1.Location, m2 => m2.Location, (m1, m2) => m1).ToList().ForEach(a => a.Score += 100);//Prefer tentacle actions over basics.
                moveActions.AddRange(basicActions);
                if (debug)
                    Console.Error.WriteLine($"Basic: {organRootId}: " + string.Join('\n', basicActions));
            }

            if (moveActions.Count > 0 && _watch.ElapsedMilliseconds > 5)
            {
                Console.Error.WriteLine($"Took too long to find Basic Actions: {_watch.ElapsedMilliseconds}");
                return moveActions;
            }

            if (proteinInfo.HasSporerProteins)
            {
                List<MoveAction> sporerActions = GetSporerMoveActions(organRootId, isMine, locationsTaken, proteinInfo);
                if (harvestActions.Any(m => m.Score < 0))
                    sporerActions.ForEach(m => m.Score += 100);//Prefer harvest actions over sporers
                if (!proteinInfo.HasManyTentacleProteins && tentacleActions.Any(m => m.Score < 0))
                    sporerActions.ForEach(m => m.Score += 100);//Prefer tentacle actions over sporers
                if (!proteinInfo.HasRootProteins)
                    sporerActions.ForEach(m => m.Score += 100);


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
                    MoveAction sporerAction = MoveAction.CreateGrow(growAction.OrganId, growAction.Location, EntityType.SPORER, growAction.OrganRootId, locationNeighbor.direction);
                    sporerAction.Score = growAction.Score;

                    if (proteinInfo.IsHarvestingSporerProteins)
                    {
                        sporerAction.Score -= 100;
                    }

                    Point2d location = growAction.Location;
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

        public List<MoveAction> GetTentacleMoveActions(int organRootId, bool isMine, HashSet<int> locationsTaken, ProteinInfo proteinInfo)
        {
            List<MoveAction> moveActions = new List<MoveAction>();
            List<MoveAction> growMoveActions = GetGrowMoveActions(organRootId, isMine, locationsTaken, proteinInfo);

            foreach (MoveAction growAction in growMoveActions)
            {
                List<MoveAction> tentacleMoveActions = new List<MoveAction>();
                //Could suggest the direction based on the shortest path.
                bool isOppIn3Spaces = IsOpponentWithin3Spaces(growAction.Location, isMine);
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
                                if (entity.Type == EntityType.ROOT && GetEntitiesByRoot(entity.OrganId, !isMine).Count > 0)
                                {
                                    tentacleAction.Score -= 50000;//hit the root!
                                    return new List<MoveAction> { tentacleAction };//Destroying roots should always be the priority
                                }
                            }
                            else
                            {
                                //Since opponent is within 3 spaces; they must be 2 away or less so go look for them and provide it as the priority
                                Point2d nextLocation = GetNextLocation(locationNeighbor.point, locationNeighbor.direction);
                                if (ValidateLocation(nextLocation))
                                {
                                    foreach (LocationNeighbor locationNeighbor2 in GetLocationNeighbors(nextLocation))
                                    {
                                        if (GetEntity(locationNeighbor2.point.index, out Entity entity2) && entity2.IsMine.HasValue && entity2.IsMine != isMine)
                                        {
                                            tentacleAction.Score -= 50;
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



        private Dictionary<string, List<MoveAction>> _moveActionCache = new Dictionary<string, List<MoveAction>>();
        private const int _maxMoves = 7;

        public List<MoveAction> GetGrowMoveActions(int organRootId, bool isMine, HashSet<int> locationsTaken, ProteinInfo proteinInfo)
        {
            string key = (isMine ? "grow1_" : "grow0_") + organRootId.ToString();
            if (!_moveActionCache.TryGetValue(key, out List<MoveAction> moveActions))
            {
                List<Entity> oppRootEntities = GetRootEntities(!isMine);
                List<Entity> harvestableEntities = GetHarvestableEntities();
                List<Entity> harvestingEntities = GetHarvestedEntities(isMine);
                HashSet<EntityType> harvestedTypes = harvestingEntities.Select(e => e.Type).ToHashSet();

                List<Entity> toHarvestEntities = harvestedTypes.Count < 4 ? harvestableEntities.Where(e => !harvestedTypes.Contains(e.Type)).ToList() : new List<Entity>();

                bool hasOppRoot = oppRootEntities.Any();
                bool hasHarvestable = toHarvestEntities.Any();


                moveActions = new List<MoveAction>();
                List<Entity> entities = GetEntitiesByRoot(organRootId, isMine);
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
                                if (hasHarvestable)
                                {
                                    moveAction.Score += toHarvestEntities.Min(r => Graph.GetShortestPathDistance(r.Location.index, moveAction.Location.index));
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
                                    bool isOppIn3Spaces = IsOpponentWithin3Spaces(moveAction.Location, isMine);
                                    if (isHarvesting.Value && !isOppIn3Spaces)
                                    {
                                        moveAction.Score += 1000;
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


                if (moveActions.Count > _maxMoves)
                {
                    moveActions = moveActions.OrderBy(m => m.Score).Take(_maxMoves).ToList();
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
                if (proteinInfo.IsHarvestingBasicProteins)
                {
                    moveAction.Score -= 100;
                }
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
                bool isOppIn3Spaces = IsOpponentWithin3Spaces(growAction.Location, isMine);
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

        private Dictionary<string, bool?> _harvestCache = new Dictionary<string, bool?>();
        public bool? IsHarvesting(int location, bool isMine)
        {
            string key = (isMine ? "harvest1_" : "harvest0_") + location.ToString();
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
            List<Entity> harvesters = GetHarvesterEntities(isMine);
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

        public bool IsHarvestExactly2spaces(Point2d location)
        {
            List<Entity> harvestEntities = GetHarvestableEntities();
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


        Dictionary<string, bool> _locationCheckCache = new Dictionary<string, bool>();
        public bool IsOpponentWithin3Spaces(Point2d location, bool isMine)
        {
            string key = (isMine ? "opponentclose_1" : "opponentclose_2") + location.index.ToString();
            if (!_locationCheckCache.TryGetValue(key, out bool result))
            {
                List<Entity> oppEntities = GetEntities(!isMine);
                foreach (Entity oppEntity in oppEntities)
                {
                    Graph.GetShortest(location.index, oppEntity.Location.index, out DistancePath distancePath);
                    double distance = distancePath.Distance;
                    if (distance < 3)
                    {
                        result = true;
                        //Follow the path and check if it's really open or if we are in the way.
                        foreach (ILink link in distancePath.Paths)
                        {
                            if (!IsOpponentOrOpenSpace(link.EndNodeId, isMine))
                            {
                                result = false;
                                break;
                            }
                        }
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

        private string[] _intToStringCache;
        private bool[] _isOpenSpaceInitial;
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
            string key = $"{(isMine ? "opposingtentacle_1" : "opposingtentacle_2")}{_intToStringCache[location]}";
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

        public List<Entity> GetHarvestedEntities(bool isMine)
        {
            string key = isMine ? "harvested_1" : "harvested_0";
            if (!_entityCache.TryGetValue(key, out List<Entity>? entities))
            {
                entities = new List<Entity>();
                List<Entity> harvesters = GetHarvesterEntities(isMine);
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
            return entity != null;
        }

        public bool GetEntity(Point2d location, out Entity entity)
        {
            return GetEntity(GetNodeIndex(location), out entity);
        }

        public List<Entity> GetHarvestableEntities()
        {
            string key = "harvestable";
            if (!_entityCache.TryGetValue(key, out List<Entity>? entities))
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
            string key = isMine ? "harvest1" : "harvest0";
            if (!_entityCache.TryGetValue(key, out List<Entity>? entities))
            {
                entities = GetEntities(isMine).Where(e => e.Type == EntityType.HARVESTER).ToList();
                _entityCache[key] = entities;
            }
            return entities;
        }

        public List<Entity> GetRootEntities(bool isMine)
        {
            string key = isMine ? "root1" : "root0";
            if (!_entityCache.TryGetValue(key, out List<Entity>? entities))
            {
                entities = GetEntities(isMine).Where(e => e.Type == EntityType.ROOT).OrderByDescending(e => e.OrganId).ToList();
                _entityCache[key] = entities;
            }
            return entities;
        }

        public List<Entity> GetSporerEntities(int organRootId, bool isMine)
        {
            string key = (isMine ? "sporer1" : "sporer0") + organRootId.ToString();
            if (!_entityCache.TryGetValue(key, out List<Entity>? entities))
            {
                entities = GetEntitiesByRoot(organRootId, isMine).Where(e => e.Type == EntityType.SPORER).ToList();
                _entityCache[key] = entities;
            }
            return entities;
        }

        public List<Entity> GetTentacleEntities(bool isMine)
        {
            string key = isMine ? "tentacles1" : "tentacles0";
            if (!_entityCache.TryGetValue(key, out List<Entity>? entities))
            {
                entities = GetTentacleEntities().Where(e => e.IsMine == isMine).ToList();
                _entityCache[key] = entities;
            }
            return entities;
        }

        public List<Entity> GetTentacleEntities()
        {
            string key = "tentacles";
            if (!_entityCache.TryGetValue(key, out List<Entity>? entities))
            {
                entities = GetEntitiesList().Where(e => e.Type == EntityType.TENTACLE).ToList();
                _entityCache[key] = entities;
            }
            return entities;
        }

        public List<Entity> GetEntitiesList()
        {
            string key = "all";
            if (!_entityCache.TryGetValue(key, out List<Entity> entities))
            {
                entities = Entities.Where(e => e != null).Select(e => e).ToList();
                _entityCache[key] = entities;
            }
            return entities;
        }


        private Dictionary<string, List<Entity>> _entityCache = new Dictionary<string, List<Entity>>();
        public List<Entity> GetEntities(bool isMine)
        {
            string key = isMine ? "e1" : "e0";
            if (!_entityCache.TryGetValue(key, out List<Entity> entities))
            {
                entities = GetEntitiesList().Where(e => e.IsMine.HasValue && e.IsMine == isMine).ToList();
                _entityCache[key] = entities;
            }
            return entities;
        }

        public List<Entity> GetEntitiesByRoot(int organRootId, bool isMine)
        {
            string key = $"{organRootId}_{isMine}";
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

        private int _initialOpenSpacesCount = 0;
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

        public void SetEntities(IList<Entity> entities, bool isFirstTurn = false)
        {
            Array.Clear(Entities);
            foreach (Entity entity in entities)
            {
                Entities[entity.Location.index]?.Dispose();
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
            Board cleanState = Get();
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


        private Point2d[][][] _locationCache;
        private int[][] _locationIndexCache = null;
        private List<LocationNeighbor>[] _locationNeighbors = null;

        public void InitializeBoard()
        {
            _intToStringCache = new string[Width * Height];
            _locationIndexCache = new int[Width][];
            for (int x = 0; x < Width; x++)
            {
                _locationIndexCache[x] = new int[Height];
                for (int y = 0; y < Height; y++)
                {
                    int index = GetNodeIndexInternal(x, y);
                    _locationIndexCache[x][y] = index;
                    _intToStringCache[index] = index.ToString();
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
            _locationNeighbors = new List<LocationNeighbor>[Width * Height];
            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
                {
                    int index = GetNodeIndex(x, y);
                    Node node = new Node(index);
                    Graph.AddNode(node);
                    _locationNeighbors[index] = new List<LocationNeighbor>();
                    for (int z = 0; z < 4; z++)
                    {
                        Point2d location = GetNextLocation(new Point2d(x, y, GetNodeIndex(x, y)), PossibleDirections[z]);
                        if (ValidateLocation(location))
                        {
                            if (IsOpenSpace(location.index) || (GetEntity(location.index, out Entity entity) && entity.Type != EntityType.WALL))//The start of the map always includes 2 root entities, but we only want to handle walls
                            {
                                int nextIndex = GetNodeIndex(location);
                                node.AddLink(new Link(node, new Node(nextIndex), 1));
                                _locationNeighbors[index].Add(new LocationNeighbor(location.x, location.y, nextIndex, PossibleDirections[z]));
                            }
                            else
                            {
                                if (_isOpenSpaceInitial[location.index])
                                    _initialOpenSpacesCount--;
                                _isOpenSpaceInitial[location.index] = false;

                            }
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

        public IEnumerable<LocationNeighbor> GetOpenSpaceLocationNeighbor(Point2d location, bool isMine)
        {
            for (int i = 0; i < _locationNeighbors[location.index].Count; i++)
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
