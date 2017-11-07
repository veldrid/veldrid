using System;

namespace Veldrid
{
    /// <summary>
    /// A <see cref="Pipeline"/> component describing the properties of the depth stencil state.
    /// </summary>
    public struct DepthStencilStateDescription : IEquatable<DepthStencilStateDescription>
    {
        /// <summary>
        /// Controls whether depth testing is enabled.
        /// </summary>
        public bool DepthTestEnabled;
        /// <summary>
        /// Controls whether new depth values are written to the depth buffer.
        /// </summary>
        public bool DepthWriteEnabled;
        /// <summary>
        /// The <see cref="DepthComparisonKind"/> used when considering new depth values.
        /// </summary>
        public DepthComparisonKind ComparisonKind;

        /// <summary>
        /// Constructs a new <see cref="DepthStencilStateDescription"/>.
        /// </summary>
        /// <param name="depthTestEnabled">Controls whether depth testing is enabled.</param>
        /// <param name="depthWriteEnabled">Controls whether new depth values are written to the depth buffer.</param>
        /// <param name="comparisonKind">The <see cref="DepthComparisonKind"/> used when considering new depth values.</param>
        public DepthStencilStateDescription(bool depthTestEnabled, bool depthWriteEnabled, DepthComparisonKind comparisonKind)
        {
            DepthTestEnabled = depthTestEnabled;
            DepthWriteEnabled = depthWriteEnabled;
            ComparisonKind = comparisonKind;
        }

        /// <summary>
        /// Describes a depth stencil state which uses a <see cref="DepthComparisonKind.LessEqual"/> comparison.
        /// Settings:
        ///     DepthTestEnabled = true
        ///     DepthWriteEnabled = true
        ///     ComparisonKind = DepthComparisonKind.LessEqual
        /// </summary>
        public static readonly DepthStencilStateDescription LessEqual = new DepthStencilStateDescription
        {
            DepthTestEnabled = true,
            DepthWriteEnabled = true,
            ComparisonKind = DepthComparisonKind.LessEqual
        };

        /// <summary>
        /// Describes a depth stencil state in which depth testing and writing is disabled.
        /// Settings:
        ///     DepthTestEnabled = false
        ///     DepthWriteEnabled = false
        ///     ComparisonKind = DepthComparisonKind.LessEqual
        /// </summary>
        public static readonly DepthStencilStateDescription Disabled = new DepthStencilStateDescription
        {
            DepthTestEnabled = false,
            DepthWriteEnabled = false,
            ComparisonKind = DepthComparisonKind.LessEqual
        };

        /// <summary>
        /// Element-wise equality.
        /// </summary>
        /// <param name="other">The instance to compare to.</param>
        /// <returns>True if all elements are equal; false otherswise.</returns>
        public bool Equals(DepthStencilStateDescription other)
        {
            return DepthTestEnabled.Equals(other.DepthTestEnabled) && DepthWriteEnabled.Equals(other.DepthWriteEnabled)
                && ComparisonKind == other.ComparisonKind;
        }

        /// <summary>
        /// Returns the hash code for this instance.
        /// </summary>
        /// <returns>A 32-bit signed integer that is the hash code for this instance.</returns>
        public override int GetHashCode()
        {
            return HashHelper.Combine(DepthTestEnabled.GetHashCode(), DepthWriteEnabled.GetHashCode(), ComparisonKind.GetHashCode());
        }
    }
}