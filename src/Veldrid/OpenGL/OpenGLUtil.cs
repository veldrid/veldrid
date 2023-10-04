using System;
using System.Diagnostics;
using Veldrid.OpenGLBinding;
using static Veldrid.OpenGLBinding.OpenGLNative;

namespace Veldrid.OpenGL
{
    internal static class OpenGLUtil
    {
        private static int? MaxLabelLength;

        [Conditional("DEBUG")]
        internal static void CheckLastError()
        {
            VerifyLastError();
        }

        internal static void VerifyLastError()
        {
            uint error = glGetError();
            if (error != 0)
            {
                ThrowLastError(error);
            }
        }

        private static void ThrowLastError(uint error)
        {
            throw new VeldridException("glGetError: " + (ErrorCode)error);
        }

        internal static unsafe void SetObjectLabel(ObjectLabelIdentifier identifier, uint target, ReadOnlySpan<char> name)
        {
            if (!HasGlObjectLabel)
            {
                return;
            }

            if (name.IsEmpty)
            {
                glObjectLabel(identifier, target, 0, null);
                CheckLastError();
                return;
            }

            int maxLabelLength = 0;
            if (!MaxLabelLength.HasValue)
            {
                glGetIntegerv(GetPName.MaxLabelLength, &maxLabelLength);
                CheckLastError();
                MaxLabelLength = maxLabelLength;
            }
            maxLabelLength = MaxLabelLength.GetValueOrDefault();
            
            int byteCount = Util.UTF8.GetByteCount(name);
            if (byteCount >= maxLabelLength)
            {
                name = name[..(maxLabelLength - 4)].ToString() + "...";
                byteCount = Util.UTF8.GetByteCount(name);
            }

            Span<byte> utf8bytes = stackalloc byte[1024];
            byteCount = Util.GetNullTerminatedUtf8(name, ref utf8bytes);

            fixed (byte* utf8bytePtr = utf8bytes)
            {
                glObjectLabel(identifier, target, (uint)byteCount, utf8bytePtr);
                CheckLastError();
            }
        }

        internal static TextureTarget GetTextureTarget(OpenGLTexture glTex, uint arrayLayer)
        {
            if ((glTex.Usage & TextureUsage.Cubemap) == TextureUsage.Cubemap)
            {
                switch (arrayLayer % 6)
                {
                    case 0:
                        return TextureTarget.TextureCubeMapPositiveX;
                    case 1:
                        return TextureTarget.TextureCubeMapNegativeX;
                    case 2:
                        return TextureTarget.TextureCubeMapPositiveY;
                    case 3:
                        return TextureTarget.TextureCubeMapNegativeY;
                    case 4:
                        return TextureTarget.TextureCubeMapPositiveZ;
                    case 5:
                        return TextureTarget.TextureCubeMapNegativeZ;
                }
            }

            return glTex.TextureTarget;
        }
    }
}
