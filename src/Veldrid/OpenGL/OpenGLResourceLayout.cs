namespace Veldrid.OpenGL
{
    internal class OpenGLResourceLayout : ResourceLayout
    {
        public ResourceLayoutDescription Description { get; }

        public OpenGLResourceLayout(ref ResourceLayoutDescription description)
        {
            Description = description;
        }

        public override void Dispose()
        {
        }
    }
}