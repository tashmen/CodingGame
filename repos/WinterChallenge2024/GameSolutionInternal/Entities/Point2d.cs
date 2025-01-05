namespace GameSolution.Entities
{
    public class Point2d
    {
        public readonly int x, y, index; // Make fields readonly for better performance and clarity

        // Constructor that initializes the point
        public Point2d(int x, int y, int index)
        {
            this.x = x;
            this.y = y;
            this.index = index;
        }

        // Copy constructor (reuse the existing constructor to avoid redundancy)
        public Point2d(Point2d point) : this(point.x, point.y, point.index) { }

        // Clone method that uses the copy constructor
        public Point2d Clone() => new Point2d(this);

        // Override Equals method with a direct type check
        public override bool Equals(object obj)
        {
            return obj is Point2d point && point.x == this.x && point.y == this.y;
        }

        // Override GetHashCode for better performance
        public override int GetHashCode()
        {
            // Combine x and y in a more efficient way
            unchecked // Disable overflow checking
            {
                return (x * 397) ^ y; // Simple prime multiplication for better hash distribution
            }
        }
    }
}
