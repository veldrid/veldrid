namespace Veldrid
{
    /// <summary>
    /// Determines how texture values are sampled from a texture.
    /// </summary>
    public enum SamplerFilter : byte
    {
        /// <summary>
        /// Point sampling is used for minification, magnification, and mip-level sampling.
        /// </summary>
        MinPoint_MagPoint_MipPoint,
        /// <summary>
        /// Point sampling is used for minification and magnification; linear interpolation is used for mip-level sampling.
        /// </summary>
        MinPoint_MagPoint_MipLinear,
        /// <summary>
        /// Point sampling is used for minification and mip-level sampling; linear interpolation is used for mip-level sampling.
        /// </summary>
        MinPoint_MagLinear_MipPoint,
        /// <summary>
        /// Point sampling is used for minification; linear interpolation is used for magnification and mip-level sampling.
        /// </summary>
        MinPoint_MagLinear_MipLinear,
        /// <summary>
        /// Linear interpolation is used for minifcation; point sampling is used for magnification and mip-level sampling.
        /// </summary>
        MinLinear_MagPoint_MipPoint,
        /// <summary>
        /// Linear interpolation is used for minification and mip-level sampling; point sampling is used for magnification.
        /// </summary>
        MinLinear_MagPoint_MipLinear,
        /// <summary>
        /// Linear interpolation is used for minification and magnification, and point sampling is used for mip-level sampling.
        /// </summary>
        MinLinear_MagLinear_MipPoint,
        /// <summary>
        /// Linear interpolation is used for minification, magnification, and mip-level sampling.
        /// </summary>
        MinLinear_MagLinear_MipLinear,
        /// <summary>
        /// Anisotropic filtering is used. The maximum anisotropy is controlled by
        /// <see cref="SamplerDescription.MaximumAnisotropy"/>.
        /// </summary>
        Anisotropic,
    }
}
