namespace Veldrid
{
    /// <summary>
    /// Identifies a unique face of a cubemap <see cref="Texture"/>.
    /// </summary>
    public enum CubeFace : byte
    {
        /// <summary>
        /// The negative-X face.
        /// </summary>
        NegativeX,
        /// <summary>
        /// The positive-X face.
        /// </summary>
        PositiveX,
        /// <summary>
        /// The negative-Y face.
        /// </summary>
        NegativeY,
        /// <summary>
        /// The positive-Y face.
        /// </summary>
        PositiveY,
        /// <summary>
        /// The negative-Z face.
        /// </summary>
        NegativeZ,
        /// <summary>
        /// The positive-Z face.
        /// </summary>
        PositiveZ,
    }
}
