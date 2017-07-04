namespace Veldrid.Graphics
{
    /// <summary>
    /// Describes how vertices are assembled into primitives in the GPU input assembler.
    /// Consult the Direct3D or OpenGL documentation for how these values behave.
    /// </summary>
    public enum PrimitiveTopology : byte
    {
        TriangleList,
        TriangleStrip,
        LineList,
        LineStrip,
        PointList
    }
}
