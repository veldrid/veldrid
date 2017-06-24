namespace Veldrid.Graphics
{
    public struct BoundSamplerStateInfo
    {
        public SamplerState SamplerState { get; }
        public bool Mipmapped { get; }

        public BoundSamplerStateInfo(SamplerState samplerState, bool mipmapped)
        {
            SamplerState = samplerState;
            Mipmapped = mipmapped;
        }
    }
}
