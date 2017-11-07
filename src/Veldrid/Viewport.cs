using System;

namespace Veldrid
{
    /// <summary>
    /// Describes a 3-dimensional region.
    /// </summary>
    public struct Viewport : IEquatable<Viewport>
    {
        /// <summary>
        /// The minimum X value.
        /// </summary>
        public float X;
        /// <summary>
        /// The minimum Y value.
        /// </summary>
        public float Y;
        /// <summary>
        /// The width.
        /// </summary>
        public float Width;
        /// <summary>
        /// The height.
        /// </summary>
        public float Height;
        /// <summary>
        /// The minimum depth.
        /// </summary>
        public float MinDepth;
        /// <summary>
        /// The maximum depth.
        /// </summary>
        public float MaxDepth;

        /// <summary>
        /// Constructs a new Viewport.
        /// </summary>
        /// <param name="x">The minimum X value.</param>
        /// <param name="y">The minimum Y value.</param>
        /// <param name="width">The width.</param>
        /// <param name="height">The height.</param>
        /// <param name="minDepth">The minimum depth.</param>
        /// <param name="maxDepth">The maximum depth.</param>
        public Viewport(float x, float y, float width, float height, float minDepth, float maxDepth)
        {
            X = x;
            Y = y;
            Width = width;
            Height = height;
            MinDepth = minDepth;
            MaxDepth = maxDepth;
        }

        /// <summary>
        /// Element-wise equality.
        /// </summary>
        /// <param name="other">The instance to compare to.</param>
        /// <returns>True if all elements are equal; false otherswise.</returns>
        public bool Equals(Viewport other)
        {
            return X.Equals(other.X) && Y.Equals(other.Y)
                && Width.Equals(other.Width) && Height.Equals(other.Height)
                && MinDepth.Equals(other.MinDepth) && MaxDepth.Equals(other.MaxDepth);
        }

        /// <summary>
        /// Returns the hash code for this instance.
        /// </summary>
        /// <returns>A 32-bit signed integer that is the hash code for this instance.</returns>
        public override int GetHashCode()
        {
            return HashHelper.Combine(
                X.GetHashCode(),
                Y.GetHashCode(),
                Width.GetHashCode(),
                Height.GetHashCode(),
                MinDepth.GetHashCode(),
                MaxDepth.GetHashCode());
        }
    }
}
