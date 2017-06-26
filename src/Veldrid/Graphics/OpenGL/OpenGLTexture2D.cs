using OpenTK.Graphics.OpenGL;
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Veldrid.Graphics.OpenGL
{
    public class OpenGLTexture2D : OpenGLTexture, DeviceTexture2D, IDisposable, PixelDataProvider
    {
        private readonly OpenTK.Graphics.OpenGL.PixelFormat _pixelFormat;
        private readonly PixelInternalFormat _internalFormat;
        private readonly PixelType _pixelType;
        private readonly Graphics.PixelFormat _veldridFormat;

        public OpenGLTexture2D(int width, int height, PixelFormat format, IntPtr pixelData)
            : this(1, width, height, format, OpenGLFormats.MapPixelInternalFormat(format), OpenGLFormats.MapPixelFormat(format), OpenGLFormats.MapPixelType(format))
        {
            SetTextureData(1, 0, 0, width, height, pixelData, FormatHelpers.GetPixelSize(format) * width * height);
        }

        public OpenGLTexture2D(
            int mipLevels,
            int width,
            int height,
            PixelFormat veldridFormat,
            PixelInternalFormat internalFormat,
            OpenTK.Graphics.OpenGL.PixelFormat pixelFormat,
            PixelType pixelType)
            : base(TextureTarget.Texture2D, width, height)
        {
            MipLevels = mipLevels;
            _veldridFormat = veldridFormat;
            _internalFormat = internalFormat;
            _pixelFormat = pixelFormat;
            _pixelType = pixelType;

            Bind();

            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);


            for (int currentLevel = 0; currentLevel < mipLevels; currentLevel++)
            {
                // Set size, load empty data into texture
                GL.TexImage2D(
                    TextureTarget.Texture2D,
                    currentLevel,
                    internalFormat,
                    width, height,
                    0, // border
                    _pixelFormat,
                    _pixelType,
                    IntPtr.Zero);
                Utilities.CheckLastGLError();
                width = Math.Max(1, width / 2);
                height = Math.Max(1, height / 2);
            }
        }

        public void SetTextureData(int mipLevel, int x, int y, int width, int height, IntPtr data, int dataSizeInBytes)
        {
            Bind();
            int pixelSize = FormatHelpers.GetPixelSize(_veldridFormat);
            if (pixelSize < 4)
            {
                GL.PixelStore(PixelStoreParameter.UnpackAlignment, pixelSize);
                Utilities.CheckLastGLError();
            }
            GL.TexSubImage2D(TextureTarget.Texture2D, mipLevel, x, y, width, height, _pixelFormat, _pixelType, data);
            Utilities.CheckLastGLError();
            if (pixelSize < 4)
            {
                GL.PixelStore(PixelStoreParameter.UnpackAlignment, 4);
                Utilities.CheckLastGLError();
            }
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
