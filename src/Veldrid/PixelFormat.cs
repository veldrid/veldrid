namespace Veldrid
{
    /// <summary>
    /// The format of data stored in a <see cref="Texture"/>.
    /// Each name is a compound identifier, where each component denotes a name and a number of bits used to store that
    /// component. The final component identifies the storage type of each component. "Float" identifies a signed, floating-point
    /// type, UNorm identifies an unsigned integer type which is normalized, meaning it occupies the full space of the integer
    /// type. The SRgb suffix for normalized integer formats indicates that the RGB components are stored in sRGB format.
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
        /// Single-channel, 8-bit unsigned normalized integer.
        /// </summary>
        R8_UNorm,
        /// <summary>
        /// Single-channel, 16-bit unsigned normalized integer. Can be used as a depth format.
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
        /// <summary>
        /// A 32-bit packed format. The 10-bit R component occupies bits 0..9, the 10-bit G component occupies bits 10..19,
        /// the 10-bit A component occupies 20..29, and the 2-bit A component occupies bits 30..31. Each value is an unsigned,
        /// normalized integer.
        /// </summary>
        R10_G10_B10_A2_UNorm,
        /// <summary>
        /// A 32-bit packed format. The 10-bit R component occupies bits 0..9, the 10-bit G component occupies bits 10..19,
        /// the 10-bit A component occupies 20..29, and the 2-bit A component occupies bits 30..31. Each value is an unsigned
        /// integer.
        /// </summary>
        R10_G10_B10_A2_UInt,
        /// <summary>
        /// A 32-bit packed format. The 11-bit R componnent occupies bits 0..10, the 11-bit G component occupies bits 11..21,
        /// and the 10-bit B component occupies bits 22..31. Each value is an unsigned floating point value.
        /// </summary>
        R11_G11_B10_Float,
        /// <summary>
        /// Single-channel, 8-bit signed normalized integer.
        /// </summary>
        R8_SNorm,
        /// <summary>
        /// Single-channel, 8-bit unsigned integer.
        /// </summary>
        R8_UInt,
        /// <summary>
        /// Single-channel, 8-bit signed integer.
        /// </summary>
        R8_SInt,
        /// <summary>
        /// Single-channel, 16-bit signed normalized integer.
        /// </summary>
        R16_SNorm,
        /// <summary>
        /// Single-channel, 16-bit unsigned integer.
        /// </summary>
        R16_UInt,
        /// <summary>
        /// Single-channel, 16-bit signed integer.
        /// </summary>
        R16_SInt,
        /// <summary>
        /// Single-channel, 16-bit signed floating-point value.
        /// </summary>
        R16_Float,
        /// <summary>
        /// Single-channel, 32-bit unsigned integer
        /// </summary>
        R32_UInt,
        /// <summary>
        /// Single-channel, 32-bit signed integer
        /// </summary>
        R32_SInt,
        /// <summary>
        /// RG component order. Each component is an 8-bit unsigned normalized integer.
        /// </summary>
        R8_G8_UNorm,
        /// <summary>
        /// RG component order. Each component is an 8-bit unsigned integer.
        /// </summary>
        R8_G8_UInt,
        /// <summary>
        /// RG component order. Each component is an 8-bit signed integer.
        /// </summary>
        R8_G8_SInt,
        /// <summary>
        /// RG component order. Each component is a 16-bit unsigned normalized integer.
        /// </summary>
        R16_G16_UNorm,
        /// <summary>
        /// RG component order. Each component is a 16-bit signed normalized integer.
        /// </summary>
        R16_G16_SNorm,
        /// <summary>
        /// RG component order. Each component is a 16-bit unsigned integer.
        /// </summary>
        R16_G16_UInt,
        /// <summary>
        /// RG component order. Each component is a 16-bit signed integer.
        /// </summary>
        R16_G16_SInt,
        /// <summary>
        /// RG component order. Each component is a 16-bit signed floating-point value.
        /// </summary>
        R16_G16_Float,
        /// <summary>
        /// RG component order. Each component is a 32-bit unsigned integer.
        /// </summary>
        R32_G32_UInt,
        /// <summary>
        /// RG component order. Each component is a 32-bit signed integer.
        /// </summary>
        R32_G32_SInt,
        /// <summary>
        /// RG component order. Each component is a 32-bit signed floating-point value.
        /// </summary>
        R32_G32_Float,
        /// <summary>
        /// RGBA component order. Each component is an 8-bit signed normalized integer.
        /// </summary>
        R8_G8_B8_A8_SNorm,
        /// <summary>
        /// RGBA component order. Each component is an 8-bit unsigned integer.
        /// </summary>
        R8_G8_B8_A8_UInt,
        /// <summary>
        /// RGBA component order. Each component is an 8-bit signed integer.
        /// </summary>
        R8_G8_B8_A8_SInt,
        /// <summary>
        /// RGBA component order. Each component is a 16-bit unsigned normalized integer.
        /// </summary>
        R16_G16_B16_A16_UNorm,
        /// <summary>
        /// RGBA component order. Each component is a 16-bit signed normalized integer.
        /// </summary>
        R16_G16_B16_A16_SNorm,
        /// <summary>
        /// RGBA component order. Each component is a 16-bit unsigned integer.
        /// </summary>
        R16_G16_B16_A16_UInt,
        /// <summary>
        /// RGBA component order. Each component is a 16-bit signed integer.
        /// </summary>
        R16_G16_B16_A16_SInt,
        /// <summary>
        /// RGBA component order. Each component is a 16-bit floating-point value.
        /// </summary>
        R16_G16_B16_A16_Float,
        /// <summary>
        /// RGBA component order. Each component is a 32-bit signed integer.
        /// </summary>
        R32_G32_B32_A32_SInt,
        /// <summary>
        /// A 64-bit, 4x4 block-compressed format storing unsigned normalized RGB data.
        /// </summary>
        ETC2_R8_G8_B8_UNorm,
        /// <summary>
        /// A 64-bit, 4x4 block-compressed format storing unsigned normalized RGB data, as well as 1 bit of alpha data.
        /// </summary>
        ETC2_R8_G8_B8_A1_UNorm,
        /// <summary>
        /// A 128-bit, 4x4 block-compressed format storing 64 bits of unsigned normalized RGB data, as well as 64 bits of alpha
        /// data.
        /// </summary>
        ETC2_R8_G8_B8_A8_UNorm,
        /// <summary>
        /// BC4 block compressed format, unsigned normalized values.
        /// </summary>
        BC4_UNorm,
        /// <summary>
        /// BC4 block compressed format, signed normalized values.
        /// </summary>
        BC4_SNorm,
        /// <summary>
        /// BC5 block compressed format, unsigned normalized values.
        /// </summary>
        BC5_UNorm,
        /// <summary>
        /// BC5 block compressed format, signed normalized values.
        /// </summary>
        BC5_SNorm,
        /// <summary>
        /// BC7 block compressed format.
        /// </summary>
        BC7_UNorm,
        /// <summary>
        /// RGBA component order. Each component is an 8-bit unsigned normalized integer.
        /// This is an sRGB format.
        /// </summary>
        R8_G8_B8_A8_UNorm_SRgb,
        /// <summary>
        /// BGRA component order. Each component is an 8-bit unsigned normalized integer.
        /// This is an sRGB format.
        /// </summary>
        B8_G8_R8_A8_UNorm_SRgb,
        /// <summary>
        /// BC1 block compressed format with no alpha.
        /// This is an sRGB format.
        /// </summary>
        BC1_Rgb_UNorm_SRgb,
        /// <summary>
        /// BC1 block compressed format with a single-bit alpha channel.
        /// This is an sRGB format.
        /// </summary>
        BC1_Rgba_UNorm_SRgb,
        /// <summary>
        /// BC2 block compressed format.
        /// This is an sRGB format.
        /// </summary>
        BC2_UNorm_SRgb,
        /// <summary>
        /// BC3 block compressed format.
        /// This is an sRGB format.
        /// </summary>
        BC3_UNorm_SRgb,
        /// <summary>
        /// BC7 block compressed format.
        /// This is an sRGB format.
        /// </summary>
        BC7_UNorm_SRgb,
    }
}
