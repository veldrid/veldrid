namespace Veldrid
{
    /// <summary>
    /// A structure describing the format expected by indirect, indexed draw commands contained in an indirect
    /// <see cref="DeviceBuffer"/>.
    /// </summary>
    public struct IndirectDrawIndexedArguments
    {
        /// <summary>
        /// The number of indices to use in the indexed draw.
        /// </summary>
        public uint IndexCount;
        /// <summary>
        /// The number of instances to draw.
        /// </summary>
        public uint InstanceCount;
        /// <summary>
        /// The value which is used as the start of the indices used for the draw.
        /// </summary>
        public uint FirstIndex;
        /// <summary>
        /// An offset which is added to each vertex element referenced by the index <see cref="DeviceBuffer"/>.
        /// </summary>
        public int VertexOffset;
        /// <summary>
        /// The first instance to draw. Subsequent instances (if InstanceCount > 1) are incremented by 1.
        /// </summary>
        public uint FirstInstance;
    }
}
