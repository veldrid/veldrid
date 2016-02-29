using System;

namespace Veldrid.Graphics
{
    public interface IndexBuffer
    {
        void SetIndices(int[] indices);
        void SetIndices(int[] indices, int stride, IntPtr offset);
        void Apply();
    }
}