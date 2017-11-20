using System;
using Veldrid.OpenGLBinding;

namespace Veldrid.OpenGL
{
    internal class OpenGLTextureView : TextureView
    {
        public new OpenGLTexture Target { get; }

        public OpenGLTextureView(ref TextureViewDescription description)
            : base(ref description)
        {
            Target = Util.AssertSubtype<Texture, OpenGLTexture>(description.Target);
        }

        public override void Dispose()
        {
        }

        public SizedInternalFormat GetReadWriteSizedInternalFormat()
        {
            switch (Target.Format)
            {
                case PixelFormat.R8_G8_B8_A8_UNorm:
                case PixelFormat.B8_G8_R8_A8_UNorm:
                    return SizedInternalFormat.Rgba8ui;
                case PixelFormat.R8_UNorm:
                    return SizedInternalFormat.R8ui;
                case PixelFormat.R16_UNorm:
                    return SizedInternalFormat.R16ui;
                case PixelFormat.R32_G32_B32_A32_Float:
                    return SizedInternalFormat.Rgba32f;
                case PixelFormat.R32_Float:
                    return SizedInternalFormat.R32f;
                default:
                    throw Illegal.Value<PixelFormat>();
            }
        }
    }
}