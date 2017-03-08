using OpenTK.Graphics.ES30;
using System.Linq;

namespace Veldrid.Graphics.OpenGLES
{
    public class OpenGLESVertexInputLayout : VertexInputLayout
    {
        public MaterialVertexInput[] InputDescription { get; }
        public OpenGLESMaterialVertexInput[] VBLayoutsBySlot { get; }

        public OpenGLESVertexInputLayout(MaterialVertexInput[] vertexInputs)
        {
            InputDescription = vertexInputs;
            VBLayoutsBySlot = vertexInputs.Select(mvi => new OpenGLESMaterialVertexInput(mvi)).ToArray();
        }

        public int SetVertexAttributes(int vertexBufferSlot, OpenGLESVertexBuffer vb, int previousAttributesBound)
        {
            // TODO: Related to OpenGLESRenderContext.PlatformSetVertexBuffer()
            // These attributes should be lazily set on a draw call or something.
            if (vertexBufferSlot <= VBLayoutsBySlot.Length)
            {
                return previousAttributesBound;
            }

            int baseSlot = GetSlotBaseIndex(vertexBufferSlot);
            OpenGLESMaterialVertexInput input = VBLayoutsBySlot[vertexBufferSlot];
            vb.Apply();
            for (int i = 0; i < input.Elements.Length; i++)
            {
                OpenGLESMaterialVertexInputElement element = input.Elements[i];
                int slot = baseSlot + i;
                GL.EnableVertexAttribArray(slot);
                Utilities.CheckLastGLES3Error();
                GL.VertexAttribPointer(slot, element.ElementCount, element.Type, element.Normalized, input.VertexSizeInBytes, element.Offset);
                Utilities.CheckLastGLES3Error();
            }
            for (int extraSlot = input.Elements.Length; extraSlot < previousAttributesBound; extraSlot++)
            {
                GL.DisableVertexAttribArray(extraSlot);
                Utilities.CheckLastGLES3Error();
            }

            return input.Elements.Length;
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

        public OpenGLESMaterialVertexInput(MaterialVertexInput genericInput)
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

        public OpenGLESMaterialVertexInputElement(MaterialVertexInputElement genericElement, int offset)
        {
            SizeInBytes = genericElement.SizeInBytes;
            ElementCount = VertexFormatHelpers.GetElementCount(genericElement.ElementFormat);
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
