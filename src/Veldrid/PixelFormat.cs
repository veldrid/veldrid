namespace Veldrid
{
    /// <summary>
    /// The format of data stored in a <see cref="Texture"/>.
    /// Each name is a compound identifier, where each component denotes a name and a number of bits used to store that
    /// component. The final component identifies the storage type of each component. "Float" identifies a signed, floating-point
    /// type, UNorm identifies an unsigned integer type which is normalized, meaning it occupies the full space of the integer
    /// type.
    /// </summary>
    public enum PixelFormat : byte
    {
        /// <summary>
        /// RGBA component order. Each component is an 8-bit unsigned normalized integer.
        /// </summary>
        R8_G8_B8_A8_UNorm,
        /// <summary>
        /// BGRA component order. Each component is an 8-bit unsigned normalized integer.
        /// </summary>
        B8_G8_R8_A8_UNorm,
        /// <summary>
        /// Single-channel, 8-bit unsigned integer.
        /// </summary>
        R8_UNorm,
        /// <summary>
        /// Single-channel, 16-bit unsigned integer. Can be used as a depth format.
        /// </summary>
        R16_UNorm,
        /// <summary>
        /// RGBA component order. Each component is a 32-bit signed floating-point value.
        /// </summary>
        R32_G32_B32_A32_Float,
        /// <summary>
        /// Single-channel, 32-bit signed floating-point value. Can be used as a depth format.
        /// </summary>
        R32_Float,
        /// <summary>
        /// BC3 block compressed format.
        /// </summary>
        BC3_UNorm,
        /// <summary>
        /// A depth-stencil format where the depth is stored in a 24-bit unsigned normalized integer, and the stencil is stored
        /// in an 8-bit unsigned integer.
        /// </summary>
        D24_UNorm_S8_UInt,
        /// <summary>
        /// A depth-stencil format where the depth is stored in a 32-bit signed floating-point value, and the stencil is stored
        /// in an 8-bit unsigned integer.
        /// </summary>
        D32_Float_S8_UInt,
        /// <summary>
        /// RGBA component order. Each component is a 32-bit unsigned integer.
        /// </summary>
        R32_G32_B32_A32_UInt,
        /// <summary>
        /// RG component order. Each component is an 8-bit signed normalized integer.
        /// </summary>
        R8_G8_SNorm,
        /// <summary>
        /// BC1 block compressed format with no alpha.
        /// </summary>
        BC1_Rgb_UNorm,
        /// <summary>
        /// BC1 block compressed format with a single-bit alpha channel.
        /// </summary>
        BC1_Rgba_UNorm,
        /// <summary>
        /// BC2 block compressed format.
        /// </summary>
        BC2_UNorm,
    }
}
