using System;

namespace Veldrid.OpenGL
{
    public class OpenGLPlatformInfo
    {
        public IntPtr OpenGLContextHandle { get; }
        public Func<string, IntPtr> GetProcAddress { get; }
        public Action<IntPtr> MakeCurrent { get; }
        public Action<IntPtr> DeleteContext { get; }
        public Action SwapBuffers { get; }

        public OpenGLPlatformInfo(
            IntPtr openGLContextHandle,
            Func<string, IntPtr> getProcAddress,
            Action<IntPtr> makeCurrent,
            Action<IntPtr> deleteContext,
            Action swapBuffers)
        {
            OpenGLContextHandle = openGLContextHandle;
            GetProcAddress = getProcAddress;
            MakeCurrent = makeCurrent;
            DeleteContext = deleteContext;
            SwapBuffers = swapBuffers;
        }
    }
}