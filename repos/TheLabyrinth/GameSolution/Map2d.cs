using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameSolution
{
    public enum Location
    {
        unknown = 0,
        wall,
        open,
        startPosition,
        controlRoom
    }
    public class Map2d
    {
        List<List<Location>> _map;
        string[] _strMap;
        bool _controlRoomFound = false;
        Tuple<int, int> _controlRoom;
        Tuple<int, int> _startLocation;
        public Map2d(string[] map, Tuple<int, int> currentLocation)
        {
            _strMap = map;
            _map = new List<List<Location>>();
            int y = 0;
            foreach(string row in map)
            {
                int x = 0;
                List<Location> rows = new List<Location>();
                foreach(char c in row)
                {
                    switch (c)
                    {
                        case '#':
                            rows.Add(Location.wall);
                            break;
                        case '?':
                            rows.Add(Location.unknown);
                            break;
                        case '.':
                            rows.Add(Location.open);
                            break;
                        case 'T':
                            rows.Add(Location.startPosition);
                            _startLocation = new Tuple<int, int>(x, y);
                            break;
                        case 'C':
                            rows.Add(Location.controlRoom);
                            _controlRoomFound = true;
                            _controlRoom = new Tuple<int, int>(x, y);
                            break;
                    }
                    if (x == currentLocation.Item1 && y == currentLocation.Item2)
                    {
                        var array = _strMap[y].ToCharArray();
                        array[x] = 'X';
                        _strMap[y] = new string(array);
                    }
                    x++;
                }
                _map.Add(rows);
                y++;
            }
        }

        public Tuple<int, int> GetStartLocation()
        {
            return _startLocation;
        }

        public Tuple<int, int> GetControlRoomLocation()
        {
            return _controlRoom;
        }

        public bool isControlRoomFound()
        {
            return _controlRoomFound;
        }

        public bool isPassable(Location location)
        {
            return location != Location.wall;
        }

        public Location GetLocation(int x, int y)
        {
            if (y > _map.Count || x > _map[y].Count || y<0 || x<0)
                return Location.wall;
            return _map[y][x];
        }


        public void Print()
        {
            foreach(string str in _strMap)
            {
                Console.Error.WriteLine(str);
            }
            /*
            foreach(List<Location> locations in _map)
            {
                foreach(Location location in locations)
                {
                    Console.Error.Write((int)location);
                }
                Console.Error.WriteLine();
            }
            */
        }
    }
}
