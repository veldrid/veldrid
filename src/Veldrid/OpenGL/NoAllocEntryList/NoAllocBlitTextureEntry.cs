namespace Veldrid.OpenGL.NoAllocEntryList
{
    internal struct NoAllocBlitTextureEntry
    {
        public Tracked<Texture> Source;
        public uint SrcX;
        public uint SrcY;
        public uint SrcWidth;
        public uint SrcHeight;
        public Tracked<Framebuffer> Destination;
        public uint DstX;
        public uint DstY;
        public uint DstWidth;
        public uint DstHeight;
        public bool LinearFilter;

        public NoAllocBlitTextureEntry(
            Tracked<Texture> source, uint srcX, uint srcY, uint srcWidth, uint srcHeight,
            Tracked<Framebuffer> destination, uint dstX, uint dstY, uint dstWidth, uint dstHeight,
            bool linearFilter)
        {
            Source = source;
            SrcX = srcX;
            SrcY = srcY;
            SrcWidth = srcWidth;
            SrcHeight = srcHeight;
            Destination = destination;
            DstX = dstX;
            DstY = dstY;
            DstWidth = dstWidth;
            DstHeight = dstHeight;
            LinearFilter = linearFilter;
        }
    }
}
