namespace Veldrid.MTL
{
    internal class MTLResourceSet : ResourceSet
    {
        public IBindableResource[] Resources { get; }
        public MTLResourceLayout Layout { get; }

        public MTLResourceSet(ref ResourceSetDescription description)
            : base(ref description)
        {
            Resources = description.BoundResources;
            Layout = Util.AssertSubtype<ResourceLayout, MTLResourceLayout>(description.Layout);
        }

        public override string Name { get; set; }

        public override void Dispose()
        {
        }
    }
}