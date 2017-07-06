namespace Veldrid.Graphics
{
    /// <summary>
    /// Describes how texture address sampling behaves outside of the [0, 1] range.
    /// </summary>
    public enum SamplerAddressMode : byte
    {
        /// <summary>
        /// Address values wrap around from 1 back to 0.
        /// </summary>
        Wrap,
        /// <summary>
        /// Address values are mirrored at edges.
        /// </summary>
        Mirror,
        /// <summary>
        /// Address values are clamped to 0 or 1.
        /// </summary>
        Clamp,
        /// <summary>
        /// Values at the border of a texture are sampled according to the <see cref="SamplerState"/>'s
        /// <see cref="SamplerState.BorderColor"/> value.
        /// </summary>
        Border,
    }
}
