namespace Veldrid
{
    /// <summary>
    /// A bitmask describing the permitted uses of a <see cref="Buffer"/> object.
    /// </summary>
    public enum BufferUsage : byte
    {
        /// <summary>
        /// Indicates that a <see cref="Buffer"/> can be used as the source of vertex data for drawing commands.
        /// This flag enables the use of a Buffer in the <see cref="CommandList.SetVertexBuffer(uint, Buffer)"/> method.
        /// </summary>
        VertexBuffer = 1 << 0,
        /// <summary>
        /// Indicates that a <see cref="Buffer"/> can be used as the source of index data for drawing commands.
        /// This flag enables the use of a Buffer in the <see cref="CommandList.SetIndexBuffer(Buffer, IndexFormat)" /> method.
        /// </summary>
        IndexBuffer = 1 << 1,
        /// <summary>
        /// Indicates that a <see cref="Buffer"/> can be used as a uniform buffer.
        /// This flag enables the use of a Buffer in a <see cref="ResourceSet"/> as a uniform Buffer.
        /// </summary>
        UniformBuffer = 1 << 2,
        /// <summary>
        /// Indicates that a <see cref="Buffer"/> can be used as a read-only structured buffer.
        /// This flag enables the use of a Buffer in a <see cref="ResourceSet"/> as a read-only structured Buffer.
        /// </summary>
        StructuredBufferReadOnly = 1 << 3,
        /// <summary>
        /// Indicates that a <see cref="Buffer"/> can be used as a read-write structured buffer.
        /// This flag enables the use of a Buffer in a <see cref="ResourceSet"/> as a read-write structured Buffer.
        /// </summary>
        StructuredBufferReadWrite = 1 << 4,
        /// <summary>
        /// Indicates that a <see cref="Buffer"/> can be used as the source of indirect drawing information.
        /// This flag enables the use of a buffer in the DrawIndirect methods of <see cref="CommandList"/>.
        /// </summary>
        IndirectBuffer = 1 << 5,
        Mappable = 1 << 6,
        /// <summary>
        /// A mappable Buffer which will be written to and read from continuously.
        /// </summary>
        Dynamic = 1 << 7,
    }
}
