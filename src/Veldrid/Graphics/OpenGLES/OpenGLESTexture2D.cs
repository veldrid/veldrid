using OpenTK.Graphics.ES30;
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Veldrid.Graphics.OpenGLES
{
    public class OpenGLESTexture2D : OpenGLESTexture, DeviceTexture2D, IDisposable
    {
        private readonly OpenTK.Graphics.ES30.PixelFormat _pixelFormat;
        private readonly PixelType _pixelType;
        private readonly Graphics.PixelFormat _veldridFormat;
        private TextureComponentCount _texComponentCount;


        public OpenGLESTexture2D(int width, int height, PixelFormat format, IntPtr pixelData)
            : this(1, width, height, format, OpenGLESFormats.MapPixelFormat(format), OpenGLESFormats.MapPixelType(format))
        {
            SetTextureData(1, 0, 0, width, height, pixelData, FormatHelpers.GetPixelSize(format) * width * height);
        }

        public OpenGLESTexture2D(
            int mipLevels,
            int width,
            int height,
            PixelFormat veldridFormat,
            OpenTK.Graphics.ES30.PixelFormat pixelFormat,
            PixelType pixelType)
            : base(TextureTarget.Texture2D, width, height)
        {
            MipLevels = mipLevels;
            _veldridFormat = veldridFormat;
            _pixelFormat = pixelFormat;
            _pixelType = pixelType;
            _texComponentCount = OpenGLESFormats.MapTextureComponentCount(veldridFormat, pixelFormat == OpenTK.Graphics.ES30.PixelFormat.DepthComponent);

            Bind();

            for (int currentLevel = 0; currentLevel < mipLevels; currentLevel++)
            {
                // Set size, load empty data into texture
                GL.TexImage2D(
                    TextureTarget2d.Texture2D,
                    currentLevel,
                    _texComponentCount,
                    width, height,
                    0, // border
                    _pixelFormat,
                    _pixelType,
                    IntPtr.Zero);
                Utilities.CheckLastGLES3Error();
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
                Utilities.CheckLastGLES3Error();
            }
            GL.TexSubImage2D(TextureTarget2d.Texture2D, mipLevel, x, y, width, height, _pixelFormat, _pixelType, data);
            Utilities.CheckLastGLES3Error();
            if (pixelSize < 4)
            {
                GL.PixelStore(PixelStoreParameter.UnpackAlignment, 4);
                Utilities.CheckLastGLES3Error();
            }
        }

        public void GetTextureData<T>(int mipLevel, T[] destination) where T : struct
        {
            throw new NotSupportedException();
        }

        public unsafe void GetTextureData(int mipLevel, IntPtr destination, int storageSizeInBytes)
        {
            throw new NotSupportedException();
        }
    }
}
