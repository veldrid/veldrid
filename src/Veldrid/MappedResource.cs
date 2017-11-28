using System;

namespace Veldrid
{
    /// <summary>
    /// A structure describing the layout of a mapped <see cref="MappableResource"/> object.
    /// </summary>
    public struct MappedResource
    {
        /// <summary>
        /// The resource which has been mapped.
        /// </summary>
        public readonly MappableResource Resource;
        /// <summary>
        /// Identifies the <see cref="MapMode"/> that was used to map the resource.
        /// </summary>
        public MapMode Mode;
        /// <summary>
        /// A pointer to the start of the mapped data region.
        /// </summary>
        public readonly IntPtr Data;
        /// <summary>
        /// The total size, in bytes, of the mapped data region.
        /// </summary>
        public readonly uint SizeInBytes;
        /// <summary>
        /// For mapped <see cref="Texture"/> resources, this is the number of bytes between each row of texels.
        /// For <see cref="Buffer"/> resources, this field has no meaning.
        /// </summary>
        public readonly uint RowPitch;
        /// <summary>
        /// For mapped <see cref="Texture"/> resources, this is the number of bytes between each slice of a 3D Texture.
        /// For <see cref="Buffer"/> resources, this field has no meaning.
        /// </summary>
        public readonly uint DepthPitch;
        /// <summary>
        /// For mapped <see cref="Texture"/> resources, this is the number of bytes between each array layer of a Texture.
        /// For <see cref="Buffer"/> resources, this field has no meaning.
        /// </summary>
        public readonly uint ArrayPitch;

        internal MappedResource(MappableResource resource, MapMode mode, IntPtr data, uint sizeInBytes)
        {
            Resource = resource;
            Mode = mode;
            Data = data;
            SizeInBytes = sizeInBytes;

            RowPitch = 0;
            DepthPitch = 0;
            ArrayPitch = 0;
        }
    }
}