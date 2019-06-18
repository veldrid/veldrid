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

        internal static unsafe bool ManualSrgbBackbufferQuery(GraphicsBackend backend, bool extSrgbWriteControl)
        {
            if (backend == GraphicsBackend.OpenGLES && !extSrgbWriteControl)
            {
                return false;
            }

            glGenTextures(1, out uint copySrc);
            CheckLastError();

            float* data = stackalloc float[4];
            data[0] = 0.5f;
            data[1] = 0.5f;
            data[2] = 0.5f;
            data[3] = 1f;

            glActiveTexture(TextureUnit.Texture0);
            CheckLastError();
            glBindTexture(TextureTarget.Texture2D, copySrc);
            CheckLastError();
            glTexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba32f, 1, 1, 0, GLPixelFormat.Rgba, GLPixelType.Float, data);
            CheckLastError();
            glGenFramebuffers(1, out uint copySrcFb);
            CheckLastError();

            glBindFramebuffer(FramebufferTarget.ReadFramebuffer, copySrc);
            CheckLastError();
            glFramebufferTexture2D(FramebufferTarget.ReadFramebuffer, GLFramebufferAttachment.ColorAttachment0, TextureTarget.Texture2D, copySrc, 0);
            CheckLastError();

            glEnable(EnableCap.FramebufferSrgb);
            CheckLastError();
            glBlitFramebuffer(
                0, 0, 1, 1,
                0, 0, 1, 1,
                ClearBufferMask.ColorBufferBit,
                BlitFramebufferFilter.Nearest);
            CheckLastError();

            glDisable(EnableCap.FramebufferSrgb);
            CheckLastError();

            glBindFramebuffer(FramebufferTarget.ReadFramebuffer, 0);
            CheckLastError();
            glBindFramebuffer(FramebufferTarget.DrawFramebuffer, copySrcFb);
            CheckLastError();
            glBlitFramebuffer(
                0, 0, 1, 1,
                0, 0, 1, 1,
                ClearBufferMask.ColorBufferBit,
                BlitFramebufferFilter.Nearest);
            CheckLastError();
            if (backend == GraphicsBackend.OpenGLES)
            {
                glBindFramebuffer(FramebufferTarget.ReadFramebuffer, copySrc);
                CheckLastError();
                glReadPixels(
                    0, 0, 1, 1,
                    GLPixelFormat.Rgba,
                    GLPixelType.Float,
                    data);
                CheckLastError();
            }
            else
            {
                glGetTexImage(TextureTarget.Texture2D, 0, GLPixelFormat.Rgba, GLPixelType.Float, data);
                CheckLastError();
            }

            glDeleteFramebuffers(1, ref copySrcFb);
            glDeleteTextures(1, ref copySrc);

            return data[0] > 0.6f;
        }
    }
}
