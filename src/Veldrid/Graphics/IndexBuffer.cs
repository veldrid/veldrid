using System;

namespace Veldrid.Graphics
{
    public interface IndexBuffer: RenderStateModifier
    {
        void SetIndices(int[] indices);
        void SetIndices(int[] indices, int stride, IntPtr offset);
    }
}