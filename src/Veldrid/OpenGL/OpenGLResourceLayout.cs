namespace Veldrid.OpenGL
{
    internal sealed class OpenGLResourceLayout : ResourceLayout
    {
        private bool _disposed;

        public ResourceLayoutElementDescription[] Elements { get; }

        public override string? Name { get; set; }

        public override bool IsDisposed => _disposed;

        public OpenGLResourceLayout(in ResourceLayoutDescription description)
            : base(description)
        {
            Elements = Util.ShallowClone(description.Elements);
        }

        public bool IsDynamicBuffer(uint slot)
        {
            return (Elements[slot].Options & ResourceLayoutElementOptions.DynamicBinding) != 0;
        }

        public override void Dispose()
        {
            _disposed = true;
        }
    }
}
