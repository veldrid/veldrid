namespace Veldrid
{
    /// <summary>
    /// The winding order used to determine the front face of a primitive.
    /// </summary>
    public enum FrontFace : byte
    {
        /// <summary>
        /// Clockwise winding order.
        /// </summary>
        Clockwise,
        /// <summary>
        /// Counter-clockwise winding order.
        /// </summary>
        CounterClockwise,
    }
}