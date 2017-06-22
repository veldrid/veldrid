using OpenTK.Graphics.ES30;
using System;

namespace Veldrid.Graphics.OpenGLES
{
    public class OpenGLESDefaultFramebuffer : OpenGLESFramebufferBase, Framebuffer, IDisposable
    {
        public OpenGLESDefaultFramebuffer(int width, int height)
        {
            Width = width;
            Height = height;
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
        public int Width { get; internal set; }

        public int Height { get; internal set; }

        internal override void Apply()
        {
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
            Utilities.CheckLastGLES3Error();
        }

        public void Dispose()
        {
        }
    }
}