using Veldrid.CommandRecording;

namespace Veldrid.MTL
{
    internal class MTLReusableCommandBuffer : ReusableCommandBuffer
    {
        public MTLReusableCommandBuffer(GraphicsDeviceFeatures features)
            : base(features)
        {
        }
    }
}
