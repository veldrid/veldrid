using System;

namespace Vd2.OpenGL
{
    internal class UpdateTexture2DEntry : OpenGLCommandEntry
    {
        public readonly Texture2D Texture2D;
        public readonly StagingBlock StagingBlock;
        public readonly uint X;
        public readonly uint Y;
        public readonly uint Width;
        public readonly uint Height;
        public readonly uint MipLevel;
        public readonly uint ArrayLayer;

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
    }
}