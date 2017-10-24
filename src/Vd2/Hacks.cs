using System;
using Vd2.D3D11;
using Vd2.OpenGL;
using Vd2.Vk;

namespace Vd2
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
            IntPtr glContext,
            Func<string, IntPtr> getProcAddress,
            Action swapBuffer,
            uint width,
            uint height,
            bool debugDevice)
        {
            return new OpenGLGraphicsDevice(glContext, getProcAddress, swapBuffer, width, height, debugDevice);
        }
    }
}
