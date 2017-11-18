namespace Veldrid.OpenGL.ManagedEntryList
{
    internal class SetGraphicsResourceSetEntry : OpenGLCommandEntry
    {
        public uint Slot;
        public ResourceSet ResourceSet;

        public SetGraphicsResourceSetEntry(uint slot, ResourceSet rs)
        {
            Slot = slot;
            ResourceSet = rs;
        }

        public SetGraphicsResourceSetEntry() { }

        public SetGraphicsResourceSetEntry Init(uint slot, ResourceSet rs)
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