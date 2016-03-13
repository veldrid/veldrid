using OpenTK.Graphics.OpenGL;
using System;
using System.Runtime.InteropServices;

namespace Veldrid.Graphics.OpenGL
{
    public class OpenGLTexture : DeviceTexture, RenderStateModifier, IDisposable, PixelDataProvider
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
            : this(width, height, internalFormat, pixelFormat, pixelType, IntPtr.Zero)
        {
        }

        public OpenGLTexture(int width, int height, PixelFormat format, IntPtr pixelData)
        : this(width, height, OpenGLFormats.MapPixelInternalFormat(format), OpenGLFormats.MapPixelFormat(format), OpenGLFormats.MapPixelType(format), pixelData)
        { }

        public OpenGLTexture(
            int width,
            int height,
            PixelInternalFormat internalFormat,
            OpenTK.Graphics.OpenGL.PixelFormat pixelFormat,
            PixelType pixelType,
            IntPtr pixelData)
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
                pixelData);
        }

        public static OpenGLTexture Create<T>(T[] pixelData, int width, int height, int pixelSizeInBytes, PixelFormat format) where T : struct
        {
            var internalFormat = OpenGLFormats.MapPixelInternalFormat(format);
            var pixelFormat = OpenGLFormats.MapPixelFormat(format);
            var pixelType = OpenGLFormats.MapPixelType(format);

            OpenGLTexture texture = new OpenGLTexture(
                width,
                height,
                internalFormat,
                pixelFormat,
                pixelType);

            GL.BindTexture(TextureTarget.Texture2D, texture.TextureID);
            GL.TexImage2D(TextureTarget.Texture2D, 0, internalFormat, width, height, 0, pixelFormat, pixelType, pixelData);
            GL.BindTexture(TextureTarget.Texture2D, 0);

            return texture;
        }

        public void Apply()
        {
            GL.BindTexture(TextureTarget.Texture2D, TextureID);
        }

        public void CopyTo(TextureData textureData)
        {
            textureData.AcceptPixelData(this);
        }

        public void SetPixelData<T>(T[] pixelData, int width, int height, int pixelSizeInBytes) where T : struct
        {
            GL.BindTexture(TextureTarget.Texture2D, TextureID);
            GL.GetTexImage(TextureTarget.Texture2D, 0, _pixelFormat, _pixelType, pixelData);

            // Need to reverse the rows vertically.
            int rowPixels = width * pixelSizeInBytes / System.Runtime.InteropServices.Marshal.SizeOf<T>();
            float[] stagingRow = new float[rowPixels];
            for (int y = height - 1, destY = 0; y > (height / 2); y--)
            {
                Array.ConstrainedCopy(pixelData, y * rowPixels, stagingRow, 0, rowPixels);
                Array.ConstrainedCopy(pixelData, destY * rowPixels, pixelData, y * rowPixels, rowPixels);
                Array.ConstrainedCopy(stagingRow, 0, pixelData, destY * rowPixels, rowPixels);

                destY++;
            }

            GL.BindTexture(TextureTarget.Texture2D, 0);
        }

        public unsafe void SetPixelData(IntPtr pixelData, int width, int height, int pixelSizeInBytes)
        {
            GL.BindTexture(TextureTarget.Texture2D, TextureID);
            GL.GetTexImage(TextureTarget.Texture2D, 0, _pixelFormat, _pixelType, pixelData);

            // Need to reverse the rows vertically.
            int rowBytes = width * pixelSizeInBytes;
            IntPtr stagingRow = Marshal.AllocHGlobal(rowBytes);
            byte* stagingPtr = (byte*)stagingRow.ToPointer();
            byte* sourcePtr = (byte*)pixelData.ToPointer();
            for (int y = height - 1, destY = 0; y > (height / 2); y--)
            {
                Buffer.MemoryCopy(sourcePtr + (y * rowBytes), stagingPtr, rowBytes, rowBytes);
                Buffer.MemoryCopy(sourcePtr + (destY * rowBytes), sourcePtr + (y * rowBytes), rowBytes, rowBytes);
                Buffer.MemoryCopy(stagingPtr, sourcePtr + (destY * rowBytes), rowBytes, rowBytes);

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
