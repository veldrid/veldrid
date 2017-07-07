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

        public void SetIndices(ushort[] indices) => SetIndices(indices, 0, 0);
        public void SetIndices(ushort[] indices, int stride, int elementOffset)
        {
            SetData(indices, sizeof(ushort) * elementOffset);
            ElementsType = DrawElementsType.UnsignedShort;
        }

        public void SetIndices(uint[] indices) => SetIndices(indices, 0, 0);
        public void SetIndices(uint[] indices, int stride, int elementOffset)
        {
            SetData(indices, sizeof(uint) * elementOffset);
            ElementsType = DrawElementsType.UnsignedInt;
        }

        public void SetIndices(IntPtr indices, IndexFormat format, int count)
            => SetIndices(indices, format, count, 0);
        public void SetIndices(IntPtr indices, IndexFormat format, int count, int elementOffset)
        {
            int elementSizeInBytes = format == IndexFormat.UInt16 ? sizeof(ushort) : sizeof(uint);
            SetData(indices, count * elementSizeInBytes, elementOffset * elementSizeInBytes);
            ElementsType = OpenGLESFormats.MapIndexFormat(format);
        }
    }
}
