using Algorithms.Space;
using System;
using System.Collections.Generic;
using System.Text;

namespace GameSolution.Entities
{
    public enum LocationType
    {
        Empty = 0,
        Ground = 1,
        Exit = 2
    }

    public class Board
    {
        private LocationType[][] Locations;
        public static int MaxHeight = 10;
        public static int MaxWidth = 19;
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
                    if(spot == '#')
                    {
                        Locations[r][c] = LocationType.Empty;
                    }
                    else if(spot == '.')
                    {
                        Locations[r][c] = LocationType.Ground;
                    }
                    else if(spot == '0')
                    {
                        Locations[r][c] = LocationType.Exit;
                        Target = new Point2d(c, r);
                    }
                }
            }
        }

        public LocationType GetLocation(int row, int column)
        {
            return Locations[row][column]; 
        }

        public bool IsInBounds(int x, int y)
        {
            if (x < 0 || y < 0 || x >= MaxWidth || y >= MaxHeight)
                return false;
            return true;
        }

        public bool IsLegalMoveForCar(int r, int c)
        {
            var location = GetLocation(r, c);
            if (IsInBounds(r, c) && IsFormalGround(location))
            {
                return true;
            }
            return false;
        }

        public bool IsFormalGround(LocationType location)
        {
            return location == LocationType.Ground || location == LocationType.Exit;
        }

        public bool IsLegalMoveForPlatform(char direction, int r, int c, out Point2d point)
        {
            if(direction == 'D')
            {
                int tempR = r + 1;
                while (IsInBounds(tempR, c))
                {
                    if (IsFormalGround(GetLocation(tempR, c))) { 
                        point = new Point2d(c, tempR - 1);
                        return true;
                    }
                    tempR++;
                }
            }
            else if(direction == 'U')
            {
                int tempR = r - 1;
                while (IsInBounds(tempR, c))
                {
                    if (IsFormalGround(GetLocation(tempR, c)))
                    {
                        point = new Point2d(c, tempR + 1);
                        return true;
                    }
                    tempR--;
                }
            }
            else if (direction == 'L')
            {
                int tempC = c - 1;
                while (IsInBounds(r, tempC))
                {
                    if (IsFormalGround(GetLocation(r, tempC)))
                    {
                        point = new Point2d(tempC + 1, r);
                        return true;
                    }
                    tempC--;
                }
            }
            else if (direction == 'R')
            {
                int tempC = c + 1;
                while (IsInBounds(r, tempC))
                {
                    if (IsFormalGround(GetLocation(r, tempC)))
                    {
                        point = new Point2d(tempC - 1, r);
                        return true;
                    }
                    tempC++;
                }
            }
            point = null;
            return false;
        }


        public string GetLegalDirections(Entity entity)
        {
            string directions = "";
            if(entity.IsCar)
            {
                int c = entity.Point.GetTruncatedX();
                int r = entity.Point.GetTruncatedY();
                if(IsLegalMoveForCar(r - 1, c))
                {
                    entity.LegalLocations['D'] = new Point2d(c, r - 1);
                    directions += 'D';
                }
                if (IsLegalMoveForCar(r + 1, c))
                {
                    entity.LegalLocations['U'] = new Point2d(c, r + 1);
                    directions += 'U';
                }
                if (IsLegalMoveForCar(r, c-1))
                {
                    entity.LegalLocations['L'] = new Point2d(c - 1, r);
                    directions += 'L';
                }
                if (IsLegalMoveForCar(r, c+1))
                {
                    entity.LegalLocations['R'] = new Point2d(c + 1, r);
                    directions += 'R';
                }
            }
            else
            {
                foreach(char direction in entity.Directions)
                {
                    if (IsLegalMoveForPlatform(direction, entity.Point.GetTruncatedY(), entity.Point.GetTruncatedX(), out Point2d point))
                    {
                        entity.LegalLocations[direction] = point;
                        directions += direction;
                    }
                }
            }

            return directions;
        }
    }
}
