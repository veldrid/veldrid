using OpenTK.Graphics.ES30;
using System;
using System.Runtime.InteropServices;

namespace Veldrid.Graphics.OpenGLES
{
    public class OpenGLESTexture2D : OpenGLESTexture, DeviceTexture2D, IDisposable, PixelDataProvider
    {
        private readonly OpenTK.Graphics.ES30.PixelFormat _pixelFormat;
        private readonly PixelType _pixelType;
        private readonly Graphics.PixelFormat _veldridFormat;
        private TextureComponentCount _texComponentCount;

        public OpenGLESTexture2D(
            int width,
            int height,
            PixelFormat veldridFormat,
            OpenTK.Graphics.ES30.PixelFormat pixelFormat,
            PixelType pixelType)
            : this(width, height, veldridFormat, pixelFormat, pixelType, IntPtr.Zero)
        {
        }

        public OpenGLESTexture2D(int width, int height, PixelFormat format, IntPtr pixelData)
        : this(width, height, format, OpenGLESFormats.MapPixelFormat(format), OpenGLESFormats.MapPixelType(format), pixelData)
        {
        }

        public OpenGLESTexture2D(
            int width,
            int height,
            PixelFormat veldridFormat,
            OpenTK.Graphics.ES30.PixelFormat pixelFormat,
            PixelType pixelType,
            IntPtr pixelData)
            : base(TextureTarget.Texture2D, width, height)
        {
            _veldridFormat = veldridFormat;
            _pixelFormat = pixelFormat;
            _pixelType = pixelType;
            _texComponentCount = OpenGLESFormats.MapTextureComponentCount(veldridFormat);

            Bind();

            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
            Utilities.CheckLastGLES3Error();
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
            Utilities.CheckLastGLES3Error();

            // Set size, load empty data into texture
            GL.TexImage2D(
                TextureTarget2d.Texture2D,
                0, // level
                _texComponentCount,
                width, height,
                0, // border
                _pixelFormat,
                _pixelType,
                pixelData);
            Utilities.CheckLastGLES3Error(); ;
        }

        public static OpenGLESTexture2D Create<T>(T[] pixelData, int width, int height, int pixelSizeInBytes, PixelFormat format) where T : struct
        {
            var pixelFormat = OpenGLESFormats.MapPixelFormat(format);
            var pixelType = OpenGLESFormats.MapPixelType(format);

            OpenGLESTexture2D texture = new OpenGLESTexture2D(
                width,
                height,
                format,
                pixelFormat,
                pixelType);

            texture.Bind();
            GL.TexImage2D(TextureTarget2d.Texture2D, 0, texture._texComponentCount, width, height, 0, pixelFormat, pixelType, pixelData);
            Utilities.CheckLastGLES3Error();
            GL.BindTexture(TextureTarget.Texture2D, 0);
            Utilities.CheckLastGLES3Error();

            return texture;
        }

        public void SetTextureData(int x, int y, int width, int height, IntPtr data, int dataSizeInBytes)
        {
            Bind();
            GL.PixelStore(PixelStoreParameter.UnpackAlignment, (int)FormatHelpers.GetPixelSize(_veldridFormat));
            Utilities.CheckLastGLES3Error();
            GL.TexSubImage2D(TextureTarget2d.Texture2D, 0, x, y, width, height, _pixelFormat, _pixelType, data);
            Utilities.CheckLastGLES3Error();
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
            // TODO: GL.ReadPixels (?)
            throw new NotImplementedException();
        }
    }
}
