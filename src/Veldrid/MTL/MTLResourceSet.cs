namespace Veldrid.MTL
{
    internal class MTLResourceSet : ResourceSet
    {
        public BindableResource[] Resources { get; }
        public new MTLResourceLayout Layout { get; }

        public MTLResourceSet(ref ResourceSetDescription description, MTLGraphicsDevice gd)
            : base(ref description)
        {
            Resources = Util.ShallowClone(description.BoundResources);
            Layout = Util.AssertSubtype<ResourceLayout, MTLResourceLayout>(description.Layout);
        }

        public override string Name { get; set; }

        public override void Dispose()
        {
        }
    }
}