using System;
using Veldrid.D3D11;
using Veldrid.OpenGL;
using Veldrid.Vk;

namespace Veldrid
{
    public static class Hacks
    {
        public static GraphicsDevice CreateD3D11(IntPtr hwnd, int width, int height)
        {
            return new D3D11GraphicsDevice(hwnd, width, height);
        }

        public static GraphicsDevice CreateVulkan(VkSurfaceSource surfaceSource, uint width, uint height, bool debugDevice)
        {
            return new VkGraphicsDevice(surfaceSource, width, height, debugDevice);
        }

        public static GraphicsDevice CreateOpenGL(
            OpenGLPlatformInfo platformInfo,
            uint width,
            uint height,
            bool debugDevice)
        {
            return new OpenGLGraphicsDevice(platformInfo, width, height, debugDevice);
        }
    }
}
