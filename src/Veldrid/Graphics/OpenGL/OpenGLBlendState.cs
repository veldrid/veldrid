using OpenTK.Graphics.OpenGL;
using System;

namespace Veldrid.Graphics.OpenGL
{
    public class OpenGLBlendState : BlendState, IDisposable
    {
        public OpenGLBlendState(
            bool isBlendEnabled,
            Blend srcAlpha, Blend destAlpha, BlendFunction alphaBlendFunc,
            Blend srcColor, Blend destColor, BlendFunction colorBlendFunc)
        {
            IsBlendEnabled = isBlendEnabled;
            SourceAlphaBlend = srcAlpha;
            DestinationAlphaBlend = destAlpha;
            AlphaBlendFunction = alphaBlendFunc;
            SourceColorBlend = srcColor;
            DestinationColorBlend = destColor;
            ColorBlendFunction = colorBlendFunc;
        }

        public bool IsBlendEnabled { get; }

        public RgbaFloat BlendFactor { get; set; }

        public Blend SourceAlphaBlend { get; }
        public Blend DestinationAlphaBlend { get; }
        public BlendFunction AlphaBlendFunction { get; }

        public Blend SourceColorBlend { get; }
        public Blend DestinationColorBlend { get; }
        public BlendFunction ColorBlendFunction { get; }

        public unsafe void Apply()
        {
            if (!IsBlendEnabled)
            {
                GL.Disable(EnableCap.Blend);
            }
            else
            {
                GL.Enable(EnableCap.Blend);
                GL.BlendFuncSeparate(
                    OpenGLFormats.ConvertBlendSrc(SourceColorBlend), OpenGLFormats.ConvertBlendDest(DestinationColorBlend),
                    OpenGLFormats.ConvertBlendSrc(SourceAlphaBlend), OpenGLFormats.ConvertBlendDest(DestinationAlphaBlend));
                GL.BlendEquationSeparate(
                    OpenGLFormats.ConvertBlendEquation(ColorBlendFunction),
                    OpenGLFormats.ConvertBlendEquation(AlphaBlendFunction));
            }
        }

        public void Dispose()
        {
        }
    }
}
