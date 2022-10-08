using System;

namespace GameSolution
{
    public static class BombDirection
    {
        public const string Unknown = "UNKNOWN";
        public const string Same = "SAME";
        public const string Warmer = "WARMER";
        public const string Colder = "COLDER";
    }

    public class Game
    {
        public int LargestX, LargestY, Turns, StartX, StartY;
        public string BombDir;

        public int currentX = -1, currentY = -1, previousX, previousY;
        public int leftX = 0, rightX;
        public int leftY = 0, rightY;
        public bool isXScan = true;

        public Game(int largestX, int largestY, int turns, int startX, int startY)
        {
            LargestX = largestX;
            LargestY = largestY;
            Turns = turns;
            StartX = startX;
            StartY = startY;

            rightX = largestX - 1;
            rightY = largestY - 1;

        }

        public void SetState(string bombDirection)
        {
            BombDir = bombDirection;
        }

        public Tuple<int, int> GetNextMove()
        {
            previousX = currentX;
            previousY = currentY;

            currentX = StartX;
            currentY = StartY;


            switch (BombDir)
            {
                case BombDirection.Unknown:
                    StartX = FindNextJump(currentX, previousX, leftX, rightX);
                    //Y0 = largestY - Y0 - 1;
                    break;
                case BombDirection.Same:
                    if (currentX == previousX)
                    {
                        if (isXScan)
                        {
                            isXScan = false;
                            StartY = (rightY + leftY) / 2;
                        }
                        else
                        {
                            if (previousY < currentY)
                            {
                                leftY = previousY;
                                rightY = currentY;
                            }
                            else
                            {
                                leftY = currentY;
                                rightY = previousY;
                            }
                            StartY = (rightY + leftY) / 2;
                        }
                    }
                    else
                    {
                        leftX = rightX = (currentX + previousX) / 2;
                        StartX = leftX;
                        isXScan = false;
                    }
                    break;
                case BombDirection.Colder:
                    if (isXScan)
                    {
                        if (previousX < currentX)
                        {
                            rightX = Math.Max(leftX, Math.Min(rightX, (previousX + currentX) / 2));
                            StartX = FindNextJump(currentX, previousX, leftX, rightX);
                        }
                        else
                        {
                            leftX = Math.Min(rightX, Math.Max(leftX, (previousX + currentX) / 2 + 2));
                            StartX = FindNextJump(currentX, previousX, leftX, rightX);
                        }
                    }
                    else
                    {
                        if (previousY < currentY)
                        {
                            rightY = Math.Max(leftY, Math.Min(rightY, (previousY + currentY) / 2));
                            StartY = FindNextJump(currentY, previousY, leftY, rightY);
                        }
                        else
                        {
                            leftY = Math.Min(rightY, Math.Max(leftY, (previousY + currentY) / 2 + 2));
                            StartY = FindNextJump(currentY, previousY, leftY, rightY);
                        }

                    }
                    break;
                case BombDirection.Warmer:
                    if (isXScan)
                    {
                        if (previousX < currentX)
                        {
                            leftX = Math.Min(rightX, Math.Max(leftX, (currentX + previousX) / 2 + 1));
                            StartX = FindNextJump(currentX, previousX, leftX, rightX);
                        }
                        else
                        {
                            rightX = Math.Max(leftX, Math.Min(rightX, (currentX + previousX) / 2));
                            StartX = FindNextJump(currentX, previousX, leftX, rightX);
                        }
                    }
                    else
                    {
                        if (previousY < currentY)
                        {
                            leftY = Math.Min(rightY, Math.Max(leftY, (currentY + previousY) / 2 + 1));
                            StartY = FindNextJump(currentY, previousY, /*leftY*/ currentY, rightY);
                        }
                        else
                        {
                            rightY = Math.Max(leftY, Math.Min(rightY, (currentY + previousY) / 2));
                            StartY = FindNextJump(currentY, previousY, leftY, /*rightY*/ currentY);
                        }
                    }
                    break;
            }

            if (leftX == rightX && isXScan)
            {
                isXScan = false;
                StartY = FindNextJump(currentY, previousY, leftY, rightY);
            }

            Console.Error.WriteLine($"X: {leftX}, {rightX} Y: {leftY}, {rightY}");

            return Tuple.Create(StartX, StartY);
        }

        static int FindNextJump(int current, int previous, int left, int right)
        {
            var midPoint = (left + right) / 2;
            if (current == midPoint && left != right)
                return current + 1;
            else
                return midPoint;
        }
    }
}
