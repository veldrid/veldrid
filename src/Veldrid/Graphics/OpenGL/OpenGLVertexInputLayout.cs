using OpenTK.Graphics.OpenGL;
using System.Linq;
using System;

namespace Veldrid.Graphics.OpenGL
{
    public class OpenGLVertexInputLayout : VertexInputLayout
    {
        public VertexInputDescription[] InputDescriptions { get; }
        public OpenGLVertexInput[] VBLayoutsBySlot { get; }

        public OpenGLVertexInputLayout(VertexInputDescription[] vertexInputs)
        {
            InputDescriptions = vertexInputs;
            VBLayoutsBySlot = vertexInputs.Select(mvi => new OpenGLVertexInput(mvi)).ToArray();
        }

        public void Dispose()
        {
        }
    }

    public class OpenGLVertexInput
    {
        public int VertexSizeInBytes { get; }
        public OpenGLVertexInputElement[] Elements { get; }

        public OpenGLVertexInput(int vertexSizeInBytes, OpenGLVertexInputElement[] elements)
        {
            VertexSizeInBytes = vertexSizeInBytes;
            Elements = elements;
        }

        public OpenGLVertexInput(VertexInputDescription genericInput)
        {
            VertexSizeInBytes = genericInput.VertexSizeInBytes;
            Elements = new OpenGLVertexInputElement[genericInput.Elements.Length];
            int offset = 0;
            for (int i = 0; i < Elements.Length; i++)
            {
                var genericElement = genericInput.Elements[i];
                Elements[i] = new OpenGLVertexInputElement(genericElement, offset);
                offset += genericElement.SizeInBytes;
            }
        }
    }

    public struct OpenGLVertexInputElement : IEquatable<OpenGLVertexInputElement>
    {
        public byte SizeInBytes { get; }
        public byte ElementCount { get; }
        public VertexAttribPointerType Type { get; }
        public int Offset { get; }
        public bool Normalized { get; }
        public int InstanceStepRate { get; set; }

        public OpenGLVertexInputElement(byte sizeInBytes, byte elementCount, VertexAttribPointerType type, int offset, bool normalized)
        {
            SizeInBytes = sizeInBytes;
            ElementCount = elementCount;
            Type = type;
            Offset = offset;
            Normalized = normalized;
            InstanceStepRate = 0;
        }

        public OpenGLVertexInputElement(
            byte sizeInBytes,
            byte elementCount,
            VertexAttribPointerType type,
            int offset,
            bool normalized,
            int instanceStepRate)
        {
            SizeInBytes = sizeInBytes;
            ElementCount = elementCount;
            Type = type;
            Offset = offset;
            Normalized = normalized;
            InstanceStepRate = instanceStepRate;
        }

        public OpenGLVertexInputElement(VertexInputElement genericElement, int offset)
        {
            SizeInBytes = genericElement.SizeInBytes;
            ElementCount = FormatHelpers.GetElementCount(genericElement.ElementFormat);
            Type = GetGenericFormatType(genericElement.ElementFormat);
            Offset = offset;
            Normalized = genericElement.SemanticType == VertexSemanticType.Color && genericElement.ElementFormat == VertexElementFormat.Byte4;
            InstanceStepRate = genericElement.InstanceStepRate;
        }

        private static VertexAttribPointerType GetGenericFormatType(VertexElementFormat format)
        {
            switch (format)
            {
                case VertexElementFormat.Float1:
                case VertexElementFormat.Float2:
                case VertexElementFormat.Float3:
                case VertexElementFormat.Float4:
                    return VertexAttribPointerType.Float;
                case VertexElementFormat.Byte1:
                case VertexElementFormat.Byte4:
                    return VertexAttribPointerType.UnsignedByte;
                default:
                    throw Illegal.Value<VertexElementFormat>();
            }
        }

        public bool Equals(OpenGLVertexInputElement other)
        {
            return SizeInBytes.Equals(other.SizeInBytes) && ElementCount.Equals(other.ElementCount)
                && Type == other.Type && Offset.Equals(other.Offset) && Normalized.Equals(other.Normalized)
                && InstanceStepRate.Equals(other.InstanceStepRate);
        }
    }
}
