using System;

namespace Veldrid
{
    /// <summary>
    /// Describes how stencil tests are performed in a <see cref="Pipeline"/>'s depth-stencil state.
    /// </summary>
    public struct StencilBehaviorDescription : IEquatable<StencilBehaviorDescription>
    {
        /// <summary>
        /// The operation performed on samples that fail the stencil test.
        /// </summary>
        public StencilOperation Fail;
        /// <summary>
        /// The operation performed on samples that pass the stencil test.
        /// </summary>
        public StencilOperation Pass;
        /// <summary>
        /// The operation performed on samples that pass the stencil test but fail the depth test.
        /// </summary>
        public StencilOperation DepthFail;
        /// <summary>
        /// The comparison operator used in the stencil test.
        /// </summary>
        public ComparisonKind Comparison;

        /// <summary>
        /// Constructs a new StencilBehaviorDescription.
        /// </summary>
        /// <param name="fail">The operation performed on samples that fail the stencil test.</param>
        /// <param name="pass">The operation performed on samples that pass the stencil test.</param>
        /// <param name="depthFail">The operation performed on samples that pass the stencil test but fail the depth test.</param>
        /// <param name="comparison">The comparison operator used in the stencil test.</param>
        public StencilBehaviorDescription(
            StencilOperation fail,
            StencilOperation pass,
            StencilOperation depthFail,
            ComparisonKind comparison)
        {
            Fail = fail;
            Pass = pass;
            DepthFail = depthFail;
            Comparison = comparison;
        }

        /// <summary>
        /// Element-wise equality.
        /// </summary>
        /// <param name="other">The instance to compare to.</param>
        /// <returns>True if all elements are equal; false otherswise.</returns>
        public bool Equals(StencilBehaviorDescription other)
        {
            return Fail == other.Fail && Pass == other.Pass && DepthFail == other.DepthFail && Comparison == other.Comparison;
        }

        /// <summary>
        /// Returns the hash code for this instance.
        /// </summary>
        /// <returns>A 32-bit signed integer that is the hash code for this instance.</returns>
        public override int GetHashCode()
        {
            return HashHelper.Combine((int)Fail, (int)Pass, (int)DepthFail, (int)Comparison);
        }
    }
}
