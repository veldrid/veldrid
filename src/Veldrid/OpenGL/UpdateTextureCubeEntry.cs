namespace Veldrid.OpenGL
{
    internal class UpdateTextureCubeEntry : OpenGLCommandEntry
    {
        public TextureCube TextureCube;
        public StagingBlock StagingBlock;
        public CubeFace Face;
        public uint X;
        public uint Y;
        public uint Width;
        public uint Height;
        public uint MipLevel;
        public uint ArrayLayer;

        public UpdateTextureCubeEntry(
            TextureCube textureCube,
            StagingBlock stagingBlock,
            CubeFace face,
            uint x,
            uint y,
            uint width,
            uint height,
            uint mipLevel,
            uint arrayLayer)
        {
            TextureCube = textureCube;
            StagingBlock = stagingBlock;
            Face = face;
            X = x;
            Y = y;
            Width = width;
            Height = height;
            MipLevel = mipLevel;
            ArrayLayer = arrayLayer;
        }

        public UpdateTextureCubeEntry() { }

        public UpdateTextureCubeEntry Init(
            TextureCube textureCube,
            StagingBlock stagingBlock,
            CubeFace face,
            uint x,
            uint y,
            uint width,
            uint height,
            uint mipLevel,
            uint arrayLayer)
        {
            TextureCube = textureCube;
            StagingBlock = stagingBlock;
            Face = face;
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
            TextureCube = null;
        }
    }
}