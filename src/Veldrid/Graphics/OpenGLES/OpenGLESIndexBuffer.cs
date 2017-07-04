using System;
using OpenTK.Graphics.ES30;
using System.Runtime.CompilerServices;

namespace Veldrid.Graphics.OpenGLES
{
    public class OpenGLESIndexBuffer : OpenGLESBuffer, IndexBuffer, IDisposable
    {
        public DrawElementsType ElementsType { get; private set; }

        public OpenGLESIndexBuffer(bool isDynamic, DrawElementsType elementsType)
            : base(BufferTarget.ElementArrayBuffer)
        {
            ElementsType = elementsType;
        }

        public void Apply()
        {
            Bind();
        }

        public void SetIndices<T>(T[] indices, IndexFormat format) where T : struct
        {
            SetIndices(indices, format, 0, 0);
        }

        public void SetIndices<T>(T[] indices, IndexFormat format, int stride, int elementOffset) where T : struct
        {
            int elementSizeInBytes = Unsafe.SizeOf<T>();
            SetData(indices, elementOffset * elementSizeInBytes);
            ElementsType = OpenGLESFormats.MapIndexFormat(format);
        }

        public void SetIndices(int[] indices) => SetIndices(indices, 0, 0);
        public void SetIndices(int[] indices, int stride, int elementOffset)
        {
            SetData(indices, sizeof(int) * elementOffset);
            ElementsType = DrawElementsType.UnsignedInt;
        }

        public void SetIndices(IntPtr indices, IndexFormat format, int elementSizeInBytes, int count)
            => SetIndices(indices, format, elementSizeInBytes, count, 0);
        public void SetIndices(IntPtr indices, IndexFormat format, int elementSizeInBytes, int count, int elementOffset)
        {
            SetData(indices, count * elementSizeInBytes, elementOffset * elementSizeInBytes);
            ElementsType = OpenGLESFormats.MapIndexFormat(format);
        }
    }
}
