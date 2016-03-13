using System;
using OpenTK.Graphics.OpenGL;

namespace Veldrid.Graphics.OpenGL
{
    public class OpenGLIndexBuffer : OpenGLBuffer, IndexBuffer, IDisposable
    {
        public DrawElementsType ElementsType { get; private set; }

        public OpenGLIndexBuffer() : base(BufferTarget.ElementArrayBuffer)
        {
        }

        public void Apply()
        {
            Bind();
        }

        public void SetIndices(int[] indices) => SetIndices(indices, 0, 0);
        public void SetIndices(int[] indices, int stride, int elementOffset)
        {
            SetData(indices, sizeof(int) * indices.Length, sizeof(int) * elementOffset);
            ElementsType = DrawElementsType.UnsignedInt;
        }

        public void SetIndices(IntPtr indices, IndexFormat format, int elementSizeInBytes, int count)
            => SetIndices(indices, format, elementSizeInBytes, count, 0);
        public void SetIndices(IntPtr indices, IndexFormat format, int elementSizeInBytes, int count, int elementOffset)
        {
            SetData(indices, elementSizeInBytes * count, elementOffset * elementSizeInBytes);
            ElementsType = OpenGLFormats.MapIndexFormat(format);
        }
    }
}
