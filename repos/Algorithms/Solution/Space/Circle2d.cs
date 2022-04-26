using Algorithms.Space;

namespace Solution.Space
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
