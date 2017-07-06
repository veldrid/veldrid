using System;

namespace Veldrid.Graphics
{
    /// <summary>
    /// A device resource describing how texture values are sampled in a shader.
    /// </summary>
    public interface SamplerState : IDisposable
    {
        /// <summary>
        /// The address handling for the U-coordinate (R-coordinate in OpenGL).
        /// </summary>
        SamplerAddressMode AddressU { get; }
        /// <summary>
        /// The address handling for the V-coordinate (S-coordinate in OpenGL).
        /// </summary>
        SamplerAddressMode AddressV { get; }
        /// <summary>
        /// The address handling for the W-coordinate (T-coordinate in OpenGL).
        /// </summary>
        SamplerAddressMode AddressW { get; }
        /// <summary>
        /// The kind of filtering used.
        /// </summary>
        SamplerFilter Filter { get; }
        /// <summary>
        /// If an anisotropic filter is used, this controls the level of anisotropy.
        /// </summary>
        int MaximumAnisotropy { get; }
        /// <summary>
        /// If a border filter is used, this controls the color sampled at texture edges.
        /// </summary>
        RgbaFloat BorderColor { get; }
        /// <summary>
        /// If a comparison filter is used, this controls the comparison function used.
        /// </summary>
        DepthComparison Comparison { get; }
        /// <summary>
        /// The most-detailed mipmap level the sampler has access to.
        /// </summary>
        int MinimumLod { get; }
        /// <summary>
        /// The least-detailed mipmap level the sampler has access to.
        /// </summary>
        int MaximumLod { get; }
        /// <summary>
        /// A bias value used before selecting which mipmap level to sample from.
        /// </summary>
        int LodBias { get; }
    }
}
