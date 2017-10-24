namespace Vd2.OpenGL
{
    internal class SetResourceSetEntry : OpenGLCommandEntry
    {
        public readonly ResourceSet ResourceSet;

        public SetResourceSetEntry(ResourceSet rs)
        {
            this.ResourceSet = rs;
        }
    }
}