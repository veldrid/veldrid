namespace Veldrid.OpenGL.ManagedEntryList
{
    internal class UpdateTextureEntry : OpenGLCommandEntry
    {
        public Texture Texture;
        public StagingBlock StagingBlock;
        public uint X;
        public uint Y;
        public uint Z;
        public uint Width;
        public uint Height;
        public uint Depth;
        public uint MipLevel;
        public uint ArrayLayer;

        public UpdateTextureEntry(
            Texture texture,
            StagingBlock stagingBlock,
            uint x,
            uint y,
            uint z,
            uint width,
            uint height,
            uint depth,
            uint mipLevel,
            uint arrayLayer)
        {
            Texture = texture;
            StagingBlock = stagingBlock;
            X = x;
            Y = y;
            Z = z;
            Width = width;
            Height = height;
            Depth = depth;
            MipLevel = mipLevel;
            ArrayLayer = arrayLayer;
        }

        public UpdateTextureEntry() { }

        public UpdateTextureEntry Init(
            Texture texture,
            StagingBlock stagingBlock,
            uint x,
            uint y,
            uint z,
            uint width,
            uint height,
            uint depth,
            uint mipLevel,
            uint arrayLayer)
        {
            Texture = texture;
            StagingBlock = stagingBlock;
            X = x;
            Y = y;
            Z = z;
            Width = width;
            Height = height;
            Depth = depth;
            MipLevel = mipLevel;
            ArrayLayer = arrayLayer;

            return this;
        }

        public override void ClearReferences()
        {
            Texture = null;
        }
    }
}