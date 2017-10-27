namespace Veldrid.OpenGL
{
    internal class SetResourceSetEntry : OpenGLCommandEntry
    {
        public uint Slot;
        public ResourceSet ResourceSet;

        public SetResourceSetEntry(uint slot, ResourceSet rs)
        {
            Slot = slot;
            ResourceSet = rs;
        }

        public SetResourceSetEntry() { }

        public SetResourceSetEntry Init(uint slot, ResourceSet rs)
        {
            Slot = slot;
            ResourceSet = rs;
            return this;
        }

        public override void ClearReferences()
        {
            ResourceSet = null;
        }
    }
}