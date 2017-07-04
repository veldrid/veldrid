using System;

namespace Veldrid.Graphics
{
    /// <summary>
    /// A device-specific object controlling the layout of vertices used by the input assembler.
    /// </summary>
    public interface VertexInputLayout : IDisposable
    {
        /// <summary>
        /// A device-agnostic description of the vertex layout.
        /// Each element in the array describes the contents of a single VertexBuffer.
        /// </summary>
        VertexInputDescription[] InputDescriptions { get; }
    }
}
