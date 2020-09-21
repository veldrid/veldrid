namespace Veldrid.WebGL
{
    internal class WebGLResourceSet : ResourceSet
    {
        private bool _disposed;

        public new WebGLResourceLayout Layout { get; }
        public new BindableResource[] Resources { get; }
        public override string Name { get; set; }

        public override bool IsDisposed => _disposed;

        public WebGLResourceSet(WebGLGraphicsDevice gd, ref ResourceSetDescription description)
            : base(ref description)
        {
            Layout = Util.AssertSubtype<ResourceLayout, WebGLResourceLayout>(description.Layout);
            Resources = Util.ShallowClone(description.BoundResources);
        }

        public override void Dispose()
        {
            _disposed = true;
        }
    }
}
