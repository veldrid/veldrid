using Veldrid.CommandRecording;

namespace Veldrid.OpenGL
{
    internal class OpenGLCommandBuffer : ReusableCommandBuffer
    {
        public OpenGLCommandBuffer(OpenGLGraphicsDevice gd, ref CommandBufferDescription description)
            : base(gd.Features)
        {
        }
    }
}
