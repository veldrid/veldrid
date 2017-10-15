using System;

namespace Vd2
{
    [Flags]
    public enum ShaderStages : byte
    {
        None = 0,
        Vertex = 1 << 0,
        Geometry = 1 << 1,
        TesselationControl = 1 << 2,
        TesselationEvaluation = 1 << 3,
        Fragment = 1 << 4,
    }
}
