namespace Veldrid.MetalBindings
{
    public struct CGPoint
    {
        public CGFloat x;
        public CGFloat y;

        public CGPoint(double x, double y)
        {
            this.x = x;
            this.y = y;
        }

        public override string ToString() => string.Format("({0},{1})", x, y);
    }
}