using OpenTK.Graphics.OpenGL;
using System.Linq;
using System;

namespace Veldrid.Graphics.OpenGL
{
    public class OpenGLVertexInputLayout : VertexInputLayout
    {
        public MaterialVertexInput[] InputDescription { get; }
        public OpenGLMaterialVertexInput[] VBLayoutsBySlot { get; }

        public OpenGLVertexInputLayout(MaterialVertexInput[] vertexInputs)
        {
            InputDescription = vertexInputs;
            VBLayoutsBySlot = vertexInputs.Select(mvi => new OpenGLMaterialVertexInput(mvi)).ToArray();
        }

        public int SetVertexAttributes(int vertexBufferSlot, OpenGLVertexBuffer vb, int previousAttributesBound)
        {
            // TODO: Related to OpenGLRenderContext.PlatformSetVertexBuffer()
            // These attributes should be lazily set on a draw call or something.
            if (vertexBufferSlot <= VBLayoutsBySlot.Length)
            {
                return previousAttributesBound;
            }

            int baseSlot = GetSlotBaseIndex(vertexBufferSlot);
            OpenGLMaterialVertexInput input = VBLayoutsBySlot[vertexBufferSlot];
            vb.Apply();
            for (int i = 0; i < input.Elements.Length; i++)
            {
                OpenGLMaterialVertexInputElement element = input.Elements[i];
                int slot = baseSlot + i;
                GL.EnableVertexAttribArray(slot);
                GL.VertexAttribPointer(slot, element.ElementCount, element.Type, element.Normalized, input.VertexSizeInBytes, element.Offset);
            }
            for (int extraSlot = input.Elements.Length; extraSlot < previousAttributesBound; extraSlot++)
            {
                GL.DisableVertexAttribArray(extraSlot);
            }

            return input.Elements.Length;
        }

        public int SetVertexAttributes(VertexBuffer[] vertexBuffers, int previousAttributesBound)
        {
            int totalSlotsBound = 0;
            for (int i = 0; i < VBLayoutsBySlot.Length; i++)
            {
                OpenGLMaterialVertexInput input = VBLayoutsBySlot[i];
                ((OpenGLVertexBuffer)vertexBuffers[i]).Apply();
                for (int slot = 0; slot < input.Elements.Length; slot++)
                {
                    OpenGLMaterialVertexInputElement element = input.Elements[slot];
                    int actualSlot = totalSlotsBound + slot;
                    GL.EnableVertexAttribArray(actualSlot);
                    GL.VertexAttribPointer(actualSlot, element.ElementCount, element.Type, element.Normalized, input.VertexSizeInBytes, element.Offset);
                    GL.VertexAttribDivisor(actualSlot, element.InstanceStepRate);
                }

                totalSlotsBound += input.Elements.Length;
            }

            for (int extraSlot = totalSlotsBound; extraSlot < previousAttributesBound; extraSlot++)
            {
                GL.DisableVertexAttribArray(extraSlot);
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

    public class OpenGLMaterialVertexInput
    {
        public int VertexSizeInBytes { get; }
        public OpenGLMaterialVertexInputElement[] Elements { get; }

        public OpenGLMaterialVertexInput(int vertexSizeInBytes, OpenGLMaterialVertexInputElement[] elements)
        {
            VertexSizeInBytes = vertexSizeInBytes;
            Elements = elements;
        }

        public OpenGLMaterialVertexInput(MaterialVertexInput genericInput)
        {
            VertexSizeInBytes = genericInput.VertexSizeInBytes;
            Elements = new OpenGLMaterialVertexInputElement[genericInput.Elements.Length];
            int offset = 0;
            for (int i = 0; i < Elements.Length; i++)
            {
                var genericElement = genericInput.Elements[i];
                Elements[i] = new OpenGLMaterialVertexInputElement(genericElement, offset);
                offset += genericElement.SizeInBytes;
            }
        }
    }

    public struct OpenGLMaterialVertexInputElement
    {
        public byte SizeInBytes { get; }
        public byte ElementCount { get; }
        public VertexAttribPointerType Type { get; }
        public int Offset { get; }
        public bool Normalized { get; }
        public int InstanceStepRate { get; set; }

        public OpenGLMaterialVertexInputElement(byte sizeInBytes, byte elementCount, VertexAttribPointerType type, int offset, bool normalized)
        {
            SizeInBytes = sizeInBytes;
            ElementCount = elementCount;
            Type = type;
            Offset = offset;
            Normalized = normalized;
            InstanceStepRate = 0;
        }

        public OpenGLMaterialVertexInputElement(
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

        public OpenGLMaterialVertexInputElement(MaterialVertexInputElement genericElement, int offset)
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
