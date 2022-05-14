namespace Algorithms.Space
{
    public class Circle2d : Point2d
    {
        public double radius;
        public Circle2d(double x, double y, double radius) : base(x, y)
        {
            this.radius = radius;
        }
    }
}
