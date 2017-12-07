namespace Veldrid.Vk
{
    internal struct DescriptorResourceCounts
    {
        public readonly uint UniformBufferCount;
        public readonly uint SampledImageCount;
        public readonly uint SamplerCount;
        public readonly uint StorageBufferCount;
        public readonly uint StorageImageCount;

        public DescriptorResourceCounts(
            uint uniformBufferCount,
            uint sampledImageCount,
            uint samplerCount,
            uint storageBufferCount,
            uint storageImageCount)
        {
            UniformBufferCount = uniformBufferCount;
            SampledImageCount = sampledImageCount;
            SamplerCount = samplerCount;
            StorageBufferCount = storageBufferCount;
            StorageImageCount = storageImageCount;
        }
    }
}
