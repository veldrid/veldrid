using OpenTK.Graphics.OpenGL;
using System;

namespace Veldrid.Graphics.OpenGL
{
    public class OpenGLDefaultFramebuffer : Framebuffer, IDisposable
    {
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

        public void Apply()
        {
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
        }

        public void Dispose()
        {
        }
    }
}
