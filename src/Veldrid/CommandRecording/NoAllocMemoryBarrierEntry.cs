namespace Veldrid.CommandRecording
{
    internal struct NoAllocMemoryBarrierEntry
    {
        public Tracked<Texture> Texture;
        public uint BaseMipLevel;
        public uint LevelCount;
        public uint BaseArrayLayer;
        public uint LayerCount;
        public ShaderStages SourceStage;
        public ShaderStages DestinationStage;

        public NoAllocMemoryBarrierEntry(
            Tracked<Texture> tracked,
            uint baseMipLevel, uint levelCount,
            uint baseArrayLayer, uint layerCount,
            ShaderStages sourceStage,
            ShaderStages destinationStage)
        {
            Texture = tracked;
            BaseMipLevel = baseMipLevel;
            LevelCount = levelCount;
            BaseArrayLayer = baseArrayLayer;
            LayerCount = layerCount;
            SourceStage = sourceStage;
            DestinationStage = destinationStage;
        }
    }
}
