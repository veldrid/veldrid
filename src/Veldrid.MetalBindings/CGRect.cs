namespace Veldrid.MetalBindings
{
    public struct CGRect
    {
        public CGPoint origin;
        public CGSize size;

        public CGRect(CGPoint origin, CGSize size)
        {
            this.origin = origin;
            this.size = size;
        }

        public override string ToString() => string.Format("{0}, {1}", origin, size);
    }
}