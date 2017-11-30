using System.Diagnostics;
using System.Text;
using Veldrid.OpenGLBinding;
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
                if (Debugger.IsAttached)
                {
                    Debugger.Break();
                }

                throw new VeldridException("glGetError indicated an error: " + (ErrorCode)error);
            }
        }

        internal static unsafe void SetObjectLabel(ObjectLabelIdentifier identifier, uint target, string name)
        {
            int byteCount = Encoding.UTF8.GetByteCount(name);
            byte* utf8Ptr = stackalloc byte[byteCount];
            fixed (char* namePtr = name)
            {
                Encoding.UTF8.GetBytes(namePtr, name.Length, utf8Ptr, byteCount);
                glObjectLabel(identifier, target, (uint)byteCount, utf8Ptr);
                CheckLastError();
            }
        }
    }
}
