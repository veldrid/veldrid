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
        /// </summary>
        VertexInputDescription[] InputDescriptions { get; }
    }
}
