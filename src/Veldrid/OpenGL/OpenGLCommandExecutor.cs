using System;
using static Veldrid.OpenGLBinding.OpenGLNative;
using static Veldrid.OpenGL.OpenGLUtil;
using Veldrid.OpenGLBinding;
using System.Numerics;

namespace Veldrid.OpenGL
{
    internal unsafe class OpenGLCommandExecutor
    {
        private readonly OpenGLTextureSamplerManager _textureSamplerManager;
        private PrimitiveType _primitiveType;
        private DrawElementsType _drawElementsType;
        private OpenGLVertexBuffer[] _vertexBuffers = new OpenGLVertexBuffer[1];
        private uint[] _vertexAttribDivisors = new uint[1];
        private OpenGLPipeline _pipeline;
        private Framebuffer _fb;
        private bool _isSwapchainFB;
        private uint _vertexAttributesBound;
        private readonly Viewport[] _viewports = new Viewport[20];
        private OpenGLExtensions _extensions;
        private OpenGLResourceSet[] _resourceSets = new OpenGLResourceSet[1];

        public OpenGLCommandExecutor(OpenGLExtensions extensions)
        {
            _extensions = extensions;
            _textureSamplerManager = new OpenGLTextureSamplerManager(extensions);
        }

        public void Begin()
        {
            ClearManagedState();
        }

        private void ClearManagedState()
        {
            Util.ClearArray(_vertexBuffers);
            _vertexAttributesBound = 0;

            _primitiveType = 0;
            _drawElementsType = 0;
            _pipeline = null;
            _fb = null;
            _isSwapchainFB = false;
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

        public void ClearDepthTarget(float depth)
        {
            glClearDepth(depth);
            CheckLastError();

            glDepthMask(true);
            glClear(ClearBufferMask.DepthBufferBit);
            CheckLastError();
        }

        public void Draw(
            uint indexCount,
            uint instanceCount,
            uint indexStart,
            int vertexOffset,
            uint instanceStart)

        {
            FlushVertexLayouts();

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

        private void FlushVertexLayouts()
        {
            uint totalSlotsBound = 0;
            VertexLayoutDescription[] layouts = _pipeline.Description.ShaderSet.VertexLayouts;
            for (int i = 0; i < layouts.Length; i++)
            {
                VertexLayoutDescription input = layouts[i];
                OpenGLVertexBuffer vb = _vertexBuffers[i];
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
                    bool normalized = true;
                    glVertexAttribPointer(
                        actualSlot,
                        FormatHelpers.GetElementCount(element.Format),
                        OpenGLFormats.VdToGLVertexAttribPointerType(element.Format),
                        normalized,
                        (uint)_pipeline.VertexStrides[i],
                        (void*)offset);

                    uint stepRate = element.InstanceStepRate;
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

        public void SetIndexBuffer(IndexBuffer ib)
        {
            OpenGLIndexBuffer glIB = Util.AssertSubtype<IndexBuffer, OpenGLIndexBuffer>(ib);
            glIB.EnsureResourcesCreated();

            glBindBuffer(BufferTarget.ElementArrayBuffer, glIB.Buffer);
            CheckLastError();

            _drawElementsType = glIB.DrawElementsType;
        }

        public void SetPipeline(Pipeline pipeline)
        {
            if (_pipeline == pipeline)
            {
                return;
            }
            _pipeline = Util.AssertSubtype<Pipeline, OpenGLPipeline>(pipeline);
            PipelineDescription desc = _pipeline.Description;
            Util.ClearArray(_resourceSets); // Invalidate resource set bindings -- they may be invalid.

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

                glDepthFunc(OpenGLFormats.VdToGLDepthFunction(dss.ComparisonKind));
                CheckLastError();
            }

            glDepthMask(dss.DepthWriteEnabled);
            CheckLastError();

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
            glUseProgram(_pipeline.Program);
            CheckLastError();

            int vertexStridesCount = _pipeline.VertexStrides.Length;
            Util.EnsureArraySize(ref _vertexBuffers, (uint)vertexStridesCount);

            uint totalVertexElements = 0;
            for (int i = 0; i < desc.ShaderSet.VertexLayouts.Length; i++)
            {
                totalVertexElements += (uint)desc.ShaderSet.VertexLayouts[i].Elements.Length;
            }
            Util.EnsureArraySize(ref _vertexAttribDivisors, totalVertexElements);

            Util.EnsureArraySize(ref _resourceSets, (uint)desc.ResourceLayouts.Length);
        }

        public void SetResourceSet(uint slot, ResourceSet rs)
        {
            if (_resourceSets[slot] == rs)
            {
                return;
            }

            _pipeline.EnsureResourcesCreated();

            OpenGLResourceSet glResourceSet = Util.AssertSubtype<ResourceSet, OpenGLResourceSet>(rs);
            _resourceSets[slot] = glResourceSet;

            uint ubBaseIndex = GetUniformBaseIndex(slot);

            for (uint element = 0; element < glResourceSet.Resources.Length; element++)
            {
                BindableResource resource = glResourceSet.Resources[(int)element];
                if (resource is OpenGLUniformBuffer glUB)
                {
                    OpenGLUniformBinding uniformBindingInfo = _pipeline.GetUniformBindingForSlot(slot, element);
                    glUniformBlockBinding(_pipeline.Program, uniformBindingInfo.BlockLocation, ubBaseIndex + element);
                    CheckLastError();

                    glBindBufferRange(BufferRangeTarget.UniformBuffer, ubBaseIndex + element, glUB.Buffer, IntPtr.Zero, (UIntPtr)glUB.SizeInBytes);
                    CheckLastError();
                }
                else if (resource is OpenGLTextureView glTexView)
                {
                    OpenGLTextureBindingSlotInfo textureBindingInfo = _pipeline.GetTextureBindingInfo(slot, element);
                    _textureSamplerManager.SetTexture((uint)textureBindingInfo.RelativeIndex, glTexView);

                    glUseProgram(_pipeline.Program); // TODO This is broken, why do i need to set this again?
                    CheckLastError();

                    glUniform1i(textureBindingInfo.UniformLocation, textureBindingInfo.RelativeIndex);
                    CheckLastError();
                }
                else if (resource is OpenGLSampler glSampler)
                {
                    glSampler.EnsureResourcesCreated();
                    OpenGLTextureBindingSlotInfo samplerBindingInfo = _pipeline.GetSamplerBindingInfo(slot, element);
                    _textureSamplerManager.SetSampler((uint)samplerBindingInfo.RelativeIndex, glSampler);
                }
            }
        }

        private uint GetUniformBaseIndex(uint slot)
        {
            uint ret = 0;
            for (uint i = 0; i < slot; i++)
            {
                ret += _pipeline.GetUniformBufferCount(i);
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

        public void SetVertexBuffer(uint index, VertexBuffer vb)
        {
            OpenGLVertexBuffer glVB = Util.AssertSubtype<VertexBuffer, OpenGLVertexBuffer>(vb);
            glVB.EnsureResourcesCreated();

            Util.EnsureArraySize(ref _vertexBuffers, index + 1);
            _vertexBuffers[index] = glVB;
        }

        public void SetViewport(uint index, ref Viewport viewport)
        {
            _viewports[(int)index] = viewport;
            glViewportIndexed(index, viewport.X, viewport.Y, viewport.Width, viewport.Height);
            CheckLastError();

            glDepthRangeIndexed(index, viewport.MinDepth, viewport.MaxDepth);
            CheckLastError();
        }

        public void UpdateBuffer(Buffer buffer, uint bufferOffsetInBytes, StagingBlock stagingBlock)
        {
            OpenGLBuffer glBuffer = Util.AssertSubtype<Buffer, OpenGLBuffer>(buffer);
            glBuffer.EnsureResourcesCreated();

            if (_extensions.ARB_DirectStateAccess)
            {
                fixed (byte* dataPtr = stagingBlock.Array)
                {
                    glNamedBufferSubData(
                        glBuffer.Buffer,
                        (IntPtr)bufferOffsetInBytes,
                        stagingBlock.SizeInBytes,
                        dataPtr);
                }
                CheckLastError();
            }
            else
            {
                glBindBuffer(glBuffer.Target, glBuffer.Buffer);
                CheckLastError();
                fixed (byte* dataPtr = &stagingBlock.Array[0])
                {
                    glBufferSubData(
                        glBuffer.Target,
                        (IntPtr)bufferOffsetInBytes,
                        (UIntPtr)stagingBlock.SizeInBytes,
                        dataPtr);
                }
                CheckLastError();
            }

            stagingBlock.Pool.Free(stagingBlock);
        }

        public void UpdateTexture2D(
            Texture2D texture2D,
            StagingBlock stagingBlock,
            uint x,
            uint y,
            uint width,
            uint height,
            uint mipLevel,
            uint arrayLayer)
        {
            OpenGLTexture2D glTex2D = Util.AssertSubtype<Texture2D, OpenGLTexture2D>(texture2D);
            glTex2D.EnsureResourcesCreated();

            glBindTexture(TextureTarget.Texture2D, glTex2D.Texture);
            CheckLastError();

            uint pixelSize = FormatHelpers.GetSizeInBytes(glTex2D.Format);
            if (pixelSize < 4)
            {
                glPixelStorei(PixelStoreParameter.UnpackAlignment, (int)pixelSize);
                CheckLastError();
            }

            fixed (byte* dataPtr = stagingBlock.Array)
            {
                glTexSubImage2D(
                    TextureTarget.Texture2D,
                    (int)mipLevel,
                    (int)x,
                    (int)y,
                    width,
                    height,
                    glTex2D.GLPixelFormat,
                    glTex2D.GLPixelType,
                    dataPtr);
                CheckLastError();
            }

            stagingBlock.Pool.Free(stagingBlock);

            if (pixelSize < 4)
            {
                glPixelStorei(PixelStoreParameter.UnpackAlignment, 4);
                CheckLastError();
            }
        }

        public void UpdateTextureCube(
            TextureCube textureCube,
            StagingBlock stagingBlock,
            CubeFace face,
            uint x,
            uint y,
            uint width,
            uint height,
            uint mipLevel,
            uint arrayLayer)
        {
            OpenGLTextureCube glTexCube = Util.AssertSubtype<TextureCube, OpenGLTextureCube>(textureCube);
            glTexCube.EnsureResourcesCreated();

            glBindTexture(TextureTarget.TextureCubeMap, glTexCube.Texture);
            CheckLastError();

            uint pixelSize = FormatHelpers.GetSizeInBytes(glTexCube.Format);
            if (pixelSize < 4)
            {
                glPixelStorei(PixelStoreParameter.UnpackAlignment, (int)pixelSize);
                CheckLastError();
            }

            TextureTarget target = GetCubeFaceTarget(face);

            fixed (byte* dataPtr = stagingBlock.Array)
            {
                glTexSubImage2D(
                    target,
                    (int)mipLevel,
                    (int)x,
                    (int)y,
                    width,
                    height,
                    glTexCube.GLPixelFormat,
                    glTexCube.GLPixelType,
                    dataPtr);
            }
            CheckLastError();

            stagingBlock.Pool.Free(stagingBlock);

            if (pixelSize < 4)
            {
                glPixelStorei(PixelStoreParameter.UnpackAlignment, 4);
                CheckLastError();
            }
        }

        private TextureTarget GetCubeFaceTarget(CubeFace face)
        {
            switch (face)
            {
                case CubeFace.NegativeX:
                    return TextureTarget.TextureCubeMapNegativeX;
                case CubeFace.PositiveX:
                    return TextureTarget.TextureCubeMapPositiveX;
                case CubeFace.NegativeY:
                    return TextureTarget.TextureCubeMapNegativeY;
                case CubeFace.PositiveY:
                    return TextureTarget.TextureCubeMapPositiveY;
                case CubeFace.NegativeZ:
                    return TextureTarget.TextureCubeMapPositiveZ;
                case CubeFace.PositiveZ:
                    return TextureTarget.TextureCubeMapNegativeZ;
                default:
                    throw Illegal.Value<CubeFace>();
            }
        }
    }
}
