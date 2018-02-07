using System;
using static Veldrid.OpenGLBinding.OpenGLNative;
using static Veldrid.OpenGL.OpenGLUtil;
using Veldrid.OpenGLBinding;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Veldrid.OpenGL
{
    internal unsafe class OpenGLCommandExecutor
    {
        private readonly OpenGLTextureSamplerManager _textureSamplerManager;
        private readonly StagingMemoryPool _stagingMemoryPool;
        private OpenGLExtensions _extensions;

        private Framebuffer _fb;
        private bool _isSwapchainFB;
        private OpenGLPipeline _graphicsPipeline;
        private OpenGLResourceSet[] _graphicsResourceSets = new OpenGLResourceSet[1];
        private OpenGLBuffer[] _vertexBuffers = new OpenGLBuffer[1];
        private uint[] _vertexAttribDivisors = new uint[1];
        private uint _vertexAttributesBound;
        private readonly Viewport[] _viewports = new Viewport[20];
        private DrawElementsType _drawElementsType;
        private PrimitiveType _primitiveType;

        private OpenGLPipeline _computePipeline;
        private OpenGLResourceSet[] _computeResourceSets = new OpenGLResourceSet[1];

        private bool _graphicsPipelineActive;

        public OpenGLCommandExecutor(OpenGLExtensions extensions, StagingMemoryPool stagingMemoryPool)
        {
            _extensions = extensions;
            _textureSamplerManager = new OpenGLTextureSamplerManager(extensions);
            _stagingMemoryPool = stagingMemoryPool;
        }

        public void Begin()
        {
        }

        public void ClearColorTarget(uint index, RgbaFloat clearColor)
        {
            if (!_isSwapchainFB)
            {
                glDrawBuffer((DrawBufferMode)((uint)DrawBufferMode.ColorAttachment0 + index));
                CheckLastError();
            }

            RgbaFloat color = clearColor;
            glClearColor(color.R, color.G, color.B, color.A);
            CheckLastError();

            glClear(ClearBufferMask.ColorBufferBit);
            CheckLastError();
        }

        public void ClearDepthStencil(float depth, byte stencil)
        {
            glClearDepth(depth);
            CheckLastError();

            glClearStencil(stencil);
            CheckLastError();

            glDepthMask(true);
            glClear(ClearBufferMask.DepthBufferBit | ClearBufferMask.StencilBufferBit);
            CheckLastError();
        }

        public void Draw(uint vertexCount, uint instanceCount, uint vertexStart, uint instanceStart)
        {
            PreDrawCommand();

            if (instanceCount == 1)
            {
                glDrawArrays(_primitiveType, (int)vertexStart, vertexCount);
                CheckLastError();
            }
            else
            {
                if (instanceStart == 0)
                {
                    glDrawArraysInstanced(_primitiveType, (int)vertexStart, vertexCount, instanceCount);
                    CheckLastError();
                }
                else
                {
                    glDrawArraysInstancedBaseInstance(_primitiveType, (int)vertexStart, vertexCount, instanceCount, instanceStart);
                    CheckLastError();
                }
            }
        }

        public void DrawIndexed(uint indexCount, uint instanceCount, uint indexStart, int vertexOffset, uint instanceStart)
        {
            PreDrawCommand();

            uint indexSize = _drawElementsType == DrawElementsType.UnsignedShort ? 2u : 4u;
            void* indices = new IntPtr(indexStart * indexSize).ToPointer();

            if (instanceCount == 1)
            {
                if (vertexOffset == 0)
                {
                    glDrawElements(_primitiveType, indexCount, _drawElementsType, indices);
                    CheckLastError();
                }
                else
                {
                    glDrawElementsBaseVertex(_primitiveType, indexCount, _drawElementsType, indices, vertexOffset);
                    CheckLastError();
                }
            }
            else
            {
                if (vertexOffset == 0)
                {
                    glDrawElementsInstanced(_primitiveType, indexCount, _drawElementsType, indices, instanceCount);
                    CheckLastError();
                }
                else
                {
                    glDrawElementsInstancedBaseVertex(
                        _primitiveType,
                        indexCount,
                        _drawElementsType,
                        indices,
                        instanceCount,
                        vertexOffset);
                    CheckLastError();
                }
            }
        }

        public void DrawIndirect(DeviceBuffer indirectBuffer, uint offset, uint drawCount, uint stride)
        {
            PreDrawCommand();

            OpenGLBuffer glBuffer = Util.AssertSubtype<DeviceBuffer, OpenGLBuffer>(indirectBuffer);
            glBindBuffer(BufferTarget.DrawIndirectBuffer, glBuffer.Buffer);
            CheckLastError();

            glMultiDrawArraysIndirect(_primitiveType, (IntPtr)offset, drawCount, stride);
            CheckLastError();
        }

        public void DrawIndexedIndirect(DeviceBuffer indirectBuffer, uint offset, uint drawCount, uint stride)
        {
            PreDrawCommand();

            OpenGLBuffer glBuffer = Util.AssertSubtype<DeviceBuffer, OpenGLBuffer>(indirectBuffer);
            glBindBuffer(BufferTarget.DrawIndirectBuffer, glBuffer.Buffer);
            CheckLastError();

            glMultiDrawElementsIndirect(_primitiveType, _drawElementsType, (IntPtr)offset, drawCount, stride);
            CheckLastError();
        }

        private void PreDrawCommand()
        {
            if (!_graphicsPipelineActive)
            {
                ActivateGraphicsPipeline();
            }

            FlushVertexLayouts();
        }

        private void FlushVertexLayouts()
        {
            uint totalSlotsBound = 0;
            VertexLayoutDescription[] layouts = _graphicsPipeline.GraphicsDescription.ShaderSet.VertexLayouts;
            for (int i = 0; i < layouts.Length; i++)
            {
                VertexLayoutDescription input = layouts[i];
                OpenGLBuffer vb = _vertexBuffers[i];
                glBindBuffer(BufferTarget.ArrayBuffer, vb.Buffer);
                uint offset = 0;
                for (uint slot = 0; slot < input.Elements.Length; slot++)
                {
                    ref VertexElementDescription element = ref input.Elements[slot]; // Large structure -- use by reference.
                    uint actualSlot = totalSlotsBound + slot;
                    if (actualSlot >= _vertexAttributesBound)
                    {
                        glEnableVertexAttribArray(actualSlot);
                    }
                    VertexAttribPointerType type = OpenGLFormats.VdToGLVertexAttribPointerType(
                        element.Format,
                        out bool normalized,
                        out bool isInteger);
                    if (isInteger && !normalized)
                    {
                        glVertexAttribIPointer(
                            actualSlot,
                            FormatHelpers.GetElementCount(element.Format),
                            type,
                            (uint)_graphicsPipeline.VertexStrides[i],
                            (void*)offset);
                        CheckLastError();
                    }
                    else
                    {
                        glVertexAttribPointer(
                            actualSlot,
                            FormatHelpers.GetElementCount(element.Format),
                            type,
                            normalized,
                            (uint)_graphicsPipeline.VertexStrides[i],
                            (void*)offset);
                        CheckLastError();
                    }

                    uint stepRate = input.InstanceStepRate;
                    if (_vertexAttribDivisors[actualSlot] != stepRate)
                    {
                        glVertexAttribDivisor(actualSlot, stepRate);
                        _vertexAttribDivisors[actualSlot] = stepRate;
                    }

                    offset += FormatHelpers.GetSizeInBytes(element.Format);
                }

                totalSlotsBound += (uint)input.Elements.Length;
            }

            for (uint extraSlot = totalSlotsBound; extraSlot < _vertexAttributesBound; extraSlot++)
            {
                glDisableVertexAttribArray(extraSlot);
            }

            _vertexAttributesBound = totalSlotsBound;
        }

        internal void Dispatch(uint groupCountX, uint groupCountY, uint groupCountZ)
        {
            if (_graphicsPipelineActive)
            {
                ActivateComputePipeline();
            }

            glDispatchCompute(groupCountX, groupCountY, groupCountZ);
            CheckLastError();

            PostDispatchCommand();
        }

        public void DispatchIndirect(DeviceBuffer indirectBuffer, uint offset)
        {
            OpenGLBuffer glBuffer = Util.AssertSubtype<DeviceBuffer, OpenGLBuffer>(indirectBuffer);
            glBindBuffer(BufferTarget.DrawIndirectBuffer, glBuffer.Buffer);
            CheckLastError();

            glDispatchComputeIndirect((IntPtr)offset);
            CheckLastError();

            PostDispatchCommand();
        }

        private static void PostDispatchCommand()
        {
            // TODO: Smart barriers?
            glMemoryBarrier(MemoryBarrierFlags.AllBarrierBits);
            CheckLastError();
        }

        public void End()
        {
        }

        public void SetFramebuffer(Framebuffer fb)
        {
            if (fb is OpenGLFramebuffer glFB)
            {
                glFB.EnsureResourcesCreated();
                glBindFramebuffer(FramebufferTarget.Framebuffer, glFB.Framebuffer);
                CheckLastError();
                _isSwapchainFB = false;
            }
            else if (fb is OpenGLSwapchainFramebuffer)
            {
                glBindFramebuffer(FramebufferTarget.Framebuffer, 0);
                CheckLastError();
                _isSwapchainFB = true;
            }
            else
            {
                throw new VeldridException("Invalid Framebuffer type: " + fb.GetType().Name);
            }

            _fb = fb;
        }

        public void SetIndexBuffer(DeviceBuffer ib, IndexFormat format)
        {
            OpenGLBuffer glIB = Util.AssertSubtype<DeviceBuffer, OpenGLBuffer>(ib);
            glIB.EnsureResourcesCreated();

            glBindBuffer(BufferTarget.ElementArrayBuffer, glIB.Buffer);
            CheckLastError();

            _drawElementsType = OpenGLFormats.VdToGLDrawElementsType(format);
        }

        public void SetPipeline(Pipeline pipeline)
        {
            if (!pipeline.IsComputePipeline && _graphicsPipeline != pipeline)
            {
                _graphicsPipeline = Util.AssertSubtype<Pipeline, OpenGLPipeline>(pipeline);
                ActivateGraphicsPipeline();
            }
            else if (pipeline.IsComputePipeline && _computePipeline != pipeline)
            {
                _computePipeline = Util.AssertSubtype<Pipeline, OpenGLPipeline>(pipeline);
                ActivateComputePipeline();
            }
        }

        private void ActivateGraphicsPipeline()
        {
            _graphicsPipelineActive = true;
            _graphicsPipeline.EnsureResourcesCreated();
            GraphicsPipelineDescription desc = _graphicsPipeline.GraphicsDescription;
            Util.ClearArray(_graphicsResourceSets); // Invalidate resource set bindings -- they may be invalid.

            // Blend State

            BlendStateDescription blendState = desc.BlendState;
            glBlendColor(blendState.BlendFactor.R, blendState.BlendFactor.G, blendState.BlendFactor.B, blendState.BlendFactor.A);
            CheckLastError();

            for (uint i = 0; i < blendState.AttachmentStates.Length; i++)
            {
                BlendAttachmentDescription attachment = blendState.AttachmentStates[i];
                if (!attachment.BlendEnabled)
                {
                    glDisablei(EnableCap.Blend, i);
                    CheckLastError();
                }
                else
                {
                    glEnablei(EnableCap.Blend, i);
                    CheckLastError();

                    glBlendFuncSeparatei(
                        i,
                        OpenGLFormats.VdToGLBlendFactorSrc(attachment.SourceColorFactor),
                        OpenGLFormats.VdToGLBlendFactorDest(attachment.DestinationColorFactor),
                        OpenGLFormats.VdToGLBlendFactorSrc(attachment.SourceAlphaFactor),
                        OpenGLFormats.VdToGLBlendFactorDest(attachment.DestinationAlphaFactor));
                    CheckLastError();

                    glBlendEquationSeparatei(
                        i,
                        OpenGLFormats.VdToGLBlendEquationMode(attachment.ColorFunction),
                        OpenGLFormats.VdToGLBlendEquationMode(attachment.AlphaFunction));
                    CheckLastError();
                }
            }

            // Depth Stencil State

            DepthStencilStateDescription dss = desc.DepthStencilState;
            if (!dss.DepthTestEnabled)
            {
                glDisable(EnableCap.DepthTest);
                CheckLastError();
            }
            else
            {
                glEnable(EnableCap.DepthTest);
                CheckLastError();

                glDepthFunc(OpenGLFormats.VdToGLDepthFunction(dss.DepthComparison));
                CheckLastError();
            }

            glDepthMask(dss.DepthWriteEnabled);
            CheckLastError();

            if (dss.StencilTestEnabled)
            {
                glEnable(EnableCap.StencilTest);
                CheckLastError();

                glStencilFuncSeparate(
                    CullFaceMode.Front,
                    OpenGLFormats.VdToGLStencilFunction(dss.StencilFront.Comparison),
                    (int)dss.StencilReference,
                    dss.StencilReadMask);
                CheckLastError();

                glStencilFuncSeparate(
                    CullFaceMode.Back,
                    OpenGLFormats.VdToGLStencilFunction(dss.StencilBack.Comparison),
                    (int)dss.StencilReference,
                    dss.StencilReadMask);
                CheckLastError();

                glStencilMask(dss.StencilWriteMask);
                CheckLastError();
            }
            else
            {
                glDisable(EnableCap.StencilTest);
                CheckLastError();
            }

            // Rasterizer State

            RasterizerStateDescription rs = desc.RasterizerState;
            if (rs.CullMode == FaceCullMode.None)
            {
                glDisable(EnableCap.CullFace);
                CheckLastError();
            }
            else
            {
                glEnable(EnableCap.CullFace);
                CheckLastError();

                glCullFace(OpenGLFormats.VdToGLCullFaceMode(rs.CullMode));
                CheckLastError();
            }

            glPolygonMode(MaterialFace.FrontAndBack, OpenGLFormats.VdToGLPolygonMode(rs.FillMode));
            CheckLastError();

            if (!rs.ScissorTestEnabled)
            {
                glDisable(EnableCap.ScissorTest);
                CheckLastError();
            }
            else
            {
                glEnable(EnableCap.ScissorTest);
                CheckLastError();
            }

            if (!rs.DepthClipEnabled)
            {
                glEnable(EnableCap.DepthClamp);
                CheckLastError();
            }
            else
            {
                glDisable(EnableCap.DepthClamp);
                CheckLastError();
            }

            glFrontFace(OpenGLFormats.VdToGLFrontFaceDirection(rs.FrontFace));
            CheckLastError();

            // Primitive Topology
            _primitiveType = OpenGLFormats.VdToGLPrimitiveType(desc.PrimitiveTopology);

            // Shader Set
            glUseProgram(_graphicsPipeline.Program);
            CheckLastError();

            int vertexStridesCount = _graphicsPipeline.VertexStrides.Length;
            Util.EnsureArrayMinimumSize(ref _vertexBuffers, (uint)vertexStridesCount);

            uint totalVertexElements = 0;
            for (int i = 0; i < desc.ShaderSet.VertexLayouts.Length; i++)
            {
                totalVertexElements += (uint)desc.ShaderSet.VertexLayouts[i].Elements.Length;
            }
            Util.EnsureArrayMinimumSize(ref _vertexAttribDivisors, totalVertexElements);

            Util.EnsureArrayMinimumSize(ref _graphicsResourceSets, (uint)desc.ResourceLayouts.Length);
        }

        private void ActivateComputePipeline()
        {
            _graphicsPipelineActive = false;
            _computePipeline.EnsureResourcesCreated();
            Util.ClearArray(_computeResourceSets); // Invalidate resource set bindings -- they may be invalid.
            Util.EnsureArrayMinimumSize(ref _computeResourceSets, (uint)_computePipeline.ComputeDescription.ResourceLayouts.Length);

            // Shader Set
            glUseProgram(_computePipeline.Program);
            CheckLastError();
        }

        public void SetGraphicsResourceSet(uint slot, ResourceSet rs)
        {
            if (_graphicsResourceSets[slot] == rs)
            {
                return;
            }

            OpenGLResourceSet glResourceSet = Util.AssertSubtype<ResourceSet, OpenGLResourceSet>(rs);
            OpenGLResourceLayout glLayout = glResourceSet.Layout;
            ResourceLayoutElementDescription[] layoutElements = glLayout.Description.Elements;
            _graphicsResourceSets[slot] = glResourceSet;

            ActivateResourceSet(slot, true, glResourceSet, layoutElements);
        }

        public void SetComputeResourceSet(uint slot, ResourceSet rs)
        {
            if (_computeResourceSets[slot] == rs)
            {
                return;
            }

            OpenGLResourceSet glResourceSet = Util.AssertSubtype<ResourceSet, OpenGLResourceSet>(rs);
            OpenGLResourceLayout glLayout = glResourceSet.Layout;
            ResourceLayoutElementDescription[] layoutElements = glLayout.Description.Elements;
            _computeResourceSets[slot] = glResourceSet;

            ActivateResourceSet(slot, false, glResourceSet, layoutElements);
        }

        private void ActivateResourceSet(
            uint slot,
            bool graphics,
            OpenGLResourceSet glResourceSet,
            ResourceLayoutElementDescription[] layoutElements)
        {
            OpenGLPipeline pipeline = graphics ? _graphicsPipeline : _computePipeline;
            uint ubBaseIndex = GetUniformBaseIndex(slot, graphics);
            uint ssboBaseIndex = GetShaderStorageBaseIndex(slot, graphics);

            uint ubOffset = 0;
            uint ssboOffset = 0;
            for (uint element = 0; element < glResourceSet.Resources.Length; element++)
            {
                ResourceKind kind = layoutElements[element].Kind;
                BindableResource resource = glResourceSet.Resources[(int)element];
                switch (kind)
                {
                    case ResourceKind.UniformBuffer:
                        OpenGLBuffer glUB = Util.AssertSubtype<BindableResource, OpenGLBuffer>(resource);
                        glUB.EnsureResourcesCreated();
                        if (pipeline.GetUniformBindingForSlot(slot, element, out OpenGLUniformBinding uniformBindingInfo))
                        {
                            if (glUB.SizeInBytes < uniformBindingInfo.BlockSize)
                            {
                                throw new VeldridException(
                                    $"Not enough data in uniform buffer. Shader expects at least {uniformBindingInfo.BlockSize}, but buffer only contains {glUB.SizeInBytes}");
                            }
                            glUniformBlockBinding(pipeline.Program, uniformBindingInfo.BlockLocation, ubBaseIndex + ubOffset);
                            CheckLastError();

                            glBindBufferRange(BufferRangeTarget.UniformBuffer, ubBaseIndex + ubOffset, glUB.Buffer, IntPtr.Zero, (UIntPtr)glUB.SizeInBytes);
                            CheckLastError();

                            ubOffset += 1;
                        }
                        break;
                    case ResourceKind.StructuredBufferReadWrite:
                    case ResourceKind.StructuredBufferReadOnly:
                        OpenGLBuffer glBuffer = Util.AssertSubtype<BindableResource, OpenGLBuffer>(resource);
                        if (pipeline.GetStorageBufferBindingForSlot(slot, element, out OpenGLShaderStorageBinding shaderStorageBinding))
                        {
                            glShaderStorageBlockBinding(pipeline.Program, shaderStorageBinding.StorageBlockBinding, ssboBaseIndex + ssboOffset);
                            CheckLastError();

                            glBindBufferRange(BufferRangeTarget.ShaderStorageBuffer, ssboBaseIndex + ssboOffset, glBuffer.Buffer, IntPtr.Zero, (UIntPtr)glBuffer.SizeInBytes);
                            CheckLastError();

                            ssboOffset += 1;
                        }
                        break;
                    case ResourceKind.TextureReadOnly:
                        OpenGLTextureView glTexView = Util.AssertSubtype<BindableResource, OpenGLTextureView>(resource);
                        glTexView.EnsureResourcesCreated();
                        if (pipeline.GetTextureBindingInfo(slot, element, out OpenGLTextureBindingSlotInfo textureBindingInfo))
                        {
                            _textureSamplerManager.SetTexture((uint)textureBindingInfo.RelativeIndex, glTexView);
                            glUniform1i(textureBindingInfo.UniformLocation, textureBindingInfo.RelativeIndex);
                            CheckLastError();
                        }
                        break;
                    case ResourceKind.TextureReadWrite:
                        OpenGLTextureView glTexViewRW = Util.AssertSubtype<BindableResource, OpenGLTextureView>(resource);
                        glTexViewRW.EnsureResourcesCreated();
                        if (pipeline.GetTextureBindingInfo(slot, element, out OpenGLTextureBindingSlotInfo imageBindingInfo))
                        {
                            glBindImageTexture(
                                (uint)imageBindingInfo.RelativeIndex,
                                glTexViewRW.Target.Texture,
                                0,
                                false,
                                0,
                                TextureAccess.ReadWrite,
                                glTexViewRW.GetReadWriteSizedInternalFormat());
                            CheckLastError();
                            glUniform1i(imageBindingInfo.UniformLocation, imageBindingInfo.RelativeIndex);
                            CheckLastError();
                        }
                        break;
                    case ResourceKind.Sampler:
                        OpenGLSampler glSampler = Util.AssertSubtype<BindableResource, OpenGLSampler>(resource);
                        glSampler.EnsureResourcesCreated();
                        if (pipeline.GetSamplerBindingInfo(slot, element, out OpenGLSamplerBindingSlotInfo samplerBindingInfo))
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

        public void ResolveTexture(Texture source, Texture destination)
        {
            OpenGLTexture glSourceTex = Util.AssertSubtype<Texture, OpenGLTexture>(source);
            OpenGLTexture glDestinationTex = Util.AssertSubtype<Texture, OpenGLTexture>(destination);
            glSourceTex.EnsureResourcesCreated();
            glDestinationTex.EnsureResourcesCreated();

            uint sourceFramebuffer = glSourceTex.GetFramebuffer(0, 0);
            uint destinationFramebuffer = glDestinationTex.GetFramebuffer(0, 0);

            glBindFramebuffer(FramebufferTarget.ReadFramebuffer, sourceFramebuffer);
            CheckLastError();

            glBindFramebuffer(FramebufferTarget.DrawFramebuffer, destinationFramebuffer);
            CheckLastError();

            glDisable(EnableCap.ScissorTest);
            CheckLastError();

            glBlitFramebuffer(
                0,
                0,
                (int)source.Width,
                (int)source.Height,
                0,
                0,
                (int)destination.Width,
                (int)destination.Height,
                ClearBufferMask.ColorBufferBit,
                BlitFramebufferFilter.Nearest);
            CheckLastError();
        }

        private uint GetUniformBaseIndex(uint slot, bool graphics)
        {
            OpenGLPipeline pipeline = graphics ? _graphicsPipeline : _computePipeline;
            uint ret = 0;
            for (uint i = 0; i < slot; i++)
            {
                ret += pipeline.GetUniformBufferCount(i);
            }

            return ret;
        }

        private uint GetShaderStorageBaseIndex(uint slot, bool graphics)
        {
            OpenGLPipeline pipeline = graphics ? _graphicsPipeline : _computePipeline;
            uint ret = 0;
            for (uint i = 0; i < slot; i++)
            {
                ret += pipeline.GetUniformBufferCount(i);
            }

            return ret;
        }

        public void SetScissorRect(uint index, uint x, uint y, uint width, uint height)
        {
            glScissorIndexed(
                index,
                (int)x,
                (int)(_viewports[(int)index].Height - (int)height - y),
                width,
                height);
            CheckLastError();
        }

        public void SetVertexBuffer(uint index, DeviceBuffer vb)
        {
            OpenGLBuffer glVB = Util.AssertSubtype<DeviceBuffer, OpenGLBuffer>(vb);
            glVB.EnsureResourcesCreated();

            Util.EnsureArrayMinimumSize(ref _vertexBuffers, index + 1);
            _vertexBuffers[index] = glVB;
        }

        public void SetViewport(uint index, ref Viewport viewport)
        {
            _viewports[(int)index] = viewport;

            float left = viewport.X;
            float bottom = _fb.Height - (viewport.Y + viewport.Height);

            glViewportIndexed(index, left, bottom, viewport.Width, viewport.Height);
            CheckLastError();

            glDepthRangeIndexed(index, viewport.MinDepth, viewport.MaxDepth);
            CheckLastError();
        }

        public void UpdateBuffer(DeviceBuffer buffer, uint bufferOffsetInBytes, IntPtr dataPtr, uint sizeInBytes)
        {
            OpenGLBuffer glBuffer = Util.AssertSubtype<DeviceBuffer, OpenGLBuffer>(buffer);
            glBuffer.EnsureResourcesCreated();

            if (_extensions.ARB_DirectStateAccess)
            {
                glNamedBufferSubData(
                    glBuffer.Buffer,
                    (IntPtr)bufferOffsetInBytes,
                    sizeInBytes,
                    dataPtr.ToPointer());
                CheckLastError();
            }
            else
            {
                BufferTarget bufferTarget = BufferTarget.CopyWriteBuffer;
                glBindBuffer(bufferTarget, glBuffer.Buffer);
                CheckLastError();
                glBufferSubData(
                    bufferTarget,
                    (IntPtr)bufferOffsetInBytes,
                    (UIntPtr)sizeInBytes,
                    dataPtr.ToPointer());
                CheckLastError();
            }
        }

        public void UpdateTexture(
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
            OpenGLTexture glTex = Util.AssertSubtype<Texture, OpenGLTexture>(texture);
            glTex.EnsureResourcesCreated();

            TextureTarget texTarget = glTex.TextureTarget;

            glBindTexture(texTarget, glTex.Texture);
            CheckLastError();

            bool isCompressed = FormatHelpers.IsCompressedFormat(texture.Format);
            uint blockSize = isCompressed ? 4u : 1u;

            uint blockAlignedWidth = Math.Max(width, blockSize);
            uint blockAlignedHeight = Math.Max(width, blockSize);

            uint rowPitch = FormatHelpers.GetRowPitch(blockAlignedWidth, texture.Format);
            uint depthPitch = FormatHelpers.GetDepthPitch(rowPitch, blockAlignedHeight, texture.Format);

            uint unpackAlignment = 4;
            if (!isCompressed)
            {
                unpackAlignment = FormatHelpers.GetSizeInBytes(glTex.Format);
            }
            if (unpackAlignment < 4)
            {
                glPixelStorei(PixelStoreParameter.UnpackAlignment, (int)unpackAlignment);
                CheckLastError();
            }

            if (texTarget == TextureTarget.Texture1D)
            {
                if (isCompressed)
                {
                    glCompressedTexSubImage1D(
                        TextureTarget.Texture1D,
                        (int)mipLevel,
                        (int)x,
                        width,
                        glTex.GLInternalFormat,
                        rowPitch,
                        dataPtr.ToPointer());
                    CheckLastError();
                }
                else
                {
                    glTexSubImage1D(
                        TextureTarget.Texture1D,
                        (int)mipLevel,
                        (int)x,
                        width,
                        glTex.GLPixelFormat,
                        glTex.GLPixelType,
                        dataPtr.ToPointer());
                    CheckLastError();
                }
            }
            else if (texTarget == TextureTarget.Texture1DArray)
            {
                if (isCompressed)
                {
                    glCompressedTexSubImage2D(
                        TextureTarget.Texture1DArray,
                        (int)mipLevel,
                        (int)x,
                        (int)arrayLayer,
                        width,
                        1,
                        glTex.GLInternalFormat,
                        rowPitch,
                        dataPtr.ToPointer());
                    CheckLastError();
                }
                else
                {
                    glTexSubImage2D(
                    TextureTarget.Texture1DArray,
                    (int)mipLevel,
                    (int)x,
                    (int)arrayLayer,
                    width,
                    1,
                    glTex.GLPixelFormat,
                    glTex.GLPixelType,
                    dataPtr.ToPointer());
                    CheckLastError();
                }
            }
            else if (texTarget == TextureTarget.Texture2D)
            {
                if (isCompressed)
                {
                    glCompressedTexSubImage2D(
                        TextureTarget.Texture2D,
                        (int)mipLevel,
                        (int)x,
                        (int)y,
                        width,
                        height,
                        glTex.GLInternalFormat,
                        depthPitch,
                        dataPtr.ToPointer());
                    CheckLastError();
                }
                else
                {
                    glTexSubImage2D(
                        TextureTarget.Texture2D,
                        (int)mipLevel,
                        (int)x,
                        (int)y,
                        width,
                        height,
                        glTex.GLPixelFormat,
                        glTex.GLPixelType,
                        dataPtr.ToPointer());
                    CheckLastError();
                }
            }
            else if (texTarget == TextureTarget.Texture2DArray)
            {
                if (isCompressed)
                {
                    glCompressedTexSubImage3D(
                        TextureTarget.Texture2DArray,
                        (int)mipLevel,
                        (int)x,
                        (int)y,
                        (int)arrayLayer,
                        width,
                        height,
                        1,
                        glTex.GLInternalFormat,
                        depthPitch,
                        dataPtr.ToPointer());
                    CheckLastError();
                }
                else
                {
                    glTexSubImage3D(
                        TextureTarget.Texture2DArray,
                        (int)mipLevel,
                        (int)x,
                        (int)y,
                        (int)arrayLayer,
                        width,
                        height,
                        1,
                        glTex.GLPixelFormat,
                        glTex.GLPixelType,
                        dataPtr.ToPointer());
                    CheckLastError();
                }
            }
            else if (texTarget == TextureTarget.Texture3D)
            {
                if (isCompressed)
                {
                    glCompressedTexSubImage3D(
                        TextureTarget.Texture3D,
                        (int)mipLevel,
                        (int)x,
                        (int)y,
                        (int)z,
                        width,
                        height,
                        depth,
                        glTex.GLInternalFormat,
                        depthPitch * depth,
                        dataPtr.ToPointer());
                    CheckLastError();
                }
                else
                {
                    glTexSubImage3D(
                        TextureTarget.Texture3D,
                        (int)mipLevel,
                        (int)x,
                        (int)y,
                        (int)z,
                        width,
                        height,
                        depth,
                        glTex.GLPixelFormat,
                        glTex.GLPixelType,
                        dataPtr.ToPointer());
                    CheckLastError();
                }
            }
            else if (texTarget == TextureTarget.TextureCubeMap)
            {
                TextureTarget cubeTarget = GetCubeTarget(arrayLayer);
                if (isCompressed)
                {
                    glCompressedTexSubImage2D(
                        cubeTarget,
                        (int)mipLevel,
                        (int)x,
                        (int)y,
                        width,
                        height,
                        glTex.GLInternalFormat,
                        depthPitch,
                        dataPtr.ToPointer());
                    CheckLastError();
                }
                else
                {
                    glTexSubImage2D(
                        cubeTarget,
                        (int)mipLevel,
                        (int)x,
                        (int)y,
                        width,
                        height,
                        glTex.GLPixelFormat,
                        glTex.GLPixelType,
                        dataPtr.ToPointer());
                    CheckLastError();
                }
            }
            else if (texTarget == TextureTarget.TextureCubeMapArray)
            {
                if (isCompressed)
                {
                    glCompressedTexSubImage3D(
                        TextureTarget.TextureCubeMapArray,
                        (int)mipLevel,
                        (int)x,
                        (int)y,
                        (int)arrayLayer,
                        width,
                        height,
                        1,
                        glTex.GLInternalFormat,
                        depthPitch,
                        dataPtr.ToPointer());
                    CheckLastError();
                }
                else
                {
                    glTexSubImage3D(
                        TextureTarget.TextureCubeMapArray,
                        (int)mipLevel,
                        (int)x,
                        (int)y,
                        (int)arrayLayer,
                        width,
                        height,
                        1,
                        glTex.GLPixelFormat,
                        glTex.GLPixelType,
                        dataPtr.ToPointer());
                    CheckLastError();
                }
            }
            else
            {
                throw new VeldridException($"Invalid OpenGL TextureTarget encountered: {glTex.TextureTarget}.");
            }

            if (unpackAlignment < 4)
            {
                glPixelStorei(PixelStoreParameter.UnpackAlignment, 4);
                CheckLastError();
            }
        }

        private TextureTarget GetCubeTarget(uint arrayLayer)
        {
            switch (arrayLayer)
            {
                case 0:
                    return TextureTarget.TextureCubeMapPositiveX;
                case 1:
                    return TextureTarget.TextureCubeMapNegativeX;
                case 2:
                    return TextureTarget.TextureCubeMapPositiveY;
                case 3:
                    return TextureTarget.TextureCubeMapNegativeY;
                case 4:
                    return TextureTarget.TextureCubeMapPositiveZ;
                case 5:
                    return TextureTarget.TextureCubeMapNegativeZ;
                default:
                    throw new VeldridException("Unexpected array layer in UpdateTexture called on a cubemap texture.");
            }
        }

        public void CopyBuffer(DeviceBuffer source, uint sourceOffset, DeviceBuffer destination, uint destinationOffset, uint sizeInBytes)
        {
            OpenGLBuffer srcGLBuffer = Util.AssertSubtype<DeviceBuffer, OpenGLBuffer>(source);
            OpenGLBuffer dstGLBuffer = Util.AssertSubtype<DeviceBuffer, OpenGLBuffer>(destination);

            srcGLBuffer.EnsureResourcesCreated();
            dstGLBuffer.EnsureResourcesCreated();

            if (_extensions.ARB_DirectStateAccess)
            {
                glCopyNamedBufferSubData(
                    srcGLBuffer.Buffer,
                    dstGLBuffer.Buffer,
                    (IntPtr)sourceOffset,
                    (IntPtr)destinationOffset,
                    sizeInBytes);
            }
            else
            {
                glBindBuffer(BufferTarget.CopyReadBuffer, srcGLBuffer.Buffer);
                CheckLastError();

                glBindBuffer(BufferTarget.CopyWriteBuffer, dstGLBuffer.Buffer);
                CheckLastError();

                glCopyBufferSubData(
                    BufferTarget.CopyReadBuffer,
                    BufferTarget.CopyWriteBuffer,
                    (IntPtr)sourceOffset,
                    (IntPtr)destinationOffset,
                    (IntPtr)sizeInBytes);
                CheckLastError();
            }
        }

        public void CopyTexture(
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
            OpenGLTexture srcGLTexture = Util.AssertSubtype<Texture, OpenGLTexture>(source);
            OpenGLTexture dstGLTexture = Util.AssertSubtype<Texture, OpenGLTexture>(destination);

            srcGLTexture.EnsureResourcesCreated();
            dstGLTexture.EnsureResourcesCreated();

            if (_extensions.ARB_CopyImage && depth == 1)
            {
                // glCopyImageSubData does not work properly when depth > 1, so use the awful roundabout copy.
                uint srcZOrLayer = Math.Max(srcBaseArrayLayer, srcZ);
                uint dstZOrLayer = Math.Max(dstBaseArrayLayer, dstZ);
                uint depthOrLayerCount = Math.Max(depth, layerCount);
                glCopyImageSubData(
                    srcGLTexture.Texture, srcGLTexture.TextureTarget, (int)srcMipLevel, (int)srcX, (int)srcY, (int)srcZOrLayer,
                    dstGLTexture.Texture, dstGLTexture.TextureTarget, (int)dstMipLevel, (int)dstX, (int)dstY, (int)dstZOrLayer,
                    width, height, depthOrLayerCount);
                CheckLastError();
            }
            else
            {
                for (uint layer = 0; layer < layerCount; layer++)
                {
                    uint srcLayer = layer + srcBaseArrayLayer;
                    uint dstLayer = layer + dstBaseArrayLayer;
                    CopyRoundabout(
                        srcGLTexture, dstGLTexture,
                        srcX, srcY, srcZ, srcMipLevel, srcLayer,
                        dstX, dstY, dstZ, dstMipLevel, dstLayer,
                        width, height, depth);
                }
            }
        }

        private void CopyRoundabout(
            OpenGLTexture srcGLTexture, OpenGLTexture dstGLTexture,
            uint srcX, uint srcY, uint srcZ, uint srcMipLevel, uint srcLayer,
            uint dstX, uint dstY, uint dstZ, uint dstMipLevel, uint dstLayer,
            uint width, uint height, uint depth)
        {
            bool isCompressed = FormatHelpers.IsCompressedFormat(srcGLTexture.Format);
            if (srcGLTexture.Format != dstGLTexture.Format)
            {
                throw new VeldridException("Copying to/from Textures with different formats is not supported.");
            }

            uint packAlignment = 4;
            uint sizeInBytes;
            TextureTarget srcTarget = srcGLTexture.TextureTarget;
            if (isCompressed)
            {
                glBindTexture(srcTarget, srcGLTexture.Texture);
                CheckLastError();

                int compressedSize;
                glGetTexLevelParameteriv(
                    srcTarget,
                    (int)srcMipLevel,
                    GetTextureParameter.TextureCompressedImageSize,
                    &compressedSize);
                CheckLastError();
                sizeInBytes = (uint)compressedSize;
            }
            else
            {
                uint pixelSize = FormatHelpers.GetSizeInBytes(srcGLTexture.Format);
                packAlignment = pixelSize;
                sizeInBytes = width * height * depth * pixelSize;
            }

            FixedStagingBlock block = _stagingMemoryPool.GetFixedStagingBlock(sizeInBytes);

            if (packAlignment < 4)
            {
                glPixelStorei(PixelStoreParameter.PackAlignment, (int)packAlignment);
                CheckLastError();
            }

            if (isCompressed)
            {
                if (_extensions.ARB_DirectStateAccess)
                {
                    glGetCompressedTextureImage(
                        srcGLTexture.Texture,
                        (int)srcMipLevel,
                        block.SizeInBytes,
                        block.Data);
                    CheckLastError();
                }
                else
                {
                    if (srcTarget == TextureTarget.Texture2DArray
                        || srcTarget == TextureTarget.Texture2DMultisampleArray
                        || srcTarget == TextureTarget.TextureCubeMapArray)
                    {
                        throw new VeldridException(
                            $"Copying an OpenGL compressed array Texture requires ARB_copy_image or ARB_direct_state_access.");
                    }

                    glBindTexture(srcTarget, srcGLTexture.Texture);
                    CheckLastError();

                    glGetCompressedTexImage(srcTarget, (int)srcMipLevel, block.Data);
                    CheckLastError();
                }

                glBindTexture(TextureTarget.Texture2D, dstGLTexture.Texture);
                CheckLastError();

                Util.GetMipDimensions(srcGLTexture, srcMipLevel, out uint mipWidth, out uint mipHeight, out uint mipDepth);
                uint fullRowPitch = FormatHelpers.GetRowPitch(mipWidth, srcGLTexture.Format);
                uint fullDepthPitch = FormatHelpers.GetDepthPitch(
                    fullRowPitch,
                    mipHeight,
                    srcGLTexture.Format);

                uint denseRowPitch = FormatHelpers.GetRowPitch(width, srcGLTexture.Format);
                uint denseDepthPitch = FormatHelpers.GetDepthPitch(denseRowPitch, height, srcGLTexture.Format);
                uint numRows = FormatHelpers.GetNumRows(height, srcGLTexture.Format);
                uint trueCopySize = denseRowPitch * numRows;
                FixedStagingBlock trueCopySrc = _stagingMemoryPool.GetFixedStagingBlock(trueCopySize);

                Util.CopyTextureRegion(
                    block.Data,
                    srcX, srcY, srcZ,
                    fullRowPitch, fullDepthPitch,
                    trueCopySrc.Data,
                    0, 0, 0,
                    denseRowPitch,
                    denseDepthPitch,
                    width, height, depth,
                    srcGLTexture.Format);

                UpdateTexture(
                    dstGLTexture,
                    (IntPtr)trueCopySrc.Data,
                    dstX, dstY, dstZ,
                    width, height, 1,
                    dstMipLevel, dstLayer);

                trueCopySrc.Free();
            }
            else // !isCompressed
            {
                if (_extensions.ARB_DirectStateAccess)
                {
                    glGetTextureSubImage(
                        srcGLTexture.Texture, (int)srcMipLevel, (int)srcX, (int)srcY, (int)srcZ,
                        width, height, depth,
                        srcGLTexture.GLPixelFormat, srcGLTexture.GLPixelType, block.SizeInBytes, block.Data);
                    CheckLastError();
                }
                else
                {
                    // We need to download the entire mip and then move the single copy region into
                    // the staging block we have.
                    uint pixelSize = FormatHelpers.GetSizeInBytes(srcGLTexture.Format);
                    Util.GetMipDimensions(srcGLTexture, srcMipLevel, out uint mipWidth, out uint mipHeight, out uint mipDepth);
                    uint fullMipSize = mipWidth * mipHeight * mipDepth * srcGLTexture.ArrayLayers * pixelSize;

                    FixedStagingBlock fullBlock = _stagingMemoryPool.GetFixedStagingBlock(fullMipSize);

                    glBindTexture(srcTarget, srcGLTexture.Texture);
                    CheckLastError();

                    glGetTexImage(
                        srcTarget,
                        (int)srcMipLevel,
                        srcGLTexture.GLPixelFormat,
                        srcGLTexture.GLPixelType,
                        fullBlock.Data);
                    CheckLastError();

                    uint fullRowSize = mipWidth * pixelSize; // Src row pitch
                    uint fullZSliceSize = fullRowSize * mipHeight; // Src depth pitch
                    uint denseRowSize = width * pixelSize; // Dst row pitch
                    uint denseZSliceSize = denseRowSize * height; // Dst depth pitch
                    byte* fullBlockSliceStart = (byte*)fullBlock.Data + fullZSliceSize * srcLayer;

                    Util.CopyTextureRegion(
                        fullBlockSliceStart,
                        srcX, srcY, srcZ,
                        fullRowSize,
                        fullZSliceSize,
                        block.Data,
                        0, 0, 0,
                        denseRowSize,
                        denseZSliceSize,
                        width, height, depth,
                        srcGLTexture.Format);

                    fullBlock.Free();
                }

                UpdateTexture(
                    dstGLTexture,
                    (IntPtr)block.Data,
                    dstX, dstY, dstZ,
                    width, height, depth, dstMipLevel, dstLayer);
            }

            if (packAlignment < 4)
            {
                glPixelStorei(PixelStoreParameter.PackAlignment, 4);
                CheckLastError();
            }

            block.Free();
        }

        private static void CopyWithFBO(
            OpenGLTexture srcGLTexture, OpenGLTexture dstGLTexture,
            uint srcX, uint srcY, uint srcZ, uint srcMipLevel, uint srcBaseArrayLayer,
            uint dstX, uint dstY, uint dstZ, uint dstMipLevel, uint dstBaseArrayLayer,
            uint width, uint height, uint depth, uint layerCount, uint layer)
        {
            TextureTarget dstTarget = dstGLTexture.TextureTarget;
            if (dstTarget == TextureTarget.Texture2D)
            {
                glBindFramebuffer(
                    FramebufferTarget.ReadFramebuffer,
                    srcGLTexture.GetFramebuffer(srcMipLevel, srcBaseArrayLayer + layer));
                CheckLastError();

                glBindTexture(TextureTarget.Texture2D, dstGLTexture.Texture);
                CheckLastError();

                glCopyTexSubImage2D(
                    TextureTarget.Texture2D,
                    (int)dstMipLevel,
                    (int)dstX, (int)dstY,
                    (int)srcX, (int)srcY,
                    width, height);
                CheckLastError();
            }
            else if (dstTarget == TextureTarget.Texture2DArray)
            {
                glBindFramebuffer(
                    FramebufferTarget.ReadFramebuffer,
                    srcGLTexture.GetFramebuffer(srcMipLevel, srcBaseArrayLayer + layerCount));

                glBindTexture(TextureTarget.Texture2DArray, dstGLTexture.Texture);
                CheckLastError();

                glCopyTexSubImage3D(
                    TextureTarget.Texture2DArray,
                    (int)dstMipLevel,
                    (int)dstX,
                    (int)dstY,
                    (int)(dstBaseArrayLayer + layer),
                    (int)srcX,
                    (int)srcY,
                    width,
                    height);
                CheckLastError();
            }
            else if (dstTarget == TextureTarget.Texture3D)
            {
                glBindTexture(TextureTarget.Texture3D, dstGLTexture.Texture);
                CheckLastError();

                for (uint i = srcZ; i < srcZ + depth; i++)
                {
                    glCopyTexSubImage3D(
                        TextureTarget.Texture3D,
                        (int)dstMipLevel,
                        (int)dstX,
                        (int)dstY,
                        (int)dstZ,
                        (int)srcX,
                        (int)srcY,
                        width,
                        height);
                }
                CheckLastError();
            }
        }
    }
}
