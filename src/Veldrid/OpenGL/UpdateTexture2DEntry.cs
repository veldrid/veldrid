namespace Veldrid.OpenGL
{
    internal class UpdateTexture2DEntry : OpenGLCommandEntry
    {
        public Texture2D Texture2D;
        public StagingBlock StagingBlock;
        public uint X;
        public uint Y;
        public uint Width;
        public uint Height;
        public uint MipLevel;
        public uint ArrayLayer;

        public UpdateTexture2DEntry(
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
            StagingBlock = stagingBlock;
            X = x;
            Y = y;
            Width = width;
            Height = height;
            MipLevel = mipLevel;
            ArrayLayer = arrayLayer;
        }

        public UpdateTexture2DEntry() { }

        public UpdateTexture2DEntry Init(
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
            StagingBlock = stagingBlock;
            X = x;
            Y = y;
            Width = width;
            Height = height;
            MipLevel = mipLevel;
            ArrayLayer = arrayLayer;

            return this;
        }

        public override void ClearReferences()
        {
            Texture2D = null;
        }
    }
}