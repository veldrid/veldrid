using System;

namespace Veldrid.OpenGLBinding
{
    public readonly struct GLsync
    {
        public IntPtr Handle { get; }

        public GLsync(IntPtr handle)
        {
            Handle = handle;
        }
    }
}
