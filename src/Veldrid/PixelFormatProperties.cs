namespace Veldrid
{
    public struct PixelFormatProperties
    {
        public readonly uint MaxWidth;
        public readonly uint MaxHeight;
        public readonly uint MaxDepth;
        public readonly uint MaxMipLevels;
        public readonly uint MaxArrayLayers;
        private readonly uint _sampleCounts;

        public bool IsSampleCountSupported(TextureSampleCount count)
        {
            int bit = (int)count;
            return (_sampleCounts & (1 << bit)) != 0;
        }

        internal PixelFormatProperties(
            uint maxWidth,
            uint maxHeight,
            uint maxDepth,
            uint maxMipLevels,
            uint maxArrayLayers,
            uint sampleCounts)
        {
            MaxWidth = maxWidth;
            MaxHeight = maxHeight;
            MaxDepth = maxDepth;
            MaxMipLevels = maxMipLevels;
            MaxArrayLayers = maxArrayLayers;
            _sampleCounts = sampleCounts;
        }
    }
}
