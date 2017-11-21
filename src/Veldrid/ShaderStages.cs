using System;

namespace Veldrid
{
    /// <summary>
    /// A bitmask representing a set of shader stages.
    /// </summary>
    [Flags]
    public enum ShaderStages : byte
    {
        /// <summary>
        /// No stages.
        /// </summary>
        None = 0,
        /// <summary>
        /// The vertex shader stage.
        /// </summary>
        Vertex = 1 << 0,
        /// <summary>
        /// The geometry shader stage.
        /// </summary>
        Geometry = 1 << 1,
        /// <summary>
        /// The tessellation control (or hull) shader stage.
        /// </summary>
        TessellationControl = 1 << 2,
        /// <summary>
        /// The tessellation evaluation (or domain) shader stage.
        /// </summary>
        TessellationEvaluation = 1 << 3,
        /// <summary>
        /// The fragment (or pixel) shader stage.
        /// </summary>
        Fragment = 1 << 4,
        /// <summary>
        /// The compute shader stage.
        /// </summary>
        Compute = 1 << 5,
    }
}
