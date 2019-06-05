using Veldrid.Utilities;

namespace Veldrid.SampleGallery
{
    public class DisposeCollectorBase : DisposeBase
    {
        public GraphicsDevice Device { get; }
        public DisposeCollectorResourceFactory Factory { get; }

        public DisposeCollectorBase(GraphicsDevice gd)
        {
            Device = gd;
            Factory = new DisposeCollectorResourceFactory(gd.ResourceFactory);
        }

        protected sealed override void DisposeCore()
        {
            Factory.DisposeCollector.DisposeAll();
            DisposeCore2();
        }

        protected virtual void DisposeCore2() { }
    }
}
