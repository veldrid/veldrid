namespace Veldrid
{
    /// <summary>
    /// Describes how new values are compared with existing values in a depth or stencil comparison.
    /// </summary>
    public enum ComparisonKind : byte
    {
        /// <summary>
        /// The comparison never succeeds.
        /// </summary>
        Never,
        /// <summary>
        /// The comparison succeeds when the new value is less than the existing value.
        /// </summary>
        Less,
        /// <summary>
        /// The comparison succeeds when the new value is equal to the existing value.
        /// </summary>
        Equal,
        /// <summary>
        /// The comparison succeeds when the new value is less than or equal to the existing value.
        /// </summary>
        LessEqual,
        /// <summary>
        /// The comparison succeeds when the new value is greater than the existing value.
        /// </summary>
        Greater,
        /// <summary>
        /// The comparison succeeds when the new value is not equal to the existing value.
        /// </summary>
        NotEqual,
        /// <summary>
        /// The comparison succeeds when the new value is greater than or equal to the existing value.
        /// </summary>
        GreaterEqual,
        /// <summary>
        /// The comparison always succeeds.
        /// </summary>
        Always,
    }
}
