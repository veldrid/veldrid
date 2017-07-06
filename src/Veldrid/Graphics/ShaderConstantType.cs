namespace Veldrid.Graphics
{
    /// <summary>
    /// Describes the kind of data stored in a shader constant.
    /// </summary>
    public enum ShaderConstantType : byte
    {
        /// <summary>
        /// A one-byte boolean value.
        /// </summary>
        Boolean,
        /// <summary>
        /// A value containing one 32-bit integer.
        /// </summary>
        Int1,
        /// <summary>
        /// A value containing two 32-bit integers.
        /// </summary>
        Int2,
        /// <summary>
        /// A value containing three 32-bit integers.
        /// </summary>
        Int3,
        /// <summary>
        /// A value containing four 32-bit integers.
        /// </summary>
        Int4,
        /// <summary>
        /// A value containing one 32-bit floating point number.
        /// </summary>
        Float1,
        /// <summary>
        /// A value containing two 32-bit floating point numbers.
        /// </summary>
        Float2,
        /// <summary>
        /// A value containing three 32-bit floating point numbers.
        /// </summary>
        Float3,
        /// <summary>
        /// A value containing four 32-bit floating point numbers.
        /// </summary>
        Float4,
        /// <summary>
        /// A value containing a 4x4 matrix, or 16 32-bit floating point numbers.
        /// </summary>
        Matrix4x4,
        /// <summary>
        /// A custom structure value, containing arbitrary data.
        /// </summary>
        Custom
    }
}