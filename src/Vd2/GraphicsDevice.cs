using System;

namespace Vd2
{
    public abstract class GraphicsDevice : IDisposable
    {
        public abstract ResourceFactory ResourceFactory { get; }
        public abstract void ExecuteCommands(CommandBuffer cb);
        public abstract void SwapBuffers();
        public abstract Framebuffer SwapchainFramebuffer { get; }

        public abstract void Dispose();
    }
}
