using OpenTK.Graphics.OpenGL;
using System;
using System.Diagnostics;

namespace Veldrid.Graphics.OpenGL
{
    public class OpenGLFramebuffer : OpenGLFramebufferBase, Framebuffer, IDisposable
    {
        private readonly int _framebufferID;

        private OpenGLTexture2D[] _colorTextures = new OpenGLTexture2D[RenderContext.MaxRenderTargets];
        private OpenGLTexture2D _depthTexture;

        public int Width => _colorTextures[0].Width;

        public int Height => _colorTextures[0].Height;

        DeviceTexture2D Framebuffer.ColorTexture
        {
            get { return GetColorTexture(0); }
            set { AttachColorTexture(0, value); }
        }

        DeviceTexture2D Framebuffer.DepthTexture
        {
            get
            {
                return _depthTexture;
            }
            set
            {
                if (value != null)
                {
                    Debug.Assert(value is OpenGLTexture2D);
                    _depthTexture = (OpenGLTexture2D)value;
                }
                else
                {
                    _depthTexture = null;
                }

                AttachDepthTexture();
            }
        }

        public bool HasDepthAttachment => _depthTexture != null;

        public OpenGLFramebuffer()
        {
            _framebufferID = GL.GenFramebuffer();
        }

        public OpenGLFramebuffer(OpenGLTexture2D colorTexture, OpenGLTexture2D depthTexture)
            : this()
        {
            _depthTexture = depthTexture;

            Bind();
            AttachColorTexture(0, colorTexture);
            AttachDepthTexture();
            Unbind();
        }

        public DeviceTexture2D GetColorTexture(int index)
        {
            return _colorTextures[index];
        }

        public void AttachColorTexture(int index, DeviceTexture2D texture)
        {
            Bind();
            Debug.Assert(texture is OpenGLTexture2D);
            OpenGLTexture2D glTex = (OpenGLTexture2D)texture;
            _colorTextures[index] = glTex;
            GL.ActiveTexture(TextureUnit.Texture0);
            glTex.Bind();
            GL.FramebufferTexture2D(
                FramebufferTarget.Framebuffer,
                FramebufferAttachment.ColorAttachment0 + index,
                TextureTarget.Texture2D,
                glTex.ID,
                0);
            GL.BindTexture(TextureTarget.Texture2D, 0);
            // TODO: I'm pretty sure this is supposed to be using glDrawBuffers (plural).
            GL.DrawBuffer(DrawBufferMode.ColorAttachment0 + index);
            Unbind();
        }

        private void AttachDepthTexture()
        {
            Bind();
            int depthTextureID;
            if (_depthTexture != null)
            {
                _depthTexture.Bind();
                depthTextureID = _depthTexture.ID;
            }
            else
            {
                depthTextureID = 0;
            }
            GL.FramebufferTexture2D(
                FramebufferTarget.Framebuffer,
                FramebufferAttachment.DepthAttachment,
                TextureTarget.Texture2D,
                depthTextureID,
                0);
            Unbind();
        }

        private void Bind()
        {
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, _framebufferID);
        }

        private static void Unbind()
        {
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
        }

        internal override void Apply()
        {
            Bind();
            FramebufferErrorCode errorCode = GL.CheckFramebufferStatus(FramebufferTarget.Framebuffer);
            if (errorCode != FramebufferErrorCode.FramebufferComplete)
            {
                throw new VeldridException("Framebuffer was not complete: " + errorCode);
            }
        }

        public void Dispose()
        {
            GL.DeleteFramebuffer(_framebufferID);
        }
    }
}
