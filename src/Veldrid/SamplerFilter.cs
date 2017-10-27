namespace Veldrid
{
    public enum SamplerFilter : byte
    {
        MinPoint_MagPoint_MipPoint,
        MinPoint_MagPoint_MipLinear,
        MinPoint_MagLinear_MipPoint,
        MinPoint_MagLinear_MipLinear,
        MinLinear_MagPoint_MipPoint,
        MinLinear_MagPoint_MipLinear,
        MinLinear_MagLinear_MipPoint,
        MinLinear_MagLinear_MipLinear,
        Anisotropic
    }
}
