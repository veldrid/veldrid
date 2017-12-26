using System;
using System.Collections.Generic;
using System.Diagnostics;
using Veldrid.MetalBindings;

namespace Veldrid.MTL
{
    internal unsafe class MTLCommandList : CommandList
    {
        private readonly MTLGraphicsDevice _gd;
        private MTLCommandBuffer _cb;
        private MTLFramebufferBase _mtlFramebuffer;
        private bool _currentFramebufferEverActive;
        private MTLRenderCommandEncoder _rce;
        private MTLBlitCommandEncoder _bce;
        private RgbaFloat?[] _clearColors = Array.Empty<RgbaFloat?>();
        private (float depth, byte stencil)? _depthClear;
        private MTLBuffer _indexBuffer;
        private MTLIndexType _indexType;
        private new MTLPipeline _graphicsPipeline;

        private MTLViewport[] _viewports = Array.Empty<MTLViewport>();
        private bool _viewportsChanged;
        private MTLScissorRect[] _scissorRects = Array.Empty<MTLScissorRect>();
        private bool _scissorRectsChanged;

        public MTLCommandBuffer CommandBuffer => _cb;

        public MTLCommandList(ref CommandListDescription description, MTLGraphicsDevice gd)
            : base(ref description)
        {
            _gd = gd;
            _cb = _gd.CommandQueue.commandBuffer();
        }

        public override string Name
        {
            get => throw new NotImplementedException();
            set => throw new NotImplementedException();
        }

        public void Commit()
        {
            _cb.commit();
            ObjectiveCRuntime.release(_cb.NativePtr);
            _cb = _gd.CommandQueue.commandBuffer();
        }

        public override void Begin()
        {
            ClearCachedState();
        }

        protected override void ClearColorTargetCore(uint index, RgbaFloat clearColor)
        {
            EnsureNoRenderPass();
            _clearColors[index] = clearColor;
        }

        protected override void ClearDepthStencilCore(float depth, byte stencil)
        {
            EnsureNoRenderPass();
            _depthClear = (depth, stencil);
        }

        public override void Dispatch(uint groupCountX, uint groupCountY, uint groupCountZ)
        {
            throw new NotImplementedException();
        }

        protected override void DrawCore(uint vertexCount, uint instanceCount, uint vertexStart, uint instanceStart)
        {
            if (PreDrawCommand())
            {
                throw new NotImplementedException();
            }

        }

        protected override void DrawIndexedCore(uint indexCount, uint instanceCount, uint indexStart, int vertexOffset, uint instanceStart)
        {
            if (PreDrawCommand())
            {
                _rce.drawIndexedPrimitives(
                    _graphicsPipeline.PrimitiveType,
                    (UIntPtr)indexCount,
                    _indexType,
                    _indexBuffer.DeviceBuffer,
                    UIntPtr.Zero,
                    (UIntPtr)instanceCount,
                    (IntPtr)vertexOffset,
                    (UIntPtr)instanceStart);

                // REMOVE
                _gd.CommandQueue.insertDebugCaptureBoundary();
            }
        }
        private bool PreDrawCommand()
        {
            if (EnsureRenderPass())
            {
                if (_viewportsChanged)
                {
                    fixed (MTLViewport* viewportsPtr = &_viewports[0])
                    {
                        _rce.setViewports(viewportsPtr, (UIntPtr)_viewports.Length);
                    }
                    _viewportsChanged = false;
                }
                if (_scissorRectsChanged)
                {
                    fixed (MTLScissorRect* scissorRectsPtr = &_scissorRects[0])
                    {
                        _rce.setScissorRects(scissorRectsPtr, (UIntPtr)_scissorRects.Length);
                    }
                    _scissorRectsChanged = false;
                }
                return true;
            }
            return false;
        }

        public override void End()
        {
            EnsureNoBlitEncoder();

            if (!_currentFramebufferEverActive && _mtlFramebuffer != null)
            {
                BeginCurrentRenderPass();
            }
            EnsureNoRenderPass();
        }

        protected override void SetPipelineCore(Pipeline pipeline)
        {
            if (pipeline.IsComputePipeline)
            {
                throw new NotImplementedException();
            }
            else
            {
                if (EnsureRenderPass())
                {
                    _graphicsPipeline = Util.AssertSubtype<Pipeline, MTLPipeline>(pipeline);
                    _rce.setRenderPipelineState(_graphicsPipeline.RenderPipelineState);
                    _rce.setCullMode(_graphicsPipeline.CullMode);
                    _rce.setFrontFacing(_graphicsPipeline.FrontFace);
                }
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

        public override void UpdateBuffer(DeviceBuffer buffer, uint bufferOffsetInBytes, IntPtr source, uint sizeInBytes)
        {
            MTLBuffer dstMTLBuffer = Util.AssertSubtype<DeviceBuffer, MTLBuffer>(buffer);
            // TODO: Cache these
            MTLBuffer copySrc = Util.AssertSubtype<DeviceBuffer, MTLBuffer>(
                _gd.ResourceFactory.CreateBuffer(new BufferDescription(sizeInBytes, BufferUsage.Staging)));
            _gd.UpdateBuffer(copySrc, 0, source, sizeInBytes);
            EnsureBlitEncoder();
            _bce.copy(
                copySrc.DeviceBuffer, UIntPtr.Zero,
                dstMTLBuffer.DeviceBuffer, (UIntPtr)bufferOffsetInBytes,
                (UIntPtr)sizeInBytes);
            copySrc.Dispose();
        }

        protected override void CopyBufferCore(
            DeviceBuffer source,
            uint sourceOffset,
            DeviceBuffer destination,
            uint destinationOffset,
            uint sizeInBytes)
        {
            EnsureBlitEncoder();
            MTLBuffer mtlSrc = Util.AssertSubtype<DeviceBuffer, MTLBuffer>(source);
            MTLBuffer mtlDst = Util.AssertSubtype<DeviceBuffer, MTLBuffer>(destination);
            _bce.copy(
                mtlSrc.DeviceBuffer, (UIntPtr)sourceOffset,
                mtlDst.DeviceBuffer, (UIntPtr)destinationOffset,
                (UIntPtr)sizeInBytes);
        }

        protected override void CopyTextureCore(
            Texture source, uint srcX, uint srcY, uint srcZ, uint srcMipLevel, uint srcBaseArrayLayer,
            Texture destination, uint dstX, uint dstY, uint dstZ, uint dstMipLevel, uint dstBaseArrayLayer,
            uint width, uint height, uint depth, uint layerCount)
        {
            if (srcX != 0 || srcY != 0 || srcZ != 0 || dstX != 0 || dstY != 0 || dstZ != 0)
            {
                throw new NotImplementedException();
            }
            if ((source.Usage & TextureUsage.Staging) == 0)
            {
                throw new NotImplementedException();
            }

            EnsureBlitEncoder();
            MTLTexture srcMTLTexture = Util.AssertSubtype<Texture, MTLTexture>(source);
            MTLTexture dstMTLTexture = Util.AssertSubtype<Texture, MTLTexture>(destination);

            if ((source.Usage & TextureUsage.Staging) != 0
                && (destination.Usage & TextureUsage.Staging) == 0)
            {
                // Staging -> Normal
                MetalBindings.MTLBuffer srcBuffer = srcMTLTexture.StagingBuffer;
                MetalBindings.MTLTexture dstTexture = dstMTLTexture.DeviceTexture;

                uint pixelSize = FormatHelpers.GetSizeInBytes(srcMTLTexture.Format);

                for (uint layer = 0; layer < layerCount; layer++)
                {
                    uint sourceOffset = srcMTLTexture.GetSubresourceOffset(srcMipLevel, layer + srcBaseArrayLayer);
                    uint sourceBytesPerRow = width * pixelSize;
                    uint sourceBytesPerImage = width * height * pixelSize;
                    MTLSize sourceSize = new MTLSize(width, height, depth);
                    _bce.copyFromBuffer(
                        srcBuffer,
                        (UIntPtr)sourceOffset,
                        (UIntPtr)sourceBytesPerRow,
                        (UIntPtr)sourceBytesPerImage,
                        sourceSize,
                        dstTexture,
                        (UIntPtr)(dstBaseArrayLayer + layer),
                        (UIntPtr)dstMipLevel,
                        new MTLOrigin(0, 0, 0));
                }
            }
            else if ((source.Usage & TextureUsage.Staging) != 0
                && (destination.Usage & TextureUsage.Staging) != 0)
            {
                // Staging -> Staging
                uint srcOffset = srcMTLTexture.GetSubresourceOffset(srcMipLevel, srcBaseArrayLayer);
                uint dstOffset = dstMTLTexture.GetSubresourceOffset(dstMipLevel, dstBaseArrayLayer);
                uint copySize = width * height * depth * FormatHelpers.GetSizeInBytes(source.Format);
                _bce.copy(
                    srcMTLTexture.StagingBuffer,
                    (UIntPtr)srcOffset,
                    dstMTLTexture.StagingBuffer,
                    (UIntPtr)dstOffset,
                    (UIntPtr)copySize);
            }
        }

        protected override void DispatchIndirectCore(DeviceBuffer indirectBuffer, uint offset)
        {
            throw new NotImplementedException();
        }

        protected override void DrawIndexedIndirectCore(DeviceBuffer indirectBuffer, uint offset, uint drawCount, uint stride)
        {
            if (PreDrawCommand())
            {
                throw new NotImplementedException();
            }
        }

        protected override void DrawIndirectCore(DeviceBuffer indirectBuffer, uint offset, uint drawCount, uint stride)
        {
            if (PreDrawCommand())
            {
                throw new NotImplementedException();
            }
        }

        protected override void ResolveTextureCore(Texture source, Texture destination)
        {
            throw new NotImplementedException();
        }

        protected override void SetComputeResourceSetCore(uint slot, ResourceSet set)
        {
            throw new NotImplementedException();
        }

        protected override void SetFramebufferCore(Framebuffer fb)
        {
            if (!_currentFramebufferEverActive && _mtlFramebuffer != null)
            {
                BeginCurrentRenderPass();
                EndCurrentRenderPass();
            }

            EnsureNoRenderPass();
            _mtlFramebuffer = Util.AssertSubtype<Framebuffer, MTLFramebufferBase>(fb);
            Util.EnsureArrayMinimumSize(ref _viewports, (uint)fb.ColorTargets.Count);
            Util.ClearArray(_viewports);
            Util.EnsureArrayMinimumSize(ref _scissorRects, (uint)fb.ColorTargets.Count);
            Util.ClearArray(_scissorRects);
            Util.EnsureArrayMinimumSize(ref _clearColors, (uint)fb.ColorTargets.Count);
            Util.ClearArray(_clearColors);
            _currentFramebufferEverActive = false;
        }

        protected override void SetGraphicsResourceSetCore(uint slot, ResourceSet rs)
        {
            if (EnsureRenderPass())
            {
                MTLResourceSet mtlRS = Util.AssertSubtype<ResourceSet, MTLResourceSet>(rs);
                MTLResourceLayout layout = mtlRS.Layout;

                for (int i = 0; i < mtlRS.Resources.Length; i++)
                {
                    var bindingInfo = layout.GetBindingInfo(i);
                    var resource = mtlRS.Resources[i];
                    switch (bindingInfo.Kind)
                    {
                        case ResourceKind.UniformBuffer:
                            MTLBuffer mtlBuffer = Util.AssertSubtype<BindableResource, MTLBuffer>(resource);
                            BindBuffer(mtlBuffer, slot, bindingInfo.Slot, bindingInfo.Stages);
                            break;
                        case ResourceKind.TextureReadOnly:
                            MTLTextureView mtlTexView = Util.AssertSubtype<BindableResource, MTLTextureView>(resource);
                            BindTexture(mtlTexView, slot, bindingInfo.Slot, bindingInfo.Stages);
                            break;
                        case ResourceKind.Sampler:
                            MTLSampler mtlSampler = Util.AssertSubtype<BindableResource, MTLSampler>(resource);
                            BindSampler(mtlSampler, slot, bindingInfo.Slot, bindingInfo.Stages);
                            break;
                        default:
                            throw new NotImplementedException();
                    }
                }
            }
        }

        private void BindBuffer(MTLBuffer mtlBuffer, uint set, uint slot, ShaderStages stages)
        {
            uint vertexBufferCount = _graphicsPipeline.VertexBufferCount;
            uint baseBuffer = GetBufferBase(set, true);
            if ((stages & ShaderStages.Vertex) == ShaderStages.Vertex)
            {
                _rce.setVertexBuffer(mtlBuffer.DeviceBuffer, UIntPtr.Zero, (UIntPtr)(slot + vertexBufferCount + baseBuffer));
            }
            if ((stages & ShaderStages.Fragment) == ShaderStages.Fragment)
            {
                _rce.setFragmentBuffer(mtlBuffer.DeviceBuffer, UIntPtr.Zero, (UIntPtr)(slot + baseBuffer));
            }
        }

        private void BindTexture(MTLTextureView mtlTexView, uint set, uint slot, ShaderStages stages)
        {
            uint baseTexture = GetTextureBase(set, true);
            if ((stages & ShaderStages.Vertex) == ShaderStages.Vertex)
            {
                _rce.setVertexTexture(mtlTexView.TargetMTLTexture.DeviceTexture, (UIntPtr)(slot + baseTexture));
            }
            if ((stages & ShaderStages.Fragment) == ShaderStages.Fragment)
            {
                _rce.setFragmentTexture(mtlTexView.TargetMTLTexture.DeviceTexture, (UIntPtr)(slot + baseTexture));
            }
        }

        private void BindSampler(MTLSampler mtlSampler, uint set, uint slot, ShaderStages stages)
        {
            uint baseSampler = GetSamplerBase(set, true);
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
            MTLResourceLayout[] layouts = graphics ? _graphicsPipeline.ResourceLayouts : throw new NotImplementedException();
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
            MTLResourceLayout[] layouts = graphics ? _graphicsPipeline.ResourceLayouts : throw new NotImplementedException();
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
            MTLResourceLayout[] layouts = graphics ? _graphicsPipeline.ResourceLayouts : throw new NotImplementedException();
            uint ret = 0;
            for (int i = 0; i < set; i++)
            {
                Debug.Assert(layouts[i] != null);
                ret += layouts[i].SamplerCount;
            }

            return ret;
        }

        private bool EnsureRenderPass()
        {
            Debug.Assert(_mtlFramebuffer != null);
            return RenderEncoderActive || BeginCurrentRenderPass();
        }

        private bool RenderEncoderActive => !_rce.IsNull;
        private bool BlitEncoderActive => !_bce.IsNull;

        private bool BeginCurrentRenderPass()
        {
            if (!_mtlFramebuffer.IsRenderable)
            {
                return false;
            }

            var rpDesc = _mtlFramebuffer.CreateRenderPassDescriptor();
            for (uint i = 0; i < _clearColors.Length; i++)
            {
                if (_clearColors[i] != null)
                {
                    var attachment = rpDesc.colorAttachments[0];
                    attachment.loadAction = MTLLoadAction.Clear;
                    RgbaFloat c = _clearColors[i].Value;
                    attachment.clearColor = new MTLClearColor(c.R, c.G, c.B, c.A);
                }
            }

            if (_depthClear != null)
            {
                var depthAttachment = rpDesc.depthAttachment;
                depthAttachment.clearDepth = _depthClear.Value.depth;
            }

            _rce = _cb.renderCommandEncoderWithDescriptor(rpDesc);
            ObjectiveCRuntime.release(rpDesc.NativePtr);
            _currentFramebufferEverActive = true;

            return true;
        }

        private void EnsureNoRenderPass()
        {
            EnsureNoBlitEncoder();
            if (RenderEncoderActive)
            {
                EndCurrentRenderPass();
            }
        }

        private void EndCurrentRenderPass()
        {
            _rce.endEncoding();
            ObjectiveCRuntime.release(_rce.NativePtr);
            _rce = default(MTLRenderCommandEncoder);
        }

        private void EnsureBlitEncoder()
        {
            EnsureNoRenderPass();
            _bce = _cb.blitCommandEncoder();
        }

        private void EnsureNoBlitEncoder()
        {
            if (BlitEncoderActive)
            {
                _bce.endEncoding();
                ObjectiveCRuntime.release(_bce.NativePtr);
                _bce = default(MTLBlitCommandEncoder);
            }
        }

        protected override void SetIndexBufferCore(DeviceBuffer buffer, IndexFormat format)
        {
            _indexBuffer = Util.AssertSubtype<DeviceBuffer, MTLBuffer>(buffer);
            _indexType = MTLFormats.VdToMTLIndexFormat(format);
        }

        protected override void SetVertexBufferCore(uint index, DeviceBuffer buffer)
        {
            if (EnsureRenderPass())
            {
                var mtlBuffer = Util.AssertSubtype<DeviceBuffer, MTLBuffer>(buffer);
                _rce.setVertexBuffer(mtlBuffer.DeviceBuffer, UIntPtr.Zero, (UIntPtr)index);
            }
        }

        public override void Dispose()
        {
            EnsureNoRenderPass();
            ObjectiveCRuntime.release(_cb.NativePtr);
        }
    }
}