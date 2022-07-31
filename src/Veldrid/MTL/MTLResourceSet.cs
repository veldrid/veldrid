namespace Veldrid.MTL
{
    internal sealed class MTLResourceSet : ResourceSet
    {
        private bool _disposed;
        public new BindableResource[] Resources { get; }
        public new MTLResourceLayout Layout { get; }

        public MTLResourceSet(in ResourceSetDescription description, MTLGraphicsDevice gd) : base(description)
        {
            Resources = Util.ShallowClone(description.BoundResources);
            Layout = Util.AssertSubtype<ResourceLayout, MTLResourceLayout>(description.Layout);
        }

        public override string? Name { get; set; }

        public override bool IsDisposed => _disposed;

        public override void Dispose()
        {
            _disposed = true;
        }
    }
}
