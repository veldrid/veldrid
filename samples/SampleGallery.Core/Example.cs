using System.Threading.Tasks;
using Veldrid.Utilities;

namespace Veldrid.SampleGallery
{
    public abstract class Example
    {
        protected GraphicsDevice Device { get; private set; }
        protected ResourceFactory Factory => _factory;
        protected Swapchain MainSwapchain { get; private set; }
        private DisposeCollectorResourceFactory _factory;

        public void Initialize(GraphicsDevice device, Swapchain mainSwapchain)
        {
            Device = device;
            MainSwapchain = mainSwapchain;
            _factory = new DisposeCollectorResourceFactory(device.ResourceFactory);
        }

        public void Shutdown()
        {
            _factory.DisposeCollector.DisposeAll();
        }

        public abstract Task LoadResourcesAsync();
        public abstract void Render(double deltaSeconds);
    }
}
