using System;

namespace Vd2
{
    public abstract class TextureView : BindableResource, IDisposable
    {
        public Texture Target { get; }
        public abstract void Dispose();
    }
}
