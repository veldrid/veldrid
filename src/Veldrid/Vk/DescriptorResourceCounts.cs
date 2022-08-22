namespace Veldrid.Vulkan
{
    internal struct DescriptorResourceCounts
    {
        public readonly uint UniformBufferCount;
        public readonly uint SampledImageCount;
        public readonly uint SamplerCount;
        public readonly uint StorageBufferCount;
        public readonly uint StorageImageCount;
        public readonly uint UniformBufferDynamicCount;
        public readonly uint StorageBufferDynamicCount;

        public DescriptorResourceCounts(
            uint uniformBufferCount,
            uint uniformBufferDynamicCount,
            uint sampledImageCount,
            uint samplerCount,
            uint storageBufferCount,
            uint storageBufferDynamicCount,
            uint storageImageCount)
        {
            UniformBufferCount = uniformBufferCount;
            UniformBufferDynamicCount = uniformBufferDynamicCount;
            SampledImageCount = sampledImageCount;
            SamplerCount = samplerCount;
            StorageBufferCount = storageBufferCount;
            StorageBufferDynamicCount = storageBufferDynamicCount;
            StorageImageCount = storageImageCount;
        }
    }
}
