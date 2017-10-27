using System.Diagnostics;
using static Veldrid.OpenGLBinding.OpenGLNative;

namespace Veldrid.OpenGL
{
    internal static class OpenGLUtil
    {
        [Conditional("DEBUG")]
        [DebuggerNonUserCode]
        internal static void CheckLastError()
        {
            uint error = glGetError();
            if (error != 0)
            {
                throw new VeldridException("glGetError indicated an error: " + error);
            }
        }
    }
}
