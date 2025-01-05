using Algorithms.Graph;
using GameSolution.Entities;
using System.Diagnostics;
using static GameSolution.Entities.Board;

namespace Benchmark
{
    public class Board3
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
        const int _maxMovesTotal = 10;
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

        public Board3()
        {

        }
    }
}
