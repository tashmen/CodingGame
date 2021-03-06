/*
 * File generated by SourceCombiner.exe using 18 source files.
 * Created On: 4/26/2022 2:08:42 PM
*/
using Algorithms.GameComponent;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Numerics;
//*** SourceCombiner -> original file IGameState.cs ***
namespace Algorithms.GameComponent
{
    public interface IGameState
    {
        /// <summary>
        /// Retrieve the possible moves
        /// </summary>
        /// <param name="isMax">Whether or not to retrieve moves for max</param>
        /// <returns>list of all possible moves</returns>
        IList GetPossibleMoves(bool isMax);
        /// <summary>
        /// Applies a move to the game state.  The game state must remember this move so that it can be retrieves with GetMove.
        /// </summary>
        /// <param name="isMax">Whether or not the move is for max</param>
        /// <param name="move">the move to apply</param>
        void ApplyMove(object move, bool isMax);
        /// <summary>
        /// Retrieves the move that was played to reach this state.
        /// </summary>
        /// <param name="isMax">Whether or not the move is for max</param>
        /// <returns>The move</returns>
        object GetMove(bool isMax);
        /// <summary>
        /// Clones the game state
        /// </summary>
        /// <returns>The copy of the state</returns>
        IGameState Clone();
        /// <summary>
        /// Returns whether or not the game is over and who won (1 - max wins, 0 - draw, -1 - min wins, null - game is not over)
        /// </summary>
        /// <returns>Who won the game</returns>
        double? GetWinner();
        /// <summary>
        /// Determines if the game state is the same as this one
        /// </summary>
        /// <param name="">the state to compare against</param>
        /// <returns>true if equal</returns>
        bool Equals(IGameState state);
        /// <summary>
        /// Evaluates the current game board closer to 1 is max wins closer to -1 is min wins
        /// </summary>
        /// <param name="isMax">true if it is max's turn else false</param>
        /// <returns>A number between [-1, 1]</returns>
        double Evaluate(bool isMax);
    }
}
namespace Algorithms.Space
{
    public class Circle2d : Point2d
    {
        public double radius { get; set; }
        public Circle2d(double x, double y, double radius) : base(x, y)
        {
            this.radius = radius;
        }
    }
}
//*** SourceCombiner -> original file Point2d.cs ***
public class Point2d
{
    public double x { get; private set; }
    public double y { get; private set; }

    public Point2d(double x, double y)
    {
        this.x = x;
        this.y = y;
    }

    public Point2d(Point2d point)
    {
        x = point.x;
        y = point.y;
    }

    public override string ToString()
    {
        return $"({x},{y})";
    }

    public bool Equals(Point2d point)
    {
        return point.x == this.x && point.y == this.y;
    }

    public Point2d GetTruncatedPoint()
    {
        return new Point2d(Math.Truncate(this.x), Math.Truncate(this.y));
    }

    public Point2d GetRoundedPoint()
    {
        return new Point2d(Math.Round(this.x), Math.Round(this.y));
    }

    public Point2d GetCeilingPoint()
    {
        return new Point2d(Math.Ceiling(x), Math.Ceiling(y));
    }

    public int GetTruncatedX()
    {
        return (int)x;
    }
    public int GetTruncatedY()
    {
        return (int)y;
    }

    public double GetAngle(Point2d point)
    {
        return Math.Atan2(point.y - y, point.x - x);
    }

    public double GetDistance(Point2d point)
    {
        return GetDistance(point.x, point.y, x, y);
    }

    public Point2d GetMidPoint(Point2d point)
    {
        return GetMidPoint(point.x, point.y, x, y);
    }

    public double LengthSquared()
    {
        return x * x + y * y;
    }

    public double Length()
    {
        return Math.Sqrt(LengthSquared());
    }

    public Point2d Normalize()
    {
        var length = Length();
        if (length == 0)
        {
            x = 0;
            y = 0;
        }
        else
        {
            x = x / length;
            y = y / length;
        }
        return this;
    }

    public Point2d Multiply(double scalar)
    {
        x = x * scalar;
        y = y * scalar;
        return this;
    }

    public Point2d Add(Point2d vector)
    {
        x = x + vector.x;
        y = y + vector.y;
        return this;
    }

    public Point2d Subtract(Point2d vector)
    {
        x = x - vector.x;
        y = y - vector.y;
        return this;
    }

    public Point2d Truncate()
    {
        x = GetTruncatedX();
        y = GetTruncatedY();
        return this;
    }

    public Point2d SymmetricTruncate(Point2d origin)
    {
        Subtract(origin).Truncate().Add(origin);
        return this;
    }

    public Point2d Clone()
    {
        return new Point2d(x, y);
    }

    public static double GetDistance(double x1, double y1, double x2, double y2)
    {
        return Math.Sqrt(Math.Pow(x1 - x2, 2) + Math.Pow(y1 - y2, 2));
    }

    public static Point2d GetMidPoint(double x1, double y1, double x2, double y2)
    {
        return new Point2d(Math.Abs(x1 - x2) / 2, Math.Abs(y1 - y2) / 2);
    }
}
//*** SourceCombiner -> original file Space2d.cs ***
namespace Algorithms.Space
{
    public class Space2d
    {
        /// <summary>
        /// Given a list of points and a circle radius, find the circle location that covers the maximum number of points
        /// </summary>
        /// <param name="points">The list of points to consider</param>
        /// <param name="radius">The radius of the circle</param>
        /// <returns>The number of points covered by the circle that is centered at the point.</returns>
        public static Tuple<int, Point2d> FindCircleWithMaximumPoints(Point2d[] points, double radius)
        {
            Tuple<int, Point2d> maxPoint = null;
            if (points == null)
                return null;
            if (radius <= 0)
                return null;

            var numberOfPoints = points.Count();
            double[,] distance = new double[numberOfPoints, numberOfPoints];
            for (int i = 0; i < numberOfPoints - 1; i++)
            {
                for (int j = i + 1; j < numberOfPoints; j++)
                {
                    distance[i, j] = distance[j, i] = points[i].GetDistance(points[j]);
                }
            }

            for (int i = 0; i < numberOfPoints; i++)
            {
                var currentAnswer = GetPointsInside(distance, points, i, radius, numberOfPoints);
                var nextPoint = new Tuple<int, Point2d>(currentAnswer.Item1, new Point2d(points[i].x + (radius * Math.Round(Math.Cos(currentAnswer.Item2), 15)), points[i].y + (radius * Math.Round(Math.Sin(currentAnswer.Item2), 15))));
                if (maxPoint == null || currentAnswer.Item1 > maxPoint.Item1 || (currentAnswer.Item1 == maxPoint.Item1 && IsInteger(nextPoint.Item2.x) && IsInteger(nextPoint.Item2.y)))
                {
                    maxPoint = nextPoint;
                }

            }

            return maxPoint;
        }

        /// <summary>
        /// Given a list of points and a circle radius, find the circle location that covers the maximum number of points, at point i.
        /// </summary>
        /// <param name="points">The list of points to consider</param>
        /// <param name="radius">The radius of the circle</param>
        /// <param name="i">The index of the point to use for the sweeping circle</param>
        /// <returns>The number of points covered by the circle that is centered at the point.</returns>
        public static Tuple<int, Point2d> FindCircleWithMaximumPoints(Point2d[] points, double radius, int i)
        {
            Tuple<int, Point2d> maxPoint = null;
            if (points == null)
                return null;
            if (radius <= 0)
                return null;

            var numberOfPoints = points.Count();
            double[,] distance = new double[numberOfPoints, numberOfPoints];

            for (int j = 0; j < numberOfPoints; j++)
            {
                distance[i, j] = distance[j, i] = points[i].GetDistance(points[j]);
            }

            var currentAnswer = GetPointsInside(distance, points, i, radius, numberOfPoints);
            maxPoint = new Tuple<int, Point2d>(currentAnswer.Item1, new Point2d(points[i].x + (radius * Math.Round(Math.Cos(currentAnswer.Item2), 15)), points[i].y + (radius * Math.Round(Math.Sin(currentAnswer.Item2), 15))));

            return maxPoint;
        }

        public static double CalculateAreaOfCircle(double radius)
        {
            return Math.PI * Math.Pow(radius, 2);
        }

        public static double CalculateOverlappingArea(Circle2d circle, Circle2d circle2)
        {
            var d = circle.GetDistance(circle2);

            if (d < circle.radius + circle2.radius)
            {
                var a = circle.radius * circle.radius;
                var b = circle2.radius * circle2.radius;

                var x = (a - b + d * d) / (2 * d);
                var z = x * x;
                var y = Math.Sqrt(a - z);

                if (d <= Math.Abs(circle2.radius - circle.radius))
                {
                    return Math.PI * Math.Min(a, b);
                }
                return a * Math.Asin(y / circle.radius) + b * Math.Asin(y / circle2.radius) - y * (x + Math.Sqrt(z + b - a));
            }
            return 0;
        }

        /// <summary>
        /// Moves the point towards the targetPoint with maximum distance
        /// </summary>
        /// <param name="startPoint">Start point</param>
        /// <param name="targetPoint">Target point</param>
        /// <param name="maximumDistance">Maximum distance to translate</param>
        /// <returns>The translated point in direction of target point with maximum distance</returns>
        public static Point2d TranslatePoint(Point2d startPoint, Point2d targetPoint, double maximumDistance)
        {
            var vector = CreateVector(startPoint, targetPoint);
            if (vector.LengthSquared() <= (maximumDistance * maximumDistance))
                return targetPoint;
            else
            {
                vector.Normalize();
                vector.Multiply(maximumDistance);

                return new Point2d(startPoint.x + vector.x, startPoint.y + vector.y);
            }

            /*
            if (point.GetDistance(targetPoint) <= maximumDistance)
                return targetPoint;
            else
            {
                var angle = point.GetAngle(targetPoint);
                var vx = Math.Cos(angle) * maximumDistance;
                var vy = Math.Sin(angle) * maximumDistance;
                return new Point2d(point.x + vx, point.y + vy);
            }
            */
        }

        public static Point2d CreateVector(Point2d startPoint, Point2d targetPoint)
        {
            var x = targetPoint.x - startPoint.x;
            var y = targetPoint.y - startPoint.y;
            return new Point2d(x, y);
        }

        private static bool IsInteger(double d)
        {
            return Math.Abs(d % 1) <= (Double.Epsilon * 100);
        }

        private static Tuple<int, double> GetPointsInside(double[,] distance, Point2d[] points, int i, double radius, int numberOfPoints)
        {
            List<Tuple<double, bool>> angles = new List<Tuple<double, bool>>();
            for (int j = 0; j < numberOfPoints; j++)
            {
                if (i != j && distance[i, j] <= 2 * radius)
                {
                    double B = Math.Acos(distance[i, j] / (2 * radius));
                    Complex c1 = new Complex(points[j].x - points[i].x, points[j].y - points[i].y);
                    double A = c1.Phase;
                    double alpha = A - B;
                    double beta = A + B;
                    angles.Add(new Tuple<double, bool>(alpha, true));
                    angles.Add(new Tuple<double, bool>(beta, false));
                }
            }
            angles = angles.OrderBy(angle => angle.Item1).ToList();
            int count = 1, res = 1;
            double maxAngle = 0;
            foreach (var angle in angles)
            {
                if (angle.Item2)
                    count++;
                else
                    count--;
                if (count > res)
                {
                    res = count;
                    maxAngle = angle.Item1;
                }

            }
            return new Tuple<int, double>(res, maxAngle);
        }
    }
}
//*** SourceCombiner -> original file GameTreeNode.cs ***
namespace Algorithms.Trees
{
    public class GameTreeNode
    {
        public IGameState state;
        public IList moves;
        public List<GameTreeNode> children;
        public double wins = 0;
        public double loses = 0;
        public int totalPlays = 0;
        public GameTreeNode parent;
        public bool isMax;
        public GameTreeNode(IGameState state, bool isMax, GameTreeNode parent = null)
        {
            this.state = state;
            moves = new List<object>();
            foreach(object obj in state.GetPossibleMoves(isMax))
            {
                moves.Add(obj);
            }
            children = new List<GameTreeNode>();
            this.parent = parent;
            this.isMax = isMax;
        }
        public double GetScore(bool isMax)
        {
            double totalPlays = TotalPlays();
            if (totalPlays == 0)
                return 0;
            if (isMax)
            {
                return (wins - loses) / totalPlays;
            }
            else
            {
                return (loses - wins) / totalPlays;
            }
        }
        public int TotalPlays()
        {
            return totalPlays;
        }
        public double? GetWinner()
        {
            return state.GetWinner();
        }
        public void ApplyWinner(double? winner)
        {
            if (winner.HasValue)
            {
                if(winner > 0)
                {
                    wins += winner.Value;
                }
                else if(winner < 0)
                {
                    loses += Math.Abs(winner.Value);
                }
                totalPlays++;
            }
        }
        public double Evaluate()
        {
            return state.Evaluate(isMax);
        }
    }
}

//*** SourceCombiner -> original file MonteCarloTreeSearch.cs ***
namespace Algorithms.Trees
{
    public class MonteCarloTreeSearch : TreeAlgorithm
    {
        private Random rand;
        private bool printErrors;
        private SearchStrategy strategy;
        public enum SearchStrategy
        {
            Random = 0,
            Sequential = 1
        }
        public MonteCarloTreeSearch(bool showErrors = true, SearchStrategy searchStrategy = SearchStrategy.Random) 
        {
            rand = new Random();
            printErrors = showErrors;
            strategy = searchStrategy;
        }
        /// <summary>
        /// Get the next move
        /// </summary>
        /// <param name="watch">timer</param>
        /// <param name="timeLimit">The amount of time to give to the search in milliseconds</param>
        /// <param name="numRollouts">The number of roll outs to play per expansion</param>
        /// <returns></returns>
        public object GetNextMove(Stopwatch watch, int timeLimit, int depth = -1, int numRollouts = 1, double? exploration = null)
        {
            if(exploration == null)
            {
                exploration = Math.Sqrt(2);
            }
            int count = 0;
            do
            {
                GameTreeNode selectedNode = SelectNodeWithUnplayedMoves(RootNode, exploration.Value);
                if(selectedNode == null)
                {
                    if(printErrors)
                        Console.Error.WriteLine("Found no more moves!");
                    break;
                }
                object move = SelectMove(selectedNode);
                GameTreeNode childNode = Expand(selectedNode, move);
                double? winner = childNode.GetWinner();
                if (winner.HasValue)
                {
                    BackPropagate(childNode, winner);
                }
                else
                {
                    for (int i = 0; i<numRollouts; i++)
                    {
                        var clonedState = childNode.state.Clone();
                        winner = SimulateGame(clonedState, watch, timeLimit, depth, childNode.isMax);
                        BackPropagate(childNode, winner);
                        count++;
                    }
                }
            }
            while (watch.ElapsedMilliseconds < timeLimit);
            if(printErrors)
                Console.Error.WriteLine($"Played {count} games!");
            GameTreeNode bestChild = null;
            double bestScore = double.MinValue;
            foreach(GameTreeNode child in RootNode.children)
            {
                double score = child.GetScore(RootNode.isMax);
                if(bestScore < score)
                {
                    bestChild = child;
                    bestScore = score;
                }
                if(printErrors)
                    Console.Error.WriteLine($"w: {child.wins} l: {child.loses} total: {child.totalPlays} move: {child.state.GetMove(RootNode.isMax)} score: {score} isMax: {RootNode.isMax}");
            }
            if(printErrors)
                Console.Error.WriteLine($"Best: w: {bestChild.wins} l: {bestChild.loses} total: {bestChild.totalPlays} score: {bestScore} move: {bestChild.state.GetMove(RootNode.isMax)}");
            return bestChild.state.GetMove(RootNode.isMax);
        }
        private void BackPropagate(GameTreeNode selectedNode, double? winner)
        {
            selectedNode.ApplyWinner(winner);
            GameTreeNode tempNode = selectedNode.parent;
            while(tempNode != null)
            {
                tempNode.ApplyWinner(winner);
                tempNode = tempNode.parent;
            }
        }
        private double? SimulateGame(IGameState state, Stopwatch watch, int timeLimit, int depth, bool isMax)
        {
            double? winner;
            do
            {
                object move = SelectMoveAtRandom(state, isMax);
                state.ApplyMove(move, isMax);
                if (watch.ElapsedMilliseconds >= timeLimit)
                {
                    return null;
                }
                depth--;
                isMax = !isMax;
                winner = state.GetWinner();
            }
            while (!winner.HasValue && depth != 0);
            if (winner.HasValue)
            {
                return winner;
            }
            if (depth == 0)
            {
                double eval = state.Evaluate(isMax);
                if (eval > 1)
                {
                    return 1;
                }
                else if (eval < -1)
                    return -1;
                else return eval;
            }
            throw new InvalidOperationException("Could not find a winner for simulation!");
        }
        private GameTreeNode SelectNodeWithUnplayedMoves(GameTreeNode node, double exploration)
        {
            Queue<GameTreeNode> queue = new Queue<GameTreeNode>();
            queue.Enqueue(node);
            GameTreeNode tempNode;
            GameTreeNode bestNode = null;
            double maxValue = -1;
            while (queue.Count > 0)
            {
                tempNode = queue.Dequeue();
                if (tempNode.moves.Count == 0)
                {
                    foreach (GameTreeNode child in tempNode.children)
                    {
                        queue.Enqueue(child);
                    }
                }
                else if(tempNode.parent != null)
                {
                    double wins = RootNode.isMax ? tempNode.wins : tempNode.loses;
                    double nodeTotal = tempNode.TotalPlays();
                    double parentTotal = tempNode.parent.TotalPlays();
                    double value = wins / nodeTotal + exploration * Math.Sqrt(Math.Log(parentTotal) / nodeTotal);
                    if(value > maxValue)
                    {
                        maxValue = value;
                        bestNode = tempNode;
                    }
                }
                else return tempNode;
            }
            return bestNode;
        }
        private object SelectMoveAtRandom(IGameState state, bool isMax)
        {
            IList moves = state.GetPossibleMoves(isMax);
            if (moves.Count == 0)
            {
                throw new Exception("No moves available!");
            }
            int index = rand.Next(0, moves.Count);
            return moves[index];
        }
        private object SelectMove(GameTreeNode node)
        {
            switch (strategy)
            {
                case SearchStrategy.Random:
                    return SelectMoveAtRandom(node);
                case SearchStrategy.Sequential:
                    return SelectMoveSequentially(node);
            }
            throw new InvalidOperationException("strategy not supported");
        }
        private object SelectMoveSequentially(GameTreeNode node)
        {
            object move;
            if (node.moves.Count == 0)//If there are no more moves then that is a problem...
            {
                throw new Exception("No moves found!");
            }
            else
            {
                move = node.moves[0];
                node.moves.RemoveAt(0);
            }
            return move;
        }
        private object SelectMoveAtRandom(GameTreeNode node)
        {
            object move;
            if (node.moves.Count == 0)//If there are no more moves then that is a problem...
            {
                throw new Exception("No moves found!");
            }
            else
            {
                int index = rand.Next(0, node.moves.Count);
                move = node.moves[index];
                node.moves.RemoveAt(index);
            }
            return move;
        }
    }
}
//*** SourceCombiner -> original file TreeAlgorithm.cs ***
namespace Algorithms.Trees
{
    public class TreeAlgorithm
    {
        protected GameTreeNode RootNode { get; private set; }
        public void SetState(IGameState rootState, bool isMax = true, bool findState = true)
        {
            if (RootNode != null && findState)
            {
                //if we have already started searching then continue to search as we go if possible; the search will scan two layers to see if only one move was played or if 2 moves were played to get back to the original players turn.
                //find the child that matches the new node
                bool isFound = false;
                //Expand any moves left in the root node (if any)
                foreach (object move in RootNode.moves)
                {
                    Expand(RootNode, move);
                }
                //Begin scanning the children
                foreach (GameTreeNode child in RootNode.children)
                {
                    if (child.state.Equals(rootState))
                    {
                        RootNode = child;
                        isFound = true;
                        break;
                    }
                    foreach (object move in child.moves)
                    {
                        Expand(child, move);
                    }
                    foreach (GameTreeNode descendent in child.children)
                    {
                        if (descendent.state.Equals(rootState))
                        {
                            RootNode = descendent;
                            isFound = true;
                            break;
                        }
                    }
                }
                if (!isFound)
                {
                    Console.Error.WriteLine("Could not find the next state in tree!  Starting over...");
                    RootNode = new GameTreeNode(rootState.Clone(), isMax);
                }
                else
                {
                    RootNode.parent = null;
                }
            }
            else
            {
                RootNode = new GameTreeNode(rootState.Clone(), isMax);
            }
        }
        /// <summary>
        /// Expands the given node by create a clone, applying the move and then adding it to the list of children.
        /// </summary>
        /// <param name="node">The node to expand</param>
        /// <param name="move">The move to play on the expanded node</param>
        /// <returns></returns>
        protected GameTreeNode Expand(GameTreeNode node, object move)
        {
            IGameState nextState = node.state.Clone();
            nextState.ApplyMove(move, node.isMax);
            GameTreeNode childNode = new GameTreeNode(nextState, !node.isMax, node);
            node.children.Add(childNode);
            return childNode;
        }
    }
}
//*** SourceCombiner -> original file BitFunctions.cs ***
namespace Algorithms.Utility
{
    public static class BitFunctions
    {
        public static bool IsBitSet(long value, int location)
        {
            long mask = GetBitMask(location);
            return (value & mask) == mask;
        }
        public static long SetBit(long value, int location)
        {
            return value | (GetBitMask(location));
        }
        public static long ClearBit(long value, int location)
        {
            return value & (~(GetBitMask(location)));
        }
        public static long SetOrClearBit(long value, int location, bool isSet)
        {
            if (isSet)
                return SetBit(value, location);
            return ClearBit(value, location);
        }
        public static int NumberOfSetBits(long i)
        {
            i = i - ((i >> 1) & 0x5555555555555555);
            i = (i & 0x3333333333333333) + ((i >> 2) & 0x3333333333333333);
            return (int)((((i + (i >> 4)) & 0xF0F0F0F0F0F0F0F) * 0x101010101010101) >> 56);
        }
        public static long GetBitMask(int index)
        {
            return (long)1 << index;
        }
    }
}
