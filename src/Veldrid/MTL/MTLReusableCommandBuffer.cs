using System;
using Veldrid.CommandRecording;
using Veldrid.MetalBindings;

namespace Veldrid.MTL
{
    internal class MTLReusableCommandBuffer : ReusableCommandBuffer
    {
        private readonly MTLCommandExecutor _executor;

        public MTLReusableCommandBuffer(MTLGraphicsDevice gd)
            : base(gd.Features)
        {
            _executor = new MTLCommandExecutor(gd);
        }

        internal MTLCommandBuffer RecordAndGetCommandBuffer()
        {
            GetEntryList().ExecuteAll(_executor);
            return _executor.CommandBuffer;
        }

        protected override void DisposeCore()
        {
            _executor.Dispose();
        }
    }
}
