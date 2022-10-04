using Algorithms.Space;

namespace GameSolution.Entities
{
    public enum LocationType
    {
        Empty = -2,
        Obstacle = -1,
    }

    public class Board
    {
        private LocationType[][] Locations;
        public static int MaxHeight = 7;
        public static int MaxWidth = 13;
        public Point2d Target;

        public Board(string[] board)
        {
            Locations = new LocationType[board.Length][];
            for(int r = 0; r <board.Length; r++)
            {
                Locations[r] = new LocationType[board[r].Length];
                for(int c = 0; c < board[r].Length; c++)
                {
                    var spot = board[r][c];
                    if(spot == '.')
                    {
                        Locations[r][c] = LocationType.Empty;
                    }
                    else if(spot == 'x')
                    {
                        Locations[r][c] = LocationType.Obstacle;
                    }
                }
            }
        }

        public LocationType GetLocation(int x, int y)
        {
            return Locations[y][x]; 
        }

        public bool IsInBounds(int x, int y)
        {
            if (x < 0 || y < 0 || x >= MaxWidth || y >= MaxHeight)
                return false;
            return true;
        }
    }
}
