namespace Vd2
{
    public abstract class Texture
    {
        public abstract PixelFormat Format { get; }
        public abstract uint MipLevels { get; }
        public abstract uint ArrayLayers { get; }
        public abstract TextureUsage Usage { get; }
    }
}
