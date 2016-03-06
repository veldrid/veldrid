using OpenTK.Graphics.OpenGL;
using System;

namespace Veldrid.Graphics.OpenGL
{
    public class OpenGLTexture : DeviceTexture, RenderStateModifier, IDisposable
    {
        private readonly OpenTK.Graphics.OpenGL.PixelFormat _pixelFormat;
        private readonly PixelType _pixelType;

        public int TextureID { get; }

        public OpenGLTexture(
            int width,
            int height,
            PixelInternalFormat internalFormat,
            OpenTK.Graphics.OpenGL.PixelFormat pixelFormat,
            PixelType pixelType)
        {
            _pixelFormat = pixelFormat;
            _pixelType = pixelType;

            TextureID = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, TextureID);

            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Clamp);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Clamp);

            // Set size, load empty data into texture
            GL.TexImage2D(
                TextureTarget.Texture2D,
                0, // level
                internalFormat,
                width, height,
                0, // border
                _pixelFormat,
                _pixelType,
                IntPtr.Zero);
        }

        public OpenGLTexture(Texture texture)
        {
            TextureID = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, TextureID);

            GL.TexEnv(TextureEnvTarget.TextureEnv, TextureEnvParameter.TextureEnvMode, (float)TextureEnvMode.Modulate);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (float)TextureMinFilter.LinearMipmapLinear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (float)TextureMagFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.GenerateMipmap, 1.0f);

            _pixelFormat = MapPixelFormat(texture.Format);
            _pixelType = MapPixelType(texture.Format);

            // load the texture
            GL.TexImage2D(
                TextureTarget.Texture2D,
                0, // level
                PixelInternalFormat.Rgba32f,
                texture.Width, texture.Height,
                0, // border
                _pixelFormat,
                _pixelType,
                texture.Pixels);

            GL.BindTexture(TextureTarget.Texture2D, 0);
        }

        private OpenTK.Graphics.OpenGL.PixelFormat MapPixelFormat(PixelFormat format)
        {
            switch (format)
            {
                case PixelFormat.R32_G32_B32_A32_Float:
                    return OpenTK.Graphics.OpenGL.PixelFormat.Bgra;
                case PixelFormat.Alpha_Int8:
                    return OpenTK.Graphics.OpenGL.PixelFormat.Alpha;
                case PixelFormat.R8_G8_B8_A8:
                    return OpenTK.Graphics.OpenGL.PixelFormat.Bgra;
                default:
                    throw Illegal.Value<PixelFormat>();
            }
        }

        private PixelType MapPixelType(PixelFormat format)
        {
            switch (format)
            {
                case PixelFormat.R32_G32_B32_A32_Float:
                    return PixelType.Float;
                case PixelFormat.Alpha_Int8:
                    return PixelType.UnsignedByte;
                case PixelFormat.R8_G8_B8_A8:
                    return PixelType.Int;
                default:
                    throw Illegal.Value<PixelFormat>();
            }
        }

        public void Apply()
        {
            GL.BindTexture(TextureTarget.Texture2D, TextureID);
        }

        public void CopyTo(Texture texture)
        {
            GL.BindTexture(TextureTarget.Texture2D, TextureID);
            GL.GetTexImage(TextureTarget.Texture2D, 0, _pixelFormat, _pixelType, texture.Pixels);

            // Need to reverse the rows vertically.
            int rowPixels = texture.Width * 4;
            float[] stagingRow = new float[rowPixels];
            for (int y = texture.Height - 1, destY = 0; y > (texture.Height / 2); y--)
            {
                Array.ConstrainedCopy(texture.Pixels, y * rowPixels, stagingRow, 0, rowPixels);
                Array.ConstrainedCopy(texture.Pixels, destY * rowPixels, texture.Pixels, y * rowPixels, rowPixels);
                Array.ConstrainedCopy(stagingRow, 0, texture.Pixels, destY * rowPixels, rowPixels);

                destY++;
            }

            GL.BindTexture(TextureTarget.Texture2D, 0);
        }

        public void Dispose()
        {
            GL.DeleteTexture(TextureID);
        }
    }
}
