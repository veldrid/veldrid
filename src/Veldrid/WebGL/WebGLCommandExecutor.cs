using System;
using System.Text;
using WebGLDotNET;
using static Veldrid.WebGL.WebGLConstants;
using static Veldrid.WebGL.WebGLUtil;

namespace Veldrid.WebGL
{
    internal unsafe class WebGLCommandExecutor : OpenGL.GLCommandExecutor
    {
        private readonly WebGLGraphicsDevice _gd;
        private readonly WebGL2RenderingContext _ctx;
        private readonly GraphicsBackend _backend;
        private readonly WebGLTextureSamplerManager _textureSamplerManager;
        private readonly StagingMemoryPool _stagingMemoryPool;
        private readonly GraphicsDeviceFeatures _features;

        private Framebuffer _fb;
        private bool _isSwapchainFB;
        private WebGLPipeline _graphicsPipeline;
        private BoundResourceSetInfo[] _graphicsResourceSets = Array.Empty<BoundResourceSetInfo>();
        private bool[] _newGraphicsResourceSets = Array.Empty<bool>();
        private WebGLBuffer[] _vertexBuffers = Array.Empty<WebGLBuffer>();
        private uint[] _vbOffsets = Array.Empty<uint>();
        private uint[] _vertexAttribDivisors = Array.Empty<uint>();
        private uint _vertexAttributesBound;
        private readonly Viewport[] _viewports = new Viewport[20];
        private uint _drawElementsType;
        private uint _ibOffset;
        private uint _primitiveType;

        private WebGLPipeline _computePipeline;
        private BoundResourceSetInfo[] _computeResourceSets = Array.Empty<BoundResourceSetInfo>();
        private bool[] _newComputeResourceSets = Array.Empty<bool>();

        private bool _graphicsPipelineActive;

        public WebGLCommandExecutor(WebGLGraphicsDevice gd)
        {
            _gd = gd;
            _ctx = gd.Ctx;
            _backend = gd.BackendType;
            _textureSamplerManager = gd.TextureSamplerManager;
            _features = gd.Features;
        }

        public override void Begin()
        {
        }

        public override void ClearColorTarget(uint index, RgbaFloat clearColor)
        {
            throw new NotImplementedException();
        }

        public override void ClearDepthStencil(float depth, byte stencil)
        {
            throw new NotImplementedException();
        }

        public override void Draw(uint vertexCount, uint instanceCount, uint vertexStart, uint instanceStart)
        {
            PreDrawCommand();

            if (instanceCount == 1 && instanceStart == 0)
            {
                _ctx.DrawArrays(_primitiveType, (int)vertexStart, (int)vertexCount);
                _gd.CheckError();
            }
            else
            {
                if (instanceStart == 0)
                {
                    _ctx.DrawArraysInstanced(_primitiveType, (int)vertexStart, (int)vertexCount, (int)instanceCount);
                    _gd.CheckError();
                }
                else
                {
                    throw new NotSupportedException();
                }
            }
        }

        public override void DrawIndexed(uint indexCount, uint instanceCount, uint indexStart, int vertexOffset, uint instanceStart)
        {
            PreDrawCommand();

            uint indexSize = _drawElementsType == UNSIGNED_SHORT ? 2u : 4u;
            void* indices = (void*)((indexStart * indexSize) + _ibOffset);

            if (instanceCount == 1 && instanceStart == 0)
            {
                if (vertexOffset == 0)
                {
                    _ctx.DrawElements(_primitiveType, (int)indexCount, _drawElementsType, (uint)indices);
                    _gd.CheckError();
                }
                else
                {
                    throw new NotSupportedException();
                }
            }
            else
            {
                if (instanceStart > 0)
                {
                    throw new NotSupportedException();
                }
                else if (vertexOffset == 0)
                {
                    _ctx.DrawElementsInstanced(_primitiveType, (int)indexCount, _drawElementsType, (uint)indices, (int)instanceCount);
                    _gd.CheckError();
                }
                else
                {
                    throw new NotSupportedException();
                }
            }
        }

        public override void DrawIndirect(DeviceBuffer indirectBuffer, uint offset, uint drawCount, uint stride)
        {
            throw new NotSupportedException();
        }

        public override void DrawIndexedIndirect(DeviceBuffer indirectBuffer, uint offset, uint drawCount, uint stride)
        {
            throw new NotSupportedException();
        }

        private void PreDrawCommand()
        {
            if (!_graphicsPipelineActive)
            {
                ActivateGraphicsPipeline();
            }

            FlushResourceSets(graphics: true);
            FlushVertexLayouts();
        }

        private void FlushResourceSets(bool graphics)
        {
            uint sets = graphics
                ? (uint)_graphicsPipeline.ResourceLayouts.Length
                : (uint)_computePipeline.ResourceLayouts.Length;
            for (uint slot = 0; slot < sets; slot++)
            {
                BoundResourceSetInfo brsi = graphics ? _graphicsResourceSets[slot] : _computeResourceSets[slot];
                WebGLResourceSet glSet = Util.AssertSubtype<ResourceSet, WebGLResourceSet>(brsi.Set);
                ResourceLayoutElementDescription[] layoutElements = glSet.Layout.Elements;
                bool isNew = graphics ? _newGraphicsResourceSets[slot] : _newComputeResourceSets[slot];

                ActivateResourceSet(slot, graphics, brsi, layoutElements, isNew);
            }

            Util.ClearArray(graphics ? _newGraphicsResourceSets : _newComputeResourceSets);
        }

        private void FlushVertexLayouts()
        {
            uint totalSlotsBound = 0;
            VertexLayoutDescription[] layouts = _graphicsPipeline.VertexLayouts;
            for (int i = 0; i < layouts.Length; i++)
            {
                VertexLayoutDescription input = layouts[i];
                WebGLBuffer vb = _vertexBuffers[i];
                _ctx.BindBuffer(ARRAY_BUFFER, vb.WglBuffer);
                _gd.CheckError();
                uint offset = 0;
                uint vbOffset = _vbOffsets[i];
                for (uint slot = 0; slot < input.Elements.Length; slot++)
                {
                    ref VertexElementDescription element = ref input.Elements[slot]; // Large structure -- use by reference.
                    uint actualSlot = totalSlotsBound + slot;
                    if (actualSlot >= _vertexAttributesBound)
                    {
                        _ctx.EnableVertexAttribArray(actualSlot);
                        _gd.CheckError();
                    }
                    uint type = VdToGLVertexAttribPointerType(
                        element.Format,
                        out bool normalized,
                        out bool isInteger);

                    uint actualOffset = element.Offset != 0 ? element.Offset : offset;
                    actualOffset += vbOffset;

                    if (isInteger && !normalized)
                    {
                        _ctx.VertexAttribIPointer(
                            actualSlot,
                            FormatHelpers.GetElementCount(element.Format),
                            type,
                            _graphicsPipeline.VertexStrides[i],
                            actualOffset);
                        _gd.CheckError();
                    }
                    else
                    {
                        _ctx.VertexAttribPointer(
                            actualSlot,
                            FormatHelpers.GetElementCount(element.Format),
                            type,
                            normalized,
                            _graphicsPipeline.VertexStrides[i],
                            actualOffset);
                        _gd.CheckError();
                    }

                    uint stepRate = input.InstanceStepRate;
                    if (_vertexAttribDivisors[actualSlot] != stepRate)
                    {
                        _ctx.VertexAttribDivisor(actualSlot, stepRate);
                        _vertexAttribDivisors[actualSlot] = stepRate;
                    }

                    offset += FormatHelpers.GetSizeInBytes(element.Format);
                }

                totalSlotsBound += (uint)input.Elements.Length;
            }

            for (uint extraSlot = totalSlotsBound; extraSlot < _vertexAttributesBound; extraSlot++)
            {
                _ctx.DisableVertexAttribArray(extraSlot);
            }

            _vertexAttributesBound = totalSlotsBound;
        }

        public override void Dispatch(uint groupCountX, uint groupCountY, uint groupCountZ)
        {
            throw new NotSupportedException();
        }

        public override void DispatchIndirect(DeviceBuffer indirectBuffer, uint offset)
        {
            throw new NotSupportedException();
        }

        public override void End()
        {
        }

        public override void SetFramebuffer(Framebuffer fb)
        {
            throw new NotSupportedException();
        }

        public override void SetIndexBuffer(DeviceBuffer ib, IndexFormat format, uint offset)
        {
            WebGLBuffer glIB = Util.AssertSubtype<DeviceBuffer, WebGLBuffer>(ib);
            _ctx.BindBuffer(ELEMENT_ARRAY_BUFFER, glIB.WglBuffer);
            _gd.CheckError();
            _drawElementsType = VdToGLDrawElementsType(format);
            _ibOffset = offset;
        }

        public override void SetPipeline(Pipeline pipeline)
        {
            if (!pipeline.IsComputePipeline && _graphicsPipeline != pipeline)
            {
                _graphicsPipeline = Util.AssertSubtype<Pipeline, WebGLPipeline>(pipeline);
                ActivateGraphicsPipeline();
            }
            else if (pipeline.IsComputePipeline && _computePipeline != pipeline)
            {
                throw new NotSupportedException();
            }
        }

        private void ActivateGraphicsPipeline()
        {
            _graphicsPipelineActive = true;

            Util.EnsureArrayMinimumSize(ref _graphicsResourceSets, (uint)_graphicsPipeline.ResourceLayouts.Length);
            Util.EnsureArrayMinimumSize(ref _newGraphicsResourceSets, (uint)_graphicsPipeline.ResourceLayouts.Length);

            // Force ResourceSets to be re-bound.
            for (int i = 0; i < _graphicsPipeline.ResourceLayouts.Length; i++)
            {
                _newGraphicsResourceSets[i] = true;
            }

            // Blend State

            BlendStateDescription blendState = _graphicsPipeline.BlendState;
            _ctx.BlendColor(blendState.BlendFactor.R, blendState.BlendFactor.G, blendState.BlendFactor.B, blendState.BlendFactor.A);
            _gd.CheckError();

            if (blendState.AlphaToCoverageEnabled)
            {
                _ctx.Enable(SAMPLE_ALPHA_TO_COVERAGE);
                _gd.CheckError();
            }
            else
            {
                _ctx.Disable(SAMPLE_ALPHA_TO_COVERAGE);
                _gd.CheckError();
            }

            BlendAttachmentDescription attachment = blendState.AttachmentStates[0];
            if (!attachment.BlendEnabled)
            {
                _ctx.Disable(BLEND);
                _gd.CheckError();
            }
            else
            {
                _ctx.Enable(BLEND);
                _gd.CheckError();

                _ctx.BlendFuncSeparate(
                    VdToGLBlendFactor(attachment.SourceColorFactor),
                    VdToGLBlendFactor(attachment.DestinationColorFactor),
                    VdToGLBlendFactor(attachment.SourceAlphaFactor),
                    VdToGLBlendFactor(attachment.DestinationAlphaFactor));
                _gd.CheckError();

                _ctx.BlendEquationSeparate(
                     VdToGLBlendEquationMode(attachment.ColorFunction),
                     VdToGLBlendEquationMode(attachment.AlphaFunction));
                _gd.CheckError();
            }

            // Depth Stencil State

            DepthStencilStateDescription dss = _graphicsPipeline.DepthStencilState;
            if (!dss.DepthTestEnabled)
            {
                _ctx.Disable(DEPTH_TEST);
                _gd.CheckError();
            }
            else
            {
                _ctx.Enable(DEPTH_TEST);
                _gd.CheckError();

                _ctx.DepthFunc(ConvertComparisonKind(dss.DepthComparison));
                _gd.CheckError();
            }

            _ctx.DepthMask(dss.DepthWriteEnabled);
            _gd.CheckError();

            if (dss.StencilTestEnabled)
            {
                _ctx.Enable(STENCIL_TEST);
                _gd.CheckError();

                _ctx.StencilFuncSeparate(
                    FRONT,
                    ConvertComparisonKind(dss.StencilFront.Comparison),
                    (int)dss.StencilReference,
                    dss.StencilReadMask);
                _gd.CheckError();

                _ctx.StencilFuncSeparate(
                    BACK,
                    ConvertComparisonKind(dss.StencilBack.Comparison),
                    (int)dss.StencilReference,
                    dss.StencilReadMask);
                _gd.CheckError();

                _ctx.StencilMask(dss.StencilWriteMask);
                _gd.CheckError();
            }
            else
            {
                _ctx.Disable(STENCIL_TEST);
                _gd.CheckError();
            }

            // Rasterizer State

            RasterizerStateDescription rs = _graphicsPipeline.RasterizerState;
            if (rs.CullMode == FaceCullMode.None)
            {
                _ctx.Disable(CULL_FACE);
                _gd.CheckError();
            }
            else
            {
                _ctx.Enable(CULL_FACE);
                _gd.CheckError();

                _ctx.CullFace(WebGLUtil.VdToGLCullFaceMode(rs.CullMode));
                _gd.CheckError();
            }

            if (!rs.ScissorTestEnabled)
            {
                _ctx.Disable(SCISSOR_TEST);
                _gd.CheckError();
            }
            else
            {
                _ctx.Enable(SCISSOR_TEST);
                _gd.CheckError();
            }

            _ctx.FrontFace(WebGLUtil.VdToGLFrontFaceDirection(rs.FrontFace));
            _gd.CheckError();

            // Primitive Topology
            _primitiveType = WebGLUtil.VdToGLPrimitiveType(_graphicsPipeline.PrimitiveTopology);

            // Shader Set
            _ctx.UseProgram(_graphicsPipeline.Program);
            _gd.CheckError();

            int vertexStridesCount = _graphicsPipeline.VertexStrides.Length;
            Util.EnsureArrayMinimumSize(ref _vertexBuffers, (uint)vertexStridesCount);
            Util.EnsureArrayMinimumSize(ref _vbOffsets, (uint)vertexStridesCount);

            uint totalVertexElements = 0;
            for (int i = 0; i < _graphicsPipeline.VertexLayouts.Length; i++)
            {
                totalVertexElements += (uint)_graphicsPipeline.VertexLayouts[i].Elements.Length;
            }
            Util.EnsureArrayMinimumSize(ref _vertexAttribDivisors, totalVertexElements);
        }

        public override void MemoryBarrier(
            Texture texture,
            uint baseMipLevel, uint levelCount,
            uint baseArrayLayer, uint layerCount,
            ShaderStages sourceStage,
            ShaderStages destinationStage)
        {
        }

        public override void EndRenderPass()
        {
        }

        public override void BeginRenderPass(in RenderPassDescription rpd)
        {
            _fb = rpd.Framebuffer;

            if (_fb is WebGLFramebuffer wglFB)
            {
                _ctx.BindFramebuffer(FRAMEBUFFER, wglFB.WglFramebuffer);
                _gd.CheckError();
                _isSwapchainFB = false;
            }
            else
            {
                _ctx.BindFramebuffer(FRAMEBUFFER, null);
                _gd.CheckError();
                _isSwapchainFB = true;
            }

            // Reset to full viewports.
            SetViewport(0, new Viewport(0, 0, _fb.Width, _fb.Height, 0, 1));
            for (uint index = 1; index < _fb.ColorTargets.Count; index++)
            {
                SetViewport(index, new Viewport(0, 0, _fb.Width, _fb.Height, 0, 1));
            }

            // Reset to full scissor rect.
            SetScissorRect(0, 0, 0, _fb.Width, _fb.Height);
            for (uint index = 1; index < _fb.ColorTargets.Count; index++)
            {
                SetScissorRect(index, 0, 0, _fb.Width, _fb.Height);
            }

            if (rpd.LoadAction == LoadAction.Clear)
            {
                _ctx.ClearDepth(rpd.ClearDepth);
                _ctx.ClearColor(rpd.ClearColor.R, rpd.ClearColor.G, rpd.ClearColor.B, rpd.ClearColor.A);
                _ctx.DepthMask(true);
                _ctx.Clear(COLOR_BUFFER_BIT | DEPTH_BUFFER_BIT);
            }
        }

        public override void GenerateMipmaps(Texture texture)
        {
            WebGLTexture glTex = Util.AssertSubtype<Texture, WebGLTexture>(texture);
            uint target = glTex.Target;
            _textureSamplerManager.SetTextureTransient(glTex.Target, glTex.WglTexture);
            _ctx.GenerateMipmap(target);
            _gd.CheckError();
        }

        public override void PushDebugGroup(string name)
        {
        }

        public override void PopDebugGroup()
        {
        }

        public override void InsertDebugMarker(string name)
        {
        }

        public override void BlitTexture(
            Texture source, uint srcX, uint srcY, uint srcWidth, uint srcHeight,
            Framebuffer destination, uint dstX, uint dstY, uint dstWidth, uint dstHeight,
            bool linearFilter)
        {
            throw new NotImplementedException();

            /*
            Debug.Assert(source.Type == TextureType.Texture2D);

            glGenFramebuffers(1, out uint readFB);
            CheckLastError();
            glBindFramebuffer(FramebufferTarget.ReadFramebuffer, readFB);

            OpenGLTexture glTex = Util.AssertSubtype<Texture, OpenGLTexture>(source);
            glFramebufferTexture2D(
                FramebufferTarget.ReadFramebuffer,
                GLFramebufferAttachment.ColorAttachment0,
                TextureTarget.Texture2D,
                glTex.Texture,
                0);
            FramebufferErrorCode errorCode = glCheckFramebufferStatus(FramebufferTarget.ReadFramebuffer);
            CheckLastError();
            if (errorCode != FramebufferErrorCode.FramebufferComplete)
            {
                throw new VeldridException("Framebuffer was not successfully created: " + errorCode);
            }

            if (destination is OpenGLFramebuffer glFB)
            {
                glBindFramebuffer(FramebufferTarget.Framebuffer, glFB.Framebuffer);
                CheckLastError();
            }
            else
            {
                Util.AssertSubtype<Framebuffer, OpenGLSwapchainFramebuffer>(destination);
                if (_platformInfo.SetSwapchainFramebufferTarget != null)
                {
                    _platformInfo.SetSwapchainFramebufferTarget((uint)FramebufferTarget.DrawFramebuffer);
                }
                else
                {
                    glBindFramebuffer(FramebufferTarget.DrawFramebuffer, 0);
                    CheckLastError();
                }
            }

            ClearBufferMask mask = FormatHelpers.IsDepthStencilFormat(source.Format)
                ? ClearBufferMask.DepthBufferBit
                : ClearBufferMask.ColorBufferBit;

            glBlitFramebuffer(
                (int)srcX, (int)srcY, (int)(srcX + srcWidth), (int)(srcY + srcHeight),
                (int)dstX, (int)dstY, (int)(dstX + dstWidth), (int)(dstY + dstHeight),
                mask,
                linearFilter ? BlitFramebufferFilter.Linear : BlitFramebufferFilter.Nearest);
            CheckLastError();

            glDeleteFramebuffers(1, ref readFB);
            CheckLastError();
            */
        }

        public override void SetGraphicsResourceSet(uint slot, ResourceSet rs, uint dynamicOffsetCount, ref uint dynamicOffsets)
        {
            // if (!_graphicsResourceSets[slot].Equals(rs, dynamicOffsetCount, ref dynamicOffsets))
            {
                _graphicsResourceSets[slot].Offsets.Dispose();
                _graphicsResourceSets[slot] = new BoundResourceSetInfo(rs, dynamicOffsetCount, ref dynamicOffsets);
                _newGraphicsResourceSets[slot] = true;
            }
        }

        public override void SetComputeResourceSet(uint slot, ResourceSet rs, uint dynamicOffsetCount, ref uint dynamicOffsets)
        {
            if (!_computeResourceSets[slot].Equals(rs, dynamicOffsetCount, ref dynamicOffsets))
            {
                _computeResourceSets[slot].Offsets.Dispose();
                _computeResourceSets[slot] = new BoundResourceSetInfo(rs, dynamicOffsetCount, ref dynamicOffsets);
                _newComputeResourceSets[slot] = true;
            }
        }

        private void ActivateResourceSet(
            uint slot,
            bool graphics,
            BoundResourceSetInfo brsi,
            ResourceLayoutElementDescription[] layoutElements,
            bool isNew)
        {
            WebGLResourceSet glResourceSet = Util.AssertSubtype<ResourceSet, WebGLResourceSet>(brsi.Set);
            WebGLPipeline pipeline = graphics ? _graphicsPipeline : _computePipeline;
            uint ubBaseIndex = GetUniformBaseIndex(slot, graphics);
            uint ssboBaseIndex = GetShaderStorageBaseIndex(slot, graphics);

            uint ubOffset = 0;
            uint ssboOffset = 0;
            uint dynamicOffsetIndex = 0;
            for (uint element = 0; element < glResourceSet.Resources.Length; element++)
            {
                ResourceKind kind = layoutElements[element].Kind;
                BindableResource resource = glResourceSet.Resources[(int)element];

                uint bufferOffset = 0;
                if (glResourceSet.Layout.IsDynamicBuffer(element))
                {
                    bufferOffset = brsi.Offsets.Get(dynamicOffsetIndex);
                    dynamicOffsetIndex += 1;
                }

                switch (kind)
                {
                    case ResourceKind.UniformBuffer:
                    {
                        if (!isNew) { continue; }

                        DeviceBufferRange range = Util.GetBufferRange(resource, bufferOffset);
                        WebGLBuffer glUB = Util.AssertSubtype<DeviceBuffer, WebGLBuffer>(range.Buffer);

                        if (pipeline.GetUniformBindingForSlot(slot, element, out WebGLUniformBinding uniformBindingInfo))
                        {
                            if (range.SizeInBytes < uniformBindingInfo.BlockSize)
                            {
                                string name = glResourceSet.Layout.Elements[element].Name;
                                throw new VeldridException(
                                    $"Not enough data in uniform buffer \"{name}\" (slot {slot}, element {element}). Shader expects at least {uniformBindingInfo.BlockSize} bytes, but buffer only contains {glUB.SizeInBytes} bytes");
                            }
                            _ctx.UniformBlockBinding(pipeline.Program, uniformBindingInfo.BlockLocation, ubBaseIndex + ubOffset);
                            _gd.CheckError();

                            Console.WriteLine($"Calling BindBufferRange");
                            _ctx.BindBufferRange(
                                UNIFORM_BUFFER,
                                ubBaseIndex + ubOffset,
                                glUB.WglBuffer,
                                range.Offset,
                                range.SizeInBytes);

                            _gd.CheckError();

                            ubOffset += 1;
                        }
                        break;
                    }
                    case ResourceKind.StructuredBufferReadWrite:
                    case ResourceKind.StructuredBufferReadOnly:
                    {
                        throw new NotSupportedException();
                    }
                    case ResourceKind.TextureReadOnly:
                        TextureView texView = Util.GetTextureView(_gd, resource);
                        WebGLTextureView glTexView = Util.AssertSubtype<TextureView, WebGLTextureView>(texView);
                        if (pipeline.GetTextureBindingInfo(slot, element, out WebGLTextureBindingSlotInfo textureBindingInfo))
                        {
                            _textureSamplerManager.SetTexture((uint)textureBindingInfo.RelativeIndex, glTexView);
                            _ctx.Uniform1i(textureBindingInfo.UniformLocation, textureBindingInfo.RelativeIndex);
                            _gd.CheckError();
                        }
                        break;
                    case ResourceKind.TextureReadWrite:
                    {
                        throw new NotSupportedException();
                    }
                    case ResourceKind.Sampler:
                        WebGLSampler glSampler = Util.AssertSubtype<BindableResource, WebGLSampler>(resource);
                        if (pipeline.GetSamplerBindingInfo(slot, element, out WebGLSamplerBindingSlotInfo samplerBindingInfo))
                        {
                            foreach (int index in samplerBindingInfo.RelativeIndices)
                            {
                                _textureSamplerManager.SetSampler((uint)index, glSampler);
                            }
                        }
                        break;
                    default: throw Illegal.Value<ResourceKind>();
                }
            }
        }

        public override void ResolveTexture(Texture source, Texture destination)
        {
            throw new NotImplementedException();
            //WebGLTexture glSourceTex = Util.AssertSubtype<Texture, WebGLTexture>(source);
            //WebGLTexture glDestinationTex = Util.AssertSubtype<Texture, WebGLTexture>(destination);
            //glSourceTex.EnsureResourcesCreated();
            //glDestinationTex.EnsureResourcesCreated();

            //uint sourceFramebuffer = glSourceTex.GetFramebuffer(0, 0);
            //uint destinationFramebuffer = glDestinationTex.GetFramebuffer(0, 0);

            //glBindFramebuffer(FramebufferTarget.ReadFramebuffer, sourceFramebuffer);
            //_gd.CheckError();

            //glBindFramebuffer(FramebufferTarget.DrawFramebuffer, destinationFramebuffer);
            //_gd.CheckError();

            //glDisable(EnableCap.ScissorTest);
            //_gd.CheckError();

            //glBlitFramebuffer(
            //    0,
            //    0,
            //    (int)source.Width,
            //    (int)source.Height,
            //    0,
            //    0,
            //    (int)destination.Width,
            //    (int)destination.Height,
            //    ClearBufferMask.ColorBufferBit,
            //    BlitFramebufferFilter.Nearest);
            //_gd.CheckError();
        }

        private uint GetUniformBaseIndex(uint slot, bool graphics)
        {
            WebGLPipeline pipeline = graphics ? _graphicsPipeline : _computePipeline;
            uint ret = 0;
            for (uint i = 0; i < slot; i++)
            {
                ret += pipeline.GetUniformBufferCount(i);
            }

            return ret;
        }

        private uint GetShaderStorageBaseIndex(uint slot, bool graphics)
        {
            WebGLPipeline pipeline = graphics ? _graphicsPipeline : _computePipeline;
            uint ret = 0;
            for (uint i = 0; i < slot; i++)
            {
                ret += pipeline.GetShaderStorageBufferCount(i);
            }

            return ret;
        }

        public override void SetScissorRect(uint index, uint x, uint y, uint width, uint height)
        {
            if (index == 0)
            {
                _ctx.Scissor(
                    (int)x,
                    (int)(_fb.Height - (int)height - y),
                    (int)width,
                    (int)height);
                _gd.CheckError();
            }
        }

        public override void SetVertexBuffer(uint index, DeviceBuffer vb, uint offset)
        {
            WebGLBuffer glVB = Util.AssertSubtype<DeviceBuffer, WebGLBuffer>(vb);

            Util.EnsureArrayMinimumSize(ref _vertexBuffers, index + 1);
            Util.EnsureArrayMinimumSize(ref _vbOffsets, index + 1);
            _vertexBuffers[index] = glVB;
            _vbOffsets[index] = offset;
        }

        public override void SetViewport(uint index, Viewport viewport) => SetViewport(index, ref viewport);
        public override void SetViewport(uint index, ref Viewport viewport)
        {
            _viewports[(int)index] = viewport;

            if (index == 0)
            {
                _ctx.Viewport((int)viewport.X, (int)viewport.Y, (int)viewport.Width, (int)viewport.Height);
                _gd.CheckError();

                _ctx.DepthRange(viewport.MinDepth, viewport.MaxDepth);
                _gd.CheckError();
            }
        }

        public override void UpdateBuffer(DeviceBuffer buffer, uint bufferOffsetInBytes, IntPtr dataPtr, uint sizeInBytes)
        {
            WebGLBuffer glBuffer = Util.AssertSubtype<DeviceBuffer, WebGLBuffer>(buffer);
            uint bufferTarget = ((buffer.Usage & BufferUsage.IndexBuffer) != 0) ? ELEMENT_ARRAY_BUFFER : COPY_WRITE_BUFFER;
            _ctx.BindBuffer(bufferTarget, glBuffer.WglBuffer);
            _gd.CheckError();

            ReadOnlySpan<byte> dataSpan = new ReadOnlySpan<byte>(dataPtr.ToPointer(), (int)sizeInBytes);
            var u8Array = WebAssembly.Core.Uint8Array.From(dataSpan);
            _ctx.BufferSubData(
                bufferTarget,
                bufferOffsetInBytes,
                u8Array,
                0,
                sizeInBytes);
            _gd.CheckError();
            u8Array.Dispose();

            _ctx.BindBuffer(bufferTarget, null);
            _gd.CheckError();
        }

        public override void UpdateTexture(
            Texture texture,
            IntPtr dataPtr,
            uint x,
            uint y,
            uint z,
            uint width,
            uint height,
            uint depth,
            uint mipLevel,
            uint arrayLayer)
        {
            throw new NotImplementedException();
            //if (width == 0 || height == 0 || depth == 0) { return; }

            //WebGLTexture glTex = Util.AssertSubtype<Texture, WebGLTexture>(texture);
            //glTex.EnsureResourcesCreated();

            //TextureTarget texTarget = glTex.TextureTarget;

            //_textureSamplerManager.SetTextureTransient(texTarget, glTex.Texture);
            //_gd.CheckError();

            //bool isCompressed = FormatHelpers.IsCompressedFormat(texture.Format);
            //uint blockSize = isCompressed ? 4u : 1u;

            //uint blockAlignedWidth = Math.Max(width, blockSize);
            //uint blockAlignedHeight = Math.Max(width, blockSize);

            //uint rowPitch = FormatHelpers.GetRowPitch(blockAlignedWidth, texture.Format);
            //uint depthPitch = FormatHelpers.GetDepthPitch(rowPitch, blockAlignedHeight, texture.Format);

            //// Compressed textures can specify regions that are larger than the dimensions.
            //// We should only pass up to the dimensions to WebGL, though.
            //Util.GetMipDimensions(glTex, mipLevel, out uint mipWidth, out uint mipHeight, out uint mipDepth);
            //width = Math.Min(width, mipWidth);
            //height = Math.Min(height, mipHeight);

            //uint unpackAlignment = 4;
            //if (!isCompressed)
            //{
            //    unpackAlignment = FormatHelpers.GetSizeInBytes(glTex.Format);
            //}
            //if (unpackAlignment < 4)
            //{
            //    glPixelStorei(PixelStoreParameter.UnpackAlignment, (int)unpackAlignment);
            //    _gd.CheckError();
            //}

            //if (texTarget == TextureTarget.Texture1D)
            //{
            //    if (isCompressed)
            //    {
            //        glCompressedTexSubImage1D(
            //            TextureTarget.Texture1D,
            //            (int)mipLevel,
            //            (int)x,
            //            width,
            //            glTex.GLInternalFormat,
            //            rowPitch,
            //            dataPtr.ToPointer());
            //        _gd.CheckError();
            //    }
            //    else
            //    {
            //        glTexSubImage1D(
            //            TextureTarget.Texture1D,
            //            (int)mipLevel,
            //            (int)x,
            //            width,
            //            glTex.GLPixelFormat,
            //            glTex.GLPixelType,
            //            dataPtr.ToPointer());
            //        _gd.CheckError();
            //    }
            //}
            //else if (texTarget == TextureTarget.Texture1DArray)
            //{
            //    if (isCompressed)
            //    {
            //        glCompressedTexSubImage2D(
            //            TextureTarget.Texture1DArray,
            //            (int)mipLevel,
            //            (int)x,
            //            (int)arrayLayer,
            //            width,
            //            1,
            //            glTex.GLInternalFormat,
            //            rowPitch,
            //            dataPtr.ToPointer());
            //        _gd.CheckError();
            //    }
            //    else
            //    {
            //        glTexSubImage2D(
            //        TextureTarget.Texture1DArray,
            //        (int)mipLevel,
            //        (int)x,
            //        (int)arrayLayer,
            //        width,
            //        1,
            //        glTex.GLPixelFormat,
            //        glTex.GLPixelType,
            //        dataPtr.ToPointer());
            //        _gd.CheckError();
            //    }
            //}
            //else if (texTarget == TextureTarget.Texture2D)
            //{
            //    if (isCompressed)
            //    {
            //        glCompressedTexSubImage2D(
            //            TextureTarget.Texture2D,
            //            (int)mipLevel,
            //            (int)x,
            //            (int)y,
            //            width,
            //            height,
            //            glTex.GLInternalFormat,
            //            depthPitch,
            //            dataPtr.ToPointer());
            //        _gd.CheckError();
            //    }
            //    else
            //    {
            //        glTexSubImage2D(
            //            TextureTarget.Texture2D,
            //            (int)mipLevel,
            //            (int)x,
            //            (int)y,
            //            width,
            //            height,
            //            glTex.GLPixelFormat,
            //            glTex.GLPixelType,
            //            dataPtr.ToPointer());
            //        _gd.CheckError();
            //    }
            //}
            //else if (texTarget == TextureTarget.Texture2DArray)
            //{
            //    if (isCompressed)
            //    {
            //        glCompressedTexSubImage3D(
            //            TextureTarget.Texture2DArray,
            //            (int)mipLevel,
            //            (int)x,
            //            (int)y,
            //            (int)arrayLayer,
            //            width,
            //            height,
            //            1,
            //            glTex.GLInternalFormat,
            //            depthPitch,
            //            dataPtr.ToPointer());
            //        _gd.CheckError();
            //    }
            //    else
            //    {
            //        glTexSubImage3D(
            //            TextureTarget.Texture2DArray,
            //            (int)mipLevel,
            //            (int)x,
            //            (int)y,
            //            (int)arrayLayer,
            //            width,
            //            height,
            //            1,
            //            glTex.GLPixelFormat,
            //            glTex.GLPixelType,
            //            dataPtr.ToPointer());
            //        _gd.CheckError();
            //    }
            //}
            //else if (texTarget == TextureTarget.Texture3D)
            //{
            //    if (isCompressed)
            //    {
            //        glCompressedTexSubImage3D(
            //            TextureTarget.Texture3D,
            //            (int)mipLevel,
            //            (int)x,
            //            (int)y,
            //            (int)z,
            //            width,
            //            height,
            //            depth,
            //            glTex.GLInternalFormat,
            //            depthPitch * depth,
            //            dataPtr.ToPointer());
            //        _gd.CheckError();
            //    }
            //    else
            //    {
            //        glTexSubImage3D(
            //            TextureTarget.Texture3D,
            //            (int)mipLevel,
            //            (int)x,
            //            (int)y,
            //            (int)z,
            //            width,
            //            height,
            //            depth,
            //            glTex.GLPixelFormat,
            //            glTex.GLPixelType,
            //            dataPtr.ToPointer());
            //        _gd.CheckError();
            //    }
            //}
            //else if (texTarget == TextureTarget.TextureCubeMap)
            //{
            //    TextureTarget cubeTarget = GetCubeTarget(arrayLayer);
            //    if (isCompressed)
            //    {
            //        glCompressedTexSubImage2D(
            //            cubeTarget,
            //            (int)mipLevel,
            //            (int)x,
            //            (int)y,
            //            width,
            //            height,
            //            glTex.GLInternalFormat,
            //            depthPitch,
            //            dataPtr.ToPointer());
            //        _gd.CheckError();
            //    }
            //    else
            //    {
            //        glTexSubImage2D(
            //            cubeTarget,
            //            (int)mipLevel,
            //            (int)x,
            //            (int)y,
            //            width,
            //            height,
            //            glTex.GLPixelFormat,
            //            glTex.GLPixelType,
            //            dataPtr.ToPointer());
            //        _gd.CheckError();
            //    }
            //}
            //else if (texTarget == TextureTarget.TextureCubeMapArray)
            //{
            //    if (isCompressed)
            //    {
            //        glCompressedTexSubImage3D(
            //            TextureTarget.TextureCubeMapArray,
            //            (int)mipLevel,
            //            (int)x,
            //            (int)y,
            //            (int)arrayLayer,
            //            width,
            //            height,
            //            1,
            //            glTex.GLInternalFormat,
            //            depthPitch,
            //            dataPtr.ToPointer());
            //        _gd.CheckError();
            //    }
            //    else
            //    {
            //        glTexSubImage3D(
            //            TextureTarget.TextureCubeMapArray,
            //            (int)mipLevel,
            //            (int)x,
            //            (int)y,
            //            (int)arrayLayer,
            //            width,
            //            height,
            //            1,
            //            glTex.GLPixelFormat,
            //            glTex.GLPixelType,
            //            dataPtr.ToPointer());
            //        _gd.CheckError();
            //    }
            //}
            //else
            //{
            //    throw new VeldridException($"Invalid WebGL TextureTarget encountered: {glTex.TextureTarget}.");
            //}

            //if (unpackAlignment < 4)
            //{
            //    glPixelStorei(PixelStoreParameter.UnpackAlignment, 4);
            //    _gd.CheckError();
            //}
        }

        private uint GetCubeTarget(uint arrayLayer)
        {
            switch (arrayLayer)
            {
                case 0:
                    return TEXTURE_CUBE_MAP_POSITIVE_X;
                case 1:
                    return TEXTURE_CUBE_MAP_NEGATIVE_X;
                case 2:
                    return TEXTURE_CUBE_MAP_POSITIVE_Y;
                case 3:
                    return TEXTURE_CUBE_MAP_NEGATIVE_Y;
                case 4:
                    return TEXTURE_CUBE_MAP_POSITIVE_Z;
                case 5:
                    return TEXTURE_CUBE_MAP_NEGATIVE_Z;
                default:
                    throw new VeldridException("Unexpected array layer in UpdateTexture called on a cubemap texture.");
            }
        }

        public override void CopyBuffer(DeviceBuffer source, uint sourceOffset, DeviceBuffer destination, uint destinationOffset, uint sizeInBytes)
        {
            WebGLBuffer srcGLBuffer = Util.AssertSubtype<DeviceBuffer, WebGLBuffer>(source);
            WebGLBuffer dstGLBuffer = Util.AssertSubtype<DeviceBuffer, WebGLBuffer>(destination);

            _ctx.BindBuffer(COPY_READ_BUFFER, srcGLBuffer.WglBuffer);
            _gd.CheckError();

            _ctx.BindBuffer(COPY_WRITE_BUFFER, dstGLBuffer.WglBuffer);
            _gd.CheckError();

            _ctx.CopyBufferSubData(
                COPY_READ_BUFFER,
                COPY_WRITE_BUFFER,
                sourceOffset,
                destinationOffset,
                sizeInBytes);
            _gd.CheckError();
        }

        public override void CopyTexture(
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
            throw new NotImplementedException();

            //WebGLTexture srcGLTexture = Util.AssertSubtype<Texture, WebGLTexture>(source);
            //WebGLTexture dstGLTexture = Util.AssertSubtype<Texture, WebGLTexture>(destination);

            //if (_extensions.CopyImage && depth == 1)
            //{
            //    // glCopyImageSubData does not work properly when depth > 1, so use the awful roundabout copy.
            //    uint srcZOrLayer = Math.Max(srcBaseArrayLayer, srcZ);
            //    uint dstZOrLayer = Math.Max(dstBaseArrayLayer, dstZ);
            //    uint depthOrLayerCount = Math.Max(depth, layerCount);
            //    // Copy width and height are allowed to be a full compressed block size, even if the mip level only contains a
            //    // region smaller than the block size.
            //    Util.GetMipDimensions(source, srcMipLevel, out uint mipWidth, out uint mipHeight, out _);
            //    width = Math.Min(width, mipWidth);
            //    height = Math.Min(height, mipHeight);
            //    glCopyImageSubData(
            //        srcGLTexture.Texture, srcGLTexture.TextureTarget, (int)srcMipLevel, (int)srcX, (int)srcY, (int)srcZOrLayer,
            //        dstGLTexture.Texture, dstGLTexture.TextureTarget, (int)dstMipLevel, (int)dstX, (int)dstY, (int)dstZOrLayer,
            //        width, height, depthOrLayerCount);
            //    _gd.CheckError();
            //}
            //else
            //{
            //    for (uint layer = 0; layer < layerCount; layer++)
            //    {
            //        uint srcLayer = layer + srcBaseArrayLayer;
            //        uint dstLayer = layer + dstBaseArrayLayer;
            //        CopyRoundabout(
            //            srcGLTexture, dstGLTexture,
            //            srcX, srcY, srcZ, srcMipLevel, srcLayer,
            //            dstX, dstY, dstZ, dstMipLevel, dstLayer,
            //            width, height, depth);
            //    }
            //}
        }

        private void CopyRoundabout(
            WebGLTexture srcGLTexture, WebGLTexture dstGLTexture,
            uint srcX, uint srcY, uint srcZ, uint srcMipLevel, uint srcLayer,
            uint dstX, uint dstY, uint dstZ, uint dstMipLevel, uint dstLayer,
            uint width, uint height, uint depth)
        {
            //bool isCompressed = FormatHelpers.IsCompressedFormat(srcGLTexture.Format);
            //if (srcGLTexture.Format != dstGLTexture.Format)
            //{
            //    throw new VeldridException("Copying to/from Textures with different formats is not supported.");
            //}

            //uint packAlignment = 4;
            //uint depthSliceSize = 0;
            //uint sizeInBytes;
            //TextureTarget srcTarget = srcGLTexture.TextureTarget;
            //if (isCompressed)
            //{
            //    _textureSamplerManager.SetTextureTransient(srcTarget, srcGLTexture.Texture);
            //    _gd.CheckError();

            //    int compressedSize;
            //    glGetTexLevelParameteriv(
            //        srcTarget,
            //        (int)srcMipLevel,
            //        GetTextureParameter.TextureCompressedImageSize,
            //        &compressedSize);
            //    _gd.CheckError();
            //    sizeInBytes = (uint)compressedSize;
            //}
            //else
            //{
            //    uint pixelSize = FormatHelpers.GetSizeInBytes(srcGLTexture.Format);
            //    packAlignment = pixelSize;
            //    depthSliceSize = width * height * pixelSize;
            //    sizeInBytes = depthSliceSize * depth;
            //}

            //StagingBlock block = _stagingMemoryPool.GetStagingBlock(sizeInBytes);

            //if (packAlignment < 4)
            //{
            //    glPixelStorei(PixelStoreParameter.PackAlignment, (int)packAlignment);
            //    _gd.CheckError();
            //}

            //if (isCompressed)
            //{
            //    if (_extensions.ARB_DirectStateAccess)
            //    {
            //        glGetCompressedTextureImage(
            //            srcGLTexture.Texture,
            //            (int)srcMipLevel,
            //            block.SizeInBytes,
            //            block.Data);
            //        _gd.CheckError();
            //    }
            //    else
            //    {
            //        _textureSamplerManager.SetTextureTransient(srcTarget, srcGLTexture.Texture);
            //        _gd.CheckError();

            //        glGetCompressedTexImage(srcTarget, (int)srcMipLevel, block.Data);
            //        _gd.CheckError();
            //    }

            //    TextureTarget dstTarget = dstGLTexture.TextureTarget;
            //    _textureSamplerManager.SetTextureTransient(dstTarget, dstGLTexture.Texture);
            //    _gd.CheckError();

            //    Util.GetMipDimensions(srcGLTexture, srcMipLevel, out uint mipWidth, out uint mipHeight, out uint mipDepth);
            //    uint fullRowPitch = FormatHelpers.GetRowPitch(mipWidth, srcGLTexture.Format);
            //    uint fullDepthPitch = FormatHelpers.GetDepthPitch(
            //        fullRowPitch,
            //        mipHeight,
            //        srcGLTexture.Format);

            //    uint denseRowPitch = FormatHelpers.GetRowPitch(width, srcGLTexture.Format);
            //    uint denseDepthPitch = FormatHelpers.GetDepthPitch(denseRowPitch, height, srcGLTexture.Format);
            //    uint numRows = FormatHelpers.GetNumRows(height, srcGLTexture.Format);
            //    uint trueCopySize = denseRowPitch * numRows;
            //    StagingBlock trueCopySrc = _stagingMemoryPool.GetStagingBlock(trueCopySize);

            //    uint layerStartOffset = denseDepthPitch * srcLayer;

            //    Util.CopyTextureRegion(
            //        (byte*)block.Data + layerStartOffset,
            //        srcX, srcY, srcZ,
            //        fullRowPitch, fullDepthPitch,
            //        trueCopySrc.Data,
            //        0, 0, 0,
            //        denseRowPitch,
            //        denseDepthPitch,
            //        width, height, depth,
            //        srcGLTexture.Format);

            //    UpdateTexture(
            //        dstGLTexture,
            //        (IntPtr)trueCopySrc.Data,
            //        dstX, dstY, dstZ,
            //        width, height, 1,
            //        dstMipLevel, dstLayer);

            //    _stagingMemoryPool.Free(trueCopySrc);
            //}
            //else // !isCompressed
            //{
            //    if (_extensions.ARB_DirectStateAccess)
            //    {
            //        glGetTextureSubImage(
            //            srcGLTexture.Texture, (int)srcMipLevel, (int)srcX, (int)srcY, (int)srcZ,
            //            width, height, depth,
            //            srcGLTexture.GLPixelFormat, srcGLTexture.GLPixelType, block.SizeInBytes, block.Data);
            //        _gd.CheckError();
            //    }
            //    else
            //    {
            //        for (uint layer = 0; layer < depth; layer++)
            //        {
            //            uint curLayer = srcZ + srcLayer + layer;
            //            uint curOffset = depthSliceSize * layer;
            //            glGenFramebuffers(1, out uint readFB);
            //            _gd.CheckError();
            //            glBindFramebuffer(FramebufferTarget.ReadFramebuffer, readFB);
            //            _gd.CheckError();

            //            if (srcGLTexture.ArrayLayers > 1 || srcGLTexture.Type == TextureType.Texture3D)
            //            {
            //                glFramebufferTextureLayer(
            //                    FramebufferTarget.ReadFramebuffer,
            //                    GLFramebufferAttachment.ColorAttachment0,
            //                    srcGLTexture.Texture,
            //                    (int)srcMipLevel,
            //                    (int)curLayer);
            //                _gd.CheckError();
            //            }
            //            else if (srcGLTexture.Type == TextureType.Texture1D)
            //            {
            //                glFramebufferTexture1D(
            //                    FramebufferTarget.ReadFramebuffer,
            //                    GLFramebufferAttachment.ColorAttachment0,
            //                    TextureTarget.Texture1D,
            //                    srcGLTexture.Texture,
            //                    (int)srcMipLevel);
            //                _gd.CheckError();
            //            }
            //            else
            //            {
            //                glFramebufferTexture2D(
            //                    FramebufferTarget.ReadFramebuffer,
            //                    GLFramebufferAttachment.ColorAttachment0,
            //                    TextureTarget.Texture2D,
            //                    srcGLTexture.Texture,
            //                    (int)srcMipLevel);
            //                _gd.CheckError();
            //            }

            //            _gd.CheckError();
            //            glReadPixels(
            //                (int)srcX, (int)srcY,
            //                width, height,
            //                srcGLTexture.GLPixelFormat,
            //                srcGLTexture.GLPixelType,
            //                (byte*)block.Data + curOffset);
            //            _gd.CheckError();
            //            glDeleteFramebuffers(1, ref readFB);
            //            _gd.CheckError();
            //        }
            //    }

            //    UpdateTexture(
            //        dstGLTexture,
            //        (IntPtr)block.Data,
            //        dstX, dstY, dstZ,
            //        width, height, depth, dstMipLevel, dstLayer);
            //}

            //if (packAlignment < 4)
            //{
            //    glPixelStorei(PixelStoreParameter.PackAlignment, 4);
            //    _gd.CheckError();
            //}

            //_stagingMemoryPool.Free(block);
        }

        private static void CopyWithFBO(
            WebGLTextureSamplerManager textureSamplerManager,
            WebGLTexture srcGLTexture, WebGLTexture dstGLTexture,
            uint srcX, uint srcY, uint srcZ, uint srcMipLevel, uint srcBaseArrayLayer,
            uint dstX, uint dstY, uint dstZ, uint dstMipLevel, uint dstBaseArrayLayer,
            uint width, uint height, uint depth, uint layerCount, uint layer)
        {
            //    TextureTarget dstTarget = dstGLTexture.TextureTarget;
            //    if (dstTarget == TextureTarget.Texture2D)
            //    {
            //        glBindFramebuffer(
            //            FramebufferTarget.ReadFramebuffer,
            //            srcGLTexture.GetFramebuffer(srcMipLevel, srcBaseArrayLayer + layer));
            //        _gd.CheckError();

            //        textureSamplerManager.SetTextureTransient(TextureTarget.Texture2D, dstGLTexture.Texture);
            //        _gd.CheckError();

            //        glCopyTexSubImage2D(
            //            TextureTarget.Texture2D,
            //            (int)dstMipLevel,
            //            (int)dstX, (int)dstY,
            //            (int)srcX, (int)srcY,
            //            width, height);
            //        _gd.CheckError();
            //    }
            //    else if (dstTarget == TextureTarget.Texture2DArray)
            //    {
            //        glBindFramebuffer(
            //            FramebufferTarget.ReadFramebuffer,
            //            srcGLTexture.GetFramebuffer(srcMipLevel, srcBaseArrayLayer + layerCount));

            //        textureSamplerManager.SetTextureTransient(TextureTarget.Texture2DArray, dstGLTexture.Texture);
            //        _gd.CheckError();

            //        glCopyTexSubImage3D(
            //            TextureTarget.Texture2DArray,
            //            (int)dstMipLevel,
            //            (int)dstX,
            //            (int)dstY,
            //            (int)(dstBaseArrayLayer + layer),
            //            (int)srcX,
            //            (int)srcY,
            //            width,
            //            height);
            //        _gd.CheckError();
            //    }
            //    else if (dstTarget == TextureTarget.Texture3D)
            //    {
            //        textureSamplerManager.SetTextureTransient(TextureTarget.Texture3D, dstGLTexture.Texture);
            //        _gd.CheckError();

            //        for (uint i = srcZ; i < srcZ + depth; i++)
            //        {
            //            glCopyTexSubImage3D(
            //                TextureTarget.Texture3D,
            //                (int)dstMipLevel,
            //                (int)dstX,
            //                (int)dstY,
            //                (int)dstZ,
            //                (int)srcX,
            //                (int)srcY,
            //                width,
            //                height);
            //        }
            //        _gd.CheckError();
            //    }
        }
    }
}
