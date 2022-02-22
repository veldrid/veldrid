using System;

namespace Veldrid
{
    /// <summary>
    /// A color bitmask representing the components which can be written to.
    /// </summary>
    [Flags]
    public enum ColorWriteMask
    {
        /// <summary>
        /// No color component will be written to.
        /// </summary>
        None,

        /// <summary>
        /// The red component will be written to.
        /// </summary>
        Red = 1 << 0,

        /// <summary>
        /// The green component will be written to.
        /// </summary>
        Green = 1 << 1,

        /// <summary>
        /// The blue component will be written to.
        /// </summary>
        Blue = 1 << 2,

        /// <summary>
        /// The alpha component will be written to.
        /// </summary>
        Alpha = 1 << 3,

        /// <summary>
        /// All color components will be written to.
        /// </summary>
        All = Red | Green | Blue | Alpha,
    }
}
