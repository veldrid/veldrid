using OpenTK.Graphics.OpenGL;
using System;
using System.Diagnostics;

namespace Veldrid.Graphics.OpenGL
{
    public class OpenGLFramebuffer : Framebuffer, IDisposable
    {
        private readonly int _framebufferID;

        private OpenGLTexture _colorTexture;
        private OpenGLTexture _depthTexture;

        DeviceTexture Framebuffer.ColorTexture
        {
            get
            {
                return _colorTexture;
            }
            set
            {
                if (value == null)
                {
                    _colorTexture = null;
                }

                Debug.Assert(value is OpenGLTexture);
                _colorTexture = (OpenGLTexture)value;
                AttachColorTexture();
            }
        }

        DeviceTexture Framebuffer.DepthTexture
        {
            get
            {
                return _depthTexture;
            }

            set
            {
                if (value == null)
                {
                    _depthTexture = null;
                }

                Debug.Assert(value is OpenGLTexture);
                _depthTexture = (OpenGLTexture)value;
                AttachDepthTexture();
            }
        }

        public int Width => _colorTexture.Width;

        public int Height => _colorTexture.Height;

        public OpenGLFramebuffer()
        {
            _framebufferID = GL.GenFramebuffer();
        }

        public OpenGLFramebuffer(OpenGLTexture colorTexture, OpenGLTexture depthTexture)
            : this()
        {
            _colorTexture = colorTexture;
            _depthTexture = depthTexture;

            Bind();
            AttachColorTexture();
            AttachDepthTexture();
            Unbind();
        }

        private void AttachDepthTexture()
        {
            Bind();
            _depthTexture.Apply();
            GL.FramebufferTexture2D(
                FramebufferTarget.Framebuffer,
                FramebufferAttachment.DepthAttachment,
                TextureTarget.Texture2D,
                _depthTexture.TextureID,
                0);
            Unbind();
        }

        private void AttachColorTexture()
        {
            GL.ActiveTexture(TextureUnit.Texture0);
            _colorTexture.Apply();
            GL.FramebufferTexture2D(
                FramebufferTarget.Framebuffer,
                FramebufferAttachment.ColorAttachment0,
                TextureTarget.Texture2D,
                _colorTexture.TextureID,
                0);
            GL.BindTexture(TextureTarget.Texture2D, 0);
            if (_colorTexture == null)
            {
                GL.DrawBuffer(DrawBufferMode.None);
            }
            else
            {
                GL.DrawBuffer(DrawBufferMode.ColorAttachment0);
            }

        }

        private void Bind()
        {
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, _framebufferID);
        }

        private static void Unbind()
        {
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
        }

        public void Apply()
        {
            Bind();
            FramebufferErrorCode errorCode = GL.CheckFramebufferStatus(FramebufferTarget.Framebuffer);
            if (errorCode != FramebufferErrorCode.FramebufferComplete)
            {
                throw new InvalidOperationException("Framebuffer was not complete: " + errorCode);
            }
        }

        public void Dispose()
        {
            GL.DeleteFramebuffer(_framebufferID);
        }
    }
}
