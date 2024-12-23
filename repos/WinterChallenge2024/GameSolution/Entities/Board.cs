using Algorithms.Space;
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
        private Dictionary<Point2d, Entity> Entities { get; set; }
        public int GlobalOrganId { get; set; } = -1;

        public static OrganDirection[] PossibleDirections = new OrganDirection[] { OrganDirection.North, OrganDirection.South, OrganDirection.East, OrganDirection.West };

        public Board(int width, int height)
        {
            Width = width;
            Height = height;
            Entities = new Dictionary<Point2d, Entity>();
            InitializeBoard();
            UpdateBoard();
        }

        public Board(Board board)
        {
            Width = board.Width;
            Height = board.Height;
            Entities = board.Entities.ToDictionary(e => e.Key.Clone(), e => e.Value.Clone());
            GlobalOrganId = board.GlobalOrganId;
            _entityCache = board._entityCache;
            _moveActionCache = board._moveActionCache;
            _myEntityCount = board._myEntityCount;
            _oppEntityCount = board._oppEntityCount;
            _locationCache = board._locationCache;
            UpdateBoard();
        }

        public bool Equals(Board board)
        {
            if (Width != board.Width || Height != board.Height)
                return false;

            foreach (Point2d location in Entities.Keys)
            {
                if (!board.Entities.TryGetValue(location, out Entity entity))
                    return false;
                else if (!Entities[location].Equals(entity))
                    return false;

            }

            return true;
        }

        public void Attack()
        {
            var tentacles = Entities.Values.Where(e => e.Type == EntityType.TENTACLE);

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
                var deathToChildren = Entities.Values.Where(e => e.OrganParentId == deadEntity.OrganId);
                foreach (Entity kill in deathToChildren)
                {
                    Entities.Remove(kill.Location);
                }
            }
        }



        public void ApplyMove(Move myMove, Move oppMove)
        {
            foreach (MoveAction action in myMove.Actions)
            {
                if (action.Type != MoveType.WAIT)
                {
                    var collisionActions = oppMove.Actions.Where(a => (a.Type == MoveType.GROW || a.Type == MoveType.SPORE) && a.Location.Equals(action.Location)).ToList();
                    if (collisionActions.Count > 0)
                    {
                        Entities[action.Location] = new Entity(action.Location, EntityType.WALL, null, 0, 0, 0, OrganDirection.None);
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
                    if (!Entities.ContainsKey(action.Location))
                    {
                        Entities[action.Location] = new Entity(action.Location, action.EntityType, isMine, GlobalOrganId++, action.OrganId, action.OrganRootId, action.OrganDirection);
                    }
                    break;
                case MoveType.SPORE:
                    if (!Entities.ContainsKey(action.Location))
                    {
                        var organId = GlobalOrganId++;
                        Entities[action.Location] = new Entity(action.Location, action.EntityType, isMine, organId, organId, organId, action.OrganDirection);
                    }
                    break;
            }
        }

        public List<Move> GetMoves(int[] proteins, bool isMine)
        {
            var moves = new List<Move>();

            var rootEntities = GetRootEntities(isMine);
            var organismCount = rootEntities.Count;
            MoveAction[][] organismToMoveActions = new MoveAction[organismCount][];

            for (int i = 0; i < organismCount; i++)
            {
                Entity root = rootEntities[i];
                var moveActions = new List<MoveAction>
                {
                    MoveAction.CreateWait()
                };


                bool canHarvest = false;
                if (proteins[2] > 0 && proteins[3] > 0)
                {
                    var harvestActions = GetHarvestMoveActions(root.OrganRootId, isMine);
                    moveActions.AddRange(harvestActions);
                    canHarvest = harvestActions.Count > 0;
                }

                if (!canHarvest && proteins[0] > 0)
                {
                    moveActions.AddRange(GetGrowBasicMoveActions(root.OrganRootId, isMine));
                }

                if (!canHarvest && proteins[1] > 0 && proteins[2] > 0)
                {
                    moveActions.AddRange(GetTentacleMoveActions(root.OrganRootId, isMine));
                }

                if (!canHarvest && proteins[1] > 0 && proteins[3] > 0)
                {
                    moveActions.AddRange(GetSporerMoveActions(root.OrganRootId, isMine));
                }

                bool canRoot = true;
                for (int j = 0; j < 4; j++)
                {
                    if (proteins[j] <= 0)
                    {
                        canRoot = false;
                        break;
                    }
                }
                if (!canHarvest && canRoot)
                {
                    moveActions.AddRange(GetSporeMoveActions(root.OrganRootId, isMine));
                }

                organismToMoveActions[i] = moveActions.ToArray();
            }


            if (organismCount > 0)
            {
                var theMoves = CartesianProduct(organismToMoveActions);
                bool hasSufficientProteins = (proteins[0] > organismCount || proteins[0] == 0) && (proteins[1] > organismCount || proteins[1] == 0) && (proteins[2] > organismCount || proteins[2] == 0) && (proteins[3] > organismCount || proteins[3] == 0);
                foreach (MoveAction[] actions in theMoves)
                {
                    var move = new Move();
                    move.SetActions(actions);
                    if (hasSufficientProteins || ValidateCost(proteins, move))
                    {
                        moves.Add(move);
                    }
                }
            }
            else
            {
                foreach (MoveAction[] actions in organismToMoveActions)
                {
                    var move = new Move();
                    move.AddAction(actions.First());
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

        public static IEnumerable<T[]> CartesianProduct<T>(T[][] sequences)
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
                yield return indices.Select((index, i) => arrays[i][index]).ToArray();

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

        public List<MoveAction> GetSporeMoveActions(int organRootId, bool isMine)
        {
            List<MoveAction> moveActions = new List<MoveAction>();

            List<Entity> sporers = GetSporerEntities(organRootId, isMine);

            foreach (Entity sporer in sporers)
            {
                var location = sporer.Location;
                while (true)
                {
                    if (IsOpenSpace(location, sporer.OrganDirection))
                    {
                        location = GetNextLocation(location, sporer.OrganDirection);
                        moveActions.Add(MoveAction.CreateSpore(sporer.OrganId, location));
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
                    if (IsOpenSpace(growAction.Location, sporerDirection))
                    {
                        moveActions.Add(MoveAction.CreateGrow(growAction.OrganId, growAction.Location, EntityType.SPORER, growAction.OrganRootId, sporerDirection));
                    }
                }
            }

            return moveActions;
        }

        public List<MoveAction> GetTentacleMoveActions(int organRootId, bool isMine)
        {
            List<MoveAction> moveActions = new List<MoveAction>();

            List<MoveAction> growMoveActions = GetGrowBasicMoveActions(organRootId, isMine);

            foreach (MoveAction growAction in growMoveActions)
            {
                foreach (OrganDirection tentacleDirection in PossibleDirections)
                {
                    if (IsOpponentOrEmptySpace(growAction.Location, tentacleDirection, isMine))
                    {
                        if (IsOpponentWithin3Spaces(growAction.Location, isMine))
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
            return Entities.Count == (Height * Width);
        }

        private Dictionary<string, List<MoveAction>> _moveActionCache = new Dictionary<string, List<MoveAction>>();
        public List<MoveAction> GetGrowBasicMoveActions(int organRootId, bool isMine)
        {
            var key = organRootId.ToString() + isMine.ToString();
            if (!_moveActionCache.TryGetValue(key, out List<MoveAction> moveActions))
            {
                moveActions = new List<MoveAction>();
                var entities = GetEntitiesByRoot(organRootId, isMine);
                foreach (Entity entity in entities)
                {
                    foreach (OrganDirection direction in PossibleDirections)
                    {
                        if (IsOpenSpace(entity.Location, direction))
                        {
                            moveActions.Add(MoveAction.CreateGrow(entity.OrganId, GetNextLocation(entity.Location, direction), EntityType.BASIC, entity.OrganRootId));
                        }
                    }
                }
                _moveActionCache[key] = moveActions;
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
                    if (IsHarvestSpace(growAction.Location, direction))
                    {
                        moveActions.Add(MoveAction.CreateGrow(growAction.OrganId, growAction.Location, EntityType.HARVESTER, growAction.OrganRootId, direction));
                    }
                }
            }

            return moveActions;
        }

        public bool IsOpponentWithin3Spaces(Point2d location, bool isMine)
        {
            var oppEntities = GetEntities(!isMine);
            foreach (var oppEntity in oppEntities)
            {
                if (oppEntity.Location.GetManhattenDistance(location) <= 3)
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
            return ValidateLocation(location, direction) && GetEntityWithDirection(location, direction, out Entity entity) && entity.IsMine != isMine;
        }
        public bool IsHarvestSpace(Point2d location, OrganDirection nextDirection)
        {
            return ValidateLocation(location, nextDirection) && GetEntityWithDirection(location, nextDirection, out Entity entity) && entity.IsOpenSpace();
        }

        public bool IsOpenSpace(Point2d location, OrganDirection nextDirection)
        {
            return ValidateLocation(location, nextDirection) && (!GetEntityWithDirection(location, nextDirection, out Entity entity) || entity.IsOpenSpace());
        }

        public bool ValidateLocation(Point2d location, OrganDirection nextDirection)
        {
            return GetNextLocation(location, nextDirection) != null;
        }

        public bool GetEntityWithDirection(Point2d currentLocation, OrganDirection nextDirection, out Entity entity)
        {
            return Entities.TryGetValue(GetNextLocation(currentLocation, nextDirection), out entity);
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
            foreach (Entity harvester in harvesters)
            {
                if (GetEntityWithDirection(harvester.Location, harvester.OrganDirection, out Entity entity) && entity.IsOpenSpace())
                {
                    proteins[entity.Type - EntityType.A]++;
                }
            }
        }

        public bool GetEntity(Point2d location, out Entity entity)
        {
            return Entities.TryGetValue(location, out entity);
        }

        public Entity GetEntityByLocation(Point2d location)
        {
            Entities.TryGetValue(location, out Entity entity);
            return entity;
        }

        public List<Entity> GetRootEntities(bool isMine)
        {
            return GetEntities(isMine).Where(e => e.Type == EntityType.ROOT).ToList();
        }

        public List<Entity> GetSporerEntities(int organRootId, bool isMine)
        {
            return GetEntitiesByRoot(organRootId, isMine).Where(e => e.Type == EntityType.SPORER).ToList();
        }


        private Dictionary<string, List<Entity>> _entityCache = new Dictionary<string, List<Entity>>();
        public List<Entity> GetEntities(bool isMine)
        {
            var key = isMine.ToString();
            if (!_entityCache.TryGetValue(key, out List<Entity> entities))
            {
                entities = Entities.Values.Where(e => e.IsMine.HasValue && e.IsMine.Value == isMine).ToList();
            }
            return entities;
        }

        public List<Entity> GetEntitiesByRoot(int organRootId, bool isMine)
        {
            var key = organRootId.ToString() + isMine.ToString();
            if (!_entityCache.TryGetValue(key, out List<Entity> entities))
            {
                entities = GetEntities(isMine).Where(e => e.OrganRootId == organRootId).ToList();
            }
            return entities;
        }

        public Dictionary<Point2d, Entity> GetEntities()
        {
            return Entities;
        }

        private int _myEntityCount = -1;
        public int GetMyEntityCount()
        {
            if (_myEntityCount < 0)
            {
                _myEntityCount = GetEntities(true).Count();
            }

            return _myEntityCount;
        }

        private int _oppEntityCount = -1;
        public int GetOppEntityCount()
        {
            if (_oppEntityCount < 0)
            {
                _oppEntityCount = GetEntities(false).Count();
            }
            return _oppEntityCount;
        }

        public void SetEntities(IList<Entity> entities)
        {
            UpdateBoard();
            Entities.Clear();
            foreach (Entity entity in entities)
            {
                Entities[entity.Location] = entity;
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
                    if (!Entities.TryGetValue(new Point2d(x, y), out Entity entity))
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
        }
    }
}
