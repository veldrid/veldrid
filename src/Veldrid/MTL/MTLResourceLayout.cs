namespace Veldrid.MTL
{
    internal class MTLResourceLayout : ResourceLayout
    {
        private readonly MTLResourceSlots.ResourceBindingInfo[] _bindingInfosByVdIndex;
        private bool _disposed;

        public ResourceLayoutElementDescription[] Elements { get; }
        public uint BufferCount { get; }
        public uint TextureCount { get; }
        public uint SamplerCount { get; }
        public MTLResourceSlots.ResourceBindingInfo GetBindingInfo(int index) => _bindingInfosByVdIndex[index];

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

        public override bool IsDisposed => _disposed;

        public override void Dispose()
        {
            _disposed = true;
        }
    }
}
