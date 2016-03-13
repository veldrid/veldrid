using System;

namespace Veldrid.Graphics
{
    public interface IndexBuffer: RenderStateModifier
    {
        void SetIndices(int[] indices);
        void SetIndices(int[] indices, int stride, int elementOffset);

        void SetIndices(IntPtr indices, IndexFormat format, int elementSizeInBytes, int count);
        void SetIndices(IntPtr indices, IndexFormat format, int elementSizeInBytes, int count, int elementOffset);
    }
}