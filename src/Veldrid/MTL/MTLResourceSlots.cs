namespace Veldrid.MTL
{
    internal class MTLResourceSlots
    {
        private static readonly ResourceBindingInfo s_unusedBinding = new ResourceBindingInfo()
        {
            IsUnused = true
        };

        private readonly ResourceBindingInfo[] _bindingInfosByVdIndex;
        public uint BufferCount { get; }
        public uint TextureCount { get; }
        public uint SamplerCount { get; }

        public MTLResourceSlots(ResourceLayoutElementDescription[] elements)
        {
            _bindingInfosByVdIndex = new ResourceBindingInfo[elements.Length];

            uint bufferIndex = 0;
            uint texIndex = 0;
            uint samplerIndex = 0;

            for (int i = 0; i < _bindingInfosByVdIndex.Length; i++)
            {
                if (elements[i].IsUnused)
                {
                    _bindingInfosByVdIndex[i].IsUnused = true;
                    continue;
                }

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

                _bindingInfosByVdIndex[i] = new ResourceBindingInfo(
                    slot,
                    elements[i].Stages,
                    elements[i].Kind,
                    (elements[i].Options & ResourceLayoutElementOptions.DynamicBinding) != 0);
            }

            BufferCount = bufferIndex;
            TextureCount = texIndex;
            SamplerCount = samplerIndex;
        }

        public ResourceBindingInfo GetBindingInfo(int index)
        {
            if (index >= _bindingInfosByVdIndex.Length)
            {
                return s_unusedBinding;
            }

            return _bindingInfosByVdIndex[index];
        }

        internal struct ResourceBindingInfo
        {
            public bool IsUnused;
            public uint Slot;
            public ShaderStages Stages;
            public ResourceKind Kind;
            public bool DynamicBuffer;

            public ResourceBindingInfo(uint slot, ShaderStages stages, ResourceKind kind, bool dynamicBuffer)
            {
                IsUnused = false;
                Slot = slot;
                Stages = stages;
                Kind = kind;
                DynamicBuffer = dynamicBuffer;
            }
        }
    }
}
