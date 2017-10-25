namespace Vd2.OpenGL
{
    internal class SetResourceSetEntry : OpenGLCommandEntry
    {
        public ResourceSet ResourceSet;

        public SetResourceSetEntry(ResourceSet rs)
        {
            ResourceSet = rs;
        }

        public SetResourceSetEntry() { }

        public SetResourceSetEntry Init(ResourceSet rs)
        {
            ResourceSet = rs;
            return this;
        }

        public override void ClearReferences()
        {
            ResourceSet = null;
        }
    }
}