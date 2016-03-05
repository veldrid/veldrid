using OpenTK.Graphics.OpenGL;
using System;

namespace Veldrid.Graphics.OpenGL
{
    public class OpenGLTextureBuffer : DeviceTexture, RenderStateModifier, IDisposable
    {
        private readonly int _textureID;
        private readonly OpenTK.Graphics.OpenGL.PixelFormat _pixelFormat;
        private readonly PixelType _pixelType;

        public OpenGLTextureBuffer(Texture texture)
        {
            _textureID = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, _textureID);

            GL.TexEnv(
                TextureEnvTarget.TextureEnv,
                TextureEnvParameter.TextureEnvMode,
                (float)TextureEnvMode.Modulate);
            GL.TexParameter(
                TextureTarget.Texture2D,
                TextureParameterName.TextureMinFilter,
                (float)TextureMinFilter.LinearMipmapLinear);
            GL.TexParameter(
                TextureTarget.Texture2D,
                TextureParameterName.TextureMagFilter,
                (float)TextureMagFilter.Linear);
            GL.TexParameter(
                TextureTarget.Texture2D,
                TextureParameterName.GenerateMipmap,
                1.0f);

            _pixelFormat = MapPixelFormat(texture.Format);
            _pixelType = MapPixelType(texture.Format);

            // load the texture
            GL.TexImage2D(
                TextureTarget.Texture2D,
                0, // level
                PixelInternalFormat.Four,
                texture.Width, texture.Height,
                0, // border
                OpenTK.Graphics.OpenGL.PixelFormat.Bgra,
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
            GL.BindTexture(TextureTarget.Texture2D, _textureID);
        }

        public void CopyTo(Texture texture)
        {
            GL.BindTexture(TextureTarget.Texture2D, _textureID);
            GL.GetTexImage(TextureTarget.Texture2D, 0, _pixelFormat, _pixelType, texture.Pixels);
            GL.BindTexture(TextureTarget.Texture2D, 0);
        }

        public void Dispose()
        {
            GL.DeleteTexture(_textureID);
        }
    }
}
