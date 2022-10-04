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
        private LocationType[][] Locations;
        public static int MaxHeight = 7;
        public static int MaxWidth = 13;
        public Point2d Target;

        public Point2d[][][][][] BresenhamDictionary;
        public static int[][][][] ManhattenDictionary = null;

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

            CreateManhattenDictionary();
            CreateBresenhamDictionary();
        }

        public static int GetManhattenDistance(Point2d point, Point2d targetPoint)
        {
            return ManhattenDictionary[point.x][point.y][targetPoint.x][targetPoint.y];
        }

        private static void CreateManhattenDictionary()
        {
            ManhattenDictionary = new int[MaxWidth][][][];
            for (int x = 0; x < MaxWidth; x++)
            {
                ManhattenDictionary[x] = new int[MaxHeight][][];
                for (int y = 0; y < MaxHeight; y++)
                {
                    var point = new Point2d(x, y);
                    ManhattenDictionary[x][y] = new int[MaxWidth][];
                    for (int tx = 0; tx < MaxWidth; tx++)
                    {
                        ManhattenDictionary[x][y][tx] = new int[MaxHeight];
                        for (int ty = 0; ty < MaxHeight; ty++)
                        {
                            var targetPoint = new Point2d(tx, ty);
                            ManhattenDictionary[x][y][tx][ty] = point.GetManhattenDistance(targetPoint);
                        }
                    }
                }
            }
        }

        private void CreateBresenhamDictionary()
        {
            BresenhamDictionary = new Point2d[MaxWidth][][][][];
            
            for(int x = 0; x<MaxWidth; x++)
            {
                BresenhamDictionary[x] = new Point2d[MaxHeight][][][];
                for(int y = 0; y<MaxHeight; y++)
                {
                    var point = new Point2d(x, y);
                    BresenhamDictionary[x][y] = new Point2d[MaxWidth][][];
                    for (int tx = 0; tx < MaxWidth; tx++)
                    {
                        BresenhamDictionary[x][y][tx] = new Point2d[MaxHeight][];
                        for (int ty = 0; ty < MaxHeight; ty++)
                        {
                            var targetPoint = new Point2d(tx, ty);
                            if (point.Equals(targetPoint))
                                continue;

                            if (GetManhattenDistance(point, targetPoint) > 6)
                                continue;

                            if (y < ty)
                                BresenhamDictionary[x][y][tx][ty] = bresenhamForward(point, targetPoint);
                            else
                                BresenhamDictionary[x][y][tx][ty] = bresenhamBackward(point, targetPoint);
                        }
                    }
                }
            }
        }

        private Point2d[] bresenhamForward(Point2d startTile, Point2d targetTile)
        {
            List<Point2d> bresenhamPoints = new List<Point2d>();
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

                bresenhamPoints.Add(new Point2d(currentX, currentY));

                if (currentX == x1 && currentY == y1) break;
                if (GetLocation(currentX, currentY) == LocationType.Obstacle)
                {
                    return bresenhamPoints.ToArray();
                }
            }

            return bresenhamPoints.ToArray();
        }

        private Point2d[] bresenhamBackward(Point2d startTile, Point2d targetTile)
        {
            List<Point2d> bresenhamPoints = new List<Point2d>();
            int x0, y0, x1, y1;

            x0 = targetTile.x;
            y0 = targetTile.y;
            x1 = startTile.x;
            y1 = startTile.y;

            bresenhamPoints.Add(targetTile);

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
                bresenhamPoints.Add(new Point2d(currentX, currentY));
            }
            
            bresenhamPoints.Reverse();
            return bresenhamPoints.ToArray();
        }

        public Point2d[] GetBresenhamPoints(Point2d startPoint, Point2d targetPoint)
        {
            return BresenhamDictionary[startPoint.x][startPoint.y][targetPoint.x][targetPoint.y];
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
