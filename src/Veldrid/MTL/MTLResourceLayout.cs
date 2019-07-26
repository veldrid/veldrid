namespace Veldrid.MTL
{
    internal class MTLResourceLayout : ResourceLayout
    {
        public ResourceLayoutElementDescription[] Elements { get; }

#if !VALIDATE_USAGE
        public ResourceKind[] ResourceKinds { get; }
        public ResourceLayoutDescription Description { get; }
#endif

        public MTLResourceLayout(ref ResourceLayoutDescription description, MTLGraphicsDevice gd)
            : base(ref description)
        {
            Elements = description.Elements;

#if !VALIDATE_USAGE
            Description = description;
            ResourceKinds = new ResourceKind[Elements.Length];
            for (int i = 0; i < Elements.Length; i++)
            {
                ResourceKinds[i] = Elements[i].Kind;
            }
#endif
        }

        public override string Name { get; set; }

        public override void Dispose()
        {
        }
    }
}
