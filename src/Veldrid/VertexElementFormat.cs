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
        /// One 8-bit unsigned integer.
        /// </summary>
        Byte1,
        /// <summary>
        /// Two 8-bit unsigned integers.
        /// </summary>
        Byte2,
        /// <summary>
        /// Four 8-bit unsigned integers.
        /// </summary>
        Byte4,
    }
}