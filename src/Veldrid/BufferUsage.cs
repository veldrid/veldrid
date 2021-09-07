using System;

namespace Veldrid
{
    /// <summary>
    /// A bitmask describing the permitted uses of a <see cref="DeviceBuffer"/> object.
    /// </summary>
    [Flags]
    public enum BufferUsage : ushort
    {
        /// <summary>
        /// The <see cref="DeviceBuffer"/> can be used as the source of vertex data for drawing commands.
        /// This flag enables the use of a Buffer in the <see cref="CommandList.SetVertexBuffer(uint, DeviceBuffer)"/> method.
        /// </summary>
        VertexBuffer = 1 << 0,

        /// <summary>
        /// The <see cref="DeviceBuffer"/> can be used as the source of index data for drawing commands.
        /// This flag enables the use of a Buffer in the <see cref="CommandList.SetIndexBuffer(DeviceBuffer, IndexFormat)" /> method.
        /// </summary>
        IndexBuffer = 1 << 1,

        /// <summary>
        /// The <see cref="DeviceBuffer"/> can be used as a uniform Buffer.
        /// This flag enables the use of a Buffer in a <see cref="ResourceSet"/> as a uniform Buffer.
        /// </summary>
        UniformBuffer = 1 << 2,

        /// <summary>
        /// The <see cref="DeviceBuffer"/> can be used as a read-only structured Buffer.
        /// This flag enables the use of a Buffer in a <see cref="ResourceSet"/> as a read-only structured Buffer.
        /// This flag can only be combined with <see cref="DynamicWrite"/>.
        /// </summary>
        StructuredBufferReadOnly = 1 << 3,

        /// <summary>
        /// The <see cref="DeviceBuffer"/> can be used as a read-write structured Buffer.
        /// This flag enables the use of a Buffer in a <see cref="ResourceSet"/> as a read-write structured Buffer.
        /// This flag cannot be combined with any other flag.
        /// </summary>
        StructuredBufferReadWrite = 1 << 4,

        /// <summary>
        /// The <see cref="DeviceBuffer"/> can be used as the source of indirect drawing information.
        /// This flag enables the use of a Buffer in the *Indirect methods of <see cref="CommandList"/>.
        /// This flag cannot be combined with <see cref="DynamicWrite"/> or <see cref="DynamicRead"/>.
        /// </summary>
        IndirectBuffer = 1 << 5,

        /// <summary>
        /// The <see cref="DeviceBuffer"/> will be updated with new data very frequently
        /// and can be mapped with <see cref="MapMode.Write"/>.
        /// This flag cannot be combined with <see cref="StructuredBufferReadWrite"/> or <see cref="IndirectBuffer"/>.
        /// </summary>
        /// <seealso cref="GraphicsDevice.Map(MappableResource, MapMode)"/>
        DynamicWrite = 1 << 7,

        /// <summary>
        /// The <see cref="DeviceBuffer"/> will be updated with new data very frequently
        /// and can be mapped with <see cref="MapMode.Read"/>.
        /// This flag cannot be combined with <see cref="StructuredBufferReadWrite"/> or <see cref="IndirectBuffer"/>.
        /// </summary>
        /// <seealso cref="GraphicsDevice.Map(MappableResource, MapMode)"/>
        DynamicRead = 1 << 6,

        /// <seealso cref="GraphicsDevice.Map(MappableResource, MapMode)"/>
        DynamicReadWrite = DynamicWrite | DynamicRead,

        /// <summary>
        /// The <see cref="DeviceBuffer"/> will be used as a writable staging buffer,
        /// which can be used to transfer data to the GPU by mapping with <see cref="MapMode.Write"/>. 
        /// This flag can only be combined with <see cref="StagingRead"/>.
        /// </summary>
        /// <seealso cref="GraphicsDevice.Map(MappableResource, MapMode)"/>
        StagingWrite = 1 << 8,

        /// <summary>
        /// The <see cref="DeviceBuffer"/> will be used as a readable staging buffer,
        /// which can be used to transfer data from the GPU by mapping with <see cref="MapMode.Read"/>. 
        /// This flag can only be combined with <see cref="StagingWrite"/>.
        /// </summary>
        /// <seealso cref="GraphicsDevice.Map(MappableResource, MapMode)"/>
        StagingRead = 1 << 9,

        /// <seealso cref="GraphicsDevice.Map(MappableResource, MapMode)"/>
        StagingReadWrite = StagingWrite | StagingRead,
    }
}
