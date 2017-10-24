namespace Vd2.OpenGL
{
    internal class OpenGLResourceSet : ResourceSet
    {
        private ResourceSetDescription description;

        public OpenGLResourceSet(ref ResourceSetDescription description)
        {
            this.description = description;
        }

        public override void Dispose()
        {
        }
    }
}