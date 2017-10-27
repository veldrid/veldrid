using System;

namespace Veldrid
{
    public abstract class TextureView : BindableResource, IDisposable
    {
        public Texture Target { get; }

        public TextureView(Texture target)
        {
            Target = target;
        }

        public abstract void Dispose();
    }
}
