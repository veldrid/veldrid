using System;
using Vd2.D3D11;

namespace Vd2
{
    public static class Hacks
    {
        public static GraphicsDevice CreateD3D11(IntPtr hwnd, int width, int height)
        {
            return new D3D11GraphicsDevice(hwnd, width, height);
        }
    }
}
