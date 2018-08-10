using SharpDX.Direct3D11;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;

namespace Veldrid.D3D11
{
    internal class D3D11ResourceCache : IDisposable
    {
        private readonly Device _device;
        private readonly object _lock = new object();

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

        public void GetPipelineResources(
            ref BlendStateDescription blendDesc,
            ref DepthStencilStateDescription dssDesc,
            ref RasterizerStateDescription rasterDesc,
            bool multisample,
            VertexLayoutDescription[] vertexLayouts,
            byte[] vsBytecode,
            out BlendState blendState,
            out DepthStencilState depthState,
            out RasterizerState rasterState,
            out InputLayout inputLayout)
        {
            lock (_lock)
            {
                blendState = GetBlendState(ref blendDesc);
                depthState = GetDepthStencilState(ref dssDesc);
                rasterState = GetRasterizerState(ref rasterDesc, multisample);
                inputLayout = GetInputLayout(vertexLayouts, vsBytecode);
            }
        }

        private BlendState GetBlendState(ref BlendStateDescription description)
        {
            Debug.Assert(Monitor.IsEntered(_lock));
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

        private DepthStencilState GetDepthStencilState(ref DepthStencilStateDescription description)
        {
            Debug.Assert(Monitor.IsEntered(_lock));
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
                DepthComparison = D3D11Formats.VdToD3D11Comparison(description.DepthComparison),
                IsDepthEnabled = description.DepthTestEnabled,
                DepthWriteMask = description.DepthWriteEnabled ? DepthWriteMask.All : DepthWriteMask.Zero,
                IsStencilEnabled = description.StencilTestEnabled,
                FrontFace = ToD3D11StencilOpDesc(description.StencilFront),
                BackFace = ToD3D11StencilOpDesc(description.StencilBack),
                StencilReadMask = description.StencilReadMask,
                StencilWriteMask = description.StencilWriteMask
            };

            return new DepthStencilState(_device, dssDesc);
        }

        private DepthStencilOperationDescription ToD3D11StencilOpDesc(StencilBehaviorDescription sbd)
        {
            return new DepthStencilOperationDescription
            {
                Comparison = D3D11Formats.VdToD3D11Comparison(sbd.Comparison),
                PassOperation = D3D11Formats.VdToD3D11StencilOperation(sbd.Pass),
                FailOperation = D3D11Formats.VdToD3D11StencilOperation(sbd.Fail),
                DepthFailOperation = D3D11Formats.VdToD3D11StencilOperation(sbd.DepthFail)
            };
        }

        private RasterizerState GetRasterizerState(ref RasterizerStateDescription description, bool multisample)
        {
            Debug.Assert(Monitor.IsEntered(_lock));
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

        private InputLayout GetInputLayout(VertexLayoutDescription[] vertexLayouts, byte[] vsBytecode)
        {
            Debug.Assert(Monitor.IsEntered(_lock));

            if (vsBytecode == null || vertexLayouts == null || vertexLayouts.Length == 0) { return null; }

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
                uint stepRate = vertexLayouts[slot].InstanceStepRate;
                int currentOffset = 0;
                for (int i = 0; i < elementDescs.Length; i++)
                {
                    VertexElementDescription desc = elementDescs[i];
                    elements[element] = new InputElement(
                        GetSemanticString(desc.Semantic),
                        SemanticIndices.GetAndIncrement(ref si, desc.Semantic),
                        D3D11Formats.ToDxgiFormat(desc.Format),
                        desc.Offset != 0 ? (int)desc.Offset : currentOffset,
                        slot,
                        stepRate == 0 ? InputClassification.PerVertexData : InputClassification.PerInstanceData,
                        (int)stepRate);

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

        public void Dispose()
        {
            foreach (KeyValuePair<BlendStateDescription, BlendState> kvp in _blendStates)
            {
                kvp.Value.Dispose();
            }
            foreach (KeyValuePair<DepthStencilStateDescription, DepthStencilState> kvp in _depthStencilStates)
            {
                kvp.Value.Dispose();
            }
            foreach (KeyValuePair<D3D11RasterizerStateCacheKey, RasterizerState> kvp in _rasterizerStates)
            {
                kvp.Value.Dispose();
            }
            foreach (KeyValuePair<InputLayoutCacheKey, InputLayout> kvp in _inputLayouts)
            {
                kvp.Value.Dispose();
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
                    vertexLayouts[i].InstanceStepRate = original[i].InstanceStepRate;
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
