namespace Veldrid.OpenGL
{
    internal class OpenGLResourceLayout : ResourceLayout
    {
        private bool _disposed;

        public ResourceLayoutElementDescription[] Elements { get; }

        public override string Name { get; set; }

        public override bool IsDisposed => _disposed;

        public OpenGLResourceLayout(ref ResourceLayoutDescription description)
            : base(ref description)
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
