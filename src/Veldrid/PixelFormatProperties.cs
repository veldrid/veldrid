namespace Veldrid
{
    /// <summary>
    /// Describes the properties that are supported for a particular combination of <see cref="PixelFormat"/>,
    /// <see cref="TextureType"/>, and <see cref="TextureUsage"/> by a <see cref="GraphicsDevice"/>.
    /// See <see cref="GraphicsDevice.GetPixelFormatSupport(PixelFormat, TextureType, TextureUsage, out PixelFormatProperties)"/>.
    /// </summary>
    public struct PixelFormatProperties
    {
        /// <summary>
        /// The maximum supported width.
        /// </summary>
        public readonly uint MaxWidth;
        /// <summary>
        /// The maximum supported height.
        /// </summary>
        public readonly uint MaxHeight;
        /// <summary>
        /// The maximum supported depth.
        /// </summary>
        public readonly uint MaxDepth;
        /// <summary>
        /// The maximum supported number of mipmap levels.
        /// </summary>
        public readonly uint MaxMipLevels;
        /// <summary>
        /// The maximum supported number of array layers.
        /// </summary>
        public readonly uint MaxArrayLayers;

        private readonly uint _sampleCounts;

        /// <summary>
        /// Gets a value indicating whether or not the given <see cref="TextureSampleCount"/> is supported.
        /// </summary>
        /// <param name="count">The <see cref="TextureSampleCount"/> to query.</param>
        /// <returns>True if the sample count is supported; false otherwise.</returns>
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
