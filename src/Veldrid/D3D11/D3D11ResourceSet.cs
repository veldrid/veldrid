namespace Veldrid.D3D11
{
    internal sealed class D3D11ResourceSet : ResourceSet
    {
        private string? _name;
        private bool _disposed;

        public new BindableResource[] Resources { get; }
        public new D3D11ResourceLayout Layout { get; }

        public D3D11ResourceSet(in ResourceSetDescription description) : base(description)
        {
            Resources = Util.ShallowClone(description.BoundResources);
            Layout = Util.AssertSubtype<ResourceLayout, D3D11ResourceLayout>(description.Layout);
        }

        public override string? Name
        {
            get => _name;
            set => _name = value;
        }

        public override bool IsDisposed => _disposed;

        public override void Dispose()
        {
            _disposed = true;
        }
    }
}
