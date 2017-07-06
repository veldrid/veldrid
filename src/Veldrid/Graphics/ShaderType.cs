namespace Veldrid.Graphics
{
    /// <summary>
    /// The specific stage to which a shader applies.
    /// </summary>
    public enum ShaderType : byte
    {
        /// <summary>
        /// The first shader stage, responsible for transforming vertices from the input assembler into input for
        /// further shader stages.
        /// </summary>
        Vertex,
        /// <summary>
        /// An optional shader stage, which performs additional mutation, manipulation, and generation of primitive data.
        /// </summary>
        Geometry,
        /// <summary>
        /// The final shader stage, responsible for outputting final image data to the framebuffer.
        /// </summary>
        Fragment
    }
}
