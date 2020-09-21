namespace Veldrid.D3D11
{
    internal class D3D11ResourceLayout : ResourceLayout
    {
        private string _name;
        private bool _disposed;

        public ResourceLayoutElementDescription[] Elements { get; }

        public D3D11ResourceLayout(ref ResourceLayoutDescription description)
            : base(ref description)
        {
            Elements = description.Elements;
        }

        public override string Name
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
