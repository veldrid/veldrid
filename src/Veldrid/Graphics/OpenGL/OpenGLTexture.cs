using System;
using OpenTK.Graphics.OpenGL;

namespace Veldrid.Graphics.OpenGL
{
    public class OpenGLTexture : DeviceTexture, IDisposable
    {
        public int ID { get; }
        public TextureTarget Target { get; }
        public int Width { get; protected set; }
        public int Height { get; protected set; }
        public int MipLevels { get; protected set; } = 1;

        public OpenGLTexture(TextureTarget target, int width, int height)
        {
            ID = GL.GenTexture();
            Target = target;
            Width = width;
            Height = height;
        }

        public void Bind()
        {
            GL.BindTexture(Target, ID);
        }

        public void Dispose()
        {
            GL.DeleteTexture(ID);
        }
    }
}