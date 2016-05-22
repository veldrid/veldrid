using System;

namespace Veldrid.Graphics
{
    public interface BlendState : IDisposable
    {
        bool IsBlendEnabled { get; }

        Blend SourceColorBlend { get; }
        Blend DestinationColorBlend { get; }
        BlendFunction ColorBlendFunction { get; }

        Blend SourceAlphaBlend { get; }
        Blend DestinationAlphaBlend { get; }
        BlendFunction AlphaBlendFunction { get; }

        RgbaFloat BlendFactor { get; set; }
    }

    public enum Blend
    {
        Zero,
        One,
        SourceAlpha,
        InverseSourceAlpha,
        DestinationAlpha,
        InverseDestinationAlpha,
        SourceColor,
        InverseSourceColor,
        DestinationColor,
        InverseDestinationColor,
        BlendFactor,
        InverseBlendFactor
    }

    public enum BlendFunction
    {
        Add,
        Subtract,
        ReverseSubtract,
        Minimum,
        Maximum
    }
}
