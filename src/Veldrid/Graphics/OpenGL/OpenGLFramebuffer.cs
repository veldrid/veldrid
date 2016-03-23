using OpenTK.Graphics.OpenGL;
using System;

namespace Veldrid.Graphics.OpenGL
{
    public class OpenGLFramebuffer : Framebuffer, IDisposable
    {
        private readonly OpenGLTexture _colorTexture;
        private readonly OpenGLTexture _depthTexture;
        private readonly int _framebufferID;

        public DeviceTexture ColorTexture => _colorTexture;
        public DeviceTexture DepthTexture => _depthTexture;

        public OpenGLFramebuffer(OpenGLTexture colorTexture, OpenGLTexture depthTexture)
        {
            _colorTexture = colorTexture;
            _depthTexture = depthTexture;
            _framebufferID = GL.GenFramebuffer();
            
            Bind();

            GL.ActiveTexture(TextureUnit.Texture0);
            _colorTexture.Apply();
            GL.FramebufferTexture2D(
                FramebufferTarget.Framebuffer,
                FramebufferAttachment.ColorAttachment0,
                TextureTarget.Texture2D,
                _colorTexture.TextureID,
                0);
            GL.BindTexture(TextureTarget.Texture2D, 0);

            _depthTexture.Apply();
            GL.FramebufferTexture2D(
                FramebufferTarget.Framebuffer,
                FramebufferAttachment.DepthAttachment,
                TextureTarget.Texture2D,
                _depthTexture.TextureID,
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
