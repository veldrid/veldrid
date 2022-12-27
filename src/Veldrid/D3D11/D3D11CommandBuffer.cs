﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Vortice.Direct3D11;
using Vortice.Mathematics;

using RawViewportF = Vortice.Mathematics.Viewport;
using RawRectangle = Vortice.RawRect;
using RawColor4 = Vortice.Mathematics.Color4;
using DeviceContext = Vortice.Direct3D11.ID3D11DeviceContext;
using DeviceContext1 = Vortice.Direct3D11.ID3D11DeviceContext1;


namespace Veldrid.D3D11
{
    internal class D3D11CommandBuffer : CommandBuffer
    {
        private readonly D3D11GraphicsDevice _gd;
        private RecordingState _state = RecordingState.Recording;
        private DeviceContext _ctx;
        private DeviceContext1 _ctx1;
        private ID3DUserDefinedAnnotation _uda;
        private RawViewportF[] _viewports = new RawViewportF[0];
        private RawRectangle[] _scissors = new RawRectangle[0];
        private bool _viewportsChanged;
        private bool _scissorRectsChanged;

        private uint _numVertexBindings = 0;
        private ID3D11Buffer[] _vertexBindings = new ID3D11Buffer[1];
        private int[] _vertexStrides;
        private int[] _vertexOffsets = new int[1];

        // Cached pipeline State
        private DeviceBuffer _ib;
        private uint _ibOffset;
        private ID3D11BlendState _blendState;
        private ID3D11DepthStencilState _depthStencilState;
        private uint _stencilReference;
        private ID3D11RasterizerState _rasterizerState;
        private Vortice.Direct3D.PrimitiveTopology _primitiveTopology;
        private ID3D11InputLayout _inputLayout;
        private ID3D11VertexShader _vertexShader;
        private ID3D11GeometryShader _geometryShader;
        private ID3D11HullShader _hullShader;
        private ID3D11DomainShader _domainShader;
        private ID3D11PixelShader _pixelShader;

        private D3D11Pipeline _graphicsPipeline;
        private BoundResourceSetInfo[] _graphicsResourceSets = new BoundResourceSetInfo[1];
        // Resource sets are invalidated when a new resource set is bound with an incompatible SRV or UAV.
        private bool[] _invalidatedGraphicsResourceSets = new bool[1];

        private D3D11Pipeline _computePipeline;
        private BoundResourceSetInfo[] _computeResourceSets = new BoundResourceSetInfo[1];
        // Resource sets are invalidated when a new resource set is bound with an incompatible SRV or UAV.
        private bool[] _invalidatedComputeResourceSets = new bool[1];
        private string _name;
        private bool _vertexBindingsChanged;
        private ID3D11Buffer[] _cbOut = new ID3D11Buffer[1];
        private int[] _firstConstRef = new int[1];
        private int[] _numConstsRef = new int[1];
        private Framebuffer _currentFB;
        private bool _disposed;
        private ID3D11CommandList _commandList;
        private RenderPassDescription _rpd;

        // Cached resources
        private const int MaxCachedUniformBuffers = 15;
        private readonly D3D11BufferRange[] _vertexBoundUniformBuffers = new D3D11BufferRange[MaxCachedUniformBuffers];
        private readonly D3D11BufferRange[] _fragmentBoundUniformBuffers = new D3D11BufferRange[MaxCachedUniformBuffers];
        private const int MaxCachedTextureViews = 16;
        private readonly D3D11TextureView[] _vertexBoundTextureViews = new D3D11TextureView[MaxCachedTextureViews];
        private readonly D3D11TextureView[] _fragmentBoundTextureViews = new D3D11TextureView[MaxCachedTextureViews];
        private const int MaxCachedSamplers = 4;
        private readonly D3D11Sampler[] _vertexBoundSamplers = new D3D11Sampler[MaxCachedSamplers];
        private readonly D3D11Sampler[] _fragmentBoundSamplers = new D3D11Sampler[MaxCachedSamplers];

        private readonly Dictionary<Texture, List<D3D11BoundTextureInfo>> _boundSRVs = new Dictionary<Texture, List<D3D11BoundTextureInfo>>();
        private readonly Dictionary<Texture, List<D3D11BoundTextureInfo>> _boundUAVs = new Dictionary<Texture, List<D3D11BoundTextureInfo>>();
        private readonly List<List<D3D11BoundTextureInfo>> _boundTextureInfoPool = new List<List<D3D11BoundTextureInfo>>(20);

        private const int MaxUAVs = 8;
        private readonly List<(DeviceBuffer, int)> _boundComputeUAVBuffers = new List<(DeviceBuffer, int)>(MaxUAVs);
        private readonly List<(DeviceBuffer, int)> _boundOMUAVBuffers = new List<(DeviceBuffer, int)>(MaxUAVs);

        private readonly List<D3D11Buffer> _availableStagingBuffers = new List<D3D11Buffer>();
        private readonly List<D3D11Buffer> _submittedStagingBuffers = new List<D3D11Buffer>();

        private readonly List<D3D11Swapchain> _referencedSwapchains = new List<D3D11Swapchain>();

        private enum RecordingState
        {
            Recording,
            ReadyToExecute,
            Disposed,
        }

        private void BeginRecording()
        {
            if (_state == RecordingState.Disposed)
            {
                throw new VeldridException("Cannot record commands into a disposed CommandBuffer.");
            }
            else if (_state == RecordingState.ReadyToExecute)
            {
                if (_commandList != null)
                {
                    _commandList.Dispose();
                    _commandList = null;
                }

                Reset();
                ClearReferencedSwapchains();
            }
        }

        internal void OnCompleted()
        {
            foreach (D3D11Buffer buffer in _submittedStagingBuffers)
            {
                _availableStagingBuffers.Add(buffer);
            }

            _submittedStagingBuffers.Clear();
        }

        private void CreateNewDeferredContext()
        {
            _ctx = _gd.Device.CreateDeferredContext();
            _ctx1 = _ctx.QueryInterfaceOrNull<DeviceContext1>();
            _uda = _ctx.QueryInterfaceOrNull<ID3DUserDefinedAnnotation>();
            _state = RecordingState.Recording;
        }

        public D3D11CommandBuffer(D3D11GraphicsDevice gd, ref CommandBufferDescription description)
                : base(gd.Features)
        {
            _gd = gd;
            CreateNewDeferredContext();
        }

        internal override void BeginRenderPassCore(in RenderPassDescription rpd)
        {
            BeginRecording();

            _rpd = rpd;
            D3D11Framebuffer d3d11FB = (D3D11Framebuffer)rpd.Framebuffer;
            _ctx.OMSetRenderTargets(d3d11FB.RenderTargetViews, d3d11FB.DepthStencilView);

            if (d3d11FB.Swapchain != null)
            {
                _referencedSwapchains.Add(d3d11FB.Swapchain);
                d3d11FB.Swapchain.AddCommandBufferReference(this);
            }

            for (uint attach = 0; attach < rpd.Framebuffer.ColorTargets.Count; attach++)
            {
                rpd.GetColorAttachment(attach, out LoadAction loadAction, out _, out RgbaFloat color);
                if (loadAction == LoadAction.Clear)
                {
                    RawColor4 rawColor = new RawColor4(color.R, color.G, color.B, color.A);
                    _ctx.ClearRenderTargetView(d3d11FB.RenderTargetViews[attach], rawColor);
                }
            }

            if (d3d11FB.DepthStencilView != null &&
                (rpd.DepthLoadAction == LoadAction.Clear || rpd.StencilLoadAction == LoadAction.Clear))
            {
                DepthStencilClearFlags flags = 0;
                if (rpd.DepthLoadAction == LoadAction.Clear) { flags |= DepthStencilClearFlags.Depth; }
                if (rpd.StencilLoadAction == LoadAction.Clear) { flags |= DepthStencilClearFlags.Stencil; }

                _ctx.ClearDepthStencilView(
                    d3d11FB.DepthStencilView,
                    flags,
                    rpd.ClearDepth,
                    rpd.ClearStencil);
            }

            _currentFB = rpd.Framebuffer;
            SetFullViewports();
            SetFullScissorRects();
        }

        private protected override void EndRenderPassCore()
        {
            for (uint i = 0; i < _rpd.Framebuffer.ColorTargets.Count; i++)
            {
                if (_rpd.Framebuffer.ResolveTargets.Count > i)
                {
                    FramebufferAttachment srcAttachment = _rpd.Framebuffer.ColorTargets[(int)i];
                    FramebufferAttachment resolveTarget = _rpd.Framebuffer.ResolveTargets[(int)i];
                    if (resolveTarget.Target != null)
                    {
                        D3D11Texture d3d11Source = Util.AssertSubtype<Texture, D3D11Texture>(srcAttachment.Target);
                        D3D11Texture d3d11Destination = Util.AssertSubtype<Texture, D3D11Texture>(resolveTarget.Target);
                        _ctx.ResolveSubresource(
                            d3d11Source.DeviceTexture,
                            D3D11Util.ComputeSubresource(srcAttachment.MipLevel, srcAttachment.Target.MipLevels, srcAttachment.ArrayLayer),
                            d3d11Destination.DeviceTexture,
                            D3D11Util.ComputeSubresource(resolveTarget.MipLevel, resolveTarget.Target.MipLevels, resolveTarget.ArrayLayer),
                            d3d11Destination.DxgiFormat);
                    }
                }
            }

            _currentFB = null;
        }

        public void Reset()
        {
            if (_commandList != null)
            {
                _commandList.Dispose();
                _commandList = null;
            }

            ClearCachedState();
            _ctx.ClearState();
            ResetManagedState();

            _state = RecordingState.Recording;
        }

        private void ClearReferencedSwapchains()
        {
            foreach (D3D11Swapchain sc in _referencedSwapchains)
            {
                sc.RemoveCommandBufferReference(this);
            }
            _referencedSwapchains.Clear();
        }

        internal void ClearCachedState()
        {
            _graphicsPipeline = null;
            _computePipeline = null;
        }

        private void ResetManagedState()
        {
            _numVertexBindings = 0;
            Util.ClearArray(_vertexBindings);
            _vertexStrides = null;
            Util.ClearArray(_vertexOffsets);

            Util.ClearArray(_viewports);
            Util.ClearArray(_scissors);
            _viewportsChanged = false;
            _scissorRectsChanged = false;

            _ib = null;
            _graphicsPipeline = null;
            _blendState = null;
            _depthStencilState = null;
            _rasterizerState = null;
            _primitiveTopology = Vortice.Direct3D.PrimitiveTopology.Undefined;
            _inputLayout = null;
            _vertexShader = null;
            _geometryShader = null;
            _hullShader = null;
            _domainShader = null;
            _pixelShader = null;

            ClearSets(_graphicsResourceSets);

            Util.ClearArray(_vertexBoundUniformBuffers);
            Util.ClearArray(_vertexBoundTextureViews);
            Util.ClearArray(_vertexBoundSamplers);

            Util.ClearArray(_fragmentBoundUniformBuffers);
            Util.ClearArray(_fragmentBoundTextureViews);
            Util.ClearArray(_fragmentBoundSamplers);

            _computePipeline = null;
            ClearSets(_computeResourceSets);

            foreach (KeyValuePair<Texture, List<D3D11BoundTextureInfo>> kvp in _boundSRVs)
            {
                List<D3D11BoundTextureInfo> list = kvp.Value;
                list.Clear();
                PoolBoundTextureList(list);
            }
            _boundSRVs.Clear();

            foreach (KeyValuePair<Texture, List<D3D11BoundTextureInfo>> kvp in _boundUAVs)
            {
                List<D3D11BoundTextureInfo> list = kvp.Value;
                list.Clear();
                PoolBoundTextureList(list);
            }
            _boundUAVs.Clear();
        }

        private void ClearSets(BoundResourceSetInfo[] boundSets)
        {
            foreach (BoundResourceSetInfo boundSetInfo in boundSets)
            {
                boundSetInfo.Offsets.Dispose();
            }
            Util.ClearArray(boundSets);
        }

        private protected override void BindIndexBufferCore(DeviceBuffer buffer, IndexFormat format, uint offset)
        {
            if (_ib != buffer || _ibOffset != offset)
            {
                _ib = buffer;
                _ibOffset = offset;
                D3D11Buffer d3d11Buffer = Util.AssertSubtype<DeviceBuffer, D3D11Buffer>(buffer);
                UnbindUAVBuffer(buffer);
                _ctx.IASetIndexBuffer(d3d11Buffer.Buffer, D3D11Formats.ToDxgiFormat(format), (int)offset);
            }
        }

        private protected unsafe override void BindPipelineCore(Pipeline pipeline)
        {
            if (!pipeline.IsComputePipeline && _graphicsPipeline != pipeline)
            {
                D3D11Pipeline d3dPipeline = Util.AssertSubtype<Pipeline, D3D11Pipeline>(pipeline);
                _graphicsPipeline = d3dPipeline;
                ClearSets(_graphicsResourceSets); // Invalidate resource set bindings -- they may be invalid.
                Util.ClearArray(_invalidatedGraphicsResourceSets);

                ID3D11BlendState blendState = d3dPipeline.BlendState;
                if (_blendState != blendState)
                {
                    _blendState = blendState;
                    _ctx.OMSetBlendState(blendState);
                }

                ID3D11DepthStencilState depthStencilState = d3dPipeline.DepthStencilState;
                uint stencilReference = d3dPipeline.StencilReference;
                if (_depthStencilState != depthStencilState || _stencilReference != stencilReference)
                {
                    _depthStencilState = depthStencilState;
                    _stencilReference = stencilReference;
                    _ctx.OMSetDepthStencilState(depthStencilState, (int)stencilReference);
                }

                ID3D11RasterizerState rasterizerState = d3dPipeline.RasterizerState;
                if (_rasterizerState != rasterizerState)
                {
                    _rasterizerState = rasterizerState;
                    _ctx.RSSetState(rasterizerState);
                }

                Vortice.Direct3D.PrimitiveTopology primitiveTopology = d3dPipeline.PrimitiveTopology;
                if (_primitiveTopology != primitiveTopology)
                {
                    _primitiveTopology = primitiveTopology;
                    _ctx.IASetPrimitiveTopology(primitiveTopology);
                }

                ID3D11InputLayout inputLayout = d3dPipeline.InputLayout;
                if (_inputLayout != inputLayout)
                {
                    _inputLayout = inputLayout;
                    _ctx.IASetInputLayout(inputLayout);
                }

                ID3D11VertexShader vertexShader = d3dPipeline.VertexShader;
                if (_vertexShader != vertexShader)
                {
                    _vertexShader = vertexShader;
                    _ctx.VSSetShader(vertexShader);
                }

                ID3D11GeometryShader geometryShader = d3dPipeline.GeometryShader;
                if (_geometryShader != geometryShader)
                {
                    _geometryShader = geometryShader;
                    _ctx.GSSetShader(geometryShader);
                }

                ID3D11HullShader hullShader = d3dPipeline.HullShader;
                if (_hullShader != hullShader)
                {
                    _hullShader = hullShader;
                    _ctx.HSSetShader(hullShader);
                }

                ID3D11DomainShader domainShader = d3dPipeline.DomainShader;
                if (_domainShader != domainShader)
                {
                    _domainShader = domainShader;
                    _ctx.DSSetShader(domainShader);
                }

                ID3D11PixelShader pixelShader = d3dPipeline.PixelShader;
                if (_pixelShader != pixelShader)
                {
                    _pixelShader = pixelShader;
                    _ctx.PSSetShader(pixelShader);
                }

                _vertexStrides = d3dPipeline.VertexStrides;
                if (_vertexStrides != null)
                {
                    int vertexStridesCount = _vertexStrides.Length;
                    Util.EnsureArrayMinimumSize(ref _vertexBindings, (uint)vertexStridesCount);
                    Util.EnsureArrayMinimumSize(ref _vertexOffsets, (uint)vertexStridesCount);
                }

                Util.EnsureArrayMinimumSize(ref _graphicsResourceSets, (uint)d3dPipeline.ResourceLayouts.Length);
                Util.EnsureArrayMinimumSize(ref _invalidatedGraphicsResourceSets, (uint)d3dPipeline.ResourceLayouts.Length);
            }
            else if (pipeline.IsComputePipeline && _computePipeline != pipeline)
            {
                D3D11Pipeline d3dPipeline = Util.AssertSubtype<Pipeline, D3D11Pipeline>(pipeline);
                _computePipeline = d3dPipeline;
                ClearSets(_computeResourceSets); // Invalidate resource set bindings -- they may be invalid.
                Util.ClearArray(_invalidatedComputeResourceSets);

                ID3D11ComputeShader computeShader = d3dPipeline.ComputeShader;
                _ctx.CSSetShader(computeShader);
                Util.EnsureArrayMinimumSize(ref _computeResourceSets, (uint)d3dPipeline.ResourceLayouts.Length);
                Util.EnsureArrayMinimumSize(ref _invalidatedComputeResourceSets, (uint)d3dPipeline.ResourceLayouts.Length);
            }
        }

        private protected override void BindGraphicsResourceSetCore(uint slot, ResourceSet rs, Span<uint> dynamicOffsets)
        {
            if (_graphicsResourceSets[slot].Equals(rs, dynamicOffsets))
            {
                return;
            }

            _graphicsResourceSets[slot].Offsets.Dispose();
            _graphicsResourceSets[slot] = new BoundResourceSetInfo(rs, dynamicOffsets);
            ActivateResourceSet(slot, _graphicsResourceSets[slot], true);
        }

        private protected override void BindComputeResourceSetCore(uint slot, ResourceSet rs, Span<uint> dynamicOffsets)
        {
            if (_computeResourceSets[slot].Equals(rs, dynamicOffsets))
            {
                return;
            }

            _computeResourceSets[slot].Offsets.Dispose();
            _computeResourceSets[slot] = new BoundResourceSetInfo(rs, dynamicOffsets);
            ActivateResourceSet(slot, _computeResourceSets[slot], false);
        }

        private void ActivateResourceSet(uint slot, BoundResourceSetInfo brsi, bool graphics)
        {
            D3D11ResourceSet d3d11RS = Util.AssertSubtype<ResourceSet, D3D11ResourceSet>(brsi.Set);

            int cbBase = GetConstantBufferBase(slot, graphics);
            int uaBase = GetUnorderedAccessBase(slot, graphics);
            int textureBase = GetTextureBase(slot, graphics);
            int samplerBase = GetSamplerBase(slot, graphics);

            D3D11ResourceSlots d3d11Slots = graphics
                ? _graphicsPipeline.ResourceSlots[slot]
                : _computePipeline.ResourceSlots[slot];
            BindableResource[] resources = d3d11RS.Resources;
            uint dynamicOffsetIndex = 0;
            for (int i = 0; i < resources.Length; i++)
            {
                D3D11ResourceSlots.ResourceBindingInfo rbi = d3d11Slots.GetDeviceSlotIndex(i);
                if (rbi.IsUnused) { continue; }

                BindableResource resource = resources[i];
                uint bufferOffset = 0;
                if (d3d11Slots.IsDynamicBuffer(i))
                {
                    bufferOffset = brsi.Offsets.Get(dynamicOffsetIndex);
                    dynamicOffsetIndex += 1;
                }
                switch (rbi.Kind)
                {
                    case ResourceKind.UniformBuffer:
                    {
                        D3D11BufferRange range = GetBufferRange(resource, bufferOffset);
                        BindUniformBuffer(range, cbBase + rbi.Slot, rbi.Stages);
                        break;
                    }
                    case ResourceKind.StructuredBufferReadOnly:
                    {
                        D3D11BufferRange range = GetBufferRange(resource, bufferOffset);
                        BindStorageBufferView(range, textureBase + rbi.Slot, rbi.Stages);
                        break;
                    }
                    case ResourceKind.StructuredBufferReadWrite:
                    {
                        D3D11BufferRange range = GetBufferRange(resource, bufferOffset);
                        ID3D11UnorderedAccessView uav = range.Buffer.GetUnorderedAccessView(range.Offset, range.Size);
                        BindUnorderedAccessView(null, range.Buffer, uav, uaBase + rbi.Slot, rbi.Stages, slot);
                        break;
                    }
                    case ResourceKind.TextureReadOnly:
                        TextureView texView = Util.GetTextureView(_gd, resource);
                        D3D11TextureView d3d11TexView = Util.AssertSubtype<TextureView, D3D11TextureView>(texView);
                        UnbindUAVTexture(d3d11TexView.Target);
                        BindTextureView(d3d11TexView, textureBase + rbi.Slot, rbi.Stages, slot);
                        break;
                    case ResourceKind.TextureReadWrite:
                        TextureView rwTexView = Util.GetTextureView(_gd, resource);
                        D3D11TextureView d3d11RWTexView = Util.AssertSubtype<TextureView, D3D11TextureView>(rwTexView);
                        UnbindSRVTexture(d3d11RWTexView.Target);
                        BindUnorderedAccessView(d3d11RWTexView.Target, null, d3d11RWTexView.UnorderedAccessView, uaBase + rbi.Slot, rbi.Stages, slot);
                        break;
                    case ResourceKind.Sampler:
                        D3D11Sampler sampler = Util.AssertSubtype<BindableResource, D3D11Sampler>(resource);
                        BindSampler(sampler, samplerBase + rbi.Slot, rbi.Stages);
                        break;
                    default: throw Illegal.Value<ResourceKind>();
                }
            }
        }

        private D3D11BufferRange GetBufferRange(BindableResource resource, uint additionalOffset)
        {
            if (resource is D3D11Buffer d3d11Buff)
            {
                return new D3D11BufferRange(d3d11Buff, additionalOffset, d3d11Buff.SizeInBytes);
            }
            else if (resource is DeviceBufferRange range)
            {
                return new D3D11BufferRange(
                    Util.AssertSubtype<DeviceBuffer, D3D11Buffer>(range.Buffer),
                    range.Offset + additionalOffset,
                    range.SizeInBytes);
            }
            else
            {
                throw new VeldridException($"Unexpected resource type used in a buffer type slot: {resource.GetType().Name}");
            }
        }

        private void UnbindSRVTexture(Texture target)
        {
            if (_boundSRVs.TryGetValue(target, out List<D3D11BoundTextureInfo> btis))
            {
                foreach (D3D11BoundTextureInfo bti in btis)
                {
                    BindTextureView(null, bti.Slot, bti.Stages, 0);

                    if ((bti.Stages & ShaderStages.Compute) == ShaderStages.Compute)
                    {
                        _invalidatedComputeResourceSets[bti.ResourceSet] = true;
                    }
                    else
                    {
                        _invalidatedGraphicsResourceSets[bti.ResourceSet] = true;
                    }
                }

                bool result = _boundSRVs.Remove(target);
                Debug.Assert(result);

                btis.Clear();
                PoolBoundTextureList(btis);
            }
        }

        private void PoolBoundTextureList(List<D3D11BoundTextureInfo> btis)
        {
            _boundTextureInfoPool.Add(btis);
        }

        private void UnbindUAVTexture(Texture target)
        {
            if (_boundUAVs.TryGetValue(target, out List<D3D11BoundTextureInfo> btis))
            {
                foreach (D3D11BoundTextureInfo bti in btis)
                {
                    BindUnorderedAccessView(null, null, null, bti.Slot, bti.Stages, bti.ResourceSet);
                    if ((bti.Stages & ShaderStages.Compute) == ShaderStages.Compute)
                    {
                        _invalidatedComputeResourceSets[bti.ResourceSet] = true;
                    }
                    else
                    {
                        _invalidatedGraphicsResourceSets[bti.ResourceSet] = true;
                    }
                }

                bool result = _boundUAVs.Remove(target);
                Debug.Assert(result);

                btis.Clear();
                PoolBoundTextureList(btis);
            }
        }

        private int GetConstantBufferBase(uint slot, bool graphics)
        {
            D3D11ResourceSlots[] slots = graphics ? _graphicsPipeline.ResourceSlots : _computePipeline.ResourceSlots;
            int ret = 0;
            for (int i = 0; i < slot; i++)
            {
                Debug.Assert(slots[i] != null);
                ret += slots[i].UniformBufferCount;
            }

            return ret;
        }

        private int GetUnorderedAccessBase(uint slot, bool graphics)
        {
            D3D11ResourceSlots[] slots = graphics ? _graphicsPipeline.ResourceSlots : _computePipeline.ResourceSlots;
            int ret = 0;
            for (int i = 0; i < slot; i++)
            {
                Debug.Assert(slots[i] != null);
                ret += slots[i].StorageBufferCount;
            }

            return ret;
        }

        private int GetTextureBase(uint slot, bool graphics)
        {
            D3D11ResourceSlots[] slots = graphics ? _graphicsPipeline.ResourceSlots : _computePipeline.ResourceSlots;
            int ret = 0;
            for (int i = 0; i < slot; i++)
            {
                Debug.Assert(slots[i] != null);
                ret += slots[i].TextureCount;
            }

            return ret;
        }

        private int GetSamplerBase(uint slot, bool graphics)
        {
            D3D11ResourceSlots[] slots = graphics ? _graphicsPipeline.ResourceSlots : _computePipeline.ResourceSlots;
            int ret = 0;
            for (int i = 0; i < slot; i++)
            {
                Debug.Assert(slots[i] != null);
                ret += slots[i].SamplerCount;
            }

            return ret;
        }

        private protected override void BindVertexBufferCore(uint index, DeviceBuffer buffer, uint offset)
        {
            D3D11Buffer d3d11Buffer = Util.AssertSubtype<DeviceBuffer, D3D11Buffer>(buffer);
            if (_vertexBindings[index] != d3d11Buffer.Buffer || _vertexOffsets[index] != offset)
            {
                _vertexBindingsChanged = true;
                UnbindUAVBuffer(buffer);
                _vertexBindings[index] = d3d11Buffer.Buffer;
                _vertexOffsets[index] = (int)offset;
                _numVertexBindings = Math.Max((index + 1), _numVertexBindings);
            }
        }

        private protected override void DrawCore(uint vertexCount, uint instanceCount, uint vertexStart, uint instanceStart)
        {
            PreDrawCommand();

            if (instanceCount == 1 && instanceStart == 0)
            {
                _ctx.Draw((int)vertexCount, (int)vertexStart);
            }
            else
            {
                _ctx.DrawInstanced((int)vertexCount, (int)instanceCount, (int)vertexStart, (int)instanceStart);
            }
        }

        private protected override void DrawIndexedCore(uint indexCount, uint instanceCount, uint indexStart, int vertexOffset, uint instanceStart)
        {
            PreDrawCommand();

            Debug.Assert(_ib != null);
            if (instanceCount == 1 && instanceStart == 0)
            {
                _ctx.DrawIndexed((int)indexCount, (int)indexStart, vertexOffset);
            }
            else
            {
                _ctx.DrawIndexedInstanced((int)indexCount, (int)instanceCount, (int)indexStart, vertexOffset, (int)instanceStart);
            }
        }

        private protected override void DrawIndirectCore(DeviceBuffer indirectBuffer, uint offset, uint drawCount, uint stride)
        {
            PreDrawCommand();

            D3D11Buffer d3d11Buffer = Util.AssertSubtype<DeviceBuffer, D3D11Buffer>(indirectBuffer);
            int currentOffset = (int)offset;
            for (uint i = 0; i < drawCount; i++)
            {
                _ctx.DrawInstancedIndirect(d3d11Buffer.Buffer, currentOffset);
                currentOffset += (int)stride;
            }
        }

        private protected override void DrawIndexedIndirectCore(DeviceBuffer indirectBuffer, uint offset, uint drawCount, uint stride)
        {
            PreDrawCommand();

            D3D11Buffer d3d11Buffer = Util.AssertSubtype<DeviceBuffer, D3D11Buffer>(indirectBuffer);
            int currentOffset = (int)offset;
            for (uint i = 0; i < drawCount; i++)
            {
                _ctx.DrawIndexedInstancedIndirect(d3d11Buffer.Buffer, currentOffset);
                currentOffset += (int)stride;
            }
        }

        private void PreDrawCommand()
        {
            FlushViewports();
            FlushScissorRects();
            FlushVertexBindings();

            int graphicsResourceCount = _graphicsPipeline.ResourceLayouts.Length;
            for (uint i = 0; i < graphicsResourceCount; i++)
            {
                if (_invalidatedGraphicsResourceSets[i])
                {
                    _invalidatedGraphicsResourceSets[i] = false;
                    ActivateResourceSet(i, _graphicsResourceSets[i], true);
                }
            }
        }

        private protected override void DispatchCore(uint groupCountX, uint groupCountY, uint groupCountZ)
        {
            PreDispatchCommand();

            _ctx.Dispatch((int)groupCountX, (int)groupCountY, (int)groupCountZ);
        }

        private protected override void DispatchIndirectCore(DeviceBuffer indirectBuffer, uint offset)
        {
            PreDispatchCommand();
            D3D11Buffer d3d11Buffer = Util.AssertSubtype<DeviceBuffer, D3D11Buffer>(indirectBuffer);
            _ctx.DispatchIndirect(d3d11Buffer.Buffer, (int)offset);
        }

        private void PreDispatchCommand()
        {
            int computeResourceCount = _computePipeline.ResourceLayouts.Length;
            for (uint i = 0; i < computeResourceCount; i++)
            {
                if (_invalidatedComputeResourceSets[i])
                {
                    _invalidatedComputeResourceSets[i] = false;
                    ActivateResourceSet(i, _computeResourceSets[i], false);
                }
            }
        }

        private void FlushViewports()
        {
            if (_viewportsChanged)
            {
                _viewportsChanged = false;
                _ctx.RSSetViewports(_viewports.Length, _viewports);
            }
        }

        private void FlushScissorRects()
        {
            if (_scissorRectsChanged)
            {
                _scissorRectsChanged = false;
                if (_scissors.Length > 0)
                {
                    // Because this array is resized using Util.EnsureMinimumArraySize, this might set more scissor rectangles
                    // than are actually needed, but this is okay -- extras are essentially ignored and should be harmless.
                    _ctx.RSSetScissorRects(_scissors);
                }
            }
        }

        private unsafe void FlushVertexBindings()
        {
            if (_vertexBindingsChanged)
            {
                _ctx.IASetVertexBuffers(
                    0, (int)_numVertexBindings,
                    _vertexBindings,
                    _vertexStrides,
                    _vertexOffsets);

                _vertexBindingsChanged = false;
            }
        }

        private protected override void SetScissorRectCore(uint index, uint x, uint y, uint width, uint height)
        {
            _scissorRectsChanged = true;
            Util.EnsureArrayMinimumSize(ref _scissors, index + 1);
            _scissors[index] = new RawRectangle((int)x, (int)y, (int)(x + width), (int)(y + height));
        }

        private protected override void SetViewportCore(uint index, Viewport viewport)
        {
            _viewportsChanged = true;
            Util.EnsureArrayMinimumSize(ref _viewports, index + 1);
            _viewports[index] = new RawViewportF(viewport.X, viewport.Y, viewport.Width, viewport.Height, viewport.MinDepth, viewport.MaxDepth);
        }

        private void SetFullViewports()
        {
            SetViewport(0, new Viewport(0, 0, _currentFB.Width, _currentFB.Height, 0, 1));

            for (uint index = 1; index < _currentFB.ColorTargets.Count; index++)
            {
                SetViewport(index, new Viewport(0, 0, _currentFB.Width, _currentFB.Height, 0, 1));
            }
        }

        private void SetFullScissorRects()
        {
            SetScissorRect(0, 0, 0, _currentFB.Width, _currentFB.Height);

            for (uint index = 1; index < _currentFB.ColorTargets.Count; index++)
            {
                SetScissorRect(index, 0, 0, _currentFB.Width, _currentFB.Height);
            }
        }

        private void BindTextureView(D3D11TextureView texView, int slot, ShaderStages stages, uint resourceSet)
        {
            ID3D11ShaderResourceView srv = texView?.ShaderResourceView ?? null;
            if (srv != null)
            {
                if (!_boundSRVs.TryGetValue(texView.Target, out List<D3D11BoundTextureInfo> list))
                {
                    list = GetNewOrCachedBoundTextureInfoList();
                    _boundSRVs.Add(texView.Target, list);
                }
                list.Add(new D3D11BoundTextureInfo { Slot = slot, Stages = stages, ResourceSet = resourceSet });
            }

            if ((stages & ShaderStages.Vertex) == ShaderStages.Vertex)
            {
                bool bind = false;
                if (slot < MaxCachedUniformBuffers)
                {
                    if (_vertexBoundTextureViews[slot] != texView)
                    {
                        _vertexBoundTextureViews[slot] = texView;
                        bind = true;
                    }
                }
                else
                {
                    bind = true;
                }
                if (bind)
                {
                    _ctx.VSSetShaderResource(slot, srv);
                }
            }
            if ((stages & ShaderStages.Geometry) == ShaderStages.Geometry)
            {
                _ctx.GSSetShaderResource(slot, srv);
            }
            if ((stages & ShaderStages.TessellationControl) == ShaderStages.TessellationControl)
            {
                _ctx.HSSetShaderResource(slot, srv);
            }
            if ((stages & ShaderStages.TessellationEvaluation) == ShaderStages.TessellationEvaluation)
            {
                _ctx.DSSetShaderResource(slot, srv);
            }
            if ((stages & ShaderStages.Fragment) == ShaderStages.Fragment)
            {
                bool bind = false;
                if (slot < MaxCachedUniformBuffers)
                {
                    if (_fragmentBoundTextureViews[slot] != texView)
                    {
                        _fragmentBoundTextureViews[slot] = texView;
                        bind = true;
                    }
                }
                else
                {
                    bind = true;
                }
                if (bind)
                {
                    _ctx.PSSetShaderResource(slot, srv);
                }
            }
            if ((stages & ShaderStages.Compute) == ShaderStages.Compute)
            {
                _ctx.CSSetShaderResource(slot, srv);
            }
        }

        private List<D3D11BoundTextureInfo> GetNewOrCachedBoundTextureInfoList()
        {
            if (_boundTextureInfoPool.Count > 0)
            {
                int index = _boundTextureInfoPool.Count - 1;
                List<D3D11BoundTextureInfo> ret = _boundTextureInfoPool[index];
                _boundTextureInfoPool.RemoveAt(index);
                return ret;
            }

            return new List<D3D11BoundTextureInfo>();
        }

        private void BindStorageBufferView(D3D11BufferRange range, int slot, ShaderStages stages)
        {
            bool compute = (stages & ShaderStages.Compute) != 0;
            UnbindUAVBuffer(range.Buffer);

            ID3D11ShaderResourceView srv = range.Buffer.GetShaderResourceView(range.Offset, range.Size);

            if ((stages & ShaderStages.Vertex) == ShaderStages.Vertex)
            {
                _ctx.VSSetShaderResource(slot, srv);
            }
            if ((stages & ShaderStages.Geometry) == ShaderStages.Geometry)
            {
                _ctx.GSSetShaderResource(slot, srv);
            }
            if ((stages & ShaderStages.TessellationControl) == ShaderStages.TessellationControl)
            {
                _ctx.HSSetShaderResource(slot, srv);
            }
            if ((stages & ShaderStages.TessellationEvaluation) == ShaderStages.TessellationEvaluation)
            {
                _ctx.DSSetShaderResource(slot, srv);
            }
            if ((stages & ShaderStages.Fragment) == ShaderStages.Fragment)
            {
                _ctx.PSSetShaderResource(slot, srv);
            }
            if (compute)
            {
                _ctx.CSSetShaderResource(slot, srv);
            }
        }

        private void BindUniformBuffer(D3D11BufferRange range, int slot, ShaderStages stages)
        {
            if ((stages & ShaderStages.Vertex) == ShaderStages.Vertex)
            {
                bool bind = false;
                if (slot < MaxCachedUniformBuffers)
                {
                    if (!_vertexBoundUniformBuffers[slot].Equals(range))
                    {
                        _vertexBoundUniformBuffers[slot] = range;
                        bind = true;
                    }
                }
                else
                {
                    bind = true;
                }
                if (bind)
                {
                    if (range.IsFullRange)
                    {
                        _ctx.VSSetConstantBuffer(slot, range.Buffer.Buffer);
                    }
                    else
                    {
                        PackRangeParams(range);
                        if (_gd.SupportsCommandLists)
                        {
                            _ctx.VSSetConstantBuffer(slot, null);
                        }
                        _ctx1.VSSetConstantBuffers1(slot, 1, _cbOut, _firstConstRef, _numConstsRef);
                    }
                }
            }
            if ((stages & ShaderStages.Geometry) == ShaderStages.Geometry)
            {
                if (range.IsFullRange)
                {
                    _ctx.GSSetConstantBuffer(slot, range.Buffer.Buffer);
                }
                else
                {
                    PackRangeParams(range);
                    if (_gd.SupportsCommandLists)
                    {
                        _ctx.GSSetConstantBuffer(slot, null);
                    }
                    _ctx1.GSSetConstantBuffers1(slot, 1, _cbOut, _firstConstRef, _numConstsRef);
                }
            }
            if ((stages & ShaderStages.TessellationControl) == ShaderStages.TessellationControl)
            {
                if (range.IsFullRange)
                {
                    _ctx.HSSetConstantBuffer(slot, range.Buffer.Buffer);
                }
                else
                {
                    PackRangeParams(range);
                    if (_gd.SupportsCommandLists)
                    {
                        _ctx.HSSetConstantBuffer(slot, null);
                    }
                    _ctx1.HSSetConstantBuffers1(slot, 1, _cbOut, _firstConstRef, _numConstsRef);
                }
            }
            if ((stages & ShaderStages.TessellationEvaluation) == ShaderStages.TessellationEvaluation)
            {
                if (range.IsFullRange)
                {
                    _ctx.DSSetConstantBuffer(slot, range.Buffer.Buffer);
                }
                else
                {
                    PackRangeParams(range);
                    if (_gd.SupportsCommandLists)
                    {
                        _ctx.DSSetConstantBuffer(slot, null);
                    }
                    _ctx1.DSSetConstantBuffers1(slot, 1, _cbOut, _firstConstRef, _numConstsRef);
                }
            }
            if ((stages & ShaderStages.Fragment) == ShaderStages.Fragment)
            {
                bool bind = false;
                if (slot < MaxCachedUniformBuffers)
                {
                    if (!_fragmentBoundUniformBuffers[slot].Equals(range))
                    {
                        _fragmentBoundUniformBuffers[slot] = range;
                        bind = true;
                    }
                }
                else
                {
                    bind = true;
                }
                if (bind)
                {
                    if (range.IsFullRange)
                    {
                        _ctx.PSSetConstantBuffer(slot, range.Buffer.Buffer);
                    }
                    else
                    {
                        PackRangeParams(range);
                        if (_gd.SupportsCommandLists)
                        {
                            _ctx.PSSetConstantBuffer(slot, null);
                        }
                        _ctx1.PSSetConstantBuffers1(slot, 1, _cbOut, _firstConstRef, _numConstsRef);
                    }
                }
            }
            if ((stages & ShaderStages.Compute) == ShaderStages.Compute)
            {
                if (range.IsFullRange)
                {
                    _ctx.CSSetConstantBuffer(slot, range.Buffer.Buffer);
                }
                else
                {
                    PackRangeParams(range);
                    if (_gd.SupportsCommandLists)
                    {
                        _ctx.CSSetConstantBuffer(slot, null);
                    }
                    _ctx1.CSSetConstantBuffers1(slot, 1, _cbOut, _firstConstRef, _numConstsRef);
                }
            }
        }

        private void PackRangeParams(D3D11BufferRange range)
        {
            _cbOut[0] = range.Buffer.Buffer;
            _firstConstRef[0] = (int)range.Offset / 16;
            uint roundedSize = range.Size < 256 ? 256u : range.Size;
            _numConstsRef[0] = (int)roundedSize / 16;
        }

        private void BindUnorderedAccessView(
            Texture texture,
            DeviceBuffer buffer,
            ID3D11UnorderedAccessView uav,
            int slot,
            ShaderStages stages,
            uint resourceSet)
        {
            bool compute = stages == ShaderStages.Compute;
            Debug.Assert(compute || ((stages & ShaderStages.Compute) == 0));
            Debug.Assert(texture == null || buffer == null);

            if (texture != null && uav != null)
            {
                if (!_boundUAVs.TryGetValue(texture, out List<D3D11BoundTextureInfo> list))
                {
                    list = GetNewOrCachedBoundTextureInfoList();
                    _boundUAVs.Add(texture, list);
                }
                list.Add(new D3D11BoundTextureInfo { Slot = slot, Stages = stages, ResourceSet = resourceSet });
            }

            int baseSlot = 0;
            if (!compute && _fragmentBoundSamplers != null)
            {
                baseSlot = _currentFB.ColorTargets.Count;
            }
            int actualSlot = baseSlot + slot;

            if (buffer != null)
            {
                TrackBoundUAVBuffer(buffer, actualSlot, compute);
            }

            if (compute)
            {
                _ctx.CSSetUnorderedAccessView(actualSlot, uav);
            }
            else
            {
                _ctx.OMSetUnorderedAccessView(actualSlot, uav);
            }
        }

        private void TrackBoundUAVBuffer(DeviceBuffer buffer, int slot, bool compute)
        {
            List<(DeviceBuffer, int)> list = compute ? _boundComputeUAVBuffers : _boundOMUAVBuffers;
            list.Add((buffer, slot));
        }

        private void UnbindUAVBuffer(DeviceBuffer buffer)
        {
            UnbindUAVBufferIndividual(buffer, false);
            UnbindUAVBufferIndividual(buffer, true);
        }

        private void UnbindUAVBufferIndividual(DeviceBuffer buffer, bool compute)
        {
            List<(DeviceBuffer, int)> list = compute ? _boundComputeUAVBuffers : _boundOMUAVBuffers;
            for (int i = 0; i < list.Count; i++)
            {
                if (list[i].Item1 == buffer)
                {
                    int slot = list[i].Item2;
                    if (compute)
                    {
                        _ctx.CSSetUnorderedAccessView(slot, null);
                    }
                    else
                    {
                        _ctx.OMSetUnorderedAccessView(slot, null);
                    }

                    list.RemoveAt(i);
                    i -= 1;
                }
            }
        }

        private void BindSampler(D3D11Sampler sampler, int slot, ShaderStages stages)
        {
            if ((stages & ShaderStages.Vertex) == ShaderStages.Vertex)
            {
                bool bind = false;
                if (slot < MaxCachedSamplers)
                {
                    if (_vertexBoundSamplers[slot] != sampler)
                    {
                        _vertexBoundSamplers[slot] = sampler;
                        bind = true;
                    }
                }
                else
                {
                    bind = true;
                }
                if (bind)
                {
                    _ctx.VSSetSampler(slot, sampler.DeviceSampler);
                }
            }
            if ((stages & ShaderStages.Geometry) == ShaderStages.Geometry)
            {
                _ctx.GSSetSampler(slot, sampler.DeviceSampler);
            }
            if ((stages & ShaderStages.TessellationControl) == ShaderStages.TessellationControl)
            {
                _ctx.HSSetSampler(slot, sampler.DeviceSampler);
            }
            if ((stages & ShaderStages.TessellationEvaluation) == ShaderStages.TessellationEvaluation)
            {
                _ctx.DSSetSampler(slot, sampler.DeviceSampler);
            }
            if ((stages & ShaderStages.Fragment) == ShaderStages.Fragment)
            {
                bool bind = false;
                if (slot < MaxCachedSamplers)
                {
                    if (_fragmentBoundSamplers[slot] != sampler)
                    {
                        _fragmentBoundSamplers[slot] = sampler;
                        bind = true;
                    }
                }
                else
                {
                    bind = true;
                }
                if (bind)
                {
                    _ctx.PSSetSampler(slot, sampler.DeviceSampler);
                }
            }
        }

        private protected unsafe override void UpdateBufferCore(DeviceBuffer buffer, uint bufferOffsetInBytes, IntPtr source, uint sizeInBytes)
        {
            D3D11Buffer d3dBuffer = Util.AssertSubtype<DeviceBuffer, D3D11Buffer>(buffer);
            if (sizeInBytes == 0)
            {
                return;
            }

            bool isDynamic = (buffer.Usage & BufferUsage.Dynamic) == BufferUsage.Dynamic;
            bool isStaging = (buffer.Usage & BufferUsage.Staging) == BufferUsage.Staging;
            bool isUniformBuffer = (buffer.Usage & BufferUsage.UniformBuffer) == BufferUsage.UniformBuffer;
            bool useMap = isDynamic;
            bool updateFullBuffer = bufferOffsetInBytes == 0 && sizeInBytes == buffer.SizeInBytes;
            bool useUpdateSubresource = !isDynamic && !isStaging && (!isUniformBuffer || updateFullBuffer);

            if (useUpdateSubresource)
            {
                Box? subregion = new Box(
                    (int)bufferOffsetInBytes, 0, 0, (int)(sizeInBytes + bufferOffsetInBytes), 1, 1);
                if (isUniformBuffer)
                {
                    subregion = null;
                }

                if (bufferOffsetInBytes == 0)
                {
                    _ctx.UpdateSubresource(d3dBuffer.Buffer, 0, subregion, source, 0, 0);
                }
                else
                {
                    UpdateSubresource_Workaround(d3dBuffer.Buffer, 0, subregion.Value, source);
                }
            }
            else if (useMap && updateFullBuffer) // Can only update full buffer with WriteDiscard.
            {
                MappedSubresource db = _ctx.Map(
                    d3dBuffer.Buffer,
                    0,
                    D3D11Formats.VdToD3D11MapMode(isDynamic, MapMode.Write),
                    MapFlags.None);
                if (sizeInBytes < 1024)
                {
                    Unsafe.CopyBlock(db.DataPointer.ToPointer(), source.ToPointer(), sizeInBytes);
                }
                else
                {
                    Buffer.MemoryCopy(source.ToPointer(), db.DataPointer.ToPointer(), buffer.SizeInBytes, sizeInBytes);
                }
                _ctx.Unmap(d3dBuffer.Buffer, 0);
            }
            else
            {
                D3D11Buffer staging = GetFreeStagingBuffer(sizeInBytes);
                _gd.UpdateBuffer(staging, 0, source, sizeInBytes);
                CopyBuffer(staging, 0, buffer, bufferOffsetInBytes, sizeInBytes);
                _submittedStagingBuffers.Add(staging);
            }
        }

        private unsafe void UpdateSubresource_Workaround(
            ID3D11Resource resource,
            int subresource,
            Box region,
            IntPtr data)
        {
            bool needWorkaround = !_gd.SupportsCommandLists;
            void* pAdjustedSrcData = data.ToPointer();
            if (needWorkaround)
            {
                Debug.Assert(region.Top == 0 && region.Front == 0);
                pAdjustedSrcData = (byte*)data - region.Left;
            }

            _ctx.UpdateSubresource(resource, subresource, region, (IntPtr)pAdjustedSrcData, 0, 0);
        }


        private D3D11Buffer GetFreeStagingBuffer(uint sizeInBytes)
        {
            foreach (D3D11Buffer buffer in _availableStagingBuffers)
            {
                if (buffer.SizeInBytes >= sizeInBytes)
                {
                    _availableStagingBuffers.Remove(buffer);
                    return buffer;
                }
            }

            DeviceBuffer staging = _gd.ResourceFactory.CreateBuffer(
                new BufferDescription(sizeInBytes, BufferUsage.Staging));

            return Util.AssertSubtype<DeviceBuffer, D3D11Buffer>(staging);
        }

        private protected override void CopyBufferCore(
            DeviceBuffer source,
            uint sourceOffset,
            DeviceBuffer destination,
            uint destinationOffset,
            uint sizeInBytes)
        {
            D3D11Buffer srcD3D11Buffer = Util.AssertSubtype<DeviceBuffer, D3D11Buffer>(source);
            D3D11Buffer dstD3D11Buffer = Util.AssertSubtype<DeviceBuffer, D3D11Buffer>(destination);

            Box region = new Box((int)sourceOffset, 0, 0, (int)(sourceOffset + sizeInBytes), 1, 1);
            _ctx.CopySubresourceRegion(dstD3D11Buffer.Buffer, 0, (int)destinationOffset, 0, 0, srcD3D11Buffer.Buffer, 0, region);
        }

        private protected override void CopyTextureCore(
            Texture source,
            uint srcX, uint srcY, uint srcZ,
            uint srcMipLevel,
            uint srcBaseArrayLayer,
            Texture destination,
            uint dstX, uint dstY, uint dstZ,
            uint dstMipLevel,
            uint dstBaseArrayLayer,
            uint width, uint height, uint depth,
            uint layerCount)
        {
            D3D11Texture srcD3D11Texture = Util.AssertSubtype<Texture, D3D11Texture>(source);
            D3D11Texture dstD3D11Texture = Util.AssertSubtype<Texture, D3D11Texture>(destination);

            uint blockSize = FormatHelpers.IsCompressedFormat(source.Format) ? 4u : 1u;
            uint clampedWidth = Math.Max(blockSize, width);
            uint clampedHeight = Math.Max(blockSize, height);

            Box region = new Box(
                (int)srcX,
                (int)srcY,
                (int)srcZ,
                (int)(srcX + clampedWidth),
                (int)(srcY + clampedHeight),
                (int)(srcZ + depth));

            for (uint i = 0; i < layerCount; i++)
            {
                int srcSubresource = D3D11Util.ComputeSubresource(srcMipLevel, source.MipLevels, srcBaseArrayLayer + i);
                int dstSubresource = D3D11Util.ComputeSubresource(dstMipLevel, destination.MipLevels, dstBaseArrayLayer + i);

                _ctx.CopySubresourceRegion(
                    dstD3D11Texture.DeviceTexture,
                    dstSubresource,
                    (int)dstX,
                    (int)dstY,
                    (int)dstZ,
                    srcD3D11Texture.DeviceTexture,
                    srcSubresource,
                    region);
            }
        }

        private protected override void GenerateMipmapsCore(Texture texture)
        {
            TextureView fullTexView = texture.GetFullTextureView(_gd);
            D3D11TextureView d3d11View = Util.AssertSubtype<TextureView, D3D11TextureView>(fullTexView);
            ID3D11ShaderResourceView srv = d3d11View.ShaderResourceView;
            _ctx.GenerateMips(srv);
        }

        public override string Name
        {
            get => _name;
            set
            {
                _name = value;
                _ctx.DebugName = value;
            }
        }

        public override void PushDebugGroup(string name)
        {
            _uda?.BeginEvent(name);
        }


        public override void PopDebugGroup()
        {
            _uda?.EndEvent();
        }

        public override void InsertDebugMarker(string name)
        {
            _uda?.SetMarker(name);
        }

        private protected override void BlitTextureCore(
            Texture source, uint srcX, uint srcY, uint srcWidth, uint srcHeight,
            Framebuffer destination, uint dstX, uint dstY, uint dstWidth, uint dstHeight,
            bool linearFilter)
        {
            if (srcWidth != dstWidth || srcHeight != dstHeight)
            {
                // Needs to use a shader to sample onto a quad a la TextureBlitter in Veldrid.VirtualReality.
                throw new NotImplementedException();
            }

            D3D11Texture d3dSrc = Util.AssertSubtype<Texture, D3D11Texture>(source);
            D3D11Texture d3dDst = Util.AssertSubtype<Texture, D3D11Texture>(destination.ColorTargets[0].Target);

            uint dstZ = destination.ColorTargets[0].ArrayLayer;
            _ctx.CopySubresourceRegion(
                d3dDst.DeviceTexture, 0, (int)dstX, (int)dstY, (int)dstZ,
                d3dSrc.DeviceTexture, 0, new Box((int)srcX, (int)srcY, 0, (int)(srcX + srcWidth), (int)(srcY + srcHeight), 1));
        }

        internal ID3D11CommandList GetCompletedList()
        {
            if (_state == RecordingState.Recording)
            {
                _commandList = _ctx.FinishCommandList(false);
                _commandList.DebugName = _name;
                _state = RecordingState.ReadyToExecute;
            }
            return _commandList;
        }

        public override void Dispose()
        {
            if (!_disposed)
            {
                ClearReferencedSwapchains();

                _uda?.Dispose();
                _commandList?.Dispose();
                _ctx1?.Dispose();
                _ctx.Dispose();

                foreach (BoundResourceSetInfo boundGraphicsSet in _graphicsResourceSets)
                {
                    boundGraphicsSet.Offsets.Dispose();
                }
                foreach (BoundResourceSetInfo boundComputeSet in _computeResourceSets)
                {
                    boundComputeSet.Offsets.Dispose();
                }

                _disposed = true;
            }
        }

        private protected override void MemoryBarrierCore(ShaderStages sourceStage, ShaderStages destinationStage)
        {
        }

        private protected override void MemoryBarrierCore(Texture texture, uint baseMipLevel, uint levelCount, uint baseArrayLayer, uint layerCount, ShaderStages sourceStage, ShaderStages destinationStage)
        {
        }
    }
}