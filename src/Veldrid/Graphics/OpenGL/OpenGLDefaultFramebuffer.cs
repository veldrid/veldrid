using OpenTK.Graphics.OpenGL;
using System;
using Veldrid.Platform;

namespace Veldrid.Graphics.OpenGL
{
    public class OpenGLDefaultFramebuffer : OpenGLFramebufferBase, Framebuffer, IDisposable
    {
        private readonly Window _window;

        public OpenGLDefaultFramebuffer(Window window)
        {
            _window = window;
        }

        public DeviceTexture2D ColorTexture
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

        public DeviceTexture2D DepthTexture
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

        public DeviceTexture2D GetColorTexture(int index)
        {
            throw new NotSupportedException("Cannot get color textures from OpenGLDefaultFramebuffer.");
        }

        public void AttachColorTexture(int index, DeviceTexture2D texture)
        {
            throw new NotSupportedException("Cannot set color textures on OpenGLDefaultFramebuffer.");
        }

        public int Width => _window.Width;

        public int Height => _window.Height;

        internal override void Apply()
        {
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
        }

        public void Dispose()
        {
        }
    }
}
