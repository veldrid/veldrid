using OpenTK.Graphics.ES30;
using System;

namespace Veldrid.Graphics.OpenGLES
{
    public class OpenGLESBlendState : BlendState, IDisposable
    {
        public OpenGLESBlendState(
            bool isBlendEnabled,
            Blend srcAlpha, Blend destAlpha, BlendFunction alphaBlendFunc,
            Blend srcColor, Blend destColor, BlendFunction colorBlendFunc,
            RgbaFloat blendFactor)
        {
            IsBlendEnabled = isBlendEnabled;
            SourceAlphaBlend = srcAlpha;
            DestinationAlphaBlend = destAlpha;
            AlphaBlendFunction = alphaBlendFunc;
            SourceColorBlend = srcColor;
            DestinationColorBlend = destColor;
            ColorBlendFunction = colorBlendFunc;
            BlendFactor = blendFactor;
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
                Utilities.CheckLastGLES3Error();
            }
            else
            {
                GL.Enable(EnableCap.Blend);
                Utilities.CheckLastGLES3Error();
                GL.BlendFuncSeparate(
                    OpenGLESFormats.ConvertBlendSrc(SourceColorBlend), OpenGLESFormats.ConvertBlendDest(DestinationColorBlend),
                    OpenGLESFormats.ConvertBlendSrc(SourceAlphaBlend), OpenGLESFormats.ConvertBlendDest(DestinationAlphaBlend));
                GL.BlendEquationSeparate(
                    OpenGLESFormats.ConvertBlendEquation(ColorBlendFunction),
                    OpenGLESFormats.ConvertBlendEquation(AlphaBlendFunction));
                GL.BlendColor(BlendFactor.R, BlendFactor.G, BlendFactor.B, BlendFactor.A);
            }
        }

        public void Dispose()
        {
        }
    }
}
