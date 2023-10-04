using System;

namespace Veldrid
{
    [Flags]
    public enum MouseButton
    {
        /// <summary>
        /// The left mouse button.
        /// </summary>
        Left = 1 << 0,

        /// <summary>
        /// The middle mouse button.
        /// </summary>
        Middle = 1 << 1,

        /// <summary>
        /// The right mouse button.
        /// </summary>
        Right = 1 << 2,

        /// <summary>
        /// The first extra mouse button.
        /// </summary>
        Button1 = 1 << 3,

        /// <summary>
        /// The second extra mouse button.
        /// </summary>
        Button2 = 1 << 4,

        /// <summary>
        /// The third extra mouse button.
        /// </summary>
        Button3 = 1 << 5,

        /// <summary>
        /// The fourth extra mouse button.
        /// </summary>
        Button4 = 1 << 6,

        /// <summary>
        /// The fifth extra mouse button.
        /// </summary>
        Button5 = 1 << 7,

        /// <summary>
        /// The sixth extra mouse button.
        /// </summary>
        Button6 = 1 << 8,

        /// <summary>
        /// The seventh extra mouse button.
        /// </summary>
        Button7 = 1 << 9,

        /// <summary>
        /// The eigth extra mouse button.
        /// </summary>
        Button8 = 1 << 10,

        /// <summary>
        /// The ninth extra mouse button.
        /// </summary>
        Button9 = 1 << 11
    }
}
