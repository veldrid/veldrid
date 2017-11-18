namespace Veldrid.OpenGL.ManagedEntryList
{
    internal class DispatchEntry : OpenGLCommandEntry
    {
        public uint GroupCountX;
        public uint GroupCountY;
        public uint GroupCountZ;

        public DispatchEntry() { }

        public DispatchEntry(uint groupCountX, uint groupCountY, uint groupCountZ)
        {
            GroupCountX = groupCountX;
            GroupCountY = groupCountY;
            GroupCountZ = groupCountZ;
        }

        public DispatchEntry Init(uint groupCountX, uint groupCountY, uint groupCountZ)
        {
            GroupCountX = groupCountX;
            GroupCountY = groupCountY;
            GroupCountZ = groupCountZ;

            return this;
        }

        public override void ClearReferences()
        {
        }
    }
}