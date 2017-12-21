namespace Veldrid.MetalBindings
{
    public struct MTLViewport
    {
        public double originX;
        public double originY;
        public double width;
        public double height;
        public double znear;
        public double zfar;

        public MTLViewport(double originX, double originY, double width, double height, double znear, double zfar)
        {
            this.originX = originX;
            this.originY = originY;
            this.width = width;
            this.height = height;
            this.znear = znear;
            this.zfar = zfar;
        }
    }
}