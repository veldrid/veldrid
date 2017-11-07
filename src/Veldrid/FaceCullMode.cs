namespace Veldrid
{
    /// <summary>
    /// Indicates which face will be culled.
    /// </summary>
    public enum FaceCullMode : byte
    {
        /// <summary>
        /// The back face.
        /// </summary>
        Back,
        /// <summary>
        /// The front face.
        /// </summary>
        Front,
        /// <summary>
        /// No face culling.
        /// </summary>
        None,
    }
}
