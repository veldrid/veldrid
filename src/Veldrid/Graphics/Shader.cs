using System;

namespace Veldrid.Graphics
{
    /// <summary>
    /// A device resource representing a single shader program.
    /// </summary>
    public interface Shader : IDisposable
    {
        /// <summary>
        /// The shader stage which this Shader applies to.
        /// </summary>
        ShaderType Type { get; }
    }
}
