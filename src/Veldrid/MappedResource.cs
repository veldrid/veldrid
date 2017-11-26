using System;

namespace Veldrid
{
    public struct MappedResource
    {
        public readonly MappableResource Resource;
        public readonly IntPtr Data;
        public readonly uint SizeInBytes;

        internal MappedResource(MappableResource resource, IntPtr data, uint sizeInBytes)
        {
            Resource = resource;
            Data = data;
            SizeInBytes = sizeInBytes;
        }
    }
}