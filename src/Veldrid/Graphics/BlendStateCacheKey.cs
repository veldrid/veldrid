using System;

namespace Veldrid.Graphics
{
    internal struct BlendStateCacheKey : IEquatable<BlendStateCacheKey>
    {
        bool IsBlendEnabled { get; }

        Blend SourceColorBlend { get; }
        Blend DestinationColorBlend { get; }
        BlendFunction ColorBlendFunction { get; }
        Blend SourceAlphaBlend { get; }
        Blend DestinationAlphaBlend { get; }
        BlendFunction AlphaBlendFunction { get; }
        RgbaFloat BlendFactor { get; set; }

        public BlendStateCacheKey(
            bool isBlendEnabled,
            Blend srcAlpha, Blend destAlpha, BlendFunction alphaBlendFunc,
            Blend srcColor, Blend destColor, BlendFunction colorBlendFunc,
            RgbaFloat blendFactor)
        {
            IsBlendEnabled = isBlendEnabled;
            SourceColorBlend = srcColor;
            DestinationColorBlend = destColor;
            ColorBlendFunction = colorBlendFunc;
            SourceAlphaBlend = srcAlpha;
            DestinationAlphaBlend = destAlpha;
            AlphaBlendFunction = alphaBlendFunc;
            BlendFactor = blendFactor;
        }

        public bool Equals(BlendStateCacheKey other)
        {
            return IsBlendEnabled.Equals(other.IsBlendEnabled) && SourceColorBlend == other.SourceColorBlend
                && DestinationColorBlend == other.DestinationColorBlend && ColorBlendFunction == other.ColorBlendFunction
                && SourceAlphaBlend == other.SourceAlphaBlend && DestinationAlphaBlend == other.DestinationAlphaBlend
                && AlphaBlendFunction == other.AlphaBlendFunction && BlendFactor.Equals(other.BlendFactor);
        }
    }
}
