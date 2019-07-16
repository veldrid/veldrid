using System;
using System.Diagnostics;
using Veldrid.CommandRecording;
using Veldrid.MetalBindings;

namespace Veldrid.MTL
{
    internal unsafe class MTLCommandExecutor : RecordedCommandExecutor
    {
        private readonly MTLGraphicsDevice _gd;
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

        private bool RenderEncoderActive => !_rce.IsNull;
        // private bool BlitEncoderActive => !_bce.IsNull;
        // private bool ComputeEncoderActive => !_cce.IsNull;

        public MTLCommandExecutor(MTLGraphicsDevice gd)
        {
            _gd = gd;
        }

        public void StartExecuting()
        {
            using (NSAutoreleasePool.Begin())
            {
                _cb = _gd.CommandQueue.commandBuffer();
                ObjectiveCRuntime.retain(_cb.NativePtr);
            }
        }

        public void StopExecuting()
        {
            _cb.commit();
            ObjectiveCRuntime.release(_cb.NativePtr);
        }

        public override void Begin()
        {
            throw new NotSupportedException();
        }

        public override void BeginRenderPass(in RenderPassDescription rpd)
        {
            MTLFramebuffer mtlFB = Util.AssertSubtype<Framebuffer, MTLFramebuffer>(rpd.Framebuffer);
            MTLRenderPassDescriptor descriptor = mtlFB.CreateRenderPassDescriptor(rpd);
            _rce = _cb.renderCommandEncoderWithDescriptor(descriptor);
            _framebuffer = rpd.Framebuffer;
        }

        public override void EndRenderPass()
        {
            _rce.endEncoding();
            ObjectiveCRuntime.release(_rce.NativePtr);
            _rce = default;
        }

        public override void BlitTexture(Texture source, uint srcX, uint srcY, uint srcWidth, uint srcHeight, Framebuffer destination, uint dstX, uint dstY, uint dstWidth, uint dstHeight, bool linearFilter)
        {
            throw new NotImplementedException();
        }

        public override void ClearColorTarget(uint index, RgbaFloat clearColor)
        {
            throw new NotSupportedException();
        }

        public override void ClearDepthStencil(float depth, byte stencil)
        {
            throw new NotSupportedException();
        }

        public override void CopyBuffer(DeviceBuffer source, uint sourceOffset, DeviceBuffer destination, uint destinationOffset, uint sizeInBytes)
        {
            throw new NotImplementedException();
        }

        public override void CopyTexture(Texture source, uint srcX, uint srcY, uint srcZ, uint srcMipLevel, uint srcBaseArrayLayer, Texture destination, uint dstX, uint dstY, uint dstZ, uint dstMipLevel, uint dstBaseArrayLayer, uint width, uint height, uint depth, uint layerCount)
        {
            throw new NotImplementedException();
        }

        public override void Dispatch(uint groupCountX, uint groupCountY, uint groupCountZ)
        {
            throw new NotImplementedException();
        }

        public override void DispatchIndirect(DeviceBuffer indirectBuffer, uint offset)
        {
            throw new NotImplementedException();
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

        public override void Draw(uint vertexCount, uint instanceCount, uint vertexStart, uint instanceStart)
        {
            _rce.drawPrimitives(
                _graphicsPipeline.PrimitiveType,
                (UIntPtr)vertexStart,
                (UIntPtr)vertexCount,
                (UIntPtr)instanceCount,
                (UIntPtr)instanceStart);
        }

        public override void DrawIndexed(uint indexCount, uint instanceCount, uint indexStart, int vertexOffset, uint instanceStart)
        {
            _rce.drawIndexedPrimitives(
                _graphicsPipeline.PrimitiveType,
                (UIntPtr)indexCount,
                MTLFormats.VdToMTLIndexFormat(_indexFormat),
                _indexBuffer.DeviceBuffer,
                (UIntPtr)_indexBufferOffset,
                (UIntPtr)instanceCount);
        }

        public override void DrawIndexedIndirect(DeviceBuffer indirectBuffer, uint offset, uint drawCount, uint stride)
        {
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

        public override void DrawIndirect(DeviceBuffer indirectBuffer, uint offset, uint drawCount, uint stride)
        {
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

        public override void End()
        {
            throw new NotSupportedException();
        }

        public override void GenerateMipmaps(Texture texture)
        {
            throw new NotImplementedException();
        }

        public override void InsertDebugMarker(string name)
        {
            throw new NotImplementedException();
        }

        public override void MemoryBarrier(Texture texture, uint baseMipLevel, uint levelCount, uint baseArrayLayer, uint layerCount, ShaderStages sourceStage, ShaderStages destinationStage)
        {
            throw new NotImplementedException();
        }

        public override void PopDebugGroup()
        {
            throw new NotImplementedException();
        }

        public override void PushDebugGroup(string name)
        {
            throw new NotImplementedException();
        }

        public override void ResolveTexture(Texture source, Texture destination)
        {
            throw new NotImplementedException();
        }

        public override void SetComputeResourceSet(uint slot, ResourceSet rs, uint dynamicOffsetCount, ref uint dynamicOffsets)
        {
            throw new NotImplementedException();
        }

        public override void SetFramebuffer(Framebuffer fb)
        {
            throw new NotImplementedException();
        }

        public override void SetGraphicsResourceSet(
            uint slot,
            ResourceSet rs,
            uint dynamicOffsetCount,
            ref uint dynamicOffsets)
        {
            if (!_graphicsResourceSets[slot].Equals(rs, dynamicOffsetCount, ref dynamicOffsets))
            {
                _graphicsResourceSets[slot].Offsets.Dispose();
                _graphicsResourceSets[slot] = new BoundResourceSetInfo(rs, dynamicOffsetCount, ref dynamicOffsets);
                _graphicsResourceSetsActive[slot] = false;
            }
        }

        public override void SetIndexBuffer(DeviceBuffer ib, IndexFormat format, uint offset)
        {
            _indexBuffer = Util.AssertSubtype<DeviceBuffer, MTLBuffer>(ib);
            _indexFormat = format;
            _indexBufferOffset = offset;
        }

        public override void SetPipeline(Pipeline pipeline)
        {
            MTLPipeline mtlPipeline = Util.AssertSubtype<Pipeline, MTLPipeline>(pipeline);
            if (pipeline.IsComputePipeline)
            {
                throw new NotImplementedException();
            }
            else
            {
                _rce.setRenderPipelineState(mtlPipeline.RenderPipelineState);
                _graphicsPipeline = mtlPipeline;
            }
        }

        public override void SetScissorRect(uint index, uint x, uint y, uint width, uint height)
        {
            _scissorRectsChanged = true;
            _scissorRects[index] = new MTLScissorRect(x, y, width, height);
        }

        public override void SetViewport(uint index, ref Viewport viewport)
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

        public override void UpdateBuffer(DeviceBuffer buffer, uint bufferOffsetInBytes, IntPtr dataPtr, uint sizeInBytes)
        {
            throw new NotImplementedException();
        }

        public override void UpdateTexture(Texture texture, IntPtr dataPtr, uint x, uint y, uint z, uint width, uint height, uint depth, uint mipLevel, uint arrayLayer)
        {
            throw new NotImplementedException();
        }

        public override void SetVertexBuffer(uint index, DeviceBuffer buffer, uint offset)
        {
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
    }
}