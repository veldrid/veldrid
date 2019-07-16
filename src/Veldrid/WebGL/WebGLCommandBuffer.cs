using Veldrid.CommandRecording;

namespace Veldrid.WebGL
{
    internal class WebGLCommandBuffer : ReusableCommandBuffer
    {
        public WebGLCommandBuffer(GraphicsDeviceFeatures features)
            : base(features)
        {
        }
    }
}
