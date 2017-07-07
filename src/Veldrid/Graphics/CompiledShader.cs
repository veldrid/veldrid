namespace Veldrid.Graphics
{
    /// <summary>
    /// A device-specific object representing the result of processing raw
    /// shader code in preparation for shader creation. Can be used by a <see cref="ResourceFactory"/>
    /// to create <see cref="Shader"/> objects.
    /// </summary>
    /// <remarks>
    /// Direct3D and Vulkan shader objects are constructed from specialized bytecode,
    /// which is often compiled ahead-of-time from shader code. OpenGL shader objects are
    /// created directly from text containing shader code. This type represents the result
    /// of taking shader source code, in text form, and performing any processing necessary
    /// to obtain the input necessary for a shader object to be created from it. For OpenGL,
    /// this means no processing is performed on the text.
    /// </remarks>
    /// 
    public interface CompiledShaderCode
    {
    }
}
