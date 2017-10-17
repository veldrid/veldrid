namespace Vd2.D3D11
{
    internal class D3D11ResourceLayout : ResourceLayout
    {
        private readonly ResourceBindingInfo[] _bindingInfosByVdIndex;

        public D3D11ResourceLayout(ref ResourceLayoutDescription description)
        {
            ResourceLayoutElementDescription[] elements = description.Elements;
            _bindingInfosByVdIndex = new ResourceBindingInfo[elements.Length];

            int cbIndex = 0;
            int texIndex = 0;
            int samplerIndex = 0;

            for (int i = 0; i < _bindingInfosByVdIndex.Length; i++)
            {
                int slot;
                switch (elements[i].Kind)
                {
                    case ResourceKind.Uniform:
                        slot = cbIndex++;
                        break;
                    case ResourceKind.Texture:
                        slot = texIndex++;
                        break;
                    case ResourceKind.Sampler:
                        slot = samplerIndex++;
                        break;
                    default: throw Illegal.Value<ResourceKind>();
                }

                _bindingInfosByVdIndex[i] = new ResourceBindingInfo(slot, elements[i].Stages);
            }
        }

        public ResourceBindingInfo GetDeviceSlotIndex(int resourceLayoutIndex)
        {
            if (resourceLayoutIndex >= _bindingInfosByVdIndex.Length)
            {
                throw new VdException($"Invalid resource index: {resourceLayoutIndex}. Maximum is: {_bindingInfosByVdIndex.Length - 1}.");
            }

            return _bindingInfosByVdIndex[resourceLayoutIndex];
        }

        public struct ResourceBindingInfo
        {
            public int Slot;
            public ShaderStages Stages;

            public ResourceBindingInfo(int slot, ShaderStages stages)
            {
                Slot = slot;
                Stages = stages;
            }
        }
    }
}