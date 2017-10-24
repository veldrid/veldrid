using System;
using static Vd2.OpenGLBinding.OpenGLNative;
using static Vd2.OpenGL.OpenGLUtil;
using Vd2.OpenGLBinding;

namespace Vd2.OpenGL
{
    internal unsafe class OpenGLCommandExecutor
    {
        private PrimitiveType _primitiveType;
        private DrawElementsType _drawElementsType;
        private OpenGLVertexBuffer[] _vertexBuffers = new OpenGLVertexBuffer[10];
        private OpenGLPipeline _pipeline;

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
            _primitiveType = 0;
            _drawElementsType = 0;
        }

        private void ClearColorTarget(ClearColorTargetEntry ccte)
        {
            glDrawBuffer((DrawBufferMode)((uint)DrawBufferMode.ColorAttachment0 + ccte.Index));
            CheckLastError();

            RgbaFloat color = ccte.ClearColor;
            glClearColor(color.R, color.G, color.B, color.A);
            CheckLastError();

            glClear(ClearBufferMask.ColorBufferBit);
            CheckLastError();
        }

        private void ClearDepthTarget(ClearDepthTargetEntry cdte)
        {
            glClearDepth(cdte.Depth);
        }

        private void Draw(DrawEntry de)
        {
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
            }
            else if (fb is OpenGLSwapchainFramebuffer)
            {
                glBindFramebuffer(FramebufferTarget.Framebuffer, 0);
                CheckLastError();
            }
            else
            {
                throw new VdException("Invalid Framebuffer type: " + fb.GetType().Name);
            }
        }

        private void SetIndexBuffer(SetIndexBufferEntry sibe)
        {
            OpenGLIndexBuffer glIB = Util.AssertSubtype<IndexBuffer, OpenGLIndexBuffer>(sibe.IndexBuffer);
            glIB.EnsureResourcesCreated();

            glBindBuffer(BufferTarget.ElementArrayBuffer, glIB.Buffer);
            CheckLastError();
        }

        private void SetPipeline(SetPipelineEntry spe)
        {
            _pipeline = Util.AssertSubtype<Pipeline, OpenGLPipeline>(spe.Pipeline);
        }

        private void SetResourceSet(SetResourceSetEntry srse)
        {
            throw new NotImplementedException();
        }

        private void SetScissorRect(SetScissorRectEntry ssre)
        {
            glScissorIndexed(
                ssre.Index,
                (int)ssre.X,
                (int)(ssre.Y + ssre.Height),
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

        private void SetViewport(SetViewportEntry sve)
        {
            glViewportIndexed(sve.Index, sve.Viewport.X, sve.Viewport.Y, sve.Viewport.Width, sve.Viewport.Height);
            CheckLastError();

            glDepthRangeIndexed(sve.Index, sve.Viewport.MinDepth, sve.Viewport.MaxDepth);
            CheckLastError();
        }

        private void UpdateBuffer(UpdateBufferEntry ube)
        {
            OpenGLBuffer glBuffer = Util.AssertSubtype<Buffer, OpenGLBuffer>(ube.Buffer);
            glBuffer.EnsureResourcesCreated();

            glNamedBufferSubData(
                glBuffer.Buffer,
                (IntPtr)ube.BufferOffsetInBytes,
                ube.StagingBlock.SizeInBytes,
                ube.StagingBlock.Data.ToPointer());
            CheckLastError();

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
                ut2de.StagingBlock.Data.ToPointer());
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
                utce.StagingBlock.Data.ToPointer());
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
                    return TextureTarget.TextureCubeMapNegativeZ;
                case CubeFace.PositiveZ:
                    return TextureTarget.TextureCubeMapPositiveZ;
                default:
                    throw Illegal.Value<CubeFace>();
            }
        }
    }
}
