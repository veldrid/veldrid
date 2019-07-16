using System;
using System.Diagnostics;
using Veldrid.MetalBindings;

namespace Veldrid.MTL
{
    // A non-reusable Metal CommandBuffer. Records directly into Metal encoders.
    internal unsafe class MTLCommandBuffer : CommandBuffer
    {
        private readonly MTLGraphicsDevice _gd;
        private State _recordingState = State.Initial;
        private MetalBindings.MTLCommandBuffer _cb;
        private MTLRenderCommandEncoder _rce;
        private MTLComputeCommandEncoder _cce;
        private Framebuffer _framebuffer;
        private MTLPipeline _graphicsPipeline;
        private MTLPipeline _computePipeline;
        private MTLBuffer _indexBuffer;
        private IndexFormat _indexFormat;
        private uint _indexBufferOffset;

        // Graphics Pipeline and resource state
        private uint _graphicsResourceSetCount;
        private BoundResourceSetInfo[] _graphicsResourceSets;
        private bool[] _graphicsResourceSetsActive;
        private bool _graphicsPipelineChanged;
        private uint _vertexBufferCount;
        private uint _nonVertexBufferCount;
        private MTLBuffer[] _vertexBuffers;
        private uint[] _vbOffsets;
        private bool[] _vertexBuffersActive;
        private bool _viewportsChanged;
        private uint _viewportCount;
        private MTLViewport[] _viewports = Array.Empty<MTLViewport>();
        private bool _scissorRectsChanged;
        private MTLScissorRect[] _scissorRects = Array.Empty<MTLScissorRect>();
        private MTLFence _completionFence;

        private bool RenderEncoderActive => !_rce.IsNull;
        // private bool BlitEncoderActive => !_bce.IsNull;
        // private bool ComputeEncoderActive => !_cce.IsNull;

        public MTLCommandBuffer(MTLGraphicsDevice gd, ref CommandBufferDescription desc)
            : base(gd.Features)
        {
            Debug.Assert((desc.Flags & CommandBufferFlags.Reusable) == 0);
            _gd = gd;
        }

        public override string Name { get; set; }

        private void BeginRecording()
        {
            if (_recordingState == State.Executing)
            {
                throw new VeldridException("Cannot record new commands into CommandBuffer while it is executing.");
            }

            if (_recordingState == State.Disposed)
            {
                throw new VeldridException("Cannot record new commands into a disposed CommandBuffer.");
            }

            if (_recordingState != State.Recording)
            {
                Debug.Assert(_recordingState == State.Initial);
                _recordingState = State.Recording;
                using (NSAutoreleasePool.Begin())
                {
                    _cb = _gd.CommandQueue.commandBuffer();
                    ObjectiveCRuntime.retain(_cb.NativePtr);
                }
            }
        }

        public MetalBindings.MTLCommandBuffer PrepareForSubmission()
        {
            _recordingState = State.Executing;
            var ret = _cb;
            _cb = default;
            return ret;
        }

        internal override void BeginRenderPassCore(in RenderPassDescription rpd)
        {
            BeginRecording();

            MTLFramebufferBase mtlFB = Util.AssertSubtype<Framebuffer, MTLFramebufferBase>(rpd.Framebuffer);
            MTLRenderPassDescriptor descriptor = mtlFB.CreateRenderPassDescriptor(rpd);
            _rce = _cb.renderCommandEncoderWithDescriptor(descriptor);
            _framebuffer = rpd.Framebuffer;
        }

        private protected override void BindVertexBufferCore(uint index, DeviceBuffer buffer, uint offset)
        {
            BeginRecording();
            Util.EnsureArrayMinimumSize(ref _vertexBuffers, index + 1);
            Util.EnsureArrayMinimumSize(ref _vbOffsets, index + 1);
            Util.EnsureArrayMinimumSize(ref _vertexBuffersActive, index + 1);
            if (_vertexBuffers[index] != buffer || _vbOffsets[index] != offset)
            {
                MTLBuffer mtlBuffer = Util.AssertSubtype<DeviceBuffer, MTLBuffer>(buffer);
                _vertexBuffers[index] = mtlBuffer;
                _vbOffsets[index] = offset;
                _vertexBuffersActive[index] = false;
            }
        }

        private protected override void BindIndexBufferCore(DeviceBuffer buffer, IndexFormat format, uint offset)
        {
            BeginRecording();
            _indexBuffer = Util.AssertSubtype<DeviceBuffer, MTLBuffer>(buffer);
            _indexFormat = format;
            _indexBufferOffset = offset;
        }

        private protected override void BindPipelineCore(Pipeline pipeline)
        {
            BeginRecording();
            if (pipeline.IsComputePipeline)
            {
                throw new NotImplementedException();
            }
            else
            {
                _graphicsPipeline = Util.AssertSubtype<Pipeline, MTLPipeline>(pipeline);
                _graphicsResourceSetCount = (uint)_graphicsPipeline.ResourceLayouts.Length;
                Util.EnsureArrayMinimumSize(ref _graphicsResourceSets, _graphicsResourceSetCount);
                Util.EnsureArrayMinimumSize(ref _graphicsResourceSetsActive, _graphicsResourceSetCount);
                Util.ClearArray(_graphicsResourceSetsActive);

                _nonVertexBufferCount = _graphicsPipeline.NonVertexBufferCount;

                _vertexBufferCount = _graphicsPipeline.VertexBufferCount;
                Util.EnsureArrayMinimumSize(ref _vertexBuffers, _vertexBufferCount);
                Util.EnsureArrayMinimumSize(ref _vbOffsets, _vertexBufferCount);
                Util.EnsureArrayMinimumSize(ref _vertexBuffersActive, _vertexBufferCount);
                Util.ClearArray(_vertexBuffersActive);
                _graphicsPipelineChanged = true;
            }
        }

        private protected override void BindGraphicsResourceSetCore(uint slot, ResourceSet rs, Span<uint> dynamicOffsets)
        {
            BeginRecording();
            if (!_graphicsResourceSets[slot].Equals(rs, dynamicOffsets))
            {
                _graphicsResourceSets[slot].Offsets.Dispose();
                _graphicsResourceSets[slot] = new BoundResourceSetInfo(rs, dynamicOffsets);
                _graphicsResourceSetsActive[slot] = false;
            }
        }

        private void PreDrawCommand()
        {
            if (_viewportsChanged)
            {
                FlushViewports();
                _viewportsChanged = false;
            }
            if (_scissorRectsChanged && _graphicsPipeline.ScissorTestEnabled)
            {
                FlushScissorRects();
                _scissorRectsChanged = false;
            }
            if (_graphicsPipelineChanged)
            {
                Debug.Assert(_graphicsPipeline != null);
                _rce.setRenderPipelineState(_graphicsPipeline.RenderPipelineState);
                _rce.setCullMode(_graphicsPipeline.CullMode);
                _rce.setFrontFacing(_graphicsPipeline.FrontFace);
                _rce.setTriangleFillMode(_graphicsPipeline.FillMode);
                RgbaFloat blendColor = _graphicsPipeline.BlendColor;
                _rce.setBlendColor(blendColor.R, blendColor.G, blendColor.B, blendColor.A);
                if (_framebuffer.DepthTarget != null)
                {
                    _rce.setDepthStencilState(_graphicsPipeline.DepthStencilState);
                    _rce.setDepthClipMode(_graphicsPipeline.DepthClipMode);
                    _rce.setStencilReferenceValue(_graphicsPipeline.StencilReference);
                }
            }

            for (uint i = 0; i < _graphicsResourceSetCount; i++)
            {
                if (!_graphicsResourceSetsActive[i])
                {
                    ActivateGraphicsResourceSet(i, _graphicsResourceSets[i]);
                    _graphicsResourceSetsActive[i] = true;
                }
            }

            for (uint i = 0; i < _vertexBufferCount; i++)
            {
                if (!_vertexBuffersActive[i])
                {
                    UIntPtr index = (UIntPtr)(_graphicsPipeline.ResourceBindingModel == ResourceBindingModel.Improved
                        ? _nonVertexBufferCount + i
                        : i);
                    _rce.setVertexBuffer(
                        _vertexBuffers[i].DeviceBuffer,
                        (UIntPtr)_vbOffsets[i],
                        index);
                }
            }
        }

        internal void ExecutionCompleted()
        {
            _recordingState = State.Initial;
        }

        private void ActivateGraphicsResourceSet(uint slot, BoundResourceSetInfo brsi)
        {
            Debug.Assert(RenderEncoderActive);
            MTLResourceSet mtlRS = Util.AssertSubtype<ResourceSet, MTLResourceSet>(brsi.Set);
            MTLResourceLayout layout = mtlRS.Layout;
            uint dynamicOffsetIndex = 0;

            for (int i = 0; i < mtlRS.Resources.Length; i++)
            {
                MTLResourceLayout.ResourceBindingInfo bindingInfo = layout.GetBindingInfo(i);
                BindableResource resource = mtlRS.Resources[i];
                uint bufferOffset = 0;
                if (bindingInfo.DynamicBuffer)
                {
                    bufferOffset = brsi.Offsets.Get(dynamicOffsetIndex);
                    dynamicOffsetIndex += 1;
                }
                switch (bindingInfo.Kind)
                {
                    case ResourceKind.UniformBuffer:
                        {
                            DeviceBufferRange range = Util.GetBufferRange(resource, bufferOffset);
                            BindBuffer(range, slot, bindingInfo.Slot, bindingInfo.Stages);
                            break;
                        }
                    case ResourceKind.TextureReadOnly:
                        TextureView texView = Util.GetTextureView(_gd, resource);
                        MTLTextureView mtlTexView = Util.AssertSubtype<TextureView, MTLTextureView>(texView);
                        BindTexture(mtlTexView, slot, bindingInfo.Slot, bindingInfo.Stages);
                        break;
                    case ResourceKind.TextureReadWrite:
                        TextureView texViewRW = Util.GetTextureView(_gd, resource);
                        MTLTextureView mtlTexViewRW = Util.AssertSubtype<TextureView, MTLTextureView>(texViewRW);
                        BindTexture(mtlTexViewRW, slot, bindingInfo.Slot, bindingInfo.Stages);
                        break;
                    case ResourceKind.Sampler:
                        MTLSampler mtlSampler = Util.AssertSubtype<BindableResource, MTLSampler>(resource);
                        BindSampler(mtlSampler, slot, bindingInfo.Slot, bindingInfo.Stages);
                        break;
                    case ResourceKind.StructuredBufferReadOnly:
                        {
                            DeviceBufferRange range = Util.GetBufferRange(resource, bufferOffset);
                            BindBuffer(range, slot, bindingInfo.Slot, bindingInfo.Stages);
                            break;
                        }
                    case ResourceKind.StructuredBufferReadWrite:
                        {
                            DeviceBufferRange range = Util.GetBufferRange(resource, bufferOffset);
                            BindBuffer(range, slot, bindingInfo.Slot, bindingInfo.Stages);
                            break;
                        }
                    default:
                        throw Illegal.Value<ResourceKind>();
                }
            }
        }

        private void BindBuffer(DeviceBufferRange range, uint set, uint slot, ShaderStages stages)
        {
            MTLBuffer mtlBuffer = Util.AssertSubtype<DeviceBuffer, MTLBuffer>(range.Buffer);
            uint baseBuffer = GetBufferBase(set, stages != ShaderStages.Compute);
            if (stages == ShaderStages.Compute)
            {
                _cce.setBuffer(mtlBuffer.DeviceBuffer, (UIntPtr)range.Offset, (UIntPtr)(slot + baseBuffer));
            }
            else
            {
                if ((stages & ShaderStages.Vertex) == ShaderStages.Vertex)
                {
                    UIntPtr index = (UIntPtr)(_graphicsPipeline.ResourceBindingModel == ResourceBindingModel.Improved
                        ? slot + baseBuffer
                        : slot + _vertexBufferCount + baseBuffer);
                    _rce.setVertexBuffer(mtlBuffer.DeviceBuffer, (UIntPtr)range.Offset, index);
                }
                if ((stages & ShaderStages.Fragment) == ShaderStages.Fragment)
                {
                    _rce.setFragmentBuffer(mtlBuffer.DeviceBuffer, (UIntPtr)range.Offset, (UIntPtr)(slot + baseBuffer));
                }
            }
        }

        private void BindTexture(MTLTextureView mtlTexView, uint set, uint slot, ShaderStages stages)
        {
            uint baseTexture = GetTextureBase(set, stages != ShaderStages.Compute);
            if (stages == ShaderStages.Compute)
            {
                _cce.setTexture(mtlTexView.TargetDeviceTexture, (UIntPtr)(slot + baseTexture));
            }
            if ((stages & ShaderStages.Vertex) == ShaderStages.Vertex)
            {
                _rce.setVertexTexture(mtlTexView.TargetDeviceTexture, (UIntPtr)(slot + baseTexture));
            }
            if ((stages & ShaderStages.Fragment) == ShaderStages.Fragment)
            {
                _rce.setFragmentTexture(mtlTexView.TargetDeviceTexture, (UIntPtr)(slot + baseTexture));
            }
        }

        private void BindSampler(MTLSampler mtlSampler, uint set, uint slot, ShaderStages stages)
        {
            uint baseSampler = GetSamplerBase(set, stages != ShaderStages.Compute);
            if (stages == ShaderStages.Compute)
            {
                _cce.setSamplerState(mtlSampler.DeviceSampler, (UIntPtr)(slot + baseSampler));
            }
            if ((stages & ShaderStages.Vertex) == ShaderStages.Vertex)
            {
                _rce.setVertexSamplerState(mtlSampler.DeviceSampler, (UIntPtr)(slot + baseSampler));
            }
            if ((stages & ShaderStages.Fragment) == ShaderStages.Fragment)
            {
                _rce.setFragmentSamplerState(mtlSampler.DeviceSampler, (UIntPtr)(slot + baseSampler));
            }
        }

        private uint GetBufferBase(uint set, bool graphics)
        {
            MTLResourceLayout[] layouts = graphics ? _graphicsPipeline.ResourceLayouts : _computePipeline.ResourceLayouts;
            uint ret = 0;
            for (int i = 0; i < set; i++)
            {
                Debug.Assert(layouts[i] != null);
                ret += layouts[i].BufferCount;
            }

            return ret;
        }

        private uint GetTextureBase(uint set, bool graphics)
        {
            MTLResourceLayout[] layouts = graphics ? _graphicsPipeline.ResourceLayouts : _computePipeline.ResourceLayouts;
            uint ret = 0;
            for (int i = 0; i < set; i++)
            {
                Debug.Assert(layouts[i] != null);
                ret += layouts[i].TextureCount;
            }

            return ret;
        }

        private uint GetSamplerBase(uint set, bool graphics)
        {
            MTLResourceLayout[] layouts = graphics ? _graphicsPipeline.ResourceLayouts : _computePipeline.ResourceLayouts;
            uint ret = 0;
            for (int i = 0; i < set; i++)
            {
                Debug.Assert(layouts[i] != null);
                ret += layouts[i].SamplerCount;
            }

            return ret;
        }

        private void FlushViewports()
        {
            if (_gd.MetalFeatures.IsSupported(MTLFeatureSet.macOS_GPUFamily1_v3))
            {
                fixed (MTLViewport* viewportsPtr = &_viewports[0])
                {
                    _rce.setViewports(viewportsPtr, (UIntPtr)_viewportCount);
                }
            }
            else
            {
                _rce.setViewport(_viewports[0]);
            }
        }

        private void FlushScissorRects()
        {
            if (_gd.MetalFeatures.IsSupported(MTLFeatureSet.macOS_GPUFamily1_v3))
            {
                fixed (MTLScissorRect* scissorRectsPtr = &_scissorRects[0])
                {
                    _rce.setScissorRects(scissorRectsPtr, (UIntPtr)_viewportCount);
                }
            }
            else
            {
                _rce.setScissorRect(_scissorRects[0]);
            }
        }

        private protected override void DrawCore(uint vertexCount, uint instanceCount, uint vertexStart, uint instanceStart)
        {
            PreDrawCommand();
            _rce.drawPrimitives(
                _graphicsPipeline.PrimitiveType,
                (UIntPtr)vertexStart,
                (UIntPtr)vertexCount,
                (UIntPtr)instanceCount,
                (UIntPtr)instanceStart);
        }

        private protected override void DrawIndirectCore(DeviceBuffer indirectBuffer, uint offset, uint drawCount, uint stride)
        {
            PreDrawCommand();
            MTLBuffer mtlIndirectBuff = Util.AssertSubtype<DeviceBuffer, MTLBuffer>(indirectBuffer);
            for (uint i = 0; i < drawCount; i++)
            {
                uint currentOffset = i * stride + offset;
                _rce.drawPrimitives(
                    _graphicsPipeline.PrimitiveType,
                    mtlIndirectBuff.DeviceBuffer,
                    (UIntPtr)currentOffset);
            }
        }

        private protected override void DrawIndexedCore(uint indexCount, uint instanceCount, uint indexStart, int vertexOffset, uint instanceStart)
        {
            PreDrawCommand();
            _rce.drawIndexedPrimitives(
                _graphicsPipeline.PrimitiveType,
                (UIntPtr)indexCount,
                MTLFormats.VdToMTLIndexFormat(_indexFormat),
                _indexBuffer.DeviceBuffer,
                (UIntPtr)_indexBufferOffset,
                (UIntPtr)instanceCount);
        }

        private protected override void DrawIndexedIndirectCore(DeviceBuffer indirectBuffer, uint offset, uint drawCount, uint stride)
        {
            PreDrawCommand();
            MTLBuffer mtlIndirectBuff = Util.AssertSubtype<DeviceBuffer, MTLBuffer>(indirectBuffer);
            for (uint i = 0; i < drawCount; i++)
            {
                uint currentOffset = i * stride + offset;
                _rce.drawIndexedPrimitives(
                    _graphicsPipeline.PrimitiveType,
                    MTLFormats.VdToMTLIndexFormat(_indexFormat),
                    _indexBuffer.DeviceBuffer,
                    (UIntPtr)_indexBufferOffset,
                    mtlIndirectBuff.DeviceBuffer,
                    (UIntPtr)currentOffset);
            }
        }

        private protected override void SetViewportCore(uint index, Viewport viewport)
        {
            _viewportsChanged = true;
            _viewports[index] = new MTLViewport(
                viewport.X,
                viewport.Y,
                viewport.Width,
                viewport.Height,
                viewport.MinDepth,
                viewport.MaxDepth);
        }

        private protected override void SetScissorRectCore(uint index, uint x, uint y, uint width, uint height)
        {
            _scissorRectsChanged = true;
            _scissorRects[index] = new MTLScissorRect(x, y, width, height);
        }

        private protected override void EndRenderPassCore()
        {
            _rce.endEncoding();
            ObjectiveCRuntime.release(_rce.NativePtr);
            _rce = default;
        }

        private protected override void MemoryBarrierCore(ShaderStages sourceStage, ShaderStages destinationStage)
        {
            BeginRecording();
            throw new NotImplementedException();
        }

        private protected override void MemoryBarrierCore(Texture texture, uint baseMipLevel, uint levelCount, uint baseArrayLayer, uint layerCount, ShaderStages sourceStage, ShaderStages destinationStage)
        {
            BeginRecording();
            throw new NotImplementedException();
        }

        private protected override void BindComputeResourceSetCore(uint slot, ResourceSet resourceSet, Span<uint> dynamicOffsets)
        {
            throw new NotImplementedException();
        }

        private protected override void DispatchCore(uint groupCountX, uint groupCountY, uint groupCountZ)
        {
            throw new NotImplementedException();
        }

        private protected override void DispatchIndirectCore(DeviceBuffer indirectBuffer, uint offset)
        {
            throw new NotImplementedException();
        }

        private protected override void CopyBufferCore(DeviceBuffer source, uint sourceOffset, DeviceBuffer destination, uint destinationOffset, uint sizeInBytes)
        {
            BeginRecording();
            throw new NotImplementedException();
        }

        private protected override void UpdateBufferCore(DeviceBuffer buffer, uint bufferOffsetInBytes, IntPtr source, uint sizeInBytes)
        {
            BeginRecording();
            throw new NotImplementedException();
        }

        private protected override void CopyTextureCore(Texture source, uint srcX, uint srcY, uint srcZ, uint srcMipLevel, uint srcBaseArrayLayer, Texture destination, uint dstX, uint dstY, uint dstZ, uint dstMipLevel, uint dstBaseArrayLayer, uint width, uint height, uint depth, uint layerCount)
        {
            BeginRecording();
            throw new NotImplementedException();
        }

        private protected override void BlitTextureCore(Texture source, uint srcX, uint srcY, uint srcWidth, uint srcHeight, Framebuffer destination, uint dstX, uint dstY, uint dstWidth, uint dstHeight, bool linearFilter)
        {
            BeginRecording();
            throw new NotImplementedException();
        }

        private protected override void GenerateMipmapsCore(Texture texture)
        {
            BeginRecording();
            throw new NotImplementedException();
        }

        public override void InsertDebugMarker(string name)
        {
            BeginRecording();
            throw new NotImplementedException();
        }

        public override void PushDebugGroup(string name)
        {
            BeginRecording();
            throw new NotImplementedException();
        }

        public override void PopDebugGroup()
        {
            throw new NotImplementedException();
        }

        public override void Dispose()
        {
        }

        private enum State
        {
            Initial,
            Recording,
            Executing,
            Disposed,
        }
    }
}