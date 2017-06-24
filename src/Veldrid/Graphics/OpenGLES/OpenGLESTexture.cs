using System;
using OpenTK.Graphics.ES30;

namespace Veldrid.Graphics.OpenGLES
{
    public class OpenGLESTexture : DeviceTexture, IDisposable
    {
        public int ID { get; }
        public TextureTarget Target { get; }
        public int Width { get; protected set; }
        public int Height { get; protected set; }
        public int MipLevels { get; protected set; } = 1;

        public OpenGLESTexture(TextureTarget target, int width, int height)
        {
            ID = GL.GenTexture();
            Utilities.CheckLastGLES3Error();
            Target = target;
            Width = width;
            Height = height;
        }

        public void Bind()
        {
            GL.BindTexture(Target, ID);
            Utilities.CheckLastGLES3Error();
        }

        public void Dispose()
        {
            GL.DeleteTexture(ID);
            Utilities.CheckLastGLES3Error();
        }
    }
}