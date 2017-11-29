using System;
using System.Runtime.CompilerServices;

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
        public readonly MapMode Mode;
        /// <summary>
        /// A pointer to the start of the mapped data region.
        /// </summary>
        public readonly IntPtr Data;
        /// <summary>
        /// The total size, in bytes, of the mapped data region.
        /// </summary>
        public readonly uint SizeInBytes;
        /// <summary>
        /// For mapped <see cref="Texture"/> resources, this is the subresource which is mapped.
        /// For <see cref="Buffer"/> resources, this field has no meaning.
        /// </summary>
        public readonly uint Subresource;
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

        internal MappedResource(
            MappableResource resource, 
            MapMode mode, 
            IntPtr data, 
            uint sizeInBytes, 
            uint subresource, 
            uint rowPitch, 
            uint depthPitch)
        {
            Resource = resource;
            Mode = mode;
            Data = data;
            SizeInBytes = sizeInBytes;
            Subresource = subresource;
            RowPitch = rowPitch;
            DepthPitch = depthPitch;
        }

        internal MappedResource(MappableResource resource, MapMode mode, IntPtr data, uint sizeInBytes)
        {
            Resource = resource;
            Mode = mode;
            Data = data;
            SizeInBytes = sizeInBytes;

            Subresource = 0;
            RowPitch = 0;
            DepthPitch = 0;
        }
    }

    public unsafe struct MappedResourceView<T> where T : struct
    {
        private static readonly int s_sizeofT = Unsafe.SizeOf<T>();

        public readonly MappedResource MappedResource;
        public readonly uint SizeInBytes;
        public readonly int Count;

        public MappedResourceView(MappedResource rawResource)
        {
            MappedResource = rawResource;
            SizeInBytes = rawResource.SizeInBytes;
            Count = (int)(SizeInBytes / s_sizeofT);
        }

        public ref T this[int index]
        {
            get
            {
                if (index >= Count || index < 0)
                {
                    throw new IndexOutOfRangeException(
                        $"Given index ({index}) must be non-negative and less than Count ({Count}).");
                }

                byte* ptr = (byte*)MappedResource.Data + (index * s_sizeofT);
                return ref Unsafe.AsRef<T>(ptr);
            }
        }
    }
}