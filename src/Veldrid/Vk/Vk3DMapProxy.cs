using System;

namespace Veldrid.Vk
{
    internal unsafe struct Vk3DMapProxy
    {
        public FixedStagingBlock StagingBlock { get; }
        public MappedResource MappedResource { get; }

        public Vk3DMapProxy(
            FixedStagingBlock block,
            MappableResource resource,
            MapMode mode,
            uint subresource,
            uint rowPitch,
            uint depthPitch)
        {
            StagingBlock = block;
            MappedResource = new MappedResource(
                resource,
                mode,
                (IntPtr)block.Data,
                block.SizeInBytes,
                subresource,
                rowPitch,
                depthPitch);
        }

        public void Free() => StagingBlock.Free();
    }
}
