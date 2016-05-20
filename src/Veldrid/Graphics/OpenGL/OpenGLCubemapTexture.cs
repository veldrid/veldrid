using System;
using OpenTK.Graphics.OpenGL;

namespace Veldrid.Graphics.OpenGL
{
    public class OpenGLCubemapTexture : OpenGLTexture, CubemapTexture
    {
        private readonly OpenTK.Graphics.OpenGL.PixelInternalFormat _internalFormat;
        private readonly OpenTK.Graphics.OpenGL.PixelFormat _format;
        private readonly PixelType _type;

        public OpenGLCubemapTexture(
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
            _internalFormat = OpenGLFormats.MapPixelInternalFormat(format);
            _format = OpenGLFormats.MapPixelFormat(format);
            _type = OpenGLFormats.MapPixelType(format);

            Bind();
            GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
            GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureMinFilter, (int)TextureMagFilter.Linear);
            GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
            GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);
            GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureWrapR, (int)TextureWrapMode.ClampToEdge);

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
                TextureTarget.TextureCubeMapPositiveX + faceIndex,
                0,
                _internalFormat,
                Width,
                Height,
                0,
                _format,
                _type,
                pixels);
        }
    }
}