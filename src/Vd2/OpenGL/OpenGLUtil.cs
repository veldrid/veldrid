using System.Diagnostics;
using static Vd2.OpenGLBinding.OpenGLNative;

namespace Vd2.OpenGL
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
                throw new VdException("glGetError indicated an error: " + error);
            }
        }
    }
}
