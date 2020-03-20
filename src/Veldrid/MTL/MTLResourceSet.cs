namespace Veldrid.MTL
{
    internal class MTLResourceSet : ResourceSet
    {
        private bool _disposed;
        public new BindableResource[] Resources { get; }
        public new MTLResourceLayout Layout { get; }

        public MTLResourceSet(ref ResourceSetDescription description, MTLGraphicsDevice gd) : base(ref description)
        {
            Resources = Util.ShallowClone(description.BoundResources);
            Layout = Util.AssertSubtype<ResourceLayout, MTLResourceLayout>(description.Layout);
        }

        public override string Name { get; set; }

        public override bool IsDisposed => _disposed;

        public override void Dispose()
        {
            _disposed = true;
        }
    }
}
