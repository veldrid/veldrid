using OpenTK.Graphics.OpenGL;
using System;

namespace Veldrid.Graphics.OpenGL
{
    public class OpenGLFramebuffer : Framebuffer, IDisposable
    {
        private readonly int _colorTextureID;
        private readonly int _depthTextureID;
        private readonly int _framebufferID;

        public OpenGLFramebuffer(int colorTextureID, int depthTextureID)
        {
            _colorTextureID = colorTextureID;
            _depthTextureID = depthTextureID;
            _framebufferID = GL.GenFramebuffer();
        }

        public void Apply()
        {
            GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0, TextureTarget.Texture2D, _colorTextureID, 0);
            GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.DepthAttachment, TextureTarget.Texture2D, _depthTextureID, 0);
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, _framebufferID);
        }

        public void Dispose()
        {
            GL.DeleteFramebuffer(_framebufferID);
        }
    }
}
