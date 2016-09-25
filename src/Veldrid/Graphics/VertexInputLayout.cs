using System;

namespace Veldrid.Graphics
{
    public interface VertexInputLayout : IDisposable
    {
        MaterialVertexInput[] InputDescription { get; }
    }
}
