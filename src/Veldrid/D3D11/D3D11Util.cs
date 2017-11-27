namespace Veldrid.D3D11
{
    internal static class D3D11Util
    {
        public static int ComputeSubresource(uint mipLevel, uint mipLevelCount, uint arrayLayer)
        {
            return (int)((arrayLayer * mipLevelCount) + mipLevel);
        }
    }
}
