namespace Veldrid.D3D11
{
    internal class D3D11ResourceLayout : ResourceLayout
    {
        private string _name;

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

        public override void Dispose()
        {
        }
    }
}
