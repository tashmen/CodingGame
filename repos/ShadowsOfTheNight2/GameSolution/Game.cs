﻿using System;

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
                    StartX = FindNextJump(currentX, previousX, leftX, rightX, LargestX);
                    break;
                case BombDirection.Same:
                    if(isXScan)
                    {
                        leftX = rightX = (currentX + previousX) / 2;
                    }
                    else
                    {
                        leftY = rightY = (currentY + previousY) / 2;
                    }
                    break;
                case BombDirection.Colder:
                    if (isXScan)
                    {
                        if (previousX < currentX)
                        {
                            rightX = UpdateRight(leftX, rightX, previousX, currentX);
                            StartX = FindNextJump(currentX, previousX, leftX, rightX, LargestX);
                        }
                        else
                        {
                            leftX = UpdateLeft(leftX, rightX, previousX, currentX);
                            StartX = FindNextJump(currentX, previousX, leftX, rightX, LargestX);
                        }
                    }
                    else
                    {
                        if (previousY < currentY)
                        {
                            rightY = UpdateRight(leftY, rightY, previousY, currentY);
                            StartY = FindNextJump(currentY, previousY, leftY, rightY, LargestY);
                        }
                        else
                        {
                            leftY = UpdateLeft(leftY, rightY, previousY, currentY);
                            StartY = FindNextJump(currentY, previousY, leftY, rightY, LargestY);
                        }

                    }
                    break;
                case BombDirection.Warmer:
                    if (isXScan)
                    {
                        if (previousX < currentX)
                        {
                            leftX = UpdateLeft(leftX, rightX, previousX, currentX);
                            StartX = FindNextJump(currentX, previousX, leftX, rightX, LargestX);
                        }
                        else
                        {
                            rightX = UpdateRight(leftX, rightX, previousX, currentX);
                            StartX = FindNextJump(currentX, previousX, leftX, rightX, LargestX);
                        }
                    }
                    else
                    {
                        if (previousY < currentY)
                        {
                            leftY = UpdateLeft(leftY, rightY, previousY, currentY);
                            StartY = FindNextJump(currentY, previousY, leftY, rightY, LargestY);
                        }
                        else
                        {
                            rightY = UpdateRight(leftY, rightY, previousY, currentY);
                            StartY = FindNextJump(currentY, previousY, leftY, rightY, LargestY);
                        }
                    }
                    break;
            }

            if (leftX == rightX && isXScan && currentX == StartX)
            {
                isXScan = false;
                StartY = FindNextJump(currentY, previousY, leftY, rightY, LargestY);
            }
            
            if(leftY == rightY && !isXScan && leftX == rightX)
            {
                StartY = leftY;
                StartX = leftX;
            }

            Console.Error.WriteLine($"X: {leftX}, {rightX} Y: {leftY}, {rightY}");

            return Tuple.Create(StartX, StartY);
        }

        static int FindNextJump(int current, int previous, int left, int right, int largest)
        {
            var midPoint = (left + right) / 2;
            if (current == midPoint && left != right)
                return current + 1;
            else
            {
                var calculatedMiddle = Math.Max(0, Math.Min(largest - 1, 2 * midPoint - current));
                if(calculatedMiddle == largest-1 || calculatedMiddle == 0)
                    return midPoint;
                return calculatedMiddle;
            }
                
        }

        static int UpdateRight(int left, int right, int previous, int current)
        {
            return Math.Max(left, Math.Min(right, (int)Math.Ceiling((previous + current) / 2.0) - 1));
        }

        static int UpdateLeft(int left, int right, int previous, int current)
        {
            return Math.Min(right, Math.Max(left, (int)((previous + current) / 2.0) + 1));
        }
    }
}
