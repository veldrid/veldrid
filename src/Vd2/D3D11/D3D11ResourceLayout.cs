namespace Vd2.D3D11
{
    internal class D3D11ResourceLayout : ResourceLayout
    {
        private readonly (int index, ShaderStages stages)[] _deviceIndexByVdResourceIndex;

        public D3D11ResourceLayout(ref ResourceLayoutDescription description)
        {
            ResourceLayoutElementDescription[] elements = description.Elements;
            _deviceIndexByVdResourceIndex = new(int, ShaderStages)[elements.Length];

            int cbIndex = 0;
            int texIndex = 0;
            int samplerIndex = 0;

            for (int i = 0; i < _deviceIndexByVdResourceIndex.Length; i++)
            {
                int slot;
                switch (elements[i].Kind)
                {
                    case ResourceKind.Uniform:
                        slot = cbIndex++;
                        break;
                    case ResourceKind.Texture2D:
                        slot = texIndex++;
                        break;
                    case ResourceKind.TextureCube:
                        slot = texIndex++;
                        break;
                    case ResourceKind.Sampler:
                        slot = samplerIndex++;
                        break;
                    default: throw Illegal.Value<ResourceKind>();
                }

                _deviceIndexByVdResourceIndex[i] = (slot, elements[i].Stages);
            }
        }

        public (int slot, ShaderStages stages) GetDeviceSlotIndex(int resourceLayoutIndex)
        {
            if (resourceLayoutIndex >= _deviceIndexByVdResourceIndex.Length)
            {
                throw new VdException($"Invalid resource index: {resourceLayoutIndex}. Maximum is: {_deviceIndexByVdResourceIndex.Length - 1}.");
            }

            return _deviceIndexByVdResourceIndex[resourceLayoutIndex];
        }
    }
}