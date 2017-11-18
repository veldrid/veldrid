using System;

namespace Veldrid
{
    /// <summary>
    /// Describes a <see cref="TextureView"/>, for creation using a <see cref="ResourceFactory"/>.
    /// </summary>
    public struct TextureViewDescription : IEquatable<TextureViewDescription>
    {
        /// <summary>
        /// The desired target <see cref="Texture"/>. This <see cref="Texture"/> must have been created with the
        /// <see cref="TextureUsage.Sampled"/> usage flag.
        /// </summary>
        public Texture Target;

        public uint BaseMipLevel;

        public uint MipLevels;

        public uint BaseArrayLayer;

        public uint ArrayLayers;

        /// <summary>
        /// Constructs a new TextureViewDescription.
        /// </summary>
        /// <param name="target">The desired target <see cref="Texture"/>. This <see cref="Texture"/> must have been created
        /// with the <see cref="TextureUsage.Sampled"/> usage flag.</param>
        public TextureViewDescription(Texture target)
        {
            Target = target;
            BaseMipLevel = 0;
            MipLevels = target.MipLevels;
            BaseArrayLayer = 0;
            ArrayLayers = target.ArrayLayers;
        }

        /// <summary>
        /// Element-wise equality.
        /// </summary>
        /// <param name="other">The instance to compare to.</param>
        /// <returns>True if all elements are equal; false otherswise.</returns>
        public bool Equals(TextureViewDescription other)
        {
            return Target.Equals(other.Target);
        }

        /// <summary>
        /// Returns the hash code for this instance.
        /// </summary>
        /// <returns>A 32-bit signed integer that is the hash code for this instance.</returns>
        public override int GetHashCode()
        {
            return Target.GetHashCode();
        }
    }
}