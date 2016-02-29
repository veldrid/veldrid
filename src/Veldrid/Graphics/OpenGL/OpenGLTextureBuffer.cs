using OpenTK.Graphics.OpenGL;
using System;

namespace Veldrid.Graphics.OpenGL
{
    public class OpenGLTextureBuffer
    {
        public int TextureID { get; }

        public OpenGLTextureBuffer(Texture texture)
        {
            TextureID = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, TextureID);

            //the following code sets certian parameters for the texture
            GL.TexEnv(TextureEnvTarget.TextureEnv,
                   TextureEnvParameter.TextureEnvMode, (float)TextureEnvMode.Modulate);
            GL.TexParameter(TextureTarget.Texture2D,
                   TextureParameterName.TextureMinFilter, (float)TextureMinFilter.LinearMipmapLinear);
            GL.TexParameter(TextureTarget.Texture2D,
                   TextureParameterName.TextureMagFilter, (float)TextureMagFilter.Linear);

            // tell OpenGL to build mipmaps out of the bitmap data
            GL.TexParameter(TextureTarget.Texture2D,
                   TextureParameterName.GenerateMipmap, (float)1.0f);

            var pixelFormat = MapPixelFormat(texture.Format);
            var pixelType = MapPixelType(texture.Format);

            // load the texture
            GL.TexImage2D(
                TextureTarget.Texture2D,
                0, // level
                PixelInternalFormat.Four,
                texture.Width, texture.Height,
                0, // border
                OpenTK.Graphics.OpenGL.PixelFormat.Bgra,
                pixelType,
                texture.Pixels
                );

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
                    throw new InvalidOperationException("Invalid pixel format: " + format);
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
                    throw new InvalidOperationException("Invalid pixel format: " + format);
            }
        }
    }
}
