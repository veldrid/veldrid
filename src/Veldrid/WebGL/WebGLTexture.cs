using System;
using System.Diagnostics;
using static Veldrid.WebGL.WebGLConstants;

namespace Veldrid.WebGL
{
    internal class WebGLTexture : Texture
    {
        private readonly WebGLGraphicsDevice _gd;
        private bool _disposed;

        public override PixelFormat Format { get; }
        public override uint Width { get; }
        public override uint Height { get; }
        public override uint Depth { get; }
        public override uint MipLevels { get; }
        public override uint ArrayLayers { get; }
        public override TextureUsage Usage { get; }
        public override TextureType Type { get; }
        public override TextureSampleCount SampleCount { get; }
        public override string Name { get; set; }
        public uint Target { get; }
        public WebGLDotNET.WebGLTexture WglTexture { get; }
        public int GLInternalFormat { get; }
        public uint GLPixelFormat { get; }
        public uint GLPixelType { get; }

        public override bool IsDisposed => _disposed;

        public WebGLTexture(WebGLGraphicsDevice gd, ref TextureDescription description)
        {
            _gd = gd;

            Width = description.Width;
            Height = description.Height;
            Depth = description.Depth;
            Format = description.Format;
            MipLevels = description.MipLevels;
            ArrayLayers = description.ArrayLayers;
            Usage = description.Usage;
            Type = description.Type;
            SampleCount = description.SampleCount;

            GLPixelFormat = WebGLUtil.VdToGLPixelFormat(Format);
            GLPixelType = WebGLUtil.VdToGLPixelType(Format);
            GLInternalFormat = (int)WebGLUtil.VdToGLPixelInternalFormat(Format);

            if ((Usage & TextureUsage.DepthStencil) == TextureUsage.DepthStencil)
            {
                GLPixelFormat = FormatHelpers.IsStencilFormat(Format)
                    ? DEPTH_STENCIL
                    : DEPTH_COMPONENT;
                if (Format == PixelFormat.R16_UNorm)
                {
                    GLInternalFormat = (int)DEPTH_COMPONENT16;
                }
                else if (Format == PixelFormat.R32_Float)
                {
                    GLInternalFormat = (int)DEPTH_COMPONENT32F;
                }
            }

            if ((Usage & TextureUsage.Cubemap) == TextureUsage.Cubemap)
            {
                Target = TEXTURE_CUBE_MAP;
            }
            else if (Type == TextureType.Texture2D)
            {
                if (ArrayLayers == 1)
                {
                    Target = TEXTURE_2D;
                }
                else
                {
                    Target = TEXTURE_2D_ARRAY;
                }
            }
            else
            {
                Debug.Assert(Type == TextureType.Texture3D);
                Target = TEXTURE_3D;
            }

            WglTexture = _gd.Ctx.CreateTexture();
            _gd.TextureSamplerManager.SetTextureTransient(Target, WglTexture);

            bool isDepthTex = (Usage & TextureUsage.DepthStencil) == TextureUsage.DepthStencil;

            if (Target == TEXTURE_2D || Target == TEXTURE_CUBE_MAP)
            {
                _gd.Ctx.TexStorage2D(
                    Target,
                    (int)MipLevels,
                    (uint)GLInternalFormat,
                    (int)Width,
                    (int)Height);
            }
            else if (Target == TEXTURE_2D_ARRAY)
            {
                _gd.Ctx.TexStorage3D(
                    TEXTURE_2D_ARRAY,
                    (int)MipLevels,
                    (uint)GLInternalFormat,
                    (int)Width,
                    (int)Height,
                    (int)ArrayLayers);
            }
            else if (Target == TEXTURE_3D)
            {
                _gd.Ctx.TexStorage3D(
                    TEXTURE_3D,
                    (int)MipLevels,
                    (uint)GLInternalFormat,
                    (int)Width,
                    (int)Height,
                    (int)Depth);
            }
            else
            {
                throw new VeldridException("Invalid texture target: " + Target);
            }
        }

        private protected override void DisposeCore()
        {
            WglTexture.Dispose();
            _disposed = true;
        }
    }
}
