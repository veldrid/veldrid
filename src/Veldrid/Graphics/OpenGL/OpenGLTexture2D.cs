using OpenTK.Graphics.OpenGL;
using System;
using System.Runtime.InteropServices;

namespace Veldrid.Graphics.OpenGL
{
    public class OpenGLTexture2D : OpenGLTexture, DeviceTexture2D, IDisposable, PixelDataProvider
    {
        private readonly OpenTK.Graphics.OpenGL.PixelFormat _pixelFormat;
        private readonly PixelInternalFormat _internalFormat;
        private readonly PixelType _pixelType;
        private readonly Graphics.PixelFormat _veldridFormat;

        public OpenGLTexture2D(
            int width,
            int height,
            PixelFormat veldridFormat,
            PixelInternalFormat internalFormat,
            OpenTK.Graphics.OpenGL.PixelFormat pixelFormat,
            PixelType pixelType)
            : this(width, height, veldridFormat, internalFormat, pixelFormat, pixelType, IntPtr.Zero)
        {
        }

        public OpenGLTexture2D(int width, int height, PixelFormat format, IntPtr pixelData)
        : this(width, height, format, OpenGLFormats.MapPixelInternalFormat(format), OpenGLFormats.MapPixelFormat(format), OpenGLFormats.MapPixelType(format), pixelData)
        {
        }

        public OpenGLTexture2D(
            int width,
            int height,
            PixelFormat veldridFormat,
            PixelInternalFormat internalFormat,
            OpenTK.Graphics.OpenGL.PixelFormat pixelFormat,
            PixelType pixelType,
            IntPtr pixelData)
            : base(TextureTarget.Texture2D, width, height)
        {
            _veldridFormat = veldridFormat;
            _internalFormat = internalFormat;
            _pixelFormat = pixelFormat;
            _pixelType = pixelType;

            Bind();

            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);

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

        public static OpenGLTexture2D Create<T>(T[] pixelData, int width, int height, int pixelSizeInBytes, PixelFormat format) where T : struct
        {
            var internalFormat = OpenGLFormats.MapPixelInternalFormat(format);
            var pixelFormat = OpenGLFormats.MapPixelFormat(format);
            var pixelType = OpenGLFormats.MapPixelType(format);

            OpenGLTexture2D texture = new OpenGLTexture2D(
                width,
                height,
                format,
                internalFormat,
                pixelFormat,
                pixelType);

            texture.Bind();
            GL.TexImage2D(TextureTarget.Texture2D, 0, internalFormat, width, height, 0, pixelFormat, pixelType, pixelData);
            GL.BindTexture(TextureTarget.Texture2D, 0);

            return texture;
        }

        public void SetTextureData(int x, int y, int width, int height, IntPtr data, int dataSizeInBytes)
        {
            Bind();
            GL.PixelStore(PixelStoreParameter.UnpackAlignment, FormatHelpers.GetPixelSize(_veldridFormat));
            GL.TexSubImage2D(TextureTarget.Texture2D, 0, x, y, width, height, _pixelFormat, _pixelType, data);
        }

        public void CopyTo(TextureData textureData)
        {
            textureData.AcceptPixelData(this);
        }

        public void SetPixelData<T>(T[] pixelData, int width, int height, int pixelSizeInBytes) where T : struct
        {
            Width = width;
            Height = height;

            GCHandle handle = GCHandle.Alloc(pixelData, GCHandleType.Pinned);
            SetPixelData(handle.AddrOfPinnedObject(), width, height, pixelSizeInBytes);
            handle.Free();
        }

        public unsafe void SetPixelData(IntPtr pixelData, int width, int height, int pixelSizeInBytes)
        {
            Width = width;
            Height = height;

            Bind();
            GL.GetTexImage(TextureTarget.Texture2D, 0, OpenTK.Graphics.OpenGL.PixelFormat.Alpha, _pixelType, pixelData);
            GL.BindTexture(TextureTarget.Texture2D, 0);
            GL.PixelStore(PixelStoreParameter.PackAlignment, 1);

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

            // Reset to default value.
            GL.PixelStore(PixelStoreParameter.PackAlignment, 4);
        }
    }
}
