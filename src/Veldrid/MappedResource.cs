using System;

namespace Veldrid
{
    public struct MappedResource
    {
        public readonly MappableResource Resource;
        public readonly IntPtr Data;
        public readonly uint SizeInBytes;
        public readonly uint RowPitch;
        public readonly uint DepthPitch;
        public readonly uint ArrayPitch;

        internal MappedResource(MappableResource resource, IntPtr data, uint sizeInBytes)
        {
            Resource = resource;
            Data = data;
            SizeInBytes = sizeInBytes;

            RowPitch = 0;
            DepthPitch = 0;
            ArrayPitch = 0;
        }
    }
}