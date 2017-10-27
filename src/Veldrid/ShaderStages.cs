using System;

namespace Veldrid
{
    [Flags]
    public enum ShaderStages : byte
    {
        None = 0,
        Vertex = 1 << 0,
        Geometry = 1 << 1,
        TessellationControl = 1 << 2,
        TessellationEvaluation = 1 << 3,
        Fragment = 1 << 4,
    }
}
