using System;

namespace Veldrid.Graphics
{
    /// <summary>
    /// Controls the blending behavior of the graphics device's output merger.
    /// </summary>
    public interface BlendState : IDisposable
    {
        /// <summary>
        ///  Indicates whether or not blending is enabled.
        /// </summary>
        bool IsBlendEnabled { get; }

        /// <summary>
        /// The source color blend factor.
        /// </summary>
        Blend SourceColorBlend { get; }
        /// <summary>
        /// The destination color blend factor.
        /// </summary>
        Blend DestinationColorBlend { get; }
        /// <summary>
        /// The color blend function.
        /// </summary>
        BlendFunction ColorBlendFunction { get; }

        /// <summary>
        /// The source alpha blend factor.
        /// </summary>
        Blend SourceAlphaBlend { get; }
        /// <summary>
        /// The destination alpha blend factor.
        /// </summary>
        Blend DestinationAlphaBlend { get; }
        /// <summary>
        /// The alpha blend function.
        /// </summary>
        BlendFunction AlphaBlendFunction { get; }

        /// <summary>
        /// The blend factor used with parameterized blend states.
        /// </summary>
        RgbaFloat BlendFactor { get; }
    }
}
