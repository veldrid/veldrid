using OpenTK.Graphics.OpenGL;
using System;
using System.Diagnostics;

namespace Veldrid.Graphics.OpenGL
{
    public class OpenGLFramebuffer : OpenGLFramebufferBase, Framebuffer, IDisposable
    {
        private readonly int _framebufferID;

        private OpenGLTexture2D _colorTexture;
        private OpenGLTexture2D _depthTexture;

        DeviceTexture2D Framebuffer.ColorTexture
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

                Debug.Assert(value is OpenGLTexture2D);
                _colorTexture = (OpenGLTexture2D)value;
                AttachColorTexture();
            }
        }

        DeviceTexture2D Framebuffer.DepthTexture
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

                Debug.Assert(value is OpenGLTexture2D);
                _depthTexture = (OpenGLTexture2D)value;
                AttachDepthTexture();
            }
        }

        public int Width => _colorTexture.Width;

        public int Height => _colorTexture.Height;

        public OpenGLFramebuffer()
        {
            _framebufferID = GL.GenFramebuffer();
        }

        public OpenGLFramebuffer(OpenGLTexture2D colorTexture, OpenGLTexture2D depthTexture)
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
            _depthTexture.Bind();
            GL.FramebufferTexture2D(
                FramebufferTarget.Framebuffer,
                FramebufferAttachment.DepthAttachment,
                TextureTarget.Texture2D,
                _depthTexture.ID,
                0);
            Unbind();
        }

        private void AttachColorTexture()
        {
            GL.ActiveTexture(TextureUnit.Texture0);
            _colorTexture.Bind();
            GL.FramebufferTexture2D(
                FramebufferTarget.Framebuffer,
                FramebufferAttachment.ColorAttachment0,
                TextureTarget.Texture2D,
                _colorTexture.ID,
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

        internal override void Apply()
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
