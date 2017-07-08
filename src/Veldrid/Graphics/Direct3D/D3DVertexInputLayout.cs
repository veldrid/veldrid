using SharpDX.Direct3D11;
using System.Linq;

namespace Veldrid.Graphics.Direct3D
{
    public class D3DVertexInputLayout : VertexInputLayout
    {
        public VertexInputDescription[] InputDescriptions { get; }

        public D3DVertexInputLayout(VertexInputDescription[] vertexInputs)
        {
            InputDescriptions = vertexInputs;
        }

        public void Dispose()
        {
        }

        public static InputLayout CreateLayout(Device device, VertexInputDescription[] vertexInputs, byte[] shaderBytecode)
        {
            int count = vertexInputs.Sum(mvi => mvi.Elements.Length);
            int element = 0;
            InputElement[] elements = new InputElement[count];
            SemanticIndices indicesTracker = new SemanticIndices();
            for (int vbSlot = 0; vbSlot < vertexInputs.Length; vbSlot++)
            {
                VertexInputDescription bufferInput = vertexInputs[vbSlot];
                int numElements = bufferInput.Elements.Length;
                int currentOffset = 0;
                for (int i = 0; i < numElements; i++)
                {
                    var genericElement = bufferInput.Elements[i];
                    elements[element] = new InputElement(
                        GetSemanticName(genericElement.SemanticType),
                        indicesTracker.GetAndIncrement(genericElement.SemanticType),
                        ConvertGenericFormat(genericElement.ElementFormat),
                        currentOffset,
                        vbSlot,
                        D3DFormats.VeldridToD3DVertexElementInputClass(genericElement.StorageClassifier),
                        genericElement.InstanceStepRate);
                    currentOffset += genericElement.SizeInBytes;
                    element += 1;
                }
            }

            return new InputLayout(device, shaderBytecode, elements);
        }

        private static string GetSemanticName(VertexSemanticType semanticType)
        {
            switch (semanticType)
            {
                case VertexSemanticType.Position:
                    return "POSITION";
                case VertexSemanticType.TextureCoordinate:
                    return "TEXCOORD";
                case VertexSemanticType.Normal:
                    return "NORMAL";
                case VertexSemanticType.Color:
                    return "COLOR";
                default:
                    throw Illegal.Value<VertexSemanticType>();
            }
        }

        private static SharpDX.DXGI.Format ConvertGenericFormat(VertexElementFormat elementFormat)
        {
            switch (elementFormat)
            {
                case VertexElementFormat.Float1:
                    return SharpDX.DXGI.Format.R32_Float;
                case VertexElementFormat.Float2:
                    return SharpDX.DXGI.Format.R32G32_Float;
                case VertexElementFormat.Float3:
                    return SharpDX.DXGI.Format.R32G32B32_Float;
                case VertexElementFormat.Float4:
                    return SharpDX.DXGI.Format.R32G32B32A32_Float;
                case VertexElementFormat.Byte1:
                    return SharpDX.DXGI.Format.A8_UNorm;
                case VertexElementFormat.Byte4:
                    return SharpDX.DXGI.Format.R8G8B8A8_UNorm;
                default:
                    throw Illegal.Value<VertexElementFormat>();
            }
        }

        private class SemanticIndices
        {
            private int _position;
            private int _texCoord;
            private int _normal;
            private int _color;

            public int GetAndIncrement(VertexSemanticType type)
            {
                switch (type)
                {
                    case VertexSemanticType.Position:
                        return _position++;
                    case VertexSemanticType.TextureCoordinate:
                        return _texCoord++;
                    case VertexSemanticType.Normal:
                        return _normal++;
                    case VertexSemanticType.Color:
                        return _color++;
                    default:
                        throw Illegal.Value<VertexSemanticType>();
                }
            }
        }
    }
}
