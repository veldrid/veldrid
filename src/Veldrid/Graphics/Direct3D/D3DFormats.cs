using SharpDX.DXGI;

namespace Veldrid.Graphics.Direct3D
{
    internal class D3DFormats
    {
        internal static Format ConvertPixelFormat(PixelFormat format)
        {
            switch (format)
            {
                case PixelFormat.R32_G32_B32_A32_Float:
                    return Format.R32G32B32A32_Float;
                case PixelFormat.Alpha_UInt8:
                    return Format.A8_UNorm;
                case PixelFormat.R8_G8_B8_A8:
                    return Format.R8G8B8A8_UInt;
                default:
                    throw Illegal.Value<PixelFormat>();
            }
        }
    }
}