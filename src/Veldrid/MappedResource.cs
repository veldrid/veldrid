using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Veldrid
{
    /// <summary>
    /// A structure describing the layout of a mapped <see cref="MappableResource"/> object.
    /// </summary>
    public readonly struct MappedResource
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
        /// The offset, in bytes, from the beginning of the mapped resource.
        /// </summary>
        public readonly uint OffsetInBytes;

        /// <summary>
        /// The total size, in bytes, of the mapped data region.
        /// </summary>
        public readonly uint SizeInBytes;

        /// <summary>
        /// For mapped <see cref="Texture"/> resources, this is the subresource which is mapped.
        /// For <see cref="DeviceBuffer"/> resources, this field has no meaning.
        /// </summary>
        public readonly uint Subresource;

        /// <summary>
        /// For mapped <see cref="Texture"/> resources, this is the number of bytes between each row of texels.
        /// For <see cref="DeviceBuffer"/> resources, this field has no meaning.
        /// </summary>
        public readonly uint RowPitch;

        /// <summary>
        /// For mapped <see cref="Texture"/> resources, this is the number of bytes between each depth slice of a 3D Texture.
        /// For <see cref="DeviceBuffer"/> resources or 2D Textures, this field has no meaning.
        /// </summary>
        public readonly uint DepthPitch;

        internal MappedResource(
            MappableResource resource,
            MapMode mode,
            IntPtr data,
            uint offsetInBytes,
            uint sizeInBytes,
            uint subresource,
            uint rowPitch,
            uint depthPitch)
        {
            Resource = resource;
            Mode = mode;
            Data = data;
            OffsetInBytes = offsetInBytes;
            SizeInBytes = sizeInBytes;
            Subresource = subresource;
            RowPitch = rowPitch;
            DepthPitch = depthPitch;
        }

        internal MappedResource(MappableResource resource, MapMode mode, IntPtr data, uint offsetInBytes, uint sizeInBytes)
        {
            Resource = resource;
            Mode = mode;
            Data = data;
            OffsetInBytes = offsetInBytes;
            SizeInBytes = sizeInBytes;

            Subresource = 0;
            RowPitch = 0;
            DepthPitch = 0;
        }

        /// <summary>
        /// Creates a span of bytes over the mapped data region.
        /// </summary>
        /// <returns>The span of bytes.</returns>
        public readonly unsafe Span<byte> AsBytes()
        {
            return new Span<byte>((void*)Data, (int)SizeInBytes);
        }
    }

    /// <summary>
    /// A typed view of a <see cref="Veldrid.MappedResource"/>. Provides by-reference structured access to individual elements in the
    /// mapped resource.
    /// </summary>
    /// <typeparam name="T">The blittable value type which mapped data is viewed as.</typeparam>
    public unsafe readonly struct MappedResourceView<T> where T : unmanaged
    {
        /// <summary>
        /// The <see cref="Veldrid.MappedResource"/> that this instance views.
        /// </summary>
        public readonly MappedResource MappedResource;

        /// <summary>
        /// The total size in bytes of the mapped resource.
        /// </summary>
        public readonly uint SizeInBytes => MappedResource.SizeInBytes;

        /// <summary>
        /// The total number of structures that is contained in the resource. This is effectively the total number of bytes
        /// divided by the size of the structure type.
        /// </summary>
        public readonly int Count;

        /// <summary>
        /// Constructs a new <see cref="MappedResourceView{T}"/> which wraps the given <see cref="Veldrid.MappedResource"/>.
        /// </summary>
        /// <param name="rawResource">The raw resource which has been mapped.</param>
        public MappedResourceView(MappedResource rawResource)
        {
            MappedResource = rawResource;
            Count = (int)(MappedResource.SizeInBytes / (uint)Unsafe.SizeOf<T>());
        }

        /// <summary>
        /// Gets a reference to the structure value at the given index.
        /// </summary>
        /// <param name="index">The index of the value.</param>
        /// <returns>A reference to the value at the given index.</returns>
        public ref T this[int index]
        {
            get
            {
                byte* ptr = (byte*)MappedResource.Data + (index * Unsafe.SizeOf<T>());
                return ref Unsafe.AsRef<T>(ptr);
            }
        }

        /// <summary>
        /// Gets a reference to the structure value at the given index.
        /// </summary>
        /// <param name="index">The index of the value.</param>
        /// <returns>A reference to the value at the given index.</returns>
        public ref T this[uint index]
        {
            get
            {
                byte* ptr = (byte*)MappedResource.Data + (index * Unsafe.SizeOf<T>());
                return ref Unsafe.AsRef<T>(ptr);
            }
        }

        /// <summary>
        /// Gets a reference to the structure at the given 2-dimensional texture coordinates.
        /// </summary>
        /// <param name="x">The X coordinate.</param>
        /// <param name="y">The Y coordinate.</param>
        /// <returns>A reference to the value at the given coordinates.</returns>
        public ref T this[int x, int y]
        {
            get
            {
                byte* ptr = (byte*)MappedResource.Data + (y * MappedResource.RowPitch) + (x * Unsafe.SizeOf<T>());
                return ref Unsafe.AsRef<T>(ptr);
            }
        }

        /// <summary>
        /// Gets a reference to the structure at the given 2-dimensional texture coordinates.
        /// </summary>
        /// <param name="x">The X coordinate.</param>
        /// <param name="y">The Y coordinate.</param>
        /// <returns>A reference to the value at the given coordinates.</returns>
        public ref T this[uint x, uint y]
        {
            get
            {
                byte* ptr = (byte*)MappedResource.Data + (y * MappedResource.RowPitch) + (x * Unsafe.SizeOf<T>());
                return ref Unsafe.AsRef<T>(ptr);
            }
        }

        /// <summary>
        /// Gets a reference to the structure at the given 3-dimensional texture coordinates.
        /// </summary>
        /// <param name="x">The X coordinate.</param>
        /// <param name="y">The Y coordinate.</param>
        /// <param name="z">The Z coordinate.</param>
        /// <returns>A reference to the value at the given coordinates.</returns>
        public ref T this[int x, int y, int z]
        {
            get
            {
                byte* ptr = (byte*)MappedResource.Data
                    + (z * MappedResource.DepthPitch)
                    + (y * MappedResource.RowPitch)
                    + (x * Unsafe.SizeOf<T>());
                return ref Unsafe.AsRef<T>(ptr);
            }
        }

        /// <summary>
        /// Gets a reference to the structure at the given 3-dimensional texture coordinates.
        /// </summary>
        /// <param name="x">The X coordinate.</param>
        /// <param name="y">The Y coordinate.</param>
        /// <param name="z">The Z coordinate.</param>
        /// <returns>A reference to the value at the given coordinates.</returns>
        public ref T this[uint x, uint y, uint z]
        {
            get
            {
                byte* ptr = (byte*)MappedResource.Data
                    + (z * MappedResource.DepthPitch)
                    + (y * MappedResource.RowPitch)
                    + (x * Unsafe.SizeOf<T>());
                return ref Unsafe.AsRef<T>(ptr);
            }
        }

        /// <summary>
        /// Creates a span of bytes over the mapped data region.
        /// </summary>
        /// <returns></returns>
        public readonly unsafe Span<byte> AsBytes()
        {
            return MappedResource.AsBytes();
        }

        /// <summary>
        /// Creates a span of structures over the mapped data region.
        /// </summary>
        /// <returns>The span of structures.</returns>
        public readonly unsafe Span<T> AsSpan()
        {
            return MemoryMarshal.Cast<byte, T>(MappedResource.AsBytes());
        }
    }
}
