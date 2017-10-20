namespace Vd2
{
    public struct SamplerDescription
    {
        public SamplerAddressMode AddressModeU;
        public SamplerAddressMode AddressModeV;
        public SamplerAddressMode AddressModeW;
        public SamplerFilter Filter;
        public DepthComparisonKind? ComparisonKind;
        public uint MaximumAnisotropy;
        public uint MinimumLod;
        public uint MaximumLod;
        public int LodBias;
        public SamplerBorderColor BorderColor;

        public SamplerDescription(
            SamplerAddressMode addressModeU,
            SamplerAddressMode addressModeV,
            SamplerAddressMode addressModeW,
            SamplerFilter filter,
            DepthComparisonKind? comparisonKind,
            uint maximumAnisotropy,
            uint minimumLod,
            uint maximumLod,
            int lodBias,
            SamplerBorderColor borderColor)
        {
            AddressModeU = addressModeU;
            AddressModeV = addressModeV;
            AddressModeW = addressModeW;
            Filter = filter;
            ComparisonKind = comparisonKind;
            MaximumAnisotropy = maximumAnisotropy;
            MinimumLod = minimumLod;
            MaximumLod = maximumLod;
            LodBias = lodBias;
            BorderColor = borderColor;
        }

        public static readonly SamplerDescription Point = new SamplerDescription
        {
            AddressModeU = SamplerAddressMode.Wrap,
            AddressModeV = SamplerAddressMode.Wrap,
            AddressModeW = SamplerAddressMode.Wrap,
            Filter = SamplerFilter.MinPoint_MagPoint_MipPoint,
            LodBias = 0,
            MinimumLod = 0,
            MaximumLod = uint.MaxValue,
            MaximumAnisotropy = 0,
        };

        public static readonly SamplerDescription Linear = new SamplerDescription
        {
            AddressModeU = SamplerAddressMode.Wrap,
            AddressModeV = SamplerAddressMode.Wrap,
            AddressModeW = SamplerAddressMode.Wrap,
            Filter = SamplerFilter.MinLinear_MagLinear_MipLinear,
            LodBias = 0,
            MinimumLod = 0,
            MaximumLod = uint.MaxValue,
            MaximumAnisotropy = 0,
        };

        public static readonly SamplerDescription Aniso4x = new SamplerDescription
        {
            AddressModeU = SamplerAddressMode.Wrap,
            AddressModeV = SamplerAddressMode.Wrap,
            AddressModeW = SamplerAddressMode.Wrap,
            Filter = SamplerFilter.Anisotropic,
            LodBias = 0,
            MinimumLod = 0,
            MaximumLod = uint.MaxValue,
            MaximumAnisotropy = 4,
        };
    }
}
