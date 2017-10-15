namespace Vd2
{
    public struct TextureDescription
    {
        public uint Width;
        public uint Height;
        public uint MipLevels;
        public uint ArrayLayers;
        public PixelFormat Format;
        public TextureUsage Usage;

        public TextureDescription(
            uint width,
            uint height,
            uint mipLevels,
            uint arrayLayers,
            PixelFormat format,
            TextureUsage usage)
        {
            Width = width;
            Height = height;
            MipLevels = mipLevels;
            ArrayLayers = arrayLayers;
            Format = format;
            Usage = usage;
        }
    }
}