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
            if (HasGlObjectLabel)
            {
                int byteCount = Util.UTF8.GetByteCount(name);
                if (MaxLabelLength == null)
                {
                    int maxLabelLength = -1;
                    glGetIntegerv(GetPName.MaxLabelLength, &maxLabelLength);
                    CheckLastError();
                    MaxLabelLength = maxLabelLength;
                }
                if (byteCount >= MaxLabelLength)
                {
                    name = name[..(MaxLabelLength.Value - 4)].ToString() + "...";
                    byteCount = Util.UTF8.GetByteCount(name);
                }

                Span<byte> utf8bytes = stackalloc byte[128];
                if(byteCount + 1 > 128) utf8bytes = new byte[byteCount + 1];

                fixed (char* namePtr = name)
                fixed (byte* utf8bytePtr = utf8bytes)
                {
                    Util.UTF8.GetBytes(namePtr, name.Length, utf8bytePtr, byteCount);
                    glObjectLabel(identifier, target, (uint)byteCount, utf8bytePtr);
                    CheckLastError();
                }
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
