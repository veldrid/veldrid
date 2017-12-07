namespace Veldrid.D3D11
{
    internal class D3D11ResourceSet : ResourceSet
    {
        private string _name;

        public BindableResource[] Resources { get; }
        public D3D11ResourceLayout Layout { get; }

        public D3D11ResourceSet(ref ResourceSetDescription description)
        {
            Resources = description.BoundResources;
            Layout = Util.AssertSubtype<ResourceLayout, D3D11ResourceLayout>(description.Layout);

            foreach (BindableResource resource in description.BoundResources)
            {
                if (!(resource is D3D11Buffer || resource is D3D11TextureView || resource is D3D11Sampler))
                {
                    throw new VeldridException("Invalid resource type present in D3D11ResourceSet: " + resource.GetType().Name);
                }
            }
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