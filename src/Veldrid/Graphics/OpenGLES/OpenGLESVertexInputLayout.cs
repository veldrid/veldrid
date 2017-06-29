using OpenTK.Graphics.ES30;
using System.Linq;

namespace Veldrid.Graphics.OpenGLES
{
    public class OpenGLESVertexInputLayout : VertexInputLayout
    {
        public VertexInputDescription[] InputDescriptions { get; }
        public OpenGLESMaterialVertexInput[] VBLayoutsBySlot { get; }

        public OpenGLESVertexInputLayout(VertexInputDescription[] vertexInputs)
        {
            InputDescriptions = vertexInputs;
            VBLayoutsBySlot = vertexInputs.Select(mvi => new OpenGLESMaterialVertexInput(mvi)).ToArray();
        }

        public int SetVertexAttributes(VertexBuffer[] vertexBuffers, int previousAttributesBound, int baseVertexOffset)
        {
            int totalSlotsBound = 0;
            for (int i = 0; i < VBLayoutsBySlot.Length; i++)
            {
                OpenGLESMaterialVertexInput input = VBLayoutsBySlot[i];
                ((OpenGLESVertexBuffer)vertexBuffers[i]).Apply();
                for (int slot = 0; slot < input.Elements.Length; slot++)
                {
                    OpenGLESMaterialVertexInputElement element = input.Elements[slot];
                    int actualSlot = totalSlotsBound + slot;
                    GL.EnableVertexAttribArray(actualSlot);
                    Utilities.CheckLastGLES3Error();
                    int baseVertexOffsetBytes = baseVertexOffset * input.VertexSizeInBytes;
                    GL.VertexAttribPointer(actualSlot, element.ElementCount, element.Type, element.Normalized, input.VertexSizeInBytes, element.Offset + baseVertexOffsetBytes);
                    Utilities.CheckLastGLES3Error();
                    GL.VertexAttribDivisor(actualSlot, element.InstanceStepRate);
                    Utilities.CheckLastGLES3Error();
                }

                totalSlotsBound += input.Elements.Length;
            }

            for (int extraSlot = totalSlotsBound; extraSlot < previousAttributesBound; extraSlot++)
            {
                GL.DisableVertexAttribArray(extraSlot);
                Utilities.CheckLastGLES3Error();
            }

            return totalSlotsBound;
        }

        private int GetSlotBaseIndex(int vertexBufferSlot)
        {
            int index = 0;
            for (int i = 0; i < vertexBufferSlot; i++)
            {
                index += VBLayoutsBySlot[i].Elements.Length;
            }

            return index;
        }

        public void Dispose()
        {
        }
    }

    public class OpenGLESMaterialVertexInput
    {
        public int VertexSizeInBytes { get; }
        public OpenGLESMaterialVertexInputElement[] Elements { get; }

        public OpenGLESMaterialVertexInput(int vertexSizeInBytes, OpenGLESMaterialVertexInputElement[] elements)
        {
            VertexSizeInBytes = vertexSizeInBytes;
            Elements = elements;
        }

        public OpenGLESMaterialVertexInput(VertexInputDescription genericInput)
        {
            VertexSizeInBytes = genericInput.VertexSizeInBytes;
            Elements = new OpenGLESMaterialVertexInputElement[genericInput.Elements.Length];
            int offset = 0;
            for (int i = 0; i < Elements.Length; i++)
            {
                var genericElement = genericInput.Elements[i];
                Elements[i] = new OpenGLESMaterialVertexInputElement(genericElement, offset);
                offset += genericElement.SizeInBytes;
            }
        }
    }

    public struct OpenGLESMaterialVertexInputElement
    {
        public byte SizeInBytes { get; }
        public byte ElementCount { get; }
        public VertexAttribPointerType Type { get; }
        public int Offset { get; }
        public bool Normalized { get; }
        public int InstanceStepRate { get; set; }

        public OpenGLESMaterialVertexInputElement(byte sizeInBytes, byte elementCount, VertexAttribPointerType type, int offset, bool normalized)
        {
            SizeInBytes = sizeInBytes;
            ElementCount = elementCount;
            Type = type;
            Offset = offset;
            Normalized = normalized;
            InstanceStepRate = 0;
        }

        public OpenGLESMaterialVertexInputElement(
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

        public OpenGLESMaterialVertexInputElement(VertexInputElement genericElement, int offset)
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
    }
}
