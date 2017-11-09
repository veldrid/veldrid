using SharpDX.Direct3D11;
using System;
using System.Collections.Generic;

namespace Veldrid.D3D11
{
    internal class D3D11ResourceCache
    {
        private readonly Device _device;

        private readonly Dictionary<BlendStateDescription, BlendState> _blendStates
            = new Dictionary<BlendStateDescription, BlendState>();

        private readonly Dictionary<DepthStencilStateDescription, DepthStencilState> _depthStencilStates
            = new Dictionary<DepthStencilStateDescription, DepthStencilState>();

        private readonly Dictionary<D3D11RasterizerStateCacheKey, RasterizerState> _rasterizerStates
            = new Dictionary<D3D11RasterizerStateCacheKey, RasterizerState>();

        private readonly Dictionary<InputLayoutCacheKey, InputLayout> _inputLayouts
            = new Dictionary<InputLayoutCacheKey, InputLayout>();

        public D3D11ResourceCache(Device device)
        {
            _device = device;
        }

        internal BlendState GetBlendState(ref BlendStateDescription description)
        {
            if (!_blendStates.TryGetValue(description, out BlendState blendState))
            {
                blendState = CreateNewBlendState(ref description);
                BlendStateDescription key = description;
                key.AttachmentStates = (BlendAttachmentDescription[])key.AttachmentStates.Clone();
                _blendStates.Add(key, blendState);
            }

            return blendState;
        }

        private BlendState CreateNewBlendState(ref BlendStateDescription description)
        {
            BlendAttachmentDescription[] attachmentStates = description.AttachmentStates;
            SharpDX.Direct3D11.BlendStateDescription d3dBlendStateDesc = new SharpDX.Direct3D11.BlendStateDescription();

            for (int i = 0; i < attachmentStates.Length; i++)
            {
                BlendAttachmentDescription state = attachmentStates[i];
                d3dBlendStateDesc.RenderTarget[i].IsBlendEnabled = state.BlendEnabled;
                d3dBlendStateDesc.RenderTarget[i].RenderTargetWriteMask = ColorWriteMaskFlags.All;
                d3dBlendStateDesc.RenderTarget[i].SourceBlend = D3D11Formats.VdToD3D11BlendOption(state.SourceColorFactor);
                d3dBlendStateDesc.RenderTarget[i].DestinationBlend = D3D11Formats.VdToD3D11BlendOption(state.DestinationColorFactor);
                d3dBlendStateDesc.RenderTarget[i].BlendOperation = D3D11Formats.VdToD3D11BlendOperation(state.ColorFunction);
                d3dBlendStateDesc.RenderTarget[i].SourceAlphaBlend = D3D11Formats.VdToD3D11BlendOption(state.SourceAlphaFactor);
                d3dBlendStateDesc.RenderTarget[i].DestinationAlphaBlend = D3D11Formats.VdToD3D11BlendOption(state.DestinationAlphaFactor);
                d3dBlendStateDesc.RenderTarget[i].AlphaBlendOperation = D3D11Formats.VdToD3D11BlendOperation(state.AlphaFunction);
            }

            return new BlendState(_device, d3dBlendStateDesc);
        }

        internal DepthStencilState GetDepthStencilState(ref DepthStencilStateDescription description)
        {
            if (!_depthStencilStates.TryGetValue(description, out DepthStencilState dss))
            {
                dss = CreateNewDepthStencilState(ref description);
                DepthStencilStateDescription key = description;
                _depthStencilStates.Add(key, dss);
            }

            return dss;
        }

        private DepthStencilState CreateNewDepthStencilState(ref DepthStencilStateDescription description)
        {
            SharpDX.Direct3D11.DepthStencilStateDescription dssDesc = new SharpDX.Direct3D11.DepthStencilStateDescription
            {
                DepthComparison = D3D11Formats.VdToD3D11DepthComparison(description.ComparisonKind),
                IsDepthEnabled = description.DepthTestEnabled,
                DepthWriteMask = description.DepthWriteEnabled ? DepthWriteMask.All : DepthWriteMask.Zero
            };

            return new DepthStencilState(_device, dssDesc);
        }

        internal RasterizerState GetRasterizerState(ref RasterizerStateDescription description, bool multisample)
        {
            D3D11RasterizerStateCacheKey key = new D3D11RasterizerStateCacheKey(description, multisample);
            if (!_rasterizerStates.TryGetValue(key, out RasterizerState rasterizerState))
            {
                rasterizerState = CreateNewRasterizerState(ref key);
                _rasterizerStates.Add(key, rasterizerState);
            }

            return rasterizerState;
        }

        private RasterizerState CreateNewRasterizerState(ref D3D11RasterizerStateCacheKey key)
        {
            SharpDX.Direct3D11.RasterizerStateDescription rssDesc = new SharpDX.Direct3D11.RasterizerStateDescription
            {
                CullMode = D3D11Formats.VdToD3D11CullMode(key.VeldridDescription.CullMode),
                FillMode = D3D11Formats.VdToD3D11FillMode(key.VeldridDescription.FillMode),
                IsDepthClipEnabled = key.VeldridDescription.DepthClipEnabled,
                IsScissorEnabled = key.VeldridDescription.ScissorTestEnabled,
                IsFrontCounterClockwise = key.VeldridDescription.FrontFace == FrontFace.CounterClockwise,
                IsMultisampleEnabled = key.Multisampled
            };

            return new RasterizerState(_device, rssDesc);
        }

        internal InputLayout GetInputLayout(VertexLayoutDescription[] vertexLayouts, byte[] vsBytecode)
        {
            InputLayoutCacheKey tempKey = InputLayoutCacheKey.CreateTempKey(vertexLayouts);
            if (!_inputLayouts.TryGetValue(tempKey, out InputLayout inputLayout))
            {
                inputLayout = CreateNewInputLayout(vertexLayouts, vsBytecode);
                InputLayoutCacheKey permanentKey = InputLayoutCacheKey.CreatePermanentKey(vertexLayouts);
                _inputLayouts.Add(permanentKey, inputLayout);
            }

            return inputLayout;
        }

        private InputLayout CreateNewInputLayout(VertexLayoutDescription[] vertexLayouts, byte[] vsBytecode)
        {
            int totalCount = 0;
            for (int i = 0; i < vertexLayouts.Length; i++)
            {
                totalCount += vertexLayouts[i].Elements.Length;
            }

            int element = 0; // Total element index across slots.
            InputElement[] elements = new InputElement[totalCount];
            SemanticIndices si = new SemanticIndices();
            for (int slot = 0; slot < vertexLayouts.Length; slot++)
            {
                VertexElementDescription[] elementDescs = vertexLayouts[slot].Elements;
                int currentOffset = 0;
                for (int i = 0; i < elementDescs.Length; i++)
                {
                    VertexElementDescription desc = elementDescs[i];
                    elements[element] = new InputElement(
                        GetSemanticString(desc.Semantic),
                        SemanticIndices.GetAndIncrement(ref si, desc.Semantic),
                        D3D11Formats.ToDxgiFormat(desc.Format),
                        currentOffset,
                        slot,
                        desc.InstanceStepRate == 0 ? InputClassification.PerVertexData : InputClassification.PerInstanceData,
                        (int)desc.InstanceStepRate);

                    currentOffset += (int)FormatHelpers.GetSizeInBytes(desc.Format);
                    element += 1;
                }
            }

            return new InputLayout(_device, vsBytecode, elements);
        }

        private string GetSemanticString(VertexElementSemantic semantic)
        {
            switch (semantic)
            {
                case VertexElementSemantic.Position:
                    return "POSITION";
                case VertexElementSemantic.Normal:
                    return "NORMAL";
                case VertexElementSemantic.TextureCoordinate:
                    return "TEXCOORD";
                case VertexElementSemantic.Color:
                    return "COLOR";
                default:
                    throw Illegal.Value<VertexElementSemantic>();
            }
        }

        private struct SemanticIndices
        {
            private int _position;
            private int _texCoord;
            private int _normal;
            private int _color;

            public static int GetAndIncrement(ref SemanticIndices si, VertexElementSemantic type)
            {
                switch (type)
                {
                    case VertexElementSemantic.Position:
                        return si._position++;
                    case VertexElementSemantic.TextureCoordinate:
                        return si._texCoord++;
                    case VertexElementSemantic.Normal:
                        return si._normal++;
                    case VertexElementSemantic.Color:
                        return si._color++;
                    default:
                        throw Illegal.Value<VertexElementSemantic>();
                }
            }
        }

        private struct InputLayoutCacheKey : IEquatable<InputLayoutCacheKey>
        {
            public VertexLayoutDescription[] VertexLayouts;

            public static InputLayoutCacheKey CreateTempKey(VertexLayoutDescription[] original)
                => new InputLayoutCacheKey { VertexLayouts = original };

            public static InputLayoutCacheKey CreatePermanentKey(VertexLayoutDescription[] original)
            {
                VertexLayoutDescription[] vertexLayouts = new VertexLayoutDescription[original.Length];
                for (int i = 0; i < original.Length; i++)
                {
                    vertexLayouts[i].Stride = original[i].Stride;
                    vertexLayouts[i].Elements = (VertexElementDescription[])original[i].Elements.Clone();
                }

                return new InputLayoutCacheKey { VertexLayouts = vertexLayouts };
            }

            public bool Equals(InputLayoutCacheKey other)
            {
                return Util.ArrayEqualsEquatable(VertexLayouts, other.VertexLayouts);
            }

            public override int GetHashCode()
            {
                return HashHelper.Array(VertexLayouts);
            }
        }

        private struct D3D11RasterizerStateCacheKey : IEquatable<D3D11RasterizerStateCacheKey>
        {
            public RasterizerStateDescription VeldridDescription;
            public bool Multisampled;

            public D3D11RasterizerStateCacheKey(RasterizerStateDescription veldridDescription, bool multisampled)
            {
                VeldridDescription = veldridDescription;
                Multisampled = multisampled;
            }

            public bool Equals(D3D11RasterizerStateCacheKey other)
            {
                return VeldridDescription.Equals(other.VeldridDescription)
                    && Multisampled.Equals(other.Multisampled);
            }

            public override int GetHashCode()
            {
                return HashHelper.Combine(VeldridDescription.GetHashCode(), Multisampled.GetHashCode());
            }
        }
    }
}
