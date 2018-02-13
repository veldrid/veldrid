namespace Veldrid.MTL
{
    internal class MTLResourceLayout : ResourceLayout
    {
        private readonly ResourceBindingInfo[] _bindingInfosByVdIndex;
        public uint BufferCount { get; }
        public uint TextureCount { get; }
        public uint SamplerCount { get; }
#if !VALIDATE_USAGE
        public ResourceKind[] ResourceKinds { get; }
#endif
        public ResourceBindingInfo GetBindingInfo(int index) => _bindingInfosByVdIndex[index];

        public MTLResourceLayout(ref ResourceLayoutDescription description, MTLGraphicsDevice gd)
            : base(ref description)
        {
            ResourceLayoutElementDescription[] elements = description.Elements;
#if !VALIDATE_USAGE
            ResourceKinds = new ResourceKind[elements.Length];
            for (int i = 0; i < elements.Length; i++)
            {
                ResourceKinds[i] = elements[i].Kind;
            }
#endif

            _bindingInfosByVdIndex = new ResourceBindingInfo[elements.Length];

            uint bufferIndex = 0;
            uint texIndex = 0;
            uint samplerIndex = 0;

            for (int i = 0; i < _bindingInfosByVdIndex.Length; i++)
            {
                uint slot;
                switch (elements[i].Kind)
                {
                    case ResourceKind.UniformBuffer:
                        slot = bufferIndex++;
                        break;
                    case ResourceKind.StructuredBufferReadOnly:
                        slot = bufferIndex++;
                        break;
                    case ResourceKind.StructuredBufferReadWrite:
                        slot = bufferIndex++;
                        break;
                    case ResourceKind.TextureReadOnly:
                        slot = texIndex++;
                        break;
                    case ResourceKind.TextureReadWrite:
                        slot = texIndex++;
                        break;
                    case ResourceKind.Sampler:
                        slot = samplerIndex++;
                        break;
                    default: throw Illegal.Value<ResourceKind>();
                }

                _bindingInfosByVdIndex[i] = new ResourceBindingInfo(slot, elements[i].Stages, elements[i].Kind);
            }

            BufferCount = bufferIndex;
            TextureCount = texIndex;
            SamplerCount = samplerIndex;
        }

        public override string Name { get; set; }

        public override void Dispose()
        {
        }

        internal struct ResourceBindingInfo
        {
            public uint Slot;
            public ShaderStages Stages;
            public ResourceKind Kind;

            public ResourceBindingInfo(uint slot, ShaderStages stages, ResourceKind kind)
            {
                Slot = slot;
                Stages = stages;
                Kind = kind;
            }
        }
    }
}