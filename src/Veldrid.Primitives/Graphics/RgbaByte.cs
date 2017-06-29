namespace Veldrid.Graphics
{
    public struct RgbaByte
    {
        public readonly byte R, G, B, A;

        public static readonly RgbaByte Black = new RgbaByte(0, 0, 0, 255);
        public static readonly RgbaByte White = new RgbaByte(255, 255, 255, 255);
        public static readonly RgbaByte Cyan = new RgbaByte(0, 255, 255, 255);

        public RgbaByte(uint data)
        {
            R = (byte)((data & 0xFF000000) >> 24);
            G = (byte)((data & 0x00FF0000) >> 16);
            B = (byte)((data & 0x0000FF00) >> 8);
            A = (byte)((data & 0x000000FF) >> 0);
        }

        public RgbaByte(byte r, byte g, byte b, byte a)
        {
            R = r;
            G = g;
            B = b;
            A = a;
        }
    }
}
