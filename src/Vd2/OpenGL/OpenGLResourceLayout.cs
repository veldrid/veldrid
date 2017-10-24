namespace Vd2.OpenGL
{
    internal class OpenGLResourceLayout : ResourceLayout
    {
        private ResourceLayoutDescription description;

        public OpenGLResourceLayout(ref ResourceLayoutDescription description)
        {
            this.description = description;
        }

        public override void Dispose()
        {
            throw new System.NotImplementedException();
        }
    }
}