using OpenTK.Graphics.OpenGL;
using System;
using Veldrid.Platform;

namespace Veldrid.Graphics.OpenGL
{
    public class OpenGLDefaultFramebuffer : Framebuffer, IDisposable
    {
        private readonly Window _window;

        public OpenGLDefaultFramebuffer(Window window)
        {
            _window = window;
        }

        public DeviceTexture ColorTexture
        {
            get
            {
                throw new NotSupportedException("The default OpenGL framebuffer does not expose its color texture.");
            }

            set
            {
                throw new NotSupportedException("The default OpenGL framebuffer does not expose its color texture.");
            }
        }

        public DeviceTexture DepthTexture
        {
            get
            {
                throw new NotSupportedException("The default OpenGL framebuffer does not expose its depth texture.");
            }

            set
            {
                throw new NotSupportedException("The default OpenGL framebuffer does not expose its depth texture.");
            }
        }

        public int Width => _window.Width;

        public int Height => _window.Height;

        public void Apply()
        {
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
        }

        public void Dispose()
        {
        }
    }
}
