using System;

namespace Veldrid
{
    public abstract class GraphicsDevice : IDisposable
    {
        public abstract GraphicsBackend BackendType { get; }

        public abstract ResourceFactory ResourceFactory { get; }
        public abstract void ExecuteCommands(CommandList cl);
        public abstract void SwapBuffers();
        public abstract Framebuffer SwapchainFramebuffer { get; }
        public abstract void ResizeMainWindow(uint width, uint height);
        public abstract void WaitForIdle();
        public abstract void Dispose();
        public abstract void SetResourceName(DeviceResource resource, string name);

        protected void PostContextCreated()
        {
            PointSampler = ResourceFactory.CreateSampler(SamplerDescription.Point);
            LinearSampler = ResourceFactory.CreateSampler(SamplerDescription.Linear);
            Aniso4xSampler = ResourceFactory.CreateSampler(SamplerDescription.Aniso4x);
        }

        public Sampler PointSampler { get; private set; }
        public Sampler LinearSampler { get; private set; }
        public Sampler Aniso4xSampler { get; private set; }
    }
}
