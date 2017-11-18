namespace Veldrid.OpenGL.ManagedEntryList
{
    internal class SetComputeResourceSetEntry : OpenGLCommandEntry
    {
        public uint Slot;
        public ResourceSet ResourceSet;

        public SetComputeResourceSetEntry(uint slot, ResourceSet rs)
        {
            Slot = slot;
            ResourceSet = rs;
        }

        public SetComputeResourceSetEntry() { }

        public SetComputeResourceSetEntry Init(uint slot, ResourceSet rs)
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