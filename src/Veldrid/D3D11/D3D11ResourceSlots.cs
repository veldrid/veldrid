namespace Veldrid.D3D11
{
    internal class D3D11ResourceSlots
    {
        private static readonly ResourceBindingInfo s_unusedBinding = new ResourceBindingInfo()
        {
            IsUnused = true
        };

        private readonly ResourceBindingInfo[] _bindingInfosByVdIndex;

        public int UniformBufferCount { get; }
        public int StorageBufferCount { get; }
        public int TextureCount { get; }
        public int SamplerCount { get; }

        public D3D11ResourceSlots(ResourceLayoutElementDescription[] elements)
        {
            _bindingInfosByVdIndex = new ResourceBindingInfo[elements.Length];

            int cbIndex = 0;
            int texIndex = 0;
            int samplerIndex = 0;
            int unorderedAccessIndex = 0;

            for (int i = 0; i < _bindingInfosByVdIndex.Length; i++)
            {
                if (elements[i].IsUnused)
                {
                    _bindingInfosByVdIndex[i].IsUnused = true;
                    continue;
                }

                int slot;
                switch (elements[i].Kind)
                {
                    case ResourceKind.UniformBuffer:
                        slot = cbIndex++;
                        break;
                    case ResourceKind.StructuredBufferReadOnly:
                        slot = texIndex++;
                        break;
                    case ResourceKind.StructuredBufferReadWrite:
                        slot = unorderedAccessIndex++;
                        break;
                    case ResourceKind.TextureReadOnly:
                        slot = texIndex++;
                        break;
                    case ResourceKind.TextureReadWrite:
                        slot = unorderedAccessIndex++;
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

            UniformBufferCount = cbIndex;
            StorageBufferCount = unorderedAccessIndex;
            TextureCount = texIndex;
            SamplerCount = samplerIndex;
        }

        public ResourceBindingInfo GetDeviceSlotIndex(int resourceLayoutIndex)
        {
            if (resourceLayoutIndex >= _bindingInfosByVdIndex.Length)
            {
                return s_unusedBinding;
            }

            return _bindingInfosByVdIndex[resourceLayoutIndex];
        }

        public bool IsDynamicBuffer(int index) => _bindingInfosByVdIndex[index].DynamicBuffer;

        internal struct ResourceBindingInfo
        {
            public bool IsUnused;
            public int Slot;
            public ShaderStages Stages;
            public ResourceKind Kind;
            public bool DynamicBuffer;

            public ResourceBindingInfo(int slot, ShaderStages stages, ResourceKind kind, bool dynamicBuffer)
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
