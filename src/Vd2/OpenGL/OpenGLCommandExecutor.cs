using System;
using static Vd2.OpenGLBinding.OpenGLNative;
using static Vd2.OpenGL.OpenGLUtil;
using Vd2.OpenGLBinding;
using System.Numerics;

namespace Vd2.OpenGL
{
    internal unsafe class OpenGLCommandExecutor
    {
        private readonly OpenGLTextureSamplerManager _textureSamplerManager;
        private PrimitiveType _primitiveType;
        private DrawElementsType _drawElementsType;
        private OpenGLVertexBuffer[] _vertexBuffers = new OpenGLVertexBuffer[10]; // TODO: Real limit
        private readonly uint[] _vertexAttribDivisors = new uint[10]; // TODO: Real limit
        private OpenGLPipeline _pipeline;
        private Framebuffer _fb;
        private bool _isSwapchainFB;
        private uint _vertexAttributesBound;

        public OpenGLCommandExecutor(OpenGLExtensions extensions)
        {
            _extensions = extensions;
            _textureSamplerManager = new OpenGLTextureSamplerManager(extensions);
        }

        public void Execute(OpenGLCommandEntryList list)
        {
            foreach (OpenGLCommandEntry command in list.Commands)
            {
                switch (command)
                {
                    case BeginEntry be:
                        Begin(be);
                        break;
                    case ClearColorTargetEntry ccte:
                        ClearColorTarget(ccte);
                        break;
                    case ClearDepthTargetEntry cdte:
                        ClearDepthTarget(cdte);
                        break;
                    case DrawEntry de:
                        Draw(de);
                        break;
                    case EndEntry ee:
                        End(ee);
                        break;
                    case SetFramebufferEntry sfbe:
                        SetFramebuffer(sfbe);
                        break;
                    case SetIndexBufferEntry sibe:
                        SetIndexBuffer(sibe);
                        break;
                    case SetPipelineEntry spe:
                        SetPipeline(spe);
                        break;
                    case SetResourceSetEntry srse:
                        SetResourceSet(srse);
                        break;
                    case SetScissorRectEntry ssre:
                        SetScissorRect(ssre);
                        break;
                    case SetVertexBufferEntry svbe:
                        SetVertexBuffer(svbe);
                        break;
                    case SetViewportEntry sve:
                        SetViewport(sve);
                        break;
                    case UpdateBufferEntry ube:
                        UpdateBuffer(ube);
                        break;
                    case UpdateTexture2DEntry ut2de:
                        UpdateTexture2D(ut2de);
                        break;
                    case UpdateTextureCubeEntry utce:
                        UpdateTextureCube(utce);
                        break;
                    default:
                        throw new NotImplementedException("Command type not handled: " + command.GetType().Name);
                }
            }
        }

        private void Begin(BeginEntry be)
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

        private void ClearColorTarget(ClearColorTargetEntry ccte)
        {
            if (!_isSwapchainFB)
            {
                glDrawBuffer((DrawBufferMode)((uint)DrawBufferMode.ColorAttachment0 + ccte.Index));
                CheckLastError();
            }

            RgbaFloat color = ccte.ClearColor;
            glClearColor(color.R, color.G, color.B, color.A);
            CheckLastError();

            glClear(ClearBufferMask.ColorBufferBit);
            CheckLastError();
        }

        private void ClearDepthTarget(ClearDepthTargetEntry cdte)
        {
            glClearDepth(cdte.Depth);
            CheckLastError();

            glDepthMask(true);
            glClear(ClearBufferMask.DepthBufferBit);
            CheckLastError();
        }

        private void Draw(DrawEntry de)
        {
            FlushVertexLayouts();

            uint indexSize = _drawElementsType == DrawElementsType.UnsignedShort ? 2u : 4u;
            void* indices = new IntPtr(de.IndexStart * indexSize).ToPointer();

            if (de.InstanceCount == 1)
            {
                if (de.VertexOffset == 0)
                {
                    glDrawElements(_primitiveType, de.IndexCount, _drawElementsType, indices);
                    CheckLastError();
                }
                else
                {
                    glDrawElementsBaseVertex(_primitiveType, de.IndexCount, _drawElementsType, indices, de.VertexOffset);
                    CheckLastError();
                }
            }
            else
            {
                if (de.VertexOffset == 0)
                {
                    glDrawElementsInstanced(_primitiveType, de.IndexCount, _drawElementsType, indices, de.InstanceCount);
                    CheckLastError();
                }
                else
                {
                    glDrawElementsInstancedBaseVertex(
                        _primitiveType,
                        de.IndexCount,
                        _drawElementsType,
                        indices,
                        de.InstanceCount,
                        de.VertexOffset);
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

        private void End(EndEntry ee)
        {
        }

        private void SetFramebuffer(SetFramebufferEntry sfbe)
        {
            Framebuffer fb = sfbe.Framebuffer;
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
                throw new VdException("Invalid Framebuffer type: " + fb.GetType().Name);
            }

            _fb = fb;
        }

        private void SetIndexBuffer(SetIndexBufferEntry sibe)
        {
            OpenGLIndexBuffer glIB = Util.AssertSubtype<IndexBuffer, OpenGLIndexBuffer>(sibe.IndexBuffer);
            glIB.EnsureResourcesCreated();

            glBindBuffer(BufferTarget.ElementArrayBuffer, glIB.Buffer);
            CheckLastError();

            _drawElementsType = glIB.DrawElementsType;
        }

        private void SetPipeline(SetPipelineEntry spe)
        {
            _pipeline = Util.AssertSubtype<Pipeline, OpenGLPipeline>(spe.Pipeline);
            PipelineDescription desc = _pipeline.Description;

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
        }

        private void SetResourceSet(SetResourceSetEntry srse)
        {
            _pipeline.EnsureResourcesCreated();

            OpenGLResourceSet glResourceSet = Util.AssertSubtype<ResourceSet, OpenGLResourceSet>(srse.ResourceSet);
            for (uint slot = 0; slot < glResourceSet.Resources.Length; slot++)
            {
                BindableResource resource = glResourceSet.Resources[(int)slot];
                if (resource is OpenGLUniformBuffer glUB)
                {
                    OpenGLUniformBinding uniformBindingInfo = _pipeline.GetUniformBindingForSlot(slot);
                    glUniformBlockBinding(_pipeline.Program, uniformBindingInfo.BlockLocation, slot);
                    CheckLastError();

                    glBindBufferRange(BufferRangeTarget.UniformBuffer, slot, glUB.Buffer, IntPtr.Zero, (UIntPtr)glUB.SizeInBytes);
                    CheckLastError();
                }
                else if (resource is OpenGLTextureView glTexView)
                {
                    OpenGLTextureBindingSlotInfo textureBindingInfo = _pipeline.GetTextureBindingInfo(slot);
                    _textureSamplerManager.SetTexture((uint)textureBindingInfo.RelativeIndex, glTexView);

                    glUseProgram(_pipeline.Program); // TODO This is broken, why do i need to set this again?
                    CheckLastError();

                    glUniform1i(textureBindingInfo.UniformLocation, textureBindingInfo.RelativeIndex);
                    CheckLastError();
                }
                else if (resource is OpenGLSampler glSampler)
                {
                    glSampler.EnsureResourcesCreated();
                    OpenGLTextureBindingSlotInfo samplerBindingInfo = _pipeline.GetSamplerBindingInfo(slot);
                    _textureSamplerManager.SetSampler((uint)samplerBindingInfo.RelativeIndex, glSampler);
                }
            }
        }

        private void SetScissorRect(SetScissorRectEntry ssre)
        {
            glScissorIndexed(
                ssre.Index,
                (int)ssre.X,
                (int)(_viewports[(int)ssre.Index].Height - (int)ssre.Height - ssre.Y),
                ssre.Width,
                ssre.Height);
            CheckLastError();
        }

        private void SetVertexBuffer(SetVertexBufferEntry svbe)
        {
            OpenGLVertexBuffer glVB = Util.AssertSubtype<VertexBuffer, OpenGLVertexBuffer>(svbe.VertexBuffer);
            glVB.EnsureResourcesCreated();

            Util.EnsureArraySize(ref _vertexBuffers, svbe.Index + 1);
            _vertexBuffers[svbe.Index] = glVB;
        }
        private readonly Viewport[] _viewports = new Viewport[20];
        private OpenGLExtensions _extensions;

        private void SetViewport(SetViewportEntry sve)
        {
            _viewports[(int)sve.Index] = sve.Viewport;
            glViewportIndexed(sve.Index, sve.Viewport.X, sve.Viewport.Y, sve.Viewport.Width, sve.Viewport.Height);
            CheckLastError();

            glDepthRangeIndexed(sve.Index, sve.Viewport.MinDepth, sve.Viewport.MaxDepth);
            CheckLastError();
        }

        private void UpdateBuffer(UpdateBufferEntry ube)
        {
            OpenGLBuffer glBuffer = Util.AssertSubtype<Buffer, OpenGLBuffer>(ube.Buffer);
            glBuffer.EnsureResourcesCreated();

            if (_extensions.ARB_DirectStateAccess)
            {
                glNamedBufferSubData(
                    glBuffer.Buffer,
                    (IntPtr)ube.BufferOffsetInBytes,
                    ube.StagingBlock.SizeInBytes,
                    ube.StagingBlock.Data);
                CheckLastError();
            }
            else
            {
                glBindBuffer(glBuffer.Target, glBuffer.Buffer);
                glBufferSubData(
                    glBuffer.Target,
                    (IntPtr)ube.BufferOffsetInBytes,
                    (UIntPtr)ube.StagingBlock.SizeInBytes,
                    ube.StagingBlock.Data);
            }

            ube.StagingBlock.Pool.Free(ube.StagingBlock);
        }

        private void UpdateTexture2D(UpdateTexture2DEntry ut2de)
        {
            OpenGLTexture2D glTex2D = Util.AssertSubtype<Texture2D, OpenGLTexture2D>(ut2de.Texture2D);
            glTex2D.EnsureResourcesCreated();

            glBindTexture(TextureTarget.Texture2D, glTex2D.Texture);
            CheckLastError();

            uint pixelSize = FormatHelpers.GetSizeInBytes(glTex2D.Format);
            if (pixelSize < 4)
            {
                glPixelStorei(PixelStoreParameter.UnpackAlignment, (int)pixelSize);
                CheckLastError();
            }

            glTexSubImage2D(
                TextureTarget.Texture2D,
                (int)ut2de.MipLevel,
                (int)ut2de.X,
                (int)ut2de.Y,
                ut2de.Width,
                ut2de.Height,
                glTex2D.GLPixelFormat,
                glTex2D.GLPixelType,
                ut2de.StagingBlock.Data);
            CheckLastError();

            ut2de.StagingBlock.Pool.Free(ut2de.StagingBlock);

            if (pixelSize < 4)
            {
                glPixelStorei(PixelStoreParameter.UnpackAlignment, 4);
                CheckLastError();
            }
        }

        private void UpdateTextureCube(UpdateTextureCubeEntry utce)
        {
            OpenGLTextureCube glTexCube = Util.AssertSubtype<TextureCube, OpenGLTextureCube>(utce.TextureCube);
            glTexCube.EnsureResourcesCreated();

            glBindTexture(TextureTarget.TextureCubeMap, glTexCube.Texture);
            CheckLastError();

            uint pixelSize = FormatHelpers.GetSizeInBytes(glTexCube.Format);
            if (pixelSize < 4)
            {
                glPixelStorei(PixelStoreParameter.UnpackAlignment, (int)pixelSize);
                CheckLastError();
            }

            TextureTarget target = GetCubeFaceTarget(utce.Face);

            glTexSubImage2D(
                target,
                (int)utce.MipLevel,
                (int)utce.X,
                (int)utce.Y,
                utce.Width,
                utce.Height,
                glTexCube.GLPixelFormat,
                glTexCube.GLPixelType,
                utce.StagingBlock.Data);
            CheckLastError();

            utce.StagingBlock.Pool.Free(utce.StagingBlock);

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
