namespace Vd2.OpenGL
{
    internal class SetResourceSetEntry : OpenGLCommandEntry
    {
        private ResourceSet rs;

        public SetResourceSetEntry(ResourceSet rs)
        {
            this.rs = rs;
        }
    }
}