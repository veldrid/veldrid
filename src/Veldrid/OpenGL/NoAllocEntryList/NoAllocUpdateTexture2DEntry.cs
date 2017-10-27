namespace Veldrid.OpenGL.NoAllocEntryList
{
    internal struct NoAllocUpdateTexture2DEntry
    {
        public readonly HandleTracked<Texture2D> Texture2D;
        public readonly HandleTrackedStagingBlock StagingBlock;
        public readonly uint X;
        public readonly uint Y;
        public readonly uint Width;
        public readonly uint Height;
        public readonly uint MipLevel;
        public readonly uint ArrayLayer;

        public NoAllocUpdateTexture2DEntry(
            Texture2D texture2D,
            StagingBlock stagingBlock,
            uint x,
            uint y,
            uint width,
            uint height,
            uint mipLevel,
            uint arrayLayer)
        {
            Texture2D = texture2D;
            StagingBlock = new HandleTrackedStagingBlock(stagingBlock);
            X = x;
            Y = y;
            Width = width;
            Height = height;
            MipLevel = mipLevel;
            ArrayLayer = arrayLayer;
        }
    }
}