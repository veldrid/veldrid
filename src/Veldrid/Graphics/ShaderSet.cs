using System;

namespace Veldrid.Graphics
{
    /// <summary>
    /// A device resource encapsulating a set of shader objects and their vertex input layout.
    /// </summary>
    public interface ShaderSet : IDisposable
    {
        /// <summary>
        /// The input to the vertex shader.
        /// </summary>
        VertexInputLayout InputLayout { get; }
        /// <summary>
        /// The vertex shader.
        /// </summary>
        Shader VertexShader { get; }
        /// <summary>
        /// The geometry shader. This stage is optional, and may be null.
        /// </summary>
        Shader GeometryShader { get; }
        /// <summary>
        /// The fragment shader.
        /// </summary>
        Shader FragmentShader { get; }
    }
}
