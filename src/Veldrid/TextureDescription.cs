using System;

namespace Veldrid
{
    public struct TextureDescription : IEquatable<TextureDescription>
    {
        public uint Width;
        public uint Height;
        public uint Depth;
        public uint MipLevels;
        public uint ArrayLayers;
        public PixelFormat Format;
        public TextureUsage Usage;

        public TextureDescription(
            uint width,
            uint height,
            uint depth,
            uint mipLevels,
            uint arrayLayers,
            PixelFormat format,
            TextureUsage usage)
        {
            Width = width;
            Height = height;
            Depth = depth;
            MipLevels = mipLevels;
            ArrayLayers = arrayLayers;
            Format = format;
            Usage = usage;
        }

        public bool Equals(TextureDescription other)
        {
            return Width.Equals(other.Width)
                && Height.Equals(other.Height)
                && Depth.Equals(other.Depth)
                && MipLevels.Equals(other.MipLevels)
                && ArrayLayers.Equals(other.ArrayLayers)
                && Format == other.Format
                && Usage == other.Usage;
        }

        public override int GetHashCode()
        {
            return HashHelper.Combine(
                Width.GetHashCode(),
                Height.GetHashCode(),
                Depth.GetHashCode(),
                MipLevels.GetHashCode(),
                ArrayLayers.GetHashCode(),
                Format.GetHashCode(),
                Usage.GetHashCode());
        }
    }
}