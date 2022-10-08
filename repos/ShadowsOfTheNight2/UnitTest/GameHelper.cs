using GameSolution;
using System;

namespace UnitTest
{
    public class GameHelper
    {
        public static void PlayGame(Game game, int bombX, int bombY)
        {
            string bombDirection = BombDirection.Unknown;
            int x0 = game.StartX;
            int y0 = game.StartY;
            Tuple<int, int> result;
            do
            {
                game.SetState(bombDirection);
                result = game.GetNextMove();
                game.Turns--;
                var currentDistance = GetDistance(result.Item1, result.Item2, bombX, bombY);
                var previousDistance = GetDistance(x0, y0, bombX, bombY);
                if (currentDistance < previousDistance)
                    bombDirection = BombDirection.Warmer;
                else if (currentDistance > previousDistance)
                    bombDirection = BombDirection.Colder;
                else if (currentDistance == previousDistance)
                    bombDirection = BombDirection.Same;
                Console.Error.WriteLine($"Batman moved from ({x0}, {y0}) to ({result.Item1}, {result.Item2}).\nBatman is now {bombDirection}");
                x0 = result.Item1;
                y0 = result.Item2;
            }
            while (!(result.Item1 == bombX && result.Item2 == bombY) && game.Turns > 0);
        }

        public static double GetDistance(int x, int y, int x1, int y1)
        {
            return Math.Sqrt(Math.Pow(x - x1, 2)    + Math.Pow(y - y1, 2));
        }
    }
}
