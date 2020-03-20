namespace Veldrid.OpenGL
{
    internal class OpenGLResourceSet : ResourceSet
    {
        private bool _disposed;

        public new OpenGLResourceLayout Layout { get; }
        public new BindableResource[] Resources { get; }
        public override string Name { get; set; }

        public override bool IsDisposed => _disposed;

        public OpenGLResourceSet(ref ResourceSetDescription description) : base(ref description)
        {
            Layout = Util.AssertSubtype<ResourceLayout, OpenGLResourceLayout>(description.Layout);
            Resources = Util.ShallowClone(description.BoundResources);
        }

        public override void Dispose()
        {
            _disposed = true;
        }
    }
}
