using OpenTK.Graphics.OpenGL;
using System;
using Veldrid.Platform;

namespace Veldrid.Graphics.OpenGL
{
    public class OpenGLDefaultFramebuffer : OpenGLFramebufferBase, Framebuffer, IDisposable
    {
        private readonly int _width;
        private readonly int _height;

        public OpenGLDefaultFramebuffer(int width, int height)
        {
            _width = width;
            _height = height;
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

        public int Width => _width;

        public int Height => _height;

        internal override void Apply()
        {
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
        }

        public void Dispose()
        {
        }
    }
}
