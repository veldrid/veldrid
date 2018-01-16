namespace Veldrid
{
    /// <summary>
    /// The format of an individual vertex element.
    /// </summary>
    public enum VertexElementFormat : byte
    {
        /// <summary>
        /// One 32-bit floating point value.
        /// </summary>
        Float1,
        /// <summary>
        /// Two 32-bit floating point values.
        /// </summary>
        Float2,
        /// <summary>
        /// Three 32-bit floating point values.
        /// </summary>
        Float3,
        /// <summary>
        /// Four 32-bit floating point values.
        /// </summary>
        Float4,
        /// <summary>
        /// Two 8-bit unsigned normalized integers.
        /// </summary>
        Byte2_Norm,
        /// <summary>
        /// Two 8-bit unisgned integers.
        /// </summary>
        Byte2,
        /// <summary>
        /// Four 8-bit unsigned normalized integers.
        /// </summary>
        Byte4_Norm,
        /// <summary>
        /// Four 8-bit unsigned integers.
        /// </summary>
        Byte4,
        SByte2_Norm,
        SByte2,
        SByte4_Norm,
        SByte4,
        UShort2_Norm,
        UShort2,
        UShort4_Norm,
        UShort4,
        Short2_Norm,
        Short2,
        Short4_Norm,
        Short4,
        UInt1,
        UInt2,
        UInt3,
        UInt4,
        Int1,
        Int2,
        Int3,
        Int4,
    }
}