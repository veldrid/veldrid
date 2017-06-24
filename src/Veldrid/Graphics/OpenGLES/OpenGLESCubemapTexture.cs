using System;
using OpenTK.Graphics.ES30;

namespace Veldrid.Graphics.OpenGLES
{
    public class OpenGLESCubemapTexture : OpenGLESTexture, CubemapTexture
    {
        private readonly TextureComponentCount _texComponentCount;
        private readonly OpenTK.Graphics.ES30.PixelFormat _format;
        private readonly PixelType _type;

        public OpenGLESCubemapTexture(
            IntPtr pixelsFront,
            IntPtr pixelsBack,
            IntPtr pixelsLeft,
            IntPtr pixelsRight,
            IntPtr pixelsTop,
            IntPtr pixelsBottom,
            int width,
            int height,
            PixelFormat format)
            : base(TextureTarget.TextureCubeMap, width, height)
        {
            _texComponentCount = OpenGLESFormats.MapTextureComponentCount(format, false);
            _format = OpenGLESFormats.MapPixelFormat(format);
            _type = OpenGLESFormats.MapPixelType(format);

            Bind();
            GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
            Utilities.CheckLastGLES3Error();
            GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureMinFilter, (int)TextureMagFilter.Linear);
            Utilities.CheckLastGLES3Error();
            GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
            Utilities.CheckLastGLES3Error();
            GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);
            Utilities.CheckLastGLES3Error();
            GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureWrapR, (int)TextureWrapMode.ClampToEdge);
            Utilities.CheckLastGLES3Error();

            SetFacePixels(0, pixelsRight);
            SetFacePixels(1, pixelsLeft);
            SetFacePixels(2, pixelsTop);
            SetFacePixels(3, pixelsBottom);
            SetFacePixels(4, pixelsBack);
            SetFacePixels(5, pixelsFront);
        }

        private void SetFacePixels(int faceIndex, IntPtr pixels)
        {
            GL.TexImage2D(
                TextureTarget2d.TextureCubeMapPositiveX + faceIndex,
                0,
                _texComponentCount,
                Width,
                Height,
                0,
                _format,
                _type,
                pixels);
        }
    }
}