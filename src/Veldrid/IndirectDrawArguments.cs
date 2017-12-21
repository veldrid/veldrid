namespace Veldrid
{
    /// <summary>
    /// A structure describing the format expected by indirect draw commands contained in an indirect <see cref="DeviceBuffer"/>.
    /// </summary>
    public struct IndirectDrawArguments
    {
        /// <summary>
        /// The number of vertices to draw.
        /// </summary>
        public uint VertexCount;
        /// <summary>
        /// The number of instances to draw.
        /// </summary>
        public uint InstanceCount;
        /// <summary>
        /// The first vertex to draw. Subsequent vertices are incremented by 1.
        /// </summary>
        public uint FirstVertex;
        /// <summary>
        /// The first instance to draw. Subsequent instances are incrmented by 1.
        /// </summary>
        public uint FirstInstance;
    }
}
