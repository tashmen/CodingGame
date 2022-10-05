using Algorithms.Space;
using System;
using System.Collections.Generic;

namespace GameSolution.Entities
{
    public enum LocationType
    {
        Empty = -2,
        Obstacle = -1,
    }

    public class Board
    {
        private LocationType[] Locations;

        private Int128 ObstacleBoard;
        private Int128 EmptyBoard;

        public static int MaxHeight = 7;
        public static int MaxWidth = 13;

        private int[][][] BresenhamDictionary;
        private static int[][] ManhattenDictionary = null;
        private int[][] NeighboringLocations = null; 


        public Board(string[] board)
        {
            ObstacleBoard = new Int128();
            EmptyBoard = new Int128();
            Locations = new LocationType[board.Length * board[0].Length];
            int locationIndex = 0;
            for(int r = 0; r <board.Length; r++)
            {
                for(int c = 0; c < board[r].Length; c++)
                {
                    var spot = board[r][c];
                    if(spot == '.')
                    {
                        EmptyBoard.SetBit(locationIndex);
                        Locations[locationIndex++] = LocationType.Empty;
                    }
                    else if(spot == 'x')
                    {
                        ObstacleBoard.SetBit(locationIndex);
                        Locations[locationIndex++] = LocationType.Obstacle;
                    }
                }
            }

            CreateManhattenDictionary();
            CreateBresenhamDictionary();
            CreateNeighboringLocations();
        }

        public static int ConvertPointToLocation(Point2d point)
        {
            return ConvertPointToLocation(point.x, point.y);
        }

        public static int ConvertPointToLocation(int x, int y)
        {
            return y * MaxWidth + x;
        }

        public static Point2d ConvertLocationToPoint(int location)
        {
            return new Point2d(location % MaxWidth, location / MaxWidth);
        }

        public static int GetManhattenDistance(int location, int targetLocation)
        {
            return ManhattenDictionary[location][targetLocation];
        }

        public int[] GetBresenhamPoints(int location, int targetLocation)
        {
            return BresenhamDictionary[location][targetLocation];
        }

        public int[] GetNeighboringLocations(int location)
        {
            return NeighboringLocations[location];
        }

        public LocationType GetLocation(int x, int y)
        {
            return Locations[y * MaxWidth + x]; 
        }

        public bool IsInBounds(int x, int y)
        {
            if (x < 0 || y < 0 || x >= MaxWidth || y >= MaxHeight)
                return false;
            return true;
        }


        private static void CreateManhattenDictionary()
        {
            ManhattenDictionary = new int[MaxWidth * MaxHeight][];
            for (int x = 0; x < MaxWidth; x++)
            {
                for (int y = 0; y < MaxHeight; y++)
                {
                    var point = new Point2d(x, y);
                    ManhattenDictionary[ConvertPointToLocation(x, y)] = new int[MaxWidth * MaxHeight];
                    for (int tx = 0; tx < MaxWidth; tx++)
                    {
                        for (int ty = 0; ty < MaxHeight; ty++)
                        {
                            var targetPoint = new Point2d(tx, ty);
                            ManhattenDictionary[ConvertPointToLocation(x, y)][ConvertPointToLocation(tx, ty)] = point.GetManhattenDistance(targetPoint);
                        }
                    }
                }
            }
        }

        private void CreateBresenhamDictionary()
        {
            BresenhamDictionary = new int[MaxWidth * MaxHeight][][];

            for (int x = 0; x < MaxWidth; x++)
            {
                for (int y = 0; y < MaxHeight; y++)
                {
                    var point = new Point2d(x, y);
                    BresenhamDictionary[ConvertPointToLocation(x, y)] = new int[MaxWidth * MaxHeight][];
                    for (int tx = 0; tx < MaxWidth; tx++)
                    {
                        for (int ty = 0; ty < MaxHeight; ty++)
                        {
                            var targetPoint = new Point2d(tx, ty);
                            if (point.Equals(targetPoint))
                                continue;

                            if (GetManhattenDistance(ConvertPointToLocation(point), ConvertPointToLocation(targetPoint)) > 6)
                                continue;

                            if (y < ty)
                                BresenhamDictionary[ConvertPointToLocation(x, y)][ConvertPointToLocation(tx, ty)] = bresenhamForward(point, targetPoint);
                            else
                                BresenhamDictionary[ConvertPointToLocation(x, y)][ConvertPointToLocation(tx, ty)] = bresenhamBackward(point, targetPoint);
                        }
                    }
                }
            }
        }


        private static int[] X_MODIFIER = new int[] { 0, 1, 0, -1 };
        private static int[] Y_MODIFIER = new int[] { -1, 0, 1, 0 };
        private void CreateNeighboringLocations()
        {
            NeighboringLocations = new int[MaxHeight * MaxWidth][];
            for (int x = 0; x < MaxWidth; x++)
            {
                for (int y = 0; y < MaxHeight; y++)
                {
                    int location = ConvertPointToLocation(x, y);
                    NeighboringLocations[location] = new int[4];
                    for (int i = 0; i < 4; i++)
                    {
                        int cx = x + X_MODIFIER[i];
                        int cy = y + Y_MODIFIER[i];

                        if (IsInBounds(cx, cy))
                        {
                            NeighboringLocations[location][i] = ConvertPointToLocation(cx, cy);
                        }
                        else
                        {
                            NeighboringLocations[location][i] = (int)LocationType.Obstacle;
                        }
                    }
                }
            }
        }

        private int[] bresenhamForward(Point2d startTile, Point2d targetTile)
        {
            List<int> bresenhamPoints = new List<int>();
            int x0, y0, x1, y1;
            x0 = startTile.x;
            y0 = startTile.y;
            x1 = targetTile.x;
            y1 = targetTile.y;


            int dx = Math.Abs(x1 - x0);
            int dy = Math.Abs(y1 - y0);

            int sx = x0 < x1 ? 1 : -1;
            int sy = y0 < y1 ? 1 : -1;

            int err = dx - dy;
            int e2;
            int currentX = x0;
            int currentY = y0;

            while (true)
            {
                e2 = 2 * err;
                if (e2 > -1 * dy)
                {
                    err -= dy;
                    currentX += sx;
                }

                if (e2 < dx)
                {
                    err += dx;
                    currentY += sy;
                }

                bresenhamPoints.Add(ConvertPointToLocation(currentX, currentY));

                if (currentX == x1 && currentY == y1) break;
                if (GetLocation(currentX, currentY) == LocationType.Obstacle)
                {
                    return bresenhamPoints.ToArray();
                }
            }

            return bresenhamPoints.ToArray();
        }

        private int[] bresenhamBackward(Point2d startTile, Point2d targetTile)
        {
            List<int> bresenhamPoints = new List<int>();
            int x0, y0, x1, y1;

            x0 = targetTile.x;
            y0 = targetTile.y;
            x1 = startTile.x;
            y1 = startTile.y;

            bresenhamPoints.Add(ConvertPointToLocation(targetTile));

            int dx = Math.Abs(x1 - x0);
            int dy = Math.Abs(y1 - y0);

            int sx = x0 < x1 ? 1 : -1;
            int sy = y0 < y1 ? 1 : -1;

            int err = dx - dy;
            int e2;
            int currentX = x0;
            int currentY = y0;

            while (true)
            {
                e2 = 2 * err;
                if (e2 > -1 * dy)
                {
                    err -= dy;
                    currentX += sx;
                }

                if (e2 < dx)
                {
                    err += dx;
                    currentY += sy;
                }

                if (currentX == x1 && currentY == y1) break;

                var location = GetLocation(currentX, currentY);
                if (location == LocationType.Obstacle)
                {
                    bresenhamPoints.Clear();
                }
                bresenhamPoints.Add(ConvertPointToLocation(currentX, currentY));
            }

            bresenhamPoints.Reverse();
            return bresenhamPoints.ToArray();
        }
    }
}
