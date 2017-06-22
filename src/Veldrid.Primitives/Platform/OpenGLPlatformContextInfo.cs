using System;

namespace Veldrid.Platform
{
    public class OpenGLPlatformContextInfo
    {
        public IntPtr ContextHandle { get; }
        public Func<string, IntPtr> GetProcAddress { get; }
        public Func<IntPtr> GetCurrentContext { get; }
        public Action SwapBuffer { get; }

        public OpenGLPlatformContextInfo(IntPtr contextHandle, Func<string, IntPtr> getProcAddress, Func<IntPtr> getCurrentContext, Action swapBuffer)
        {
            ContextHandle = contextHandle;
            GetProcAddress = getProcAddress;
            GetCurrentContext = getCurrentContext;
            SwapBuffer = swapBuffer;
        }
    }
}
